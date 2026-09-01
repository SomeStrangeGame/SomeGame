# Agent: tzm-wardrobe-prefab-variant

- Status: ready-for-integration
- Task: реализовать отдельный prefab variant гардероба ТЗМ, стилизованный ближе к показанному референсу, с безопасным fallback на общий prefab для остальных историй.
- Scope: ContentAddressing wardrobe address, EpisodeAssetLoader/NovelRuntime presentation wiring, OptionList/Wardrobe prefab injection and serialized visual overrides, `Projects/novels-tzm/Assets/Presentation/wardrobe/**`, focused validation, own coordination records and shared handoff.
- Contract: Ink не меняется; динамика и сохранения гардероба не меняются; ТЗМ получает локальный prefab variant, остальные истории используют общий `Resources/OptionListScreen`; только Unity Personal-compatible APIs/assets.
- Base commit: `8d3512787e6e` plus current uncommitted `wardrobe-reference-parity` worktree changes.
- Requested UTC: `2026-08-31T13:03:45Z`.
- Acquired UTC: `2026-08-31T13:18:30Z`.
- Result: runtime опционально загружает `story/presentation/wardrobe/screen-variant.prefab`; ТЗМ содержит локальный prefab variant с белой нижней панелью, синими sliced controls/header и Liberation Sans. При отсутствии story asset сохраняется общий Resources-prefab.
- Validation: scoped `git diff --check` passed; TZM content validation passed; TZM editor build passed and imported wardrobe prefab GUID `4bb1c2193bcf4b238a510594d07de05f`; fresh Novels Unity compile passed, Console contains warnings only and no compiler errors. Automated Play shortcut was blocked by macOS Accessibility, so portrait visual replay remains manual.
