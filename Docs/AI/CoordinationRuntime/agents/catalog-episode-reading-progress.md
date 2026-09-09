# Agent: `catalog-episode-reading-progress`

- Status: ready-for-final-validation
- Task: Implement approved per-episode reading progress bar and percentage above Continue, preserving branching and saves
- Scope: Novels/Assets/Novels/NovelRuntime.cs, NovelProgress.cs and CatalogFlow.cs; Packages/NovelsContentSdk/Runtime/Catalog/CatalogItem.cs and View/Card.cs; Projects/novels-catalog/Assets/RemoteAssets/catalog/fallback.prefab and README.md; Novels/Assets/Editor scoped progress validation; own coordination/handoff. Add authored per-episode reading fill and percentage, completed 100%, locked hidden; preserve old saves and reset-dependent episodes. No story worktree edits, catalog registry changes, branch switch or commits. Await current route-check release before writing; recheck branch and ownership on resume. Default static validation only; Unity slot separately gated.
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-08T14:21:52Z`.

- Scope refinement: additionally Novels/Assets/Novels/EpisodeReadingProgress.cs (+meta), NovelRuntime.EpisodeComposition.cs, Save/SaveSystem.cs; optional isolated static validation fixtures in /private/tmp. No NovelProgress envelope migration. Non-mutating persisted-save decode, bounded route estimate stored as hash/version-matched episode sidecar after save flush. UI marks unfinished percentage approximate; old saves without estimate remain readable and show unknown until next reading exit. Existing completed markers alone supply 100%. No source-map percentage or all-branches denominator.
- Concurrency: explicit user request «Давай не будем ждать, там проверки на эмуляторе»; checkout transfer confirmed by chernaya-melnitsa-route-check, ParallelWorkDetails immutable-APK exception read. Shared unity remains theirs; no Unity/build/ADB/Git mutations. Static compilation/format checks only; no Player acceptance claimed.
- Additional exact scope: Packages/NovelInk/StoryProcessor/Entity.cs, opt-in time-limited speculative read. Default live/replay reading unchanged. Prevent silent Ink loops in forecast from blocking save flush.
- Final documentation scope: Docs/AI/memory/Architecture.md (route to catalog README), Docs/AI/archive/reports/CoordinationHandoffHistory-2026-09-08-episode-progress.md (preserve completed background-delivery handoff and scoped evidence). No active overlapping memory owner observed.

- Result: authored gold reading bar/label/percentage above primary action; completed 100%, locked hidden, unknown legacy estimate —; ≈ denotes single-route forecast. Main save formats untouched; sidecar hash/version matched and removed with episode resets.
- Validation: four current C# assemblies compiled with isolated Roslyn/current reference assemblies; 28 managed probes passed; prefab IDs/references/bindings/horizontal fill/nonblocking raycasts/spacing and scoped diff/docs checks passed. Fixture runner in /private/tmp/somegame-episode-progress-Ts4FYa. No Unity, build, ADB, Git/branch/index mutations.
- Pending: separately authorized catalog rebuild, live Unity portrait/settings/reset/scroll/media checks and updated Player validation. No actual Game View acceptance claimed. Existing APK remains unchanged; user saves untouched.
- Released UTC: 2026-09-08T14:50:00Z. Waiting automation automation is PAUSED.
