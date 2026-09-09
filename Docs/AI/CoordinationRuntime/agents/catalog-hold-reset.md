# Agent: `catalog-hold-reset`

- Status: ready-for-review
- Task: Require a continuous 2-second hold to confirm episode reset
- Scope: Packages/NovelsContentSdk/Runtime/Catalog/View/Card.cs;Packages/NovelsContentSdk/Runtime/Catalog/View/HoldToConfirm.cs;Packages/NovelsContentSdk/Runtime/Catalog/View/HoldToConfirm.cs.meta;Projects/novels-catalog/Assets/RemoteAssets/catalog/fallback.prefab;Novels/Assets/Editor/CatalogVisualValidation.cs;Projects/novels-catalog/README.md
- Base commit: `c5a431e30ff857a00e70fd50ed14ae0d559995ad`
- Requested UTC: `2026-09-08T06:03:21Z`
- Validation: build/compile; pointer cancellation and full hold on an isolated cloned button with a counter callback; never reset real saves.
- Result: catalog build and fresh Novels compile passed; live catalog and hold/cancel tests passed, final Game View inspected. User saves untouched; Editor left playing, confirmation closed.
