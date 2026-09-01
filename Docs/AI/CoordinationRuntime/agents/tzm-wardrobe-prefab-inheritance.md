# Agent: tzm-wardrobe-prefab-inheritance

- Status: ready-for-integration
- Task: заменить ручную заготовку TZM wardrobe prefab настоящим Unity Prefab Variant общего fallback и привести variant-layout к референсу без изменения Ink.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/OptionListScreen.prefab`, `Projects/novels-tzm/Assets/Presentation/wardrobe/**`, временный `Projects/novels-tzm/Assets/Editor/TzmWardrobePrefabVariantBuilder.cs`, собственные coordination records и общий handoff.
- Contract: TZM asset обязан иметь `PrefabAssetType.Variant` и наследовать общий Resources fallback; остальные истории сохраняют текущий fallback; только Unity Personal-compatible API; чужие dirty changes не затрагиваются.
- Base commit: `8f89f27b0f01` plus existing uncommitted wardrobe changes in the scoped files.
- Requested UTC: `2026-08-31T16:23:45Z`.
- Result: Unity сохранил fallback serialized defaults и TZM Prefab Variant с reference-layout override; вариант наследует fallback GUID, не содержит root-transform overrides и сохраняет собственный GUID `4bb1c2193bcf4b238a510594d07de05f`.
- Validation: Unity `PrefabAssetType.Variant` и corresponding-source checks passed; TZM content validate/editor build passed; fresh Novels compile passed without compiler errors; scoped `git diff --check` passed. Manual portrait visual replay remains.
