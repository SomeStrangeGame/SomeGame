"""Regression fixtures for warm spill and bounded matte repair; no Unity."""
import unittest
import numpy as np
from PIL import Image
from cleanup_character_edges import dilate
from repair_character_matte import repair


class MatteRepairTests(unittest.TestCase):
    def fixture(self):
        a = np.zeros((320, 96, 4), dtype=np.uint8)
        a[16:300, 30:66] = (90, 80, 50, 255)
        return a

    def test_warm_spill_missed_by_old_threshold(self):
        a = self.fixture()
        a[40:70, 30] = (110, 80, 70, 255)
        self.assertLessEqual(min(110, 70) - 80, 10)
        out, report = repair(Image.fromarray(a))
        out = np.asarray(out)
        self.assertGreater(report['edgeChromaPixels'], 0)
        self.assertTrue(np.all(out[40:70, 30, 0] < 110))
        self.assertTrue(np.all(out[40:70, 30, 2] < 70))

    def test_interior_identity_and_alpha_never_grow(self):
        a = self.fixture()
        a[80:100, 43:48] = (180, 30, 150, 255)
        out, _ = repair(Image.fromarray(a))
        out = np.asarray(out)
        core = (a[..., 3] > 0) & ~dilate(a[..., 3] == 0, 6)
        np.testing.assert_array_equal(out[core], a[core])
        self.assertTrue(np.all(out[..., 3] <= a[..., 3]))
        self.assertEqual(out.shape, a.shape)

    def test_thin_head_hair_protected(self):
        a = self.fixture()
        a[20:45, 29] = (110, 80, 70, 255)
        out, _ = repair(Image.fromarray(a))
        self.assertTrue(np.all(np.asarray(out)[20:45, 29, 3] > 0))

    def test_connected_coloured_ribbon_removed(self):
        a = self.fixture()
        a[210:270, 68:70] = (110, 80, 70, 255)
        a[210:212, 66:70] = (110, 80, 70, 255)
        a[268:270, 66:70] = (110, 80, 70, 255)
        out, report = repair(Image.fromarray(a))
        self.assertGreater(report['ribbonPixelsRemoved'], 0)
        self.assertFalse(np.asarray(out)[220:260, 68:70, 3].any())

    def test_clean_thin_detail_is_not_geometry_deleted(self):
        a = self.fixture()
        a[220:240, 66:69] = (90, 80, 50, 255)
        out, _ = repair(Image.fromarray(a))
        self.assertTrue(np.all(np.asarray(out)[220:240, 66:69, 3] > 0))

    def test_clean_rgb_and_registration_preserved(self):
        a = self.fixture()
        out, report = repair(Image.fromarray(a))
        np.testing.assert_array_equal(np.asarray(out)[..., :3][a[..., 3] > 0], a[..., :3][a[..., 3] > 0])
        self.assertEqual(report['alphaBoundsBefore'], report['alphaBoundsAfter'])

    def test_reviewed_mask_requires_exact_selector_and_canvas(self):
        with self.assertRaises(AssertionError):
            repair(Image.fromarray(self.fixture()), 'яков/view/whole/work/guarded.png')

    def test_determinism(self):
        a = Image.fromarray(self.fixture())
        first, r1 = repair(a)
        second, r2 = repair(a)
        np.testing.assert_array_equal(first, second)
        self.assertEqual(r1, r2)


if __name__ == '__main__':
    unittest.main()
