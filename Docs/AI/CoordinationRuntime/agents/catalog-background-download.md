# Agent: `catalog-background-download`

- Status: ready-for-review
- Task: Lightweight catalog preview, ordered background story delivery, per-episode readiness/progress and retry
- Base commit: c5a431e30ff857a00e70fd50ed14ae0d559995ad
- Requested UTC: `2026-09-08T07:42:00Z`
- Scope: Novels/Assets/Novels/CatalogFlow.cs; Novels/Assets/Novels/CatalogDownloads.cs and .meta; Novels/Assets/Novels/ApplicationRuntime.cs; Novels/Assets/Novels/NovelProgress.cs; Novels/Assets/Novels/ContentDeliveryFlow.cs; Packages/NovelsContentSdk/Runtime/Catalog/CatalogItem.cs; Packages/NovelsContentSdk/Runtime/Catalog/CatalogController.cs; Packages/NovelsContentSdk/Runtime/Catalog/View/Card.cs; Packages/NovelsContentSdk/Runtime/CatalogContracts/CatalogContracts.cs; Packages/NovelsContentSdk/Runtime/ContentAddressing/ContentPackageConvention.cs; Packages/NovelsContentSdk/Editor/ContentPipeline.cs; Novels/Assets/Editor/CatalogVisualValidation.cs; Projects/novels-catalog/README.md; Docs/AI/guides/ContentPipeline.md; Docs/AI/memory/Architecture.md; own coordination/handoff/archive; generated zdm/tzm/catalog validation output
- Contract: build-generated platform preview JSON from NovelDefinition with release id/version/episodes, read before payloads. Existing card.json schema remains unchanged; old clients ignore extra sidecar; new clients require rebuilt story releases. No per-episode dependency mapping exists, so each episode conservatively requires all shared story groups.
- Validation: scoped static/compile; preview export and loaded catalog background/retry/cancellation controls. Preserve saves, no commits/publish.
- Additional validation scope: Novels/Assets/Editor/Novels.Editor.asmdef (references for existing live-validation helper); editor-only friend declaration in CatalogDownloads.cs. No new test assembly.
- Result: zdm/tzm previews built, compile passed; live queue/cancellation/retry/controls and catalog/settings/snapping passed. Editor startup catalog ~1.7 s in measured local run. Shared story groups gate episodes together; Android/iOS outputs require rebuild, no Player/network timing gate run. User saves/cache preserved; isolated fixture cache removed.
- Final verification: final runtime cleanup compiled without new errors; isolated delivery suite passed again (editor log marker line 4113). Validation temporarily enables background frames and restores the previous setting.
