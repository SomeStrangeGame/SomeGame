# Agent: tzm-wardrobe-fallback-isolation

- Status: ready-for-manual-validation
- Task: временно исключить TZM wardrobe variant из content bundle, чтобы проверить общий fallback wardrobe prefab.
- Scope: `Projects/novels-tzm/Assets/tzm.asset`, focused TZM editor content build, Novels Editor lifecycle, own coordination records and handoff.
- Contract: не удалять TZM prefab/UI art и не менять Ink/save/runtime wardrobe logic; исключить только story-specific wardrobe assets из TZM release; сохранить все чужие изменения.
- Base commit: `8f89f27b0f01` plus preserved shared dirty tree.
- Requested UTC: `2026-09-01T06:42:45Z`.
- Completed UTC: `2026-09-01T06:49:10Z`.
- Result: TZM `screen-variant.prefab` and its nine generated UI sprites are retained in source but marked authoring-unused, so the rebuilt editor release has no `presentation/wardrobe` asset and runtime must instantiate the shared `Resources/OptionListScreen` fallback.
- Validation: TZM validate/build editor passed; bundle string audit found no wardrobe variant addresses; Novels Editor restarted and compiled with no errors. Manual visual acceptance remains with the user.
