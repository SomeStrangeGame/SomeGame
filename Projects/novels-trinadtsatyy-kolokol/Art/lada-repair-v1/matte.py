"""User-authorized deterministic green-matte removal; never changes interior art."""
import argparse
import hashlib
import json
from pathlib import Path
import unittest

import numpy as np
from PIL import Image, ImageFilter


def expand(mask, radius):
    return np.asarray(Image.fromarray(mask.astype('uint8') * 255).filter(
        ImageFilter.MaxFilter(radius * 2 + 1))) > 0


def remove_green(image):
    if image.mode != 'RGB':
        raise ValueError('Expected authored RGB green-matte input')
    rgb = np.asarray(image).astype('float32')
    delta = rgb[..., 1] - np.maximum(rgb[..., 0], rgb[..., 2])
    background = (delta > 90) & (rgb[..., 1] > 140)
    if background.mean() < .25:
        raise ValueError('Not a green-matte sprite; refuse checkerboard/unknown input')
    border = np.zeros(background.shape, bool)
    border[:2] = border[-2:] = True
    border[:, :2] = border[:, -2:] = True
    if not background[border].all():
        raise ValueError('Character touches canvas edge or background is not green')
    key = np.median(rgb[border], axis=0)
    # Only a narrow edge ring can contain blended green. Figure core is immutable.
    band = expand(background, 3) & ~background
    core = ~background & ~band
    known = core.copy()
    estimate = rgb.copy()
    for _ in range(5):
        sums = np.zeros_like(rgb)
        count = np.zeros(background.shape, 'float32')
        for dy, dx in ((-1, 0), (1, 0), (0, -1), (0, 1)):
            k = np.roll(known, (dy, dx), axis=(0, 1))
            if dy == -1: k[-1] = False
            if dy == 1: k[0] = False
            if dx == -1: k[:, -1] = False
            if dx == 1: k[:, 0] = False
            sums += np.roll(estimate, (dy, dx), axis=(0, 1)) * k[..., None]
            count += k
        new = ~known & (count > 0)
        estimate[new] = sums[new] / count[new, None]
        known |= new
    expected_delta = estimate[..., 1] - np.maximum(estimate[..., 0], estimate[..., 2])
    key_delta = key[1] - max(key[0], key[2])
    spill = band & known & (delta > expected_delta + 5)
    alpha = np.ones(background.shape, 'float32')
    alpha[background] = 0
    alpha[spill] = np.clip(1 - (delta[spill] - expected_delta[spill]) /
                           np.maximum(key_delta - expected_delta[spill], 1), 0, 1)
    cleaned = rgb.copy()
    # Physical unmix against the measured service field, only in mixed edge pixels.
    cleaned[spill] = np.clip((rgb[spill] - (1-alpha[spill, None])*key) /
                             np.maximum(alpha[spill, None], .02), 0, 255)
    # Isolated fine hair/laces may have no thick foreground donor. Remove only
    # residual key-green dominance in the reviewed edge ring, never in the core.
    residual = band & (cleaned[..., 1] > np.maximum(cleaned[..., 0], cleaned[..., 2]) + 4)
    cleaned[..., 1][residual] = np.maximum(cleaned[..., 0], cleaned[..., 2])[residual]
    cleaned[background] = 0
    result = np.dstack((np.rint(cleaned).astype('uint8'),
                       np.rint(alpha*255).astype('uint8')))
    assert np.array_equal(result[..., :3][core], rgb.astype('uint8')[core])
    assert np.all(result[..., 3][core] == 255)
    assert not result[..., 3][border].any()
    out = Image.fromarray(result)
    return out, {'size': list(out.size), 'mode': out.mode,
                 'keyMedian': key.tolist(), 'backgroundPixels': int(background.sum()),
                 'mixedEdgePixels': int(spill.sum()), 'residualGreenPixels': int(residual.sum()),
                 'corePixels': int(core.sum()),
                 'coreRgbUnchanged': True, 'alphaBounds': list(out.getbbox()),
                 'inputPixelSha256': hashlib.sha256(np.asarray(image).tobytes()).hexdigest(),
                 'outputPixelSha256': hashlib.sha256(result.tobytes()).hexdigest()}


class Tests(unittest.TestCase):
    def fixture(self):
        a = np.full((80, 80, 3), (4, 249, 9), dtype='uint8')
        a[10:70, 20:60] = (25, 70, 100)  # teal garment, not background
        a[25:35, 30:40] = (180, 120, 95)  # face
        a[45:55, 38:42] = (4, 249, 9)  # enclosed gap
        return Image.fromarray(a)

    def test_core_and_gaps(self):
        out, report = remove_green(self.fixture())
        a = np.asarray(out)
        self.assertEqual(tuple(a[30, 35]), (180, 120, 95, 255))
        self.assertEqual(tuple(a[60, 30]), (25, 70, 100, 255))
        self.assertEqual(a[50, 40, 3], 0)
        self.assertTrue(report['coreRgbUnchanged'])

    def test_refuse_unknown_background(self):
        with self.assertRaises(ValueError):
            remove_green(Image.new('RGB', (80, 80), 'white'))

    def test_refuse_clipped_figure(self):
        im = self.fixture(); im.putpixel((0, 0), (20, 30, 40))
        with self.assertRaises(ValueError): remove_green(im)

    def test_deterministic(self):
        a, r1 = remove_green(self.fixture()); b, r2 = remove_green(self.fixture())
        self.assertEqual(a.tobytes(), b.tobytes()); self.assertEqual(r1, r2)

    def test_thin_green_fringe(self):
        im = self.fixture()
        for y in range(12, 23): im.putpixel((19, y), (50, 85, 35))
        out, _ = remove_green(im)
        a = np.asarray(out)
        self.assertTrue(np.all(a[12:23, 19, 1] <= np.maximum(a[12:23, 19, 0], a[12:23, 19, 2])))
        self.assertTrue(np.all(a[12:23, 19, 3] > 0))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--self-test', action='store_true')
    parser.add_argument('--input', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    if args.self_test:
        suite = unittest.defaultTestLoader.loadTestsFromTestCase(Tests)
        if not unittest.TextTestRunner().run(suite).wasSuccessful(): raise SystemExit(1)
        return
    if not args.input or not args.output: parser.error('input and output required')
    if args.output.exists(): raise SystemExit('Refuse to overwrite an existing candidate')
    output, report = remove_green(Image.open(args.input))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    output.save(args.output)
    for label, color in [('light', '#ffffff'), ('dark', '#101016'), ('contrast', '#b548ab')]:
        proof = Image.new('RGBA', output.size, color)
        proof.alpha_composite(output)
        proof.convert('RGB').save(args.output.with_name(args.output.stem+'-'+label+'.png'))
    args.output.with_suffix('.json').write_text(json.dumps(report, indent=2)+'\n')
    print(json.dumps(report))


if __name__ == '__main__': main()
