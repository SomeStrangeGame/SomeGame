# Agent: ios-texture-profile

- Статус: completed
- heartbeat_utc: 2026-08-24T13:12:00Z
- Завершено UTC: 2026-08-24T13:12:00Z
- Результат: iOS переведён на ASTC 8×8; TZM/ZDM bundles стали меньше baseline,
  регрессия устранена.
- Задача: устранить регрессию размера iOS story bundles после ASTC 6x6.
- Область:
  - `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`
  - iOS generated releases TZM и ZDM
  - size/status документация и собственные runtime coordination files
- Не изменять: Android profile, Catalog, video/Ink/audio source assets.
- Проверки: validate/build iOS для TZM и ZDM, размер против baseline.
- Создано UTC: 2026-08-24T12:57:22Z
