import importlib.util
import json
import subprocess
import sys
import tempfile
import unittest
from collections import OrderedDict
from pathlib import Path


MODULE_PATH = Path(__file__).resolve().parents[1] / "channel_manifest.py"
SPEC = importlib.util.spec_from_file_location("channel_manifest", MODULE_PATH)
assert SPEC and SPEC.loader
MODULE = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = MODULE
SPEC.loader.exec_module(MODULE)


class ChannelManifestTests(unittest.TestCase):
    def test_merge_preserves_order_and_existing_stories(self):
        base = OrderedDict((("chernaya-melnitsa", "1"), ("old-story", "3")))
        updates = MODULE.parse_selectors(["chernaya-melnitsa=2", "new-story=1"])

        stories, summary = MODULE.compose(updates, base)

        self.assertEqual(
            list(stories.items()),
            [("chernaya-melnitsa", "2"), ("old-story", "3"), ("new-story", "1")],
        )
        self.assertEqual(summary["retained"], ["old-story"])
        self.assertEqual(summary["updated"], ["chernaya-melnitsa"])
        self.assertEqual(summary["added"], ["new-story"])
        self.assertEqual(summary["removed"], [])

    def test_replace_is_explicit_and_contains_only_selectors(self):
        updates = MODULE.parse_selectors(["only-story=1"])
        stories, summary = MODULE.compose(updates, None)
        self.assertEqual(stories, {"only-story": "1"})
        self.assertEqual(summary["mode"], "replace")
        self.assertFalse(summary["comparisonAvailable"])
        self.assertIsNone(summary["removed"])

    def test_invalid_or_duplicate_selector_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "Invalid"):
            MODULE.parse_selectors(["Bad Story=1"])
        with self.assertRaisesRegex(ValueError, "Duplicate"):
            MODULE.parse_selectors(["story=1", "story=2"])

    def test_invalid_base_manifest_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "manifest.json"
            path.write_text(json.dumps({"schema": 2, "stories": {}}), encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "schema 1"):
                MODULE.load_manifest(path)

    def test_stage_channel_requires_an_explicit_safe_or_replace_mode(self):
        command = MODULE_PATH.parent / "novels-content"
        result = subprocess.run(
            [str(command), "stage-channel", "dev", "story=1"],
            capture_output=True,
            check=False,
            text=True,
        )
        self.assertEqual(result.returncode, 2)
        self.assertIn("Choose --base-manifest", result.stderr)


if __name__ == "__main__":
    unittest.main()
