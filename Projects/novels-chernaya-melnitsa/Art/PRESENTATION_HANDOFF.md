# Story-local Bubble handoff

Status: composition partially Player-validated; overall acceptance `blocked`
by character alpha-edge contamination and incomplete visual/route coverage.
Current prefabs were imported and built into the composition APK (SHA256
098ff515a0e17ac90f07fad6efb8ede99ed3369eee7d11918a0a12d10d49a504).
Long named lines50/77 and Lada172/204 preserve face/text separation; C2 choices
fit. Lada hair/clothing edges visibly fail. Full details and outstanding matrix
are in `ACCEPTANCE_EVIDENCE.md`, Composition APK validation.

## Current composition — user revision, 2026-09-08

The second APK fixed the first three-option choice, but the named panel at
Ink:48 covered Nastasiya's face. The user requested characters higher, Bubble
lower, and larger characters. The initial unbuilt named-panel-up experiment
was discarded.

- New `Assets/Presentation/character/screen-variant.prefab` inherits the shared
  character screen (`f5229a1934c0e49869f12b3adf69a449`). Only viewport y changes
  from -220 to150 and uniform XY scale from1 to1.2; root name is screen-variant.
  Runtime resets sprite positions to the viewport in ShowImage, so moving that
  viewport preserves the offset for every sprite/emotion and both screen sides.
  Source PNGs/alpha registration, identity, layer bindings and runtime are intact.
  Unity imported new `.meta` files; character prefab GUID is
  c33639195f26345c2a2f184e22156798, with its address present in Android release.
- Bubble left/right/thought roots move from y=-115 to-180; narrator from35 to-55.
  This lowers panels65/90 logical units versus the second APK. The previous
  viewport210, fonts22/20, button96 and slice-border settings remain.
- Static audit checks338 dialogue strings,200 named strings,12 labels and360
  real pre-choice contexts across72 routes. C5 can follow named dialogue, so
  narrator-only estimates are insufficient. Worst choice bottom958 leaves66
  units at logicalheight1024; max named panel bottom794 includes padding.
  Alpha-top estimates for11 whole sprites at3 portrait heights preserve an
  approximate140-unit head zone above named headers. This is not face detection
  or a visual proof of portrait crop, scaling, contrast or safe-area handling.
- Second-APK screenshots remain history only. Current APK partially validates
  this geometry, but broad Saveliy, Mitya465, first three-choice group and C5
  speaker variants remain unverified. Complete the matrix below after the
  character-edge defect is corrected; no complete Player pass is claimed.

## Previous choice-clipping correction (retained)

2026-09-08: fresh single-story APK exposed overflow at the first three-option
choice (`s01e01.ink:22`): stretched panel edges crossed the text and the last
option was clipped. The story-local prefab now uses 22-unit body text, 20-unit
centered choice text, 96-unit choice targets and a viewport y-offset of 210.
The five dialogue panel images use slicing with PPU multiplier 8; existing
character/no-character panel import borders are 240/120/240/120. Button PPU is
8 so its already-authored slice border leaves room for two lines. Existing
PNG bytes, sprite GUIDs, hierarchy, component bindings and shared runtime are
unchanged; no raster generation or new originality iteration was needed.

`audit_bubble_layout.py` (Pillow) verifies inherited IDs, unique overrides,
slice settings and all 338 source dialogue strings/12 labels with the shipped
font. The earlier estimate considered simultaneous longest text/three buttons;
the current lower layout uses every actual route context, as described above.
These estimates are NOT Unity's text layout or a
Player screenshot: the complete state matrix below must be rerun in a fresh APK.
The failed APK/log/screenshots are recorded in `ACCEPTANCE_EVIDENCE.md`.

The story owns `Assets/Presentation/bubble/screen-variant.prefab`. It preserves
the known-good runtime component hierarchy and serialized bindings from the
current SCP story-local implementation while replacing only its referenced
surface sprites with original «Чёрная мельница» art and preserving their GUIDs.
The prefab's shared text references resolve to the single story-local
`Assets/Presentation/Fonts/liberationsans-regular.ttf` copy with the established
font GUID; no per-prefab font duplicate is used.

- Dialogue/no-character panels use the low soot surface with rye-gold woven edge.
- Choice buttons use the compact matching card; accessible runtime labels remain.
- `promise-knot.png` is decorative story evidence and contains no embedded text.
- No flashing animation is authored; reduced-motion behavior is unchanged.

Required final Player matrix: longest narrator line on kitchen and river scenes;
longest named line with two characters; all three ending choices; pressed and
disabled button state; permitted missing-icon fallback; narrow portrait safe
area; strongest grinding-room state. Check contrast, wrapping, tap areas,
sprite alpha/halos, fallback UI and missing/pink assets after a fresh build.
