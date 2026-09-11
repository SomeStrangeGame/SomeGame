# Story-local Bubble handoff

The Bubble override uses the repository’s maintained base prefab as a variant and changes only story-local presentation properties. Authored art includes a bark/root dialogue panel, oxidized-bronze choice button, split-oak nameplate and root-eye indicator. The palette preserves readable light text over dark surfaces and avoids copying the SCP containment treatment.

`Assets/Presentation/bubble/screen-variant.prefab` references the project-local sprite GUIDs and the shared repository font. Sprite import, nine-slice borders, safe-area behavior, long Russian copy, portrait/landscape layout and contrast require the final Unity/manual visual slot.
