# Story-local Bubble handoff

The Bubble override uses the repository’s maintained base prefab as a variant and changes only story-local presentation properties. Authored art includes a bark/root dialogue panel, oxidized-bronze choice button, split-oak nameplate and root-eye indicator. The palette preserves readable light text over dark surfaces and avoids copying the SCP containment treatment.

`Assets/Presentation/bubble/screen-variant.prefab` references the project-local sprite GUIDs and the shared repository font. Sprite import, nine-slice borders, safe-area behavior, long Russian copy, portrait/landscape layout and contrast require the final Unity/manual visual slot.

On 2026-09-11 the complete `Art/Source/bubble-kit.png` was inspected outside
Unity. Its four surfaces are stylistically coherent, contain no embedded words,
logos or watermarks, and retain clear empty text areas. This does not validate
their slicing, serialized prefab bindings, pressed/disabled states or real
viewport readability.
