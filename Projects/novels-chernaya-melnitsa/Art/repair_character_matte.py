"""Second technical pass on v3 PNGs: remove warm extraction fringe, not design.

Only edge-band chroma and opacity are changed. The radius is NOT an erosion
of the figure. Reuses the explicit candidate/backup/hash-checked apply workflow.
"""
import hashlib
import numpy as np
from PIL import Image, ImageFilter

import cleanup_character_edges as common


def repair(src, relative_path=None):
    a = np.asarray(src).copy()
    visible = a[..., 3] > 0
    rgb = a[..., :3].astype(float)
    # The first pass accepted warm/red fringe as clean. Estimate foreground
    # chroma from pixels six pixels inside the existing silhouette instead.
    band = visible & common.dilate(~visible, 6)
    core = visible & ~band
    estimate, reached = common.propagate(rgb, core, 16)
    delta_r = (rgb[..., 0] - rgb[..., 1]) - (estimate[..., 0] - estimate[..., 1])
    delta_b = (rgb[..., 2] - rgb[..., 1]) - (estimate[..., 2] - estimate[..., 1])
    spill = band & reached & (delta_r > 5) & (delta_b > 5)
    # Keep green (the extraction field had none), replacing only excess chroma.
    out = a.copy()
    corrected = rgb.copy()
    corrected[..., 0] -= np.maximum(delta_r, 0)
    corrected[..., 2] -= np.maximum(delta_b, 0)
    out[..., :3][spill] = np.rint(corrected[spill]).clip(0, 255).astype('uint8')
    # A conservative partial coverage, not the previous nearly opaque matte.
    fraction = np.clip(np.minimum(delta_r, delta_b) / 80, 0, .7)
    out[..., 3][spill] = np.rint(a[..., 3][spill] * (1 - fraction[spill])).astype('uint8')
    # Thin ribbons detached from the main mass but connected at their ends:
    # identify only sub-4px remnants with strongly shifted edge chroma. Do not
    # delete ordinary thin fingers/hair based on geometry alone.
    opened = np.asarray(Image.fromarray(visible.astype('uint8') * 255).filter(
        ImageFilter.MinFilter(5)).filter(ImageFilter.MaxFilter(5))) > 0
    ribbon = visible & ~opened & common.dilate(spill, 2)
    # Preserve the head/hair region; the specific observed ribbon is clothing.
    ys = np.where(visible)[0]
    ribbon[:int(ys.min()) + 170] = False
    out[..., 3][ribbon] = 0
    # Removing the connecting ribbon can expose colour-neutral islands. Remove
    # only newly isolated nearby remnants, never arbitrary distant body detail.
    newly_detached = common.detached_debris(out[..., 3] > 0, near_radius=1)
    newly_detached &= common.dilate(ribbon, 12) & band
    newly_detached[:int(ys.min()) + 170] = False
    out[..., 3][newly_detached] = 0
    ribbon |= newly_detached
    # Two reviewed ends of the guarded sleeve's extraction ribbon remain
    # connected and colour-neutral. Trim only their narrow outer edge band;
    # never extrapolate this path-specific correction to other artwork.
    reviewed = np.zeros_like(visible)
    if relative_path == 'яков/view/whole/work/guarded.png':
        assert src.size == (768, 1280), 'Reviewed mask requires original canvas'
        assert hashlib.sha256(a.tobytes()).hexdigest() == (
            '005a208e7b340119d5991de97e72bcf6c61ef61fcb15b7b0c7be22e97256d3d8'
        ), 'Reviewed mask requires exact v3 guarded pixels'
        for y0, y1, x0, x1 in ((548, 579, 468, 462), (610, 645, 457, 451)):
            for y in range(y0, y1):
                cutoff = int(round(x0 + (x1 - x0) * (y - y0) / (y1 - y0 - 1)))
                reviewed[y, cutoff:480] = True
        reviewed &= band & (out[..., 3] > 0)
        out[..., 3][reviewed] = 0
    # Soft, inward-only antialias coverage on a one-pixel boundary. No expansion,
    # canvas shift, resampling or loss of interior detail.
    remaining = out[..., 3] > 0
    boundary = remaining & common.dilate(~remaining, 1)
    softened = np.asarray(Image.fromarray(out[..., 3]).filter(ImageFilter.GaussianBlur(.45)))
    out[..., 3][boundary] = np.minimum(out[..., 3][boundary], softened[boundary])
    # Remove all stale hidden RGB; extend clean foreground through an 8px ring.
    out[..., :3][~remaining] = 0
    bleed, _ = common.propagate(out[..., :3], remaining, 8)
    ring = ~remaining & common.dilate(remaining, 8)
    out[..., :3][ring] = np.rint(bleed[ring]).clip(0, 255).astype('uint8')
    assert np.array_equal(out[core], a[core]), 'Interior changed'
    assert np.all(out[..., 3] <= a[..., 3]), 'Alpha grew'
    assert not np.any(ribbon & core), 'Interior ribbon removed'
    return Image.fromarray(out), {
        'edgeChromaPixels': int(spill.sum()), 'ribbonPixelsRemoved': int(ribbon.sum()),
        'reviewedSleevePixelsRemoved': int(reviewed.sum()),
        'boundaryAlphaPixels': int(boundary.sum()), 'interiorUnchanged': True,
        'dimensions': list(src.size), 'alphaBoundsBefore': list(src.getbbox()),
        'alphaBoundsAfter': list(Image.fromarray(out).getbbox())}


if __name__ == '__main__':
    common.main(repair, {'algorithm': 'edge-matte-v6', 'foregroundCoreInset': 6,
                        'maxConnectedEdgeBandPixels': 6, 'boundaryBlurSigma': .45}, path_aware=True)
