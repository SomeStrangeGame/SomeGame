# Agent: somegame-automation-runners

- Status: ready-for-live-validation
- Task: реализовать единый `Tools/somegame` и шесть bounded automation workflows: docs-check, editor-gate, content-gate, player-build, android-smoke, licensing-preflight.
- Scope: `Tools/somegame`, `Tools/somegame-tools/**`, узкие вызовы существующих `Tools/novels-tools/**`, `Tools/unity-mcp-helper/**`, `Novels/Tools/build-player*.sh` без изменения их контрактов, `Docs/AI/guides/**`, `Docs/AI/memory/**`, собственные runtime-записи и `HANDOFF.md`.
- Expected changes: local tooling/tests/docs only; Unity projects and content assets unchanged.
- Heavy operations: implementation validation is static/unit only; no Unity, Player build, emulator or licensing process termination without a separate live gate.
- Started UTC: 2026-08-29T12:02:02Z.
- Lock acquired UTC: 2026-08-29T12:02:02Z.
- Completed UTC: 2026-08-29T12:17:30Z.
- Result: шесть bounded workflows реализованы через единый JSON CLI; 35 unit tests и safe static/preflight gates прошли, тяжёлые live gates отложены.
