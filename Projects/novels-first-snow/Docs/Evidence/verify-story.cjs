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
const projectRoot = path.resolve(path.dirname(sourcePath), '..', '..');
const definitionSource = fs.readFileSync(path.join(projectRoot, 'Assets/first-snow.asset'), 'utf8');
// Deliberately bounded to this story's single-line YAML fields, not a general YAML parser.
const scalar = value => value.trim().startsWith('"') ? JSON.parse(value.trim()) : value.trim();
const field = key => {
  const match = definitionSource.match(new RegExp(`^  ${key}: (.+)$`, 'm'));
  assert(match, `Missing definition field ${key}`);
  return scalar(match[1]);
};
const canonical = value => value.normalize('NFC').trim().toLowerCase();
const mainCharacter = field('_mainCharacter');
const runtimeName = speaker => speaker === mainCharacter ? 'maincharacter' : canonical(speaker);
const defaults = new Map([...definitionSource.matchAll(/^  - _character: (.+)\n    _clothes:([^\n]*)/gm)]
  .map(match => [runtimeName(scalar(match[1])), scalar(match[2])]));
const aliases = new Map([...definitionSource.matchAll(/^  - _alias: (.+)\n    _target: (.+)$/gm)]
  .map(match => [scalar(match[1]), scalar(match[2])]));
const card = JSON.parse(fs.readFileSync(path.join(projectRoot, 'Config/card.json'), 'utf8'));
assert.equal(card.storyId, field('_id'));
assert(fs.existsSync(path.join(projectRoot, 'Config', card.cover)), 'Missing cover');
assert.equal(fs.readFileSync(path.join(projectRoot, 'Assets/Ink/first-snow.ink'), 'utf8').replace(/\/\/[^\n]*/g, '').trim(),
  'INCLUDE s01e01.ink', 'Audit all root Ink includes when adding episodes');
const rootGuid = fs.readFileSync(path.join(projectRoot, 'Assets/Ink/first-snow.ink.meta'), 'utf8')
  .match(/^guid: (\w+)$/m)[1];
assert.equal(field('_authoringRootInkGuid'), rootGuid);
const assetAudit = { locations: [], characterVariants: [], runtimeMappings: [], missing: [] };
const checkedAddresses = new Map();
function resolveAsset(address, category) {
  if (checkedAddresses.has(address)) return checkedAddresses.get(address);
  const visited = new Set();
  let target = address;
  while (aliases.has(target)) {
    assert(!visited.has(target), `Cyclic art alias: ${address}`);
    visited.add(target);
    target = aliases.get(target);
  }
  const relative = target.replace(/^story\/character\/characters\//, 'Assets/Characters/')
    .replace(/^story\/location\/locations\//, 'Assets/Locations/');
  assert(relative.startsWith('Assets/') && !relative.includes('..'), `Unsupported address: ${target}`);
  const absolute = path.join(projectRoot, relative);
  assert(fs.existsSync(absolute), `Missing runtime asset: ${address} -> ${relative}`);
  const png = fs.readFileSync(absolute);
  assert.equal(png.subarray(0, 8).toString('hex'), '89504e470d0a1a0a', `Not a PNG: ${relative}`);
  assert(png.readUInt32BE(16) > 0 && png.readUInt32BE(20) > 0, `Empty PNG: ${relative}`);
  if (!assetAudit[category].includes(relative)) assetAudit[category].push(relative);
  if (category === 'characterVariants') assetAudit.runtimeMappings.push({ address, file: relative });
  checkedAddresses.set(address, relative);
  return relative;
}
for (const speaker of ['Лёша', 'Мия', 'Соня']) {
  const name = runtimeName(speaker);
  const outfit = defaults.get(name);
  assert(outfit, `Missing initial clothes for ${speaker}; current whole loader exits before selector resolution`);
  resolveAsset(`story/character/characters/${name}/view/whole/${outfit}/main.png`, 'characterVariants');
}
function auditPresentation(text, appearances) {
  for (const line of text.split('\n').map(value => value.trim()).filter(Boolean)) {
    const location = line.match(/^Локация:\s*(.+)$/);
    if (location) {
      resolveAsset(`story/location/locations/${canonical(location[1])}.png`, 'locations');
      continue;
    }
    const dialogue = line.match(/^(Лёша|Мия|Соня)(?:\s*\(([^)]*)\))?:/);
    if (!dialogue) {
      assert(/^(?:\.\.\.|Название|Серия|Жанры|Аннотация|Статы|Уведомление):/.test(line),
        `Unreviewed output command or speaker: ${line}`);
      continue;
    }
    const name = runtimeName(dialogue[1]);
    if (dialogue[2]) {
      const selectors = dialogue[2].split(',').map(canonical);
      assert.equal(selectors.length, 2, 'Review new selector syntax before changing the audit');
      appearances[name] = { outfit: selectors[0], variant: selectors[1] };
    }
    const look = appearances[name] || { outfit: defaults.get(name), variant: 'main' };
    resolveAsset(`story/character/characters/${name}/view/whole/${look.outfit}/${look.variant}.png`, 'characterVariants');
  }
}
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
function walk(stateJson, transcript, choices, appearances = {}) {
  assert(++statesVisited < 5000, 'Unexpected cycle or branch explosion');
  const story = new Story(json);
  story.onError = (message, type) => runtimeProblems.push({ message, type });
  if (stateJson) story.state.LoadJson(stateJson);
  let text = transcript;
  let lines = 0;
  while (story.canContinue) {
    assert(++lines < 3000, 'Unexpected non-terminating path');
    const output = story.Continue();
    auditPresentation(output, appearances);
    text += output;
  }
  if (story.currentChoices.length) {
    assert(story.currentChoices.length >= 2, 'One-option gate');
    const saved = story.state.ToJson();
    const labels = story.currentChoices.map(choice => choice.text);
    for (let index = 0; index < labels.length; index++) {
      story.state.LoadJson(saved);
      story.ChooseChoiceIndex(index);
      walk(story.state.ToJson(), text, [...choices, labels[index]], { ...appearances });
    }
    return;
  }
  routes++;
  const found = [...text.matchAll(/КОНЦОВКА: ([^\n]+)/g)].map(match => match[1].trim());
  assert.equal(found.length, 1, 'Path must reach exactly one ending');
  assert(text.includes('КОНЕЦ СЕРИИ'), 'Path must reach episode completion');
  const ending = found[0];
  endings[ending] = (endings[ending] || 0) + 1;
  const variables = Object.fromEntries(['attention', 'courage', 'admitted_closeness', 'contact_choice', 'photo_choice', 'star_read', 'star_opened']
    .map(key => [key, story.variablesState.$(key)]));
  witnesses[ending] ||= { variables, choices };
  const offeredCarry = choices.includes('Не спрашивать при Соне');
  assert.equal(text.includes('Я предлагал отнести, а не ремонтировать.'), offeredCarry);
  assert.equal(text.includes('Сейчас я хотел спросить, с какой стороны держать коробку.'), !offeredCarry);
  if (variables.contact_choice === 'keep_hands')
    assert(text.includes('соответствующие снимки на контактном листе тоже разрезает'));
  assert.equal(text.includes('Сохранён только согласованный кадр рук со звездой'), variables.contact_choice === 'keep_hands');
  assert.equal(text.includes('Несогласованные кадры Мии уничтожены'), variables.contact_choice === 'destroy');
  if (!variables.star_read) assert(text.includes('До встречи не покажу.'));
  assert.equal(variables.star_opened, true, 'The star must be opened by the end on both historical-choice branches');
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
    assert(text.includes('Лёша нажимает «Восстановить» и отправляет Мие файл.'));
    assert(text.includes('можно ли распечатать восстановленный портрет в фотокружке и показать руководителю'));
    assert(!text.includes('Позже, под безопасным красным светом'));
  }
  assert(text.includes('В следующую пятницу Лёша возвращается к летнему экрану.'));
  const sneeze = 'Соня чихает в самый момент спуска.';
  const anecdote = 'Просто рассказывает, как Соня чихнула в самый важный момент.';
  assert.equal(text.split(sneeze).length - 1, 1, 'The sneeze occurs once on either film branch');
  assert(text.indexOf(sneeze) < text.indexOf(anecdote), 'The later anecdote must follow its event');
  assert(text.includes('заново рисует школу, набережную и остановку'));
  assert(!text.includes('что переснял экран'), 'The first screen photograph is not a reshoot');
  assert(text.includes('Подаренная ею звезда лежит у Лёши'));
  assert.equal(text.split('Первое воскресенье — первый из четырёх договорённых звонков').length - 1, 1);
  assert.equal(text.split('Четвёртое воскресенье — договорённость выполнена').length - 1, 1);
  assert(text.includes('Десятиклассник Лёша Ветров остаётся в школьной фотолаборатории'));
  assert(text.includes('Соня, его близкая подруга с пятого класса'));
  assert.equal(text.split('Первый разговор длится сорок три минуты.').length - 1, 1);
  assert.equal(text.includes('Лёша не предлагает закрепить момент снимком'), ending === 'ПЕРВЫЙ СНЕГ');
  assert.equal(text.includes('В календаре появляется новая встреча'), ending === 'ЧЕТЫРЕ ВОСКРЕСЕНЬЯ');
  assert.equal(text.includes('оба записывают дату'), ending === 'НЕРЕЗКИЙ, НО НАШ');
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
assetAudit.locations.sort();
assetAudit.characterVariants.sort();
assetAudit.runtimeMappings.sort((a, b) => a.address.localeCompare(b.address, 'en'));
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
  assetAudit,
  scope: 'Standalone Ink compilation, exhaustive traversal, initial clothes, protagonist aliases and inherited selector audit against current source contracts; not Unity execution or measured playtime'
};
console.log(JSON.stringify(result, null, 2));
