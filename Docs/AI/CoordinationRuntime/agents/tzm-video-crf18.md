# Agent: tzm-video-crf18

- Статус: completed
- heartbeat_utc: 2026-08-24T13:00:00Z
- Завершено UTC: 2026-08-24T13:00:00Z
- Результат: 51 видео сжаты на 17,06%, звук удалён, три platform release
  пересобраны; ручная визуальная приёмка отложена.
- Задача: удалить аудиодорожки и перекодировать 51 TZM MP4 в H.264 CRF 18.
- Область:
  - `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/*.mp4`
  - `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/*.mp4.meta`
    только если Unity обновит import metadata
  - TZM generated release/build output
  - size/status документация и собственные runtime coordination files
- Не изменять: ZDM, Catalog, Unity bundle assets, Ink и audio payload catalog.
- Проверки: video/audio streams, duration, размеры, TZM validate/build android.
- Создано UTC: 2026-08-24T12:25:44Z
