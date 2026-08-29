# Agent: project-root-chat-restart-protocol

- Status: completed
- Task: закрепить `SomeGame` как project root и добавить безопасный протокол перезапуска разросшихся чатов.
- Scope: `AGENTS.md`, `Docs/AI/README.md`, coordination/integration/memory contracts, handoff rotation and own runtime records.
- Baseline commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`; existing dirty tree preserved.
- Acceptance: root is unambiguous; restart checkpoint preserves scope/evidence and never transfers a live lock implicitly; docs checks pass.
- Started UTC: 2026-08-29T13:04:44Z.
- Finished UTC: 2026-08-29T13:08:00Z.
- Result: canonical SomeGame root and checkpointed chat restart contract added;
  handoff rotated without loss; docs-check and planner 6/6 passed.
- External UI step: save `/Users/iantonishin/Fork/SomeGame` as a Codex project.
