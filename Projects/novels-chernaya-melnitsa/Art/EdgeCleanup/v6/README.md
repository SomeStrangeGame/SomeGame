# Character matte v6 — 2026-09-08

Status: source correction applied; subsequent fresh APK39049f824adf visual FAIL.
User decision2026-09-08: residual contours accepted as non-blocking. This does
not change pixels or erase the observation; full runtime acceptance is pending.
Large sleeve ribbon removed, Lada215 fringe reduced, but Yakov217 still has
red/maroon trousers edge and greenish left-sleeve contour. See latest
`../../ACCEPTANCE_EVIDENCE.md`. Offline checks below are not visual acceptance.
User approved continued technical mask repair without regeneration. No Unity,
APK build, ADB action, new artwork, shared import change or commit in this pass.

## Cause isolated

Decoded the actual Texture2D data from APK a712f2ffeabc using UnityPy1.25.3.
All 11 textures were768x1280 ASTC8 (format51). In the sampled Lada wary and
Yakov guarded textures, alpha MAE versus v3 source was only0.0358/0.0487 on
the0–255 scale;12/28 source-transparent pixels became alpha>32. See
`source-apk.json` and `*-source-apk.png`. Wide coloured sleeve ribbons already
existed in source; ASTC added blockiness but did not create the wide ribbon.
The v3 rule `min(R,B)-G > 10` missed warm red/brown extraction residue.

## Applied correction

`../../repair_character_matte.py` estimates edge chroma from foreground at
least6px inside the existing silhouette, changes only excess R/G and B/G
differences, reduces contaminated opacity and adds inward-only1px antialiasing.
A5x5 opening is a narrow-ribbon detector, NOT a global figure erosion. Removal
requires nearby chroma evidence; head/hair is protected from ribbon deletion.
Two reviewed, connected colour-neutral ribbon ends at Yakov guarded's right
sleeve use an explicit selector/canvas/pixel-hash-specific mask within the same6px band.
Hidden RGB is rebuilt in an8px ring. This is deterministic Pillow/numpy work.

- All11 PNGs:17,300 edge-chroma pixels corrected;941 thin-ribbon pixels removed,
  plus162 reviewed sleeve pixels.40,731 boundary pixels considered for AA.
- Interior RGBA beyond6px is byte-identical; alpha never increases. Canvas stays
 768x1280, registration/pose/identity/costume unchanged. No scaling/resampling.
- Ten alpha bounds unchanged; Mitya's rightmost extraction edge moves529→528
  (one pixel). This does not reposition or crop the canvas.
- All11 `.meta` bytes/GUIDs unchanged. Existing viewporty150/scale1.2,
  Bubble placement, story Ink and canonical ASTC8 policy unchanged.
- `report.json` records v3 input, v6 output and metadata SHA256 per selector.
  All applied PNG/meta hashes independently verified after application.

## Review and validation

`review-00.png`, `review-04.png`, `review-08.png`: all11 final full-body variants,
reviewed on dark/light backgrounds. Index00–03Lada guilt/main/resolve/wary;
04Mitya,05Nastasya,06Saveliy,07–10Yakov alarm/guarded/honest/main.
No visible identity/pose/interior-art loss at this review scale.

`lada-astc-proof.png`, `yakov-astc-proof.png`: v3(left) versus v6(right) after
controlled OFFLINE ASTC8 encode/decode, dark/light/blue. astc-encoder-py0.1.12,
LDR_SRGB,8x8,quality100,threads1. These show reduced coloured fringe and sleeve
ribbon; they are NOT Unity's exact fresh import or an APK/device screenshot.
Fine staircase/compression artefacts remain a runtime-review risk, especially
at enlarged display scale; no physical ASTC-device/performance claim.

15 regression tests pass (7 v3+8 matte): warm fringe formerly missed, core
identity, no alpha growth, protected thin hair, evidence-bound ribbon deletion,
clean thin detail, registration/colour preservation, exact reviewed selector
canvas and determinism. Static story audit72routes/3endings, layout338dialogues/
12labels/360choice-contexts, configuration doctor and scoped diff checks pass.
The plan's Unity content gate is deliberately deferred to fresh human approval.

## Reproduction / next gate

Original pre-v3 bytes remain recoverable from Git HEADf1721a63e046 and the v3
backup. Exact v3 inputs for THIS pass and all11 full/detail before/after proofs:
`Novels/Build/Logs/automation/chernaya-edge-repair-20260908/v6/` (ignored).
Generate from that separate `before` directory using `repair_character_matte.py
--source <baseline> --output <new-empty-dir>`. Review first; `--apply` checks all
source/candidate/meta hashes before copying. Do not apply repeatedly to v6.
Diagnostic scripts and isolated decoder dependencies remain under the same
ignored parent directory; no project package dependency was added.

APK a712f2ffeabc is stale for v6; its visual FAIL remains historical. The later
separately authorized APK39049f824adf contains only chernaya-melnitsa and confirms
new releasefb7c51d4b322, with the residual observation stated above. Following
the user's exception, do not resume edge correction automatically. Next: remaining
runtime checks in `../../ACCEPTANCE_PLAN.md` after fresh approval. Do not rebuild all.
Full semantic routes/endings/save-resume acceptance also remains open.
