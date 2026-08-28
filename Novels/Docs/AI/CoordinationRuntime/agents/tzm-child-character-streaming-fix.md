# Agent: tzm-child-character-streaming-fix

- Status: completed
- Task: вернуть детскую Салли в streaming-сборку TZM без изменений Ink.
- Scope: анализатор распределения character art по чанкам, авторский layout TZM после регенерации, собственный status/handoff.
- Expected files: `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`, проверки анализатора, `Projects/novels-tzm/Assets/tzm.asset` после регенерации.
- Started UTC: 2026-08-28T10:28:55Z
- Yielded UTC: 2026-08-28T10:34:00Z
- Pending: остановить Play Mode, дождаться Unity refresh, пересобрать Editor content и повторить строку 421.
- Resumed UTC: 2026-08-28T10:34:51Z
- Yielded UTC: 2026-08-28T10:38:00Z
- Pending: основной Editor не выполняет auto-refresh; закрыть Unity для последовательного batch build и повторного запуска на 5 Мбит/с.
- Resumed UTC: 2026-08-28T10:37:18Z
- Completed UTC: 2026-08-28T10:41:14Z
- Result: TZM Editor release пересобран, все четыре child body находятся в `chunk-1`; основной Editor снова открыт на 5 Мбит/с.
