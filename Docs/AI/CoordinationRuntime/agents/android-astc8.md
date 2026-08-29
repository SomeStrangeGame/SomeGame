# Agent: android-astc8

- Status: ready-for-integration
- Task: перевести Android story art на ASTC 8×8 через общий postprocessor.
- Scope: texture postprocessor, актуальные size/authoring docs, собственные coordination files.
- Expected files: `NovelContentTexturePostprocessor.cs`, три документа и собственный status.
- Base commit: `7e9c7727`.

## Result

- Android story art переведён с ASTC 6×6 на ASTC 8×8.
- Версия postprocessor увеличена с 5 до 6 для принудительного реимпорта.
- Authoring и size-optimization документы синхронизированы.
- Doctor и `git diff --check` прошли.
- Licensing IPC восстановлен; Android TZM/ZDM builds завершены успешно.
- Bundle totals: TZM 27 792 318 B, ZDM 56 445 255 B; сравнение со старым
  baseline включает chunking/exclude-unused pipeline, а не только ASTC.
