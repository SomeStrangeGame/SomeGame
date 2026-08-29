# Agent: android-version-editor-restart

- Status: completed
- Task: fully restart Unity so the active Android Build Profile reloads bundleVersion 0.2.0, then verify repeated Play Mode startup.
- Scope: Unity lifecycle and own coordination files only; preserve all dirty project files.
- Expected project changes: none.
- Started UTC: 2026-08-29T09:05:22Z.

## Result

- Полный Unity restart перечитал bundleVersion `0.2.0`.
- Два последовательных Play Mode запуска успешно активировали catalog release без initialization/version/schema failure.
- Unity PID 8705 оставлен во втором Play Mode для пользователя.
- Completed UTC: 2026-08-29T09:08:44Z.
