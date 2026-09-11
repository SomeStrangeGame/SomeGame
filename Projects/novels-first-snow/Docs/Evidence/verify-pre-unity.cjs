// Static pre-Unity audit for the story-owned web preview and dated archive.
// node verify-pre-unity.cjs [project-root]
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');

const root = path.resolve(process.argv[2] || path.join(__dirname, '..', '..'));
const previewPath = path.join(root, 'Config/Preview/preview.json');
const preview = JSON.parse(fs.readFileSync(previewPath, 'utf8'));
const card = JSON.parse(fs.readFileSync(path.join(root, 'Config/card.json'), 'utf8'));
const source = fs.readFileSync(path.join(root, preview.source), 'utf8');

assert.equal(preview.schemaVersion, 1);
assert.equal(preview.storyId, card.storyId);
assert.equal(preview.episodeId, 's01e01');
assert.equal(preview.title, 'Первый снег');
assert(Number.isInteger(preview.estimatedReadingMinutes) && preview.estimatedReadingMinutes > 0);
const supported = new Set(['character', 'narration', 'dialogue', 'separator']);
const markers = new Set();
let cursor = 0;
for (const block of preview.blocks) {
  assert(supported.has(block.type), `Unsupported preview block: ${block.type}`);
  if (block.type === 'character') {
    assert(preview.characters[block.character], `Unknown preview character: ${block.character}`);
    markers.add(block.character);
    continue;
  }
  if (block.type === 'separator') continue;
  assert.equal(typeof block.text, 'string');
  const index = source.indexOf(block.text, cursor);
  assert(index >= 0, `Preview text absent or out of order: ${block.text.slice(0, 48)}`);
  if (block.type === 'dialogue') {
    const start = source.lastIndexOf('\n', index) + 1;
    const prefix = source.slice(start, index);
    assert(prefix.startsWith(block.speaker) && prefix.trimEnd().endsWith(':'),
      `Preview speaker mismatch: ${block.speaker}`);
  }
  cursor = index + block.text.length;
}
assert.deepEqual([...markers].sort(), Object.keys(preview.characters).sort());
const referenced = [];
const approvedMasters = {
  lesha: 'Assets/Characters/лёша/view/whole/school/main.png',
  sonya: 'Assets/Characters/соня/view/whole/school/main.png'
};
for (const [characterId, character] of Object.entries(preview.characters)) {
  assert(/^characters\/[A-Za-z0-9_.-]+\.png$/.test(character.image), `Unsafe preview path: ${character.image}`);
  const file = path.join(path.dirname(previewPath), character.image);
  const bytes = fs.readFileSync(file);
  assert.equal(bytes.subarray(0, 8).toString('hex'), '89504e470d0a1a0a');
  assert(fs.readFileSync(path.join(root, approvedMasters[characterId])).equals(bytes),
    `Preview image is not the approved school master: ${characterId}`);
  referenced.push(character.image);
}
const characterRoot = path.join(path.dirname(previewPath), 'characters');
const actual = fs.readdirSync(characterRoot).map(name => `characters/${name}`).sort();
assert.deepEqual(actual, referenced.sort(), 'Preview directory must contain only referenced images');
assert(!preview.blocks.some(block => block.text && block.text.includes('Мия уедет')),
  'Opening preview must not spoil the departure');
assert(source.indexOf('*', cursor) >= cursor, 'Preview must stop before the first meaningful choice');

const archive = path.join(root, 'archive');
const manifests = [];
for (const entry of fs.readdirSync(archive, { recursive: true })) {
  if (/\d{4}-\d{2}-\d{2}_manifest_v\d{3}\.json$/.test(entry)) manifests.push(entry);
  if (fs.statSync(path.join(archive, entry)).isFile())
    assert(/^\d{4}-\d{2}-\d{2}(?:_\d{6}Z)?_.+_v\d{3}\.[^.]+$/.test(path.basename(entry)),
      `Undated archive filename: ${entry}`);
}
assert(manifests.length, 'Missing dated archive manifest');
manifests.sort();
const manifestRel = manifests.at(-1);
const manifest = JSON.parse(fs.readFileSync(path.join(archive, manifestRel), 'utf8'));
assert.equal(manifest.storyId, 'first-snow');
for (const artifact of manifest.artifacts) {
  const file = path.join(root, artifact.path);
  assert(fs.existsSync(file), `Missing archived artifact: ${artifact.path}`);
  const hash = crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
  assert.equal(hash, artifact.sha256, `Archive hash mismatch: ${artifact.path}`);
}
console.log(JSON.stringify({
  previewBlocks: preview.blocks.length,
  previewCharacters: Object.keys(preview.characters),
  archiveManifest: `archive/${manifestRel}`,
  archiveArtifacts: manifest.artifacts.length,
  status: 'passed',
  scope: 'Static preview/source/archive checks; no website rendering, Unity, build or publication'
}, null, 2));
