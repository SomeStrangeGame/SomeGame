# Agent: tzm-precise-usage-order

- Статус: completed
- Задача: исправить порядок чанков по точному первому использованию команд Ink.
- Область:
  - `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
  - `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
  - `Packages/NovelsContentSdk/Editor/Novels.ContentSdk.Editor.asmdef`
  - `Projects/novels-tzm/Assets/tzm.asset`
  - `Novels/Docs/AI/ContentAuthoringGuide.md`
  - `Novels/Docs/AI/ParallelWork.tzm-precise-usage-order.md`
  - собственные coordination-файлы и новая запись в `CoordinationRuntime/HANDOFF.md`
- Ожидаемые изменения: точные command-aware first-use позиции через общий Ink parser и более специфичные токены арта; повторный расчёт TZM без изменения PNG/media.
- Последнее обновление UTC: 2026-08-27T16:23:29Z
