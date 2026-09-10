"""Source-file checks only; does not validate Unity catalog assignment."""
import hashlib
import json
import struct
import unittest
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EXPECTED = [f"s01e{i:02d}" for i in range(1, 11)]


class EpisodeCoversTest(unittest.TestCase):
    def test_complete_unique_manifest(self):
        manifest = json.loads((ROOT / "Art/episode-cover-prompts.json").read_text())
        rows = manifest["prompts"]
        self.assertEqual([row["episodeId"] for row in rows], EXPECTED)
        self.assertFalse(manifest["definitionAssigned"])
        self.assertEqual(manifest["mode"], "built-in")
        self.assertEqual(
            sorted(p.name for p in (ROOT / "Config/EpisodeCovers").iterdir()),
            [f"{episode}.png" for episode in EXPECTED],
        )
        hashes = set()
        for row in rows:
            with self.subTest(episode=row["episodeId"]):
                self.assertEqual(row["fileName"], row["episodeId"] + ".png")
                self.assertTrue((ROOT / "Assets/Ink" / (row["episodeId"] + ".ink")).is_file())
                self.assertTrue(row["prompt"])
                data = (ROOT / "Config/EpisodeCovers" / row["fileName"]).read_bytes()
                self.assertEqual(data[:8], b"\x89PNG\r\n\x1a\n")
                offset, chunks, compressed = 8, [], bytearray()
                while offset < len(data):
                    size = struct.unpack_from(">I", data, offset)[0]
                    kind = data[offset + 4:offset + 8]
                    payload = data[offset + 8:offset + 8 + size]
                    self.assertEqual(len(payload), size)
                    crc = struct.unpack_from(">I", data, offset + 8 + size)[0]
                    self.assertEqual(zlib.crc32(kind + payload) & 0xffffffff, crc)
                    chunks.append(kind)
                    if kind == b"IHDR":
                        width, height, depth, color, comp, filtering, interlace = struct.unpack(">IIBBBBB", payload)
                        self.assertEqual((width, height, depth, color, comp, filtering, interlace),
                                         (1024, 1536, 8, 2, 0, 0, 0))
                    if kind == b"IDAT":
                        compressed.extend(payload)
                    offset += size + 12
                self.assertEqual(offset, len(data))
                self.assertEqual(chunks[0], b"IHDR")
                self.assertEqual(chunks[-1], b"IEND")
                self.assertNotIn(b"tRNS", chunks)
                self.assertEqual(len(zlib.decompress(compressed)), 1536 * (1024 * 3 + 1))
                hashes.add(hashlib.sha256(data).hexdigest())
        self.assertEqual(len(hashes), 10, "Every episode must have distinct image bytes")


if __name__ == "__main__":
    unittest.main()
