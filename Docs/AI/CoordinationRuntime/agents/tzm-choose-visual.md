# Agent: `tzm-choose-visual`

- Status: completed
- Task: launch Novels for manual visual review of the newly authored TZM Choose screen.
- Scope: Unity runtime state for `Novels`, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records; no source edits planned.
- Base: `3f594659235cdde212cd728dda1d6a472d6fec54` plus the current uncommitted TZM Choose implementation.
- Validation: persistent Editor process and loaded Novels project.
- Result: Unity Editor PID 61516 is open on the exact Novels project and foregrounded; the user must click Play because macOS blocks synthetic keystrokes without Accessibility permission.
