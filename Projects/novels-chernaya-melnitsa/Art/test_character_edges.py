"""Small in-memory regression fixtures; no Unity or production writes."""
import unittest

import numpy as np
from PIL import Image

from cleanup_character_edges import clean, detached_debris, propagate


class EdgeCleanupTests(unittest.TestCase):
    def fixture(self):
        a = np.zeros((96, 96, 4), dtype=np.uint8)
        a[16:80, 30:66] = (80, 70, 60, 255)
        return a

    def test_clean_art_preserved(self):
        a = self.fixture()
        out, report = clean(Image.fromarray(a))
        out = np.asarray(out)
        np.testing.assert_array_equal(out[a[..., 3] > 0], a[a[..., 3] > 0])
        np.testing.assert_array_equal(out[..., 3], a[..., 3])
        self.assertEqual(report['edgePixelsCleaned'], 0)

    def test_spill_changes_only_edge_and_hidden_rgb(self):
        a = self.fixture()
        a[20:70, 30] = (150, 40, 130, 255)
        out, report = clean(Image.fromarray(a))
        out = np.asarray(out)
        self.assertEqual(report['edgePixelsCleaned'], 50)
        self.assertTrue(np.all(out[20:70, 30, 3] < 255))
        np.testing.assert_array_equal(out[..., 3] > 0, a[..., 3] > 0)
        np.testing.assert_array_equal(out[20:70, 32:64], a[20:70, 32:64])
        self.assertEqual(report['magentaAfter'], 0)

    def test_isolated_interior_colour_is_not_removed(self):
        a = self.fixture()
        a[40:45, 47:50] = (150, 40, 130, 255)
        out, _ = clean(Image.fromarray(a))
        np.testing.assert_array_equal(np.asarray(out)[40:45, 47:50], a[40:45, 47:50])

    def test_detached_debris_removed_but_near_hair_retained(self):
        a = self.fixture()
        a[18:22, 27] = (80, 70, 60, 255)  # detached but near silhouette
        a[40:45, 8:10] = (80, 70, 60, 255)  # remote extraction debris
        removed = detached_debris(a[..., 3] > 0)
        self.assertEqual(int(removed.sum()), 10)
        self.assertFalse(removed[18:22, 27].any())
        out, report = clean(Image.fromarray(a))
        out = np.asarray(out)
        self.assertTrue(np.all(out[40:45, 8:10, 3] == 0))
        self.assertEqual(report['detachedDebrisRemovedPixels'], 10)

    def test_diagonal_connectivity(self):
        a = self.fixture()
        a[15, 29] = (80, 70, 60, 255)
        self.assertFalse(detached_debris(a[..., 3] > 0).any())

    def test_bleed_never_wraps_canvas(self):
        rgb = np.zeros((20, 20, 3), np.uint8)
        known = np.zeros((20, 20), bool)
        rgb[10, 0], known[10, 0] = (80, 70, 60), True
        out, reached = propagate(rgb, known, 3)
        self.assertFalse(reached[:, -1].any())
        self.assertTrue(np.all(out[:, -1] == 0))

    def test_deterministic_from_same_source(self):
        a = self.fixture()
        a[20:70, 30] = (150, 40, 130, 255)
        first, first_report = clean(Image.fromarray(a))
        second, second_report = clean(Image.fromarray(a))
        np.testing.assert_array_equal(np.asarray(first), np.asarray(second))
        self.assertEqual(first_report, second_report)


if __name__ == '__main__':
    unittest.main()
