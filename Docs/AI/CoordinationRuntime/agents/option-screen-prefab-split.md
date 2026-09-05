# Agent: `option-screen-prefab-split`

- Status: ready-for-integration
- Task: unity
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListController.cs`, temporary `Packages/NovelsContentSdk/Editor/OptionListPrefabMigration.cs(.meta)`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/ChoiceScreen.prefab(.meta)`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/WardrobeScreen.prefab(.meta)`, removal of legacy combined `OptionListScreen.prefab(.meta)`, `Projects/novels-tzm/Assets/Presentation/choose/screen-variant.prefab`, `Projects/novels-tzm/Assets/Presentation/wardrobe/screen-variant.prefab`, `Docs/AI/memory/Architecture.md`, `Docs/AI/CoordinationRuntime/HANDOFF.md`.
- Base commit: `5d5da448ad1a4b5b29c4c46d1af1b7e688037d3d`.
- Requested UTC: `2026-09-05T12:30:32Z`.
- Contract: independent authored fallback prefabs for Choose and Wardrobe; TZM variants inherit only their corresponding fallback; runtime selects the fallback by `OptionListLayout` while preserving story-provided variants.
- Compatibility: preserve the existing base GUID/fileIDs for the Choice migration; preserve Wardrobe fileIDs in its new base before repointing the TZM Wardrobe variant.
- Validation: serialized reference audit, scoped diff-check, SDK/EditMode checks selected by verify, TZM editor content build, fresh Novels compile, manual Choose/Wardrobe visual smoke.
- Result: independent Choice and Wardrobe fallback hierarchies generated with stable inheritance; runtime fallback selection, TZM content build and Novels compilation passed. Manual visual smoke remains.
