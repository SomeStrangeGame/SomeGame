"""Regression probes mutate reads in memory only; no project files are changed."""
import contextlib
import io
import json
from pathlib import Path
import runpy
import sys
import unittest
from unittest.mock import patch

SCRIPT = Path(__file__).with_name('check_story.py')
PROJECT = SCRIPT.parents[1]
SOURCE = PROJECT / 'Assets/Ink/s01e01.ink'
PREVIEW = PROJECT / 'Config/Preview/preview.json'
READ_TEXT = Path.read_text


class StoryChecks(unittest.TestCase):
    def run_check(self, source_change=None, preview_change=None):
        def read(path, *args, **kwargs):
            value = READ_TEXT(path, *args, **kwargs)
            if path == SOURCE and source_change:
                changed = source_change(value)
                self.assertNotEqual(value, changed, 'Probe did not modify source')
                return changed
            if path == PREVIEW and preview_change:
                obj = json.loads(value)
                preview_change(obj)
                return json.dumps(obj, ensure_ascii=False)
            return value
        with patch.object(Path, 'read_text', read), patch.object(sys, 'argv', [str(SCRIPT)]), contextlib.redirect_stdout(io.StringIO()):
            return runpy.run_path(str(SCRIPT))

    def test_baseline(self):
        self.assertEqual(len(self.run_check()['ROUTES']), 372)

    def test_wrong_ending_state(self):
        with self.assertRaises(AssertionError):
            self.run_check(lambda s: s.replace('~ miron_location = 3', '~ miron_location = 2'))

    def test_wrong_name_retention(self):
        with self.assertRaises(AssertionError):
            self.run_check(lambda s: s.replace('~ yar_name_intact = false', '~ yar_name_intact = true'))

    def test_false_release_notification(self):
        with self.assertRaises(AssertionError):
            self.run_check(lambda s: s.replace('Уведомление: Мирон жив на севере', 'Уведомление: Мирон освобождён'))

    def test_wrong_preview_speaker(self):
        def change(obj):
            next(b for b in obj['blocks'] if b['type'] == 'dialogue')['speaker'] = 'Весна'
        with self.assertRaisesRegex(AssertionError, 'Preview prose/order/speaker'):
            self.run_check(preview_change=change)

    def test_reordered_preview(self):
        def change(obj):
            obj['blocks'][0], obj['blocks'][1] = obj['blocks'][1], obj['blocks'][0]
        with self.assertRaisesRegex(AssertionError, 'Preview prose/order/speaker'):
            self.run_check(preview_change=change)

    def test_unsafe_image_path(self):
        def change(obj):
            obj['characters']['maincharacter']['image'] = '../cover.png'
        with self.assertRaisesRegex(AssertionError, 'Preview character mapping'):
            self.run_check(preview_change=change)


if __name__ == '__main__':
    unittest.main()
