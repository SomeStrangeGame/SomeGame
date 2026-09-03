# Agent: `episode-launch-actions`

- Status: ready-with-limitations
- Task: remove the story settings-prefab launch step and move new/continue actions to the existing episode selection screen.
- Scope: `Novels/Assets/Novels/ApplicationTexts.cs`, `CatalogFlow.cs`, `ApplicationRuntime.cs`, `NovelRuntime.cs`, `NovelRuntime.Content.cs`, `NovelRuntime.NovelPreparation.cs`, removal of `SettingProcess.cs(.meta)` if unused; `Packages/NovelsContentSdk/Runtime/Catalog/CatalogItem.cs`, `CatalogController.cs`, `View/CatalogScreen.cs`; `Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab`; narrow tests and canonical docs if required; own coordination records and handoff.
- Contract: episode selection exposes existing save semantics as `Продолжить` and `Начать заново`; story launch never requires `<story>/application/setting/screen.prefab`; restart clears only the selected episode save; continuation behavior and episode progression remain compatible.
- Evidence: supplied Sobibor runtime stack; Mac release audit confirms missing setting prefab in `deti`, `devyataev`, `mamkin`, `maresyev`, `mmm`, `okt`, `poletaev`, `sobibor`, and `zmt`, while runtime requires it unconditionally.
- Base commit: `7e8cf0b30f0fa2c754dbb09524ced4a2f77ef584` (`origin/main`) on canonical branch `codex/episode-launch-actions`; no worktree was created.
- Planned validation: focused save/launch tests, scoped serialization audit, `git diff --check`, catalog build, Sobibor validation/build, fresh Novels compile, and bounded runtime reproduction if available.
- Queue note: yielded request `20260903T072222Z-episode-launch-actions` so `toybedtime-publish` can integrate and restore a safe canonical checkout before this shared-runtime change.
- Active request: `20260903T073033Z-episode-launch-actions`.
- Result: episode cards now expose `Новая игра` without a save and both `Продолжить` / `Начать заново` with a save. Restart clears only the selected episode. The runtime no longer resolves or loads a story setting prefab; the obsolete `SettingProcess` was removed. The shared catalog API remains source-compatible through the original `Select` method and adds `SelectAction` for two-action consumers.
- Validation: fresh Novels Unity compile passed twice with zero compiler errors; `novels-content validate catalog` and `validate sobibor` passed; scoped `git diff --check` and runtime setting-reference audit passed. The Sobibor validator's Unity import added trailing spaces to tracked YAML, which were normalized back to the exact clean state; no Sobibor content diff remains.
- Limitation: a manual portrait interaction pass of the dynamically added secondary button was not run.
- Heartbeat UTC: `2026-09-03T08:04:38Z`
