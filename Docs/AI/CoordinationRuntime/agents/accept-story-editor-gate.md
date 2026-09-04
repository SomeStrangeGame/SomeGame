# Agent: `accept-story-editor-gate`

- Status: ready-for-integration
- Task: сделать свежую проверку готовой истории в Android-эмуляторе обязательным runtime gate `$somegame-accept-story`; прежний план интерактивной Editor-проверки отменён пользователем.
- Scope: `.agents/skills/somegame-accept-story/SKILL.md`, `.agents/skills/somegame-accept-story/references/acceptance-checklist.md`, own coordination records and handoff/archive only if required by line limit.
- Contract: acceptance собирает свежий Android Embedded APK, устанавливает его в точный эмулятор и через реальный catalog/runtime flow покрывает все эпизоды, значимые ветки и концовки; проверяет crash/ANR/runtime markers/fallback/selectors/save-state/визуальные сцены. Отсутствующее или устаревшее emulator evidence блокирует readiness; Unity Editor остаётся только техническим build/compile механизмом, не интерактивной средой приёмки.
- Validation: skill quick validation, requirement/routing audit, scoped `git diff --check`, `Tools/somegame docs-check`.
- Base: `1daa8129`
- Requested UTC: `2026-09-03T16:22:51Z`.
- Contract changed UTC: `2026-09-03T16:34:30Z` by explicit user request; mandatory interactive Editor replay replaced with Android emulator smoke.
- Lock acquired UTC: `2026-09-03T16:36:49Z`.
- Completed UTC: `2026-09-03T16:38:30Z`; skill/checklist updated and static validation passed.
