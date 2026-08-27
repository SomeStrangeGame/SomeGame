# Agent: tzm-video-posters-unused

- Статус: completed
- Задача: пометить постеры локаций с видео как неиспользуемые и убрать runtime-зависимость видео от постера.
- Область:
  - `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
  - `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
  - `Packages/NovelsContentSdk/Runtime/Features/Location/BackgroundPresentationController.cs`
  - `Projects/novels-tzm/Assets/tzm.asset`
  - `Novels/Docs/AI/ContentAuthoringGuide.md`
  - `Novels/Docs/AI/ParallelWork.tzm-video-posters-unused.md`
  - собственные coordination-файлы и новая запись в `CoordinationRuntime/HANDOFF.md`
- Ожидаемые изменения: отдельная Inspector-группа «Не используется», исключение отмеченных GUID из генерации чанков и ожидание первого кадра видео на переходном экране; статические PNG без видео сохраняются в чанках.
- Последнее обновление UTC: 2026-08-27T16:02:12Z
