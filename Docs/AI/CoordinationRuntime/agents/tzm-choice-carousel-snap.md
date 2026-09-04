# Agent: `tzm-choice-carousel-snap`

- Status: completed
- Task: make the shared OptionList fallback carousel snap the nearest object to the viewport center after drag, matching the catalog interaction.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records.
- Base: `81aa4e2673537741a19193b5d0de9269dcfd9df8`.
- Validation: scoped diff check, Unity Editor compile, and relevant scoped verification.
- Result: drag-end and side-card focus now snap the normalized nearest card to viewport center. Scoped diff check and fresh Novels compile passed; aggregate verify was blocked at catalog content-build by pre-existing catalog MCP helper processes.
