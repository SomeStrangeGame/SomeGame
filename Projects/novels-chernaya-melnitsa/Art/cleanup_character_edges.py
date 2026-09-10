#!/usr/bin/env python3
"""Deterministic, edge-only magenta unmatting; never regenerates artwork.

Requires Pillow and numpy. First generate into an empty external --output;
inspect proofs, then use --apply to copy the hash-checked candidate to source.
Original PNG/meta bytes are retained in output/before. No Unity is invoked.
"""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def dilate(mask, radius):
    return np.asarray(Image.fromarray(mask.astype('uint8') * 255).filter(
        ImageFilter.MaxFilter(radius * 2 + 1))) > 0


def propagate(rgb, known, count):
    """Nearest Chebyshev-ring average; no wraparound, no interior recolour."""
    rgb = rgb.astype(np.float32).copy()
    known = known.copy()
    for _ in range(count):
        weights = np.zeros(known.shape, np.float32)
        total = np.zeros(rgb.shape, np.float32)
        kp = np.pad(known, 1)
        rp = np.pad(rgb * known[..., None], ((1, 1), (1, 1), (0, 0)))
        h, w = known.shape
        for dy in range(3):
            for dx in range(3):
                if dx == dy == 1:
                    continue
                weights += kp[dy:dy+h, dx:dx+w]
                total += rp[dy:dy+h, dx:dx+w]
        fill = ~known & (weights > 0)
        rgb[fill] = total[fill] / weights[fill, None]
        known |= fill
    return rgb, known


def detached_debris(visible, near_radius=6):
    """8-connected scanline components; retain nearby detached hair strands."""
    parent, runs, previous = [], [], []

    def root(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i

    for y, row in enumerate(visible):
        changes = np.diff(np.pad(row.astype(int), 1))
        current = []
        for left, right in zip(np.where(changes == 1)[0], np.where(changes == -1)[0]):
            i = len(parent)
            parent.append(i)
            runs.append((y, int(left), int(right), i))
            for pl, pr, pi in previous:
                if pl <= right and pr >= left:
                    parent[root(i)] = root(pi)
            current.append((left, right, i))
        previous = current
    areas = {}
    for y, left, right, i in runs:
        r = root(i)
        areas[r] = areas.get(r, 0) + right - left
    assert areas, 'Empty character'
    principal = max(areas, key=areas.get)
    body = np.zeros_like(visible)
    for y, left, right, i in runs:
        if root(i) == principal:
            body[y, left:right] = True
    near = dilate(body, near_radius)
    protected = {principal}
    for y, left, right, i in runs:
        if near[y, left:right].any() or areas[root(i)] >= areas[principal] * .01:
            protected.add(root(i))
    debris = np.zeros_like(visible)
    for y, left, right, i in runs:
        if root(i) not in protected:
            debris[y, left:right] = True
    return debris


def clean(src):
    rgba = np.asarray(src).copy()
    rgb = rgba[..., :3].astype(np.float32)
    alpha = rgba[..., 3]
    original_visible = alpha > 0
    debris = detached_debris(original_visible)
    visible = original_visible & ~debris
    band = visible & dilate(~visible, 6)
    excess = np.minimum(rgb[..., 0], rgb[..., 2]) - rgb[..., 1]
    contaminated = band & (excess > 10)
    # Follow only connected spill from the edge; a few sleeves have a wider
    # remnant of the old extraction field. Isolated interior colour is retained.
    connected_limit = visible & dilate(~visible, 12) & (excess > 10)
    for _ in range(12):
        contaminated |= dilate(contaminated, 1) & connected_limit
    estimate, reached = propagate(rgb, visible & (excess <= 10), 12)
    assert np.all(reached[contaminated]), 'Unresolved edge pixels'
    # Estimate the amount of the former magenta extraction field. Use the
    # nearby clean foreground chroma as baseline, retaining warm/cool palette.
    old_chroma = (rgb[..., 0] + rgb[..., 2]) * .5 - rgb[..., 1]
    fg_chroma = (estimate[..., 0] + estimate[..., 2]) * .5 - estimate[..., 1]
    matte = np.clip((old_chroma - fg_chroma) / np.maximum(255 - fg_chroma, 1), 0, .98)
    out = rgba.copy()
    out[debris] = 0
    out[..., :3][contaminated] = np.rint(estimate[contaminated]).clip(0, 255).astype('uint8')
    out[..., 3][contaminated] = np.rint(alpha[contaminated] * (1 - matte[contaminated])).astype('uint8')
    # RGB dilation stays invisible (alpha=0). It avoids black/magenta texels
    # entering bilinear filtering; it is not a compression-policy override.
    bleed, _ = propagate(out[..., :3], visible, 8)
    hidden_ring = ~visible & dilate(visible, 8)
    out[..., :3][hidden_ring] = np.rint(bleed[hidden_ring]).clip(0, 255).astype('uint8')
    assert np.array_equal(out[visible & ~contaminated], rgba[visible & ~contaminated])
    assert np.all(out[..., 3] <= alpha), 'Silhouette growth'
    assert np.array_equal(out[..., 3] > 0, visible), 'Unexpected silhouette loss'
    assert np.array_equal(out[~(contaminated | hidden_ring | debris)], rgba[~(contaminated | hidden_ring | debris)])
    remaining = visible & ((np.minimum(out[..., 0].astype(int), out[..., 2]) - out[..., 1]) > 10)
    return Image.fromarray(out), {
        'visiblePixels': int(original_visible.sum()), 'edgePixelsCleaned': int(contaminated.sum()),
        'magentaBefore': int((original_visible & (excess > 10)).sum()),
        'magentaAfter': int(remaining.sum()),
        'alphaReducedPixels': int((out[..., 3] < alpha).sum()),
        'minRetainedEdgeAlpha': int(out[..., 3][contaminated].min()) if contaminated.any() else 255,
        'hiddenRgbBleedPixels': int(hidden_ring.sum()),
        'detachedDebrisRemovedPixels': int(debris.sum()),
        'interiorUnchanged': True, 'bodySilhouetteUnchanged': True,
        'dimensions': list(src.size), 'alphaBoundsBefore': list(src.getbbox()),
        'alphaBoundsAfter': list(Image.fromarray(out).getbbox()),
    }


def composite(im, colour):
    bg = Image.new('RGBA', im.size, colour)
    bg.alpha_composite(im)
    return bg.convert('RGB')


def proofs(before, after, target, label):
    colours = [(18, 22, 27), (245, 245, 245), (30, 90, 130)]
    sheet = Image.new('RGB', (6 * 256, 457), 'white')
    draw = ImageDraw.Draw(sheet)
    for j, colour in enumerate(colours):
        for k, im in enumerate((before, after)):
            x = (j * 2 + k) * 256
            sheet.paste(composite(im, colour).resize((256, 427), Image.Resampling.LANCZOS), (x, 30))
            draw.text((x + 5, 5), f'{label} / {"before" if k == 0 else "after"}', fill='black')
    sheet.save(target.with_suffix('.png'))
    x0, y0, x1, _ = before.getbbox()
    box = (max(0, x0 - 12), max(0, y0 - 12), min(before.width, x1 + 12), y0 + 230)
    w, h = (box[2] - box[0]) * 2, (box[3] - box[1]) * 2
    detail = Image.new('RGB', (w * 2, (h + 24) * 3), 'white')
    draw = ImageDraw.Draw(detail)
    for j, colour in enumerate(colours):
        for k, im in enumerate((before, after)):
            draw.text((k*w+4, j*(h+24)+4), 'before' if k == 0 else 'after', fill='black')
            detail.paste(composite(im.crop(box), colour).resize((w, h), Image.Resampling.NEAREST), (k*w, j*(h+24)+24))
    detail.save(target.with_name(target.name + '-detail').with_suffix('.png'))


def main(processor=clean, metadata=None, path_aware=False):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    source, output = args.source.resolve(), args.output.resolve()
    assert source not in output.parents and output not in source.parents and source != output
    if args.apply:
        report = json.loads((output / 'report.json').read_text())
        for row in report['files']:
            original, candidate = source / row['path'], output / 'after' / row['path']
            assert digest(original) == row['beforeSha256'], f'Source changed: {original}'
            assert digest(candidate) == row['afterSha256'], f'Candidate changed: {candidate}'
            assert digest(original.with_suffix('.png.meta')) == row['metaSha256']
        for row in report['files']:
            shutil.copy2(output / 'after' / row['path'], source / row['path'])
        print(json.dumps({'applied': len(report['files']), 'backup': str(output / 'before')}))
        return
    output.mkdir(parents=True, exist_ok=False)
    (output / 'proofs').mkdir()
    rows = []
    for index, path in enumerate(sorted(source.rglob('*.png'))):
        relative = path.relative_to(source)
        original = Image.open(path)
        assert original.mode == 'RGBA', path
        candidate, row = processor(original, relative.as_posix()) if path_aware else processor(original)
        for folder in ('before', 'after'):
            (output / folder / relative).parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, output / 'before' / relative)
        shutil.copy2(path.with_suffix('.png.meta'), (output / 'before' / relative).with_suffix('.png.meta'))
        candidate.save(output / 'after' / relative)
        row.update(path=relative.as_posix(), beforeSha256=digest(path),
                   afterSha256=digest(output / 'after' / relative),
                   metaSha256=digest(path.with_suffix('.png.meta')))
        proofs(original, candidate, output / 'proofs' / f'{index:02d}', f'{index:02d}')
        rows.append(row)
    report = {'algorithm': 'edge-magenta-unmatte-v3', 'edgeSeedPixels': 6,
              'maxConnectedEdgeBandPixels': 12,
              'hiddenBleedPixels': 8, 'files': rows}
    if metadata:
        report.update(metadata)
    (output / 'report.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps(report, ensure_ascii=False))


if __name__ == '__main__':
    main()
