// Static source/PNG checks only; no Unity import, preview generation or writes.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const zlib = require('node:zlib');
const root = path.resolve(__dirname, '../..');
const definition = fs.readFileSync(path.join(root, 'Assets/volchya-poshlina.asset'), 'utf8');
const episodes = definition.match(/^  _episodes:\n([\s\S]*?)(?=^  _\w+:)/m)?.[1];
assert(episodes, 'Missing episode metadata');
const entries = [...episodes.matchAll(/^  - _id: (\S+)\n([\s\S]*?)(?=^  - _id:|$(?![\s\S]))/gm)];
assert.equal(entries.length, 1);
assert.equal(entries[0][1], 's01e01');
const filename = entries[0][2].match(/^    _catalogCover: (\S+)$/m)?.[1];
assert.equal(filename, 's01e01.png');
assert(/^[A-Za-z0-9_.-]+\.png$/.test(filename) && !filename.includes('..'));
const png = fs.readFileSync(path.join(root, 'Config/EpisodeCovers', filename));
assert.deepEqual(png.subarray(0, 8), Buffer.from([137,80,78,71,13,10,26,10]));
let offset = 8, width, height, ended = false;
const idat = [];
function crc32(bytes) {
  let crc = 0xffffffff;
  for (const byte of bytes) {
    crc ^= byte;
    for (let i = 0; i < 8; i++) crc = (crc >>> 1) ^ ((crc & 1) ? 0xedb88320 : 0);
  }
  return (crc ^ 0xffffffff) >>> 0;
}
while (offset < png.length) {
  assert(offset + 12 <= png.length, 'Truncated PNG chunk');
  const length = png.readUInt32BE(offset);
  assert(offset + length + 12 <= png.length, 'Truncated PNG data');
  const type = png.toString('ascii', offset + 4, offset + 8);
  const bytes = png.subarray(offset + 8, offset + 8 + length);
  assert.equal(crc32(png.subarray(offset + 4, offset + 8 + length)),
    png.readUInt32BE(offset + 8 + length), `Invalid CRC: ${type}`);
  if (type === 'IHDR') {
    assert.equal(offset, 8);
    assert.equal(length, 13);
    width = bytes.readUInt32BE(0); height = bytes.readUInt32BE(4);
    assert.equal(width, 1024); assert.equal(height, 1536);
    assert.deepEqual([...bytes.subarray(8)], [8, 2, 0, 0, 0], 'Expected opaque RGB8 noninterlaced PNG');
  }
  if (type === 'IDAT') idat.push(bytes);
  offset += length + 12;
  if (type === 'IEND') { assert.equal(length, 0); ended = true; break; }
}
assert(ended && offset === png.length && idat.length > 0);
const pixels = zlib.inflateSync(Buffer.concat(idat), {maxOutputLength: height * (width * 3 + 1)});
assert.equal(pixels.length, height * (width * 3 + 1));
for (let row = 0; row < height; row++) assert(pixels[row * (width * 3 + 1)] <= 4);
assert(!png.equals(fs.readFileSync(path.join(root, 'Config/cover.png'))), 'Do not duplicate story cover');
const rootInkMeta = fs.readFileSync(path.join(root, 'Assets/Ink/volchya-poshlina.ink.meta'), 'utf8');
assert.equal(definition.match(/_authoringRootInkGuid: (\w+)/)?.[1], rootInkMeta.match(/^guid: (\w+)$/m)?.[1]);
assert(definition.includes('_contentVersion: 2'));
for (const name of ['Вея', 'Лука']) assert(definition.includes(`_character: "${name}"`));
console.log(JSON.stringify({episode: entries[0][1], catalogCover: filename,
  width, height, rgb: true, pngCrcAndDeflate: 'passed', distinctFromStoryCover: true,
  rootInkGuid: 'preserved', expandedCast: 'preserved', bytes: png.length,
  sha256: crypto.createHash('sha256').update(png).digest('hex')}, null, 2));
