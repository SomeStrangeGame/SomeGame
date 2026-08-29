# Agent: token-efficient-unity-workflow

- Status: completed
- Task: сократить токенный расход и число Unity/tool циклов через changed-path validation gates, компактный verify runner, агрегированный MCP check и cleanup текущего coordination snapshot.
- Scope: `Tools/novels-tools/**`, `Tools/unity-mcp-helper/**`, `Docs/AI/**`, собственные coordination records.
- Expected project changes: CLI/helper scripts and tests, current handoff rotation, canonical coordination/build/MCP documentation.
- Started UTC: 2026-08-29T10:45:08Z.
- Lock acquired UTC: 2026-08-29T11:20:30Z.
- Completed UTC: 2026-08-29T11:35:00Z.
- Result: added changed-path plan/verify, one-call Editor check, compact log
  policy, handoff rotation and verified FIFO cleanup.
