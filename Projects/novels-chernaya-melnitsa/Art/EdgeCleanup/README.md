# Technical character-edge cleanup — 2026-09-08

Current status: v6 source repair applied; fresh APK39049f824adf improves sleeve
ribbon/Lada fringe but still FAILS residual Yakov pants/sleeve edge review.
The user subsequently accepted these residual contours as non-blocking; do not
resume cleanup automatically. Current integration steps: `../ACCEPTANCE_PLAN.md`.
See [v6 evidence](v6/README.md) for current hashes, proofs and limitations.
Everything below documents historical v3, NOT current production hashes.

Historical v3 status: source iteration preserved, but fresh Player alpha gate FAILED.
APK a712f2ffeabc on API34 software emulator still shows coloured strips at
Yakov206 and Lada215; exact new release identity confirmed. The limited colour
metric below is insufficient. This iteration needs further technical correction;
see ../ACCEPTANCE_EVIDENCE.md for current evidence. No physical ASTC-device pass.

User approved technical mask/edge-colour cleanup without regenerating drawings.
All 11 runtime PNGs changed; 14,046 contaminated edge pixels were recoloured
from adjacent clean foreground with a corresponding bounded opacity reduction.
Two remote scraps from the old sheet extraction were removed: Saveliy229px,
Yakov guarded244px. Originals are recoverable; no character body part removed.

## Method and invariants

`../cleanup_character_edges.py` uses Pillow/numpy, no AI or new references.
It starts with visible pixels within6px of transparency and magenta excess
`min(R,B)-G > 10`, following connected spill at most12px from the boundary.
RGB is estimated from neighbouring clean foreground; opacity accounts for the
estimated former magenta field. It does not globally blur or erode silhouettes.
Invisible RGB is extended8px into alpha-zero texels for filtering. Small remote
8-connected components are removed only if separated from the principal body
by more than6px; nearby detached hair is protected. Proofs were reviewed before
applying the candidate. Application checks original/candidate/meta SHA256 first.

- All canvases remain768x1280 RGBA, with unchanged coordinates and actual body
  silhouettes; no visible pixels are added, no body pixel loses visibility.
- Interior RGBA is byte-identical; changes are confined to detected edge spill,
  invisible RGB and the two discarded scraps. Metadata/GUIDs are byte-identical.
- Saveliy alpha bounds x-min154→210 and Yakov guarded x-max560→482 reflect ONLY
  removed remote scraps. The body was not cropped, resized or repositioned.
- Under the stated colour heuristic, visible magenta-count fell14,067→17.
  Seventeen low-count interior/rounding matches remain deliberately; this metric
  is not a colour-independent alpha guarantee or a reason to recolour interiors.
- No shared import policy, compression override, prefab or story text changed.

## Evidence and reproduction

`report.json` maps every runtime-relative path to original/candidate/meta hashes
and per-file statistics. Proof labels are before(left)/after(right) on dark,
light and blue backgrounds:

- `lada-wary-before-after.png`: enlarged head/coat edges.
- `yakov-guarded-before-after.png`: enlarged head/sleeve edges.
- `saveliy-mask-before-after.png`, `yakov-mask-before-after.png`: full bodies and
  removal of isolated scraps, without moving the body on its canvas.

Full 11-variant comparisons, candidates and exact original PNG/meta backups:
`Novels/Build/Logs/automation/chernaya-alpha-20260908/v3/` (ignored, local).
The baseline PNGs also match Git HEADf1721a63e046 for recovery. Earlier identity
sheets and alpha proofs are retained as historical provenance, not current PNGs.

To reproduce, provide a separate restored baseline Characters directory and a
new empty output path to `cleanup_character_edges.py --source ... --output ...`.
Review the resulting proofs first; `--apply` is explicit and refuses a changed
source/candidate/meta hash. Do not run repeatedly on already processed sprites.
Run `test_character_edges.py` with Pillow/numpy Python: seven regression tests
cover interior preservation, edge correction, isolated interior colour, remote
scraps versus near hair, diagonal connectivity, non-wrapping bleed and determinism.

Source review found no new identity/pose/costume changes. Originality evidence
remains applicable as documented in `../ORIGINALITY_EVIDENCE.md`. The old APK
098ff515a0e1 is stale for these textures. The separately authorized follow-up
APK a712f2ffeabc tested this iteration and failed as stated above. No Unity/ADB
was used during the source cleanup itself; the follow-up is a separate record.
