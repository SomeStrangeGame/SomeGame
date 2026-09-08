// Standalone Ink audit. No Unity files or generated runtime payloads are written.
// Usage: node story-audit.cjs /path/to/inkjs/dist/ink-full.js
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const { Compiler, CompilerOptions } = require(path.resolve(process.argv[2]));
const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'Assets/Ink/s01e01.ink'), 'utf8');
const compiler = new Compiler(source, new CompilerOptions(null, [], true));
const story = compiler.Compile();
assert.deepEqual(compiler.errors, []);
assert.deepEqual(compiler.warnings, []);
story.onError = (message) => { throw new Error(message); };

const expectedLocations = ['bridge', 'boundary', 'charcoal', 'snow-chapel',
  'ravine', 'toll-village', 'ford', 'drowned-belfry', 'watch-hut', 'wolf-den',
  'oath-oak', 'berezhki'];
const endingNames = ['names', 'witnesses', 'nameless', 'pathless'];
const counts = Object.fromEntries(endingNames.map(x => [x, 0]));
const regressionCoverage = {pathlessDamage: 0, severedBell: 0,
  hamletReservation: 0, hamletReservationAfterBelfry: 0,
  ribbonTaken: 0, witnessesRibbon: 0};
const examples = {};
const labels = new Set();
const characterSelectors = new Set();
const audioIds = new Set();
let total = 0;
let minWords = Infinity;
let maxWords = 0;
let nodes = 0;

// Validate the literal selectors on every source branch, not only one replay.
for (const match of source.matchAll(/^\s*([^\n:()]+?)\s+\(([^,()]+),\s*([^()]+)\):/gm)) {
  const [, name, outfit, variant] = match.map(x => x.trim());
  const key = `${name.toLowerCase()}/view/whole/${outfit}/${variant}.png`;
  assert(fs.existsSync(path.join(root, 'Assets/Characters', key)), key);
  assert(fs.existsSync(path.join(root, 'Assets/Characters', name.toLowerCase(),
    'view/whole', outfit, 'main.png')), `Missing neutral: ${name}/${outfit}`);
  characterSelectors.add(key);
}
for (const match of source.matchAll(/^\s*(?:Звук|Музыка|Звуки окружения):\s*(\S+)/gm)) {
  audioIds.add(match[1]);
  assert(fs.existsSync(path.join(root, 'Assets/Audio', `${match[1]}.wav`)), match[1]);
}
for (const location of expectedLocations) {
  assert(fs.existsSync(path.join(root, 'Assets/Locations', `${location}.png`)), location);
}

function visit(route, previousLines, previousLocations, words) {
  assert(++nodes < 60000, 'Traversal budget exceeded: possible loop');
  const lines = [...previousLines];
  const locations = [...previousLocations];
  let continuations = 0;
  while (story.canContinue) {
    assert(++continuations < 1000, 'Continuation loop');
    const text = story.Continue().trim();
    if (!text) continue;
    lines.push(text);
    const location = /^Локация:\s*(\S+)/.exec(text);
    if (location) locations.push(location[1]);
    // Count displayed narration/dialogue and selected labels, excluding commands.
    if (/^(?:\.\.\.|[^:]+\([^)]*\)):\s*/.test(text)) {
      words += (text.replace(/^[^:]+:\s*/, '').match(/[\p{L}\p{N}]+(?:[-’][\p{L}\p{N}]+)*/gu) || []).length;
    }
    const v = story.variablesState;
    assert(v.$('memory') >= 0 && v.$('memory') <= 2, 'Memory out of bounds');
    assert(v.$('medicine') >= 1 && v.$('medicine') <= 2, 'Medicine out of bounds');
  }
  const choices = story.currentChoices.map(c => c.text);
  if (choices.length) {
    assert(choices.length >= 2 && choices.length <= 3, `Bad menu: ${choices}`);
    assert(route.length < 10, 'Unexpected extra decision');
    const state = story.state.ToJson();
    choices.forEach((text, index) => {
      labels.add(text);
      story.state.LoadJson(state);
      story.ChooseChoiceIndex(index);
      visit([...route, text], lines, locations,
        words + (text.match(/[\p{L}\p{N}]+/gu) || []).length);
    });
    return;
  }
  assert.equal(route.length, 10, `Premature end: ${route}`);
  assert.deepEqual(locations, expectedLocations);
  assert.equal(lines.filter(x => x === '...: КОНЕЦ СЕРИИ').length, 1);
  const endings = endingNames.filter(x => story.state.VisitCountAtPathString(`ending_${x}`) > 0);
  assert.equal(endings.length, 1);
  const ending = endings[0];
  const v = story.variablesState;
  if (ending !== 'nameless') {
    assert(v.$('memory') > 0, 'Good ending with no memory');
    assert.equal(v.$('coercion'), false, 'Good ending after coercion');
  }
  if (ending === 'witnesses') {
    assert.equal(v.$('luka_saved'), true);
    assert.equal(v.$('cub_mercy'), true);
    assert.equal(v.$('collar_state'), 'buried');
    assert(v.$('truth') >= 3);
  }
  if (v.$('frost_debt') >= 2) assert.equal(v.$('medicine_damaged'), true);
  if (ending === 'pathless') {
    regressionCoverage.pathlessDamage++;
    assert.equal(v.$('medicine_damaged'), true, 'Late arrival damage missing from state');
  }
  // Named choice labels compile to nested containers; use the actual selected
  // menu text here, not VisitCountAtPathString with a source-level choice name.
  const chose = label => route.includes(label);
  const ribbonTaken = chose('Забрать ленту как доказательство для Бережков');
  assert.equal(v.$('road_mark') === 'ribbon', ribbonTaken);
  if (ribbonTaken) {
    regressionCoverage.ribbonTaken++;
    assert(lines.some(x => x.includes('Ярина развязала ленту')));
    assert(!lines.some(x => x.includes('Щенок позволил')),
      'Ribbon taken without asking is described as permitted');
  }
  if (ending === 'witnesses') {
    regressionCoverage.witnessesRibbon++;
    assert.equal(ribbonTaken, false);
    assert(lines.some(x => x.includes('Лента всё ещё висела на его шее')));
    assert(!lines.some(x => x.includes('снять её мог тот, кто когда-то завязал')),
      'Ending invents an exclusive ribbon-removal rule');
  }
  if (chose('Перерезать верёвку утонувшего колокола')) {
    regressionCoverage.severedBell++;
    assert(!lines.some(x => x.includes('Подлёдный звон затих лишь тогда')),
      'Severed bell still ringing after departure');
  }
  const reservedForHamlet = chose('Спасти второй ящик') &&
    v.$('makar_choice') !== 'turn' && v.$('anfisa_choice') === 'return';
  assert.equal(lines.some(x => x.includes('Помеченный ящик она доставила на выселки')),
    reservedForHamlet, 'Reserved crate payoff does not match inventory');
  if (reservedForHamlet) regressionCoverage.hamletReservation++;
  if (reservedForHamlet && chose('Рискнуть лекарством ради дорожной сумки')) {
    regressionCoverage.hamletReservationAfterBelfry++;
    assert(!lines.some(x => x.includes('Из последнего ящика')),
      'Berezhki crate called last while hamlet crate still exists');
  }
  if (v.$('medicine_damaged')) {
    assert(!lines.some(x => x.includes('Оба ящика для Бережков добрались целыми') ||
      x.includes('Оба ящика успели внести')), 'Damaged medicine reported intact');
  }
  if (!v.$('luka_saved')) {
    assert(!lines.some(x => x.includes('На выходе из ложбины Лука написал')), 'Absent Luka speaks');
  }
  if (v.$('anfisa_choice') === 'return') {
    assert(lines.some(x => x.includes('Анфиса дошла до выселков')), 'Missing Anfisa payoff');
  }
  counts[ending]++;
  examples[ending] ??= route;
  total++;
  minWords = Math.min(minWords, words);
  maxWords = Math.max(maxWords, words);
  if (total % 5000 === 0) console.error(`Checked ${total} routes`);
}
visit([], [], [], 0);
assert.equal(total, 26244);
assert.equal(labels.size, 28);
for (const count of Object.values(counts)) assert(count > 0);
for (const count of Object.values(regressionCoverage)) assert(count > 0);
console.log(JSON.stringify({sourceSha256: crypto.createHash('sha256').update(source).digest('hex'),
  routes: total, endings: counts, decisionGroups: 10, distinctOptions: labels.size,
  locations: expectedLocations.length, characterSelectors: characterSelectors.size,
  audioIds: [...audioIds].sort(), displayedWords: {min: minWords, max: maxWords},
  regressionCoverage, compilerErrors: compiler.errors,
  compilerWarnings: compiler.warnings, examples}, null, 2));
