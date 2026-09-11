// Source-only audit; optional argument is the reference repository root.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const approvedHash = '72adeb9703aaaa2d66f7d7f068bf0a618bcb7d5d9f7b55e7fb0b950a9a502c82';
function verify(definition, readCover) {
  const block = definition.match(/^  _episodes:\n([\s\S]*?)(?=^  _\w+:)/m);
  assert(block, 'Missing episode list');
  const entries = [...block[1].matchAll(/^  - _id: ([^\n]+)\n([\s\S]*?)(?=^  - _id:|$)/gm)];
  // Bounded to the current one-episode story; require review when extending it.
  assert.equal(entries.length, 1, 'Review episode-cover coverage for new episodes');
  assert.equal(entries[0][1], 's01e01');
  const cover = block[1].match(/^    _catalogCover: ([^\n]+)$/m)?.[1];
  assert(cover, 'Missing episode cover binding');
  assert(/^[A-Za-z0-9_.-]+\.(?:png|jpe?g)$/.test(cover) && !cover.includes('..'), 'Unsafe cover filename');
  assert.equal(cover, 's01e01.png', 'Unexpected approved cover');
  const bytes = readCover(cover);
  assert.equal(bytes.subarray(0, 8).toString('hex'), '89504e470d0a1a0a');
  assert.equal(bytes.readUInt32BE(16), 1024);
  assert.equal(bytes.readUInt32BE(20), 1536);
  assert.equal(crypto.createHash('sha256').update(bytes).digest('hex'), approvedHash, 'Approved cover bytes changed');
  return cover;
}
const definition = fs.readFileSync(path.join(root, 'Assets/first-snow.asset'), 'utf8');
const readCover = name => fs.readFileSync(path.join(root, 'Config/EpisodeCovers', name));
const cover = verify(definition, readCover);
assert.throws(() => verify(definition.replace('    _catalogCover: s01e01.png\n', ''), readCover), /Missing episode cover binding/);
assert.throws(() => verify(definition.replace('_catalogCover: s01e01.png', '_catalogCover: ../s01e01.png'), readCover), /Unsafe cover filename/);
assert.throws(() => verify(definition, name => {
  const bytes = Buffer.from(readCover(name));
  bytes[bytes.length - 1] ^= 1;
  return bytes;
}), /Approved cover bytes changed/);
const sdkPath = 'Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs';
const supportsCover = repo => fs.readFileSync(path.join(repo, sdkPath), 'utf8').includes('private string _catalogCover;');
const localSdkSupportsEpisodeCovers = supportsCover(path.resolve(root, '../..'));
let referenceSdkSupportsEpisodeCovers = null;
if (process.argv[2]) {
  referenceSdkSupportsEpisodeCovers = supportsCover(path.resolve(process.argv[2]));
  assert(referenceSdkSupportsEpisodeCovers, 'Reference SDK lacks episode-cover contract');
}
console.log(JSON.stringify({ episode: 's01e01', cover, sha256: approvedHash,
  width: 1024, height: 1536, negativeProbes: 3,
  localSdkSupportsEpisodeCovers, referenceSdkSupportsEpisodeCovers,
  scope: 'Source binding and approved PNG only; Unity/catalog display not validated' }, null, 2));
