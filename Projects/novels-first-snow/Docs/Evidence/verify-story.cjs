// Standalone source-level review; does not launch Unity or write compiled assets.
// node verify-story.cjs /path/to/inkjs /path/to/s01e01.ink
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const [inkjsPath, sourcePath] = process.argv.slice(2);
assert(inkjsPath && sourcePath, 'Provide the installed inkjs directory and Ink source');
const { Compiler } = require(path.join(inkjsPath, 'compiler/Compiler.js'));
const { Story } = require(path.join(inkjsPath, 'engine/Story.js'));
const source = fs.readFileSync(sourcePath, 'utf8');
const compiler = new Compiler(source);
const compiled = compiler.Compile();
assert.deepEqual(compiler.errors, []);
assert.deepEqual(compiler.warnings, []);
const json = compiled.ToJson();
const runtimeProblems = [];
const endings = {};
const witnesses = {};
const ranges = { words: [Infinity, 0], dialogueSteps: [Infinity, 0], decisions: [Infinity, 0] };
const words = s => (s.match(/[\p{L}\p{N}]+(?:[-’][\p{L}\p{N}]+)*/gu) || []).length;
const extractProse = text => text.split('\n')
  .filter(line => /^(?:\.\.\.|Лёша|Мия|Соня)(?:\s*\([^)]*\))?:/.test(line))
  .map(line => line.replace(/^[^:]+:\s*/, ''));
let routes = 0;
let statesVisited = 0;
function walk(stateJson, transcript, choices) {
  assert(++statesVisited < 5000, 'Unexpected cycle or branch explosion');
  const story = new Story(json);
  story.onError = (message, type) => runtimeProblems.push({ message, type });
  if (stateJson) story.state.LoadJson(stateJson);
  let text = transcript;
  let lines = 0;
  while (story.canContinue) {
    assert(++lines < 3000, 'Unexpected non-terminating path');
    text += story.Continue();
  }
  if (story.currentChoices.length) {
    assert(story.currentChoices.length >= 2, 'One-option gate');
    const saved = story.state.ToJson();
    const labels = story.currentChoices.map(choice => choice.text);
    for (let index = 0; index < labels.length; index++) {
      story.state.LoadJson(saved);
      story.ChooseChoiceIndex(index);
      walk(story.state.ToJson(), text, [...choices, labels[index]]);
    }
    return;
  }
  routes++;
  const found = [...text.matchAll(/КОНЦОВКА: ([^\n]+)/g)].map(match => match[1].trim());
  assert.equal(found.length, 1, 'Path must reach exactly one ending');
  assert(text.includes('КОНЕЦ СЕРИИ'), 'Path must reach episode completion');
  const ending = found[0];
  endings[ending] = (endings[ending] || 0) + 1;
  const variables = Object.fromEntries(['attention', 'courage', 'admitted_closeness', 'contact_choice', 'photo_choice', 'star_read']
    .map(key => [key, story.variablesState.$(key)]));
  witnesses[ending] ||= { variables, choices };
  if (variables.admitted_closeness) {
    assert(text.includes('На крыше ты сказал, что тебе понравилось.'));
    assert(!text.includes('А ты ни разу не сказал, что снимаешь меня не только ради конкурса.'));
  } else {
    assert(text.includes('А ты ни разу не сказал, что снимаешь меня не только ради конкурса.'));
    assert(!text.includes('На крыше ты сказал, что тебе понравилось.'));
  }
  if (variables.photo_choice === 'take_photo') {
    assert(text.includes('Покажу тебе, прежде чем кому-нибудь ещё.'));
    assert(text.includes('В субботу'));
    assert(text.includes('Позже, под безопасным красным светом'));
    assert(!text.includes('Он печатает цифровой портрет на фотопринтере'));
  } else {
    assert(text.includes('Он печатает цифровой портрет на фотопринтере'));
    assert(!text.includes('Позже, под безопасным красным светом'));
  }
  assert(text.includes('В следующую пятницу Лёша возвращается к летнему экрану.'));
  assert(text.includes('Подаренная ею звезда лежит у Лёши'));
  const prose = extractProse(text);
  const metrics = { words: words(prose.join(' ')), dialogueSteps: prose.length, decisions: choices.length };
  for (const [key, value] of Object.entries(metrics)) {
    ranges[key][0] = Math.min(ranges[key][0], value);
    ranges[key][1] = Math.max(ranges[key][1], value);
  }
}
walk(null, '', []);
assert.deepEqual(runtimeProblems, []);
assert.equal(Object.keys(endings).length, 3, 'All three endings must be reachable');
assert(!source.includes('список в радиорубке, которой ещё даже не видел'));
assert(!source.includes('восемь лет в одном классе'));
const result = {
  sourceSha256: crypto.createHash('sha256').update(source).digest('hex'),
  checker: 'inkjs ' + require(path.join(inkjsPath, 'package.json')).version,
  compileErrors: compiler.errors,
  compileWarnings: compiler.warnings,
  runtimeProblems,
  routes,
  endings,
  ranges,
  witnesses,
  scope: 'Standalone Ink language and exhaustive traversal only; not Unity integration or measured playtime'
};
console.log(JSON.stringify(result, null, 2));
