const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const storyRoot = path.resolve(__dirname, '../..');
const previewRoot = path.join(storyRoot, 'Config/Preview');
const preview = JSON.parse(fs.readFileSync(path.join(previewRoot, 'preview.json'), 'utf8'));
const ink = fs.readFileSync(path.join(storyRoot, preview.source), 'utf8');

assert.equal(preview.schemaVersion, 1);
assert.equal(preview.storyId, 'volchya-poshlina');
assert.equal(preview.episodeId, 'VPs01e01');
assert(Number.isInteger(preview.estimatedReadingMinutes) && preview.estimatedReadingMinutes > 0);

const referencedImages = new Set();
for (const [id, character] of Object.entries(preview.characters)) {
  assert(character.name && character.image, `Incomplete character ${id}`);
  assert(/^characters\/[a-z0-9-]+\.png$/.test(character.image), character.image);
  const imagePath = path.join(previewRoot, character.image);
  assert(fs.existsSync(imagePath), imagePath);
  referencedImages.add(path.resolve(imagePath));
}

let activeCharacter = null;
let previousInkOffset = -1;
for (const block of preview.blocks) {
  assert(['character', 'narration', 'dialogue', 'separator'].includes(block.type), block.type);
  if (block.type === 'character') {
    assert(preview.characters[block.character], block.character);
    activeCharacter = preview.characters[block.character].name;
    continue;
  }
  if (block.type === 'separator') continue;
  assert(block.text && typeof block.text === 'string');
  const sourceLine = block.type === 'dialogue'
    ? `${block.speaker} (`
    : `...: ${block.text}`;
  let offset;
  if (block.type === 'dialogue') {
    assert.equal(block.speaker, activeCharacter, `Speaker ${block.speaker} lacks active portrait`);
    offset = ink.indexOf(sourceLine, previousInkOffset + 1);
    assert(offset >= 0, `Dialogue speaker missing after prior block: ${block.speaker}`);
    const lineEnd = ink.indexOf('\n', offset);
    assert(ink.slice(offset, lineEnd).endsWith(`: ${block.text}`), `Dialogue text differs: ${block.text}`);
  } else {
    offset = ink.indexOf(sourceLine, previousInkOffset + 1);
    assert(offset >= 0, `Narration differs or is out of order: ${block.text}`);
  }
  assert(offset > previousInkOffset, 'Preview blocks are not in canonical source order');
  previousInkOffset = offset;
}

const actualImages = fs.readdirSync(path.join(previewRoot, 'characters'))
  .filter(name => name.endsWith('.png'))
  .map(name => path.resolve(previewRoot, 'characters', name));
assert.deepEqual(new Set(actualImages), referencedImages, 'Preview contains unreferenced PNG files');
console.log(JSON.stringify({blocks: preview.blocks.length, characters: Object.keys(preview.characters), sourceOrder: 'passed'}));
