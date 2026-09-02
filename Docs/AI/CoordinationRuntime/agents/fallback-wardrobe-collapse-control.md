# Agent: `fallback-wardrobe-collapse-control`

- Status: ready-for-manual-validation
- Task: убрать дублирующую верхнюю правую кнопку сворачивания из authored fallback wardrobe; оставить единственным collapse control стрелку над панелью.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/OptionListScreen.prefab`, live Novels compile, own coordination records and shared handoff.
- Contract: selection, character switching, cancel/confirm, tabs and TZM story content не менять; сохранить prefab GUID/file IDs остальных объектов и существующий dirty tree.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: 2026-09-01T13:20:12Z
- Result: `WardrobeMode` скрыта в authored fallback prefab и удалена из runtime binding; `Collapse` над панелью остаётся единственным control сворачивания.
- Validation: scoped `git diff --check` passed; `editor-gate --project Novels --compile` passed без compiler errors, Editor оставлен открытым.
