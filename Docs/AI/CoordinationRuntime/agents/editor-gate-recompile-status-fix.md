# Agent: editor-gate-recompile-status-fix

- Status: completed
- Task: исправить live timeout `editor-gate --compile` в контракте Official MCP recompile/status.
- Scope: `Tools/unity-mcp-helper/**`, `Tools/somegame-tools/**`, связанные automation/MCP docs/tests, собственные runtime-записи и `HANDOFF.md`.
- Baseline commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`.
- Acceptance: подтверждённая причина, regression test, успешный повторный live `editor-gate --compile`, без изменений Unity assets/settings.
- Started UTC: 2026-08-29T12:48:39Z.
- Finished UTC: 2026-08-29T12:57:00Z.
- Root cause: final `recompile=up_to_date` was ignored; cold-start port evidence
  also preceded actual Pipeline readiness and retryable trigger failures were polled.
- Validation: helper 26/26; warm live gate 2.209 s; two cold-start live gates
  12.032 s and 11.442 s; no Unity assets/settings changed.
