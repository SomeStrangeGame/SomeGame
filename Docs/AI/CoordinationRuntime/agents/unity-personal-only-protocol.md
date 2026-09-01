# Agent: unity-personal-only-protocol

- Status: completed
- Task: add a fail-closed Unity Personal-only rule to the canonical project/build and MCP protocols.
- Scope: `Docs/AI/guides/ContentPipeline.md`, `Docs/AI/guides/UnityMcpWorkflow.md`, `Docs/AI/guides/AutomationRunners.md`, `Docs/AI/memory/Workflows.md`, own coordination records and handoff/archive rotation if required.
- Acceptance: one canonical definition covers source, packages, settings, Editor/MCP actions and builds; references do not duplicate the contract; Markdown links and scoped diff check pass.
- Constraints: do not touch the active GPL episode 3 scope or its runtime records; do not acquire lock until this request is first and the existing lock is released.
- Started UTC: 2026-08-31T09:11:11Z.
- Finished UTC: 2026-08-31T09:35:37Z.
- Result: `ContentPipeline.md` now defines Unity Personal as the mandatory
  baseline and prohibits paid-tier dependencies in project sources, packages,
  services, settings, Editor/MCP automation and builds, including temporary
  artifact generation. MCP, runner and workflow memory reference that source.
- Validation: `Tools/somegame docs-check` passed with no failures; scoped
  `git diff --check` passed.
- Coordination: stale `gpl-episode3-full-smoke` request/write-lock was removed
  after explicit user approval and repeated stale/process checks; its agent
  record and all GPL files remain untouched and must requeue before resuming.
