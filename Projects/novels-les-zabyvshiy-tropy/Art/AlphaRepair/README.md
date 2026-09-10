# Character alpha repair

The twenty reviewed, pose-specific masks in `masks/` replace destructive sparse
alpha in all twenty character variants. The repair changes alpha only: decoded RGB pixels,
character identity, pose, costume, expression artwork, dimensions, filenames,
and Unity `.meta` files remain unchanged.

Masks were initialized with local human-portrait segmentation, reduced to the
single connected character component to remove detached background artifacts,
and reviewed on both `#1c1f23` and `#eeeeea` in `dark-light-proof.png`.
Every expression is shown in `all-variants-proof.png` on alternating dark and
light backgrounds.

Run from the repository root with the bundled Pillow/NumPy Python runtime:

```sh
python3 Projects/novels-les-zabyvshiy-tropy/Art/AlphaRepair/apply_character_masks.py
```

The script fails if any source RGB pixel changes and writes per-file hashes to
`repair-report.json`.
