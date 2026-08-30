# Agent: ink-line-overlay

- Status: waiting-user-validation
- Task: вернуть debug overlay с текущей строкой исходного Ink.
- Scope: source-map publication/loading/propagation in Content SDK and Novels runtime, compact development-only overlay, focused content build and Unity compile, own coordination records and shared handoff.
- Expected files: `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`, `Novels/Assets/Novels/StorySourceOverlay.cs` and meta, `EntryPoint.cs`, `ApplicationRuntime.cs`, `NovelProcess.cs`, `NovelRuntime.cs`, `NovelRuntime.EpisodeComposition.cs`, `NovelRuntime.NovelPreparation.cs`, `NovelRuntime.StoryQueue.cs`, own coordination files and shared handoff.
- Constraints: Ink/content не менять; overlay только в Editor/Development Build; не возвращать удалённые streaming diagnostics/restart controls.
- Started UTC: 2026-08-30T11:24:00Z
- Heartbeat UTC: 2026-08-30T11:37:00Z
- Result: компактный overlay восстановлен только для Editor/Development Build;
  карта строк публикуется вместе с compiled Ink и передаётся от processor до
  EntryPoint. TZM editor content rebuilt, актуальный Mac release содержит
  `tzm.ink.json.source-map.json`; fresh Unity compile passed без ошибок, Editor
  оставлен открытым. Pending: visual replay пользователем.
