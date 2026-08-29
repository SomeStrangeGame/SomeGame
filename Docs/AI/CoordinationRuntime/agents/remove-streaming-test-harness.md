# Agent: remove-streaming-test-harness

- Status: completed
- Task: удалить HUD, Cold/Warm, Editor network throttle и experimental build flag, сохранив production streaming.
- Scope: основной Novels runtime entry/diagnostics, story streaming diagnostic hooks, Content SDK pipeline flag, собственный status/handoff.
- Expected files: `EntryPoint.cs`, overlay/diagnostics files, `ApplicationEnvironment.cs`, `ApplicationRuntime.cs`, `NovelRuntime*.cs`, `StoryStreamingController.cs`, `ContentDeliveryFlow.cs`, `ContentPipeline.cs`, coordination records.
- Started UTC: 2026-08-28T11:06:28Z
- Completed UTC: 2026-08-28T11:35:54Z
- Result: тестовый HUD, Cold/Warm restart, network throttle, cache generations,
  source-map runtime overlay и experimental flag удалены; story streaming стал
  штатным режимом сборки.
- Validation: Roslyn успешно собрал `Bundles`, `Novels.Location`, `Novels` и
  `Novels.ContentSdk.Editor`; `git diff --check` успешен. Unity batch refresh
  не стартовал из-за таймаута Licensing Client IPC.
