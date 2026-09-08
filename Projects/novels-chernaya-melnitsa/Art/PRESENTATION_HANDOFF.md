# Story-local Bubble handoff

Status: serialization prepared; final Player state matrix deferred.

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
