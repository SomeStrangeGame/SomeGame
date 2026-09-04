# Agent: `tzm-choose-bundle-fix`

- Status: completed
- Task: fix the TZM Choose prefab falling back because its GUID is absent from the explicit authoring chunk manifest.
- Scope: `Projects/novels-tzm/Assets/tzm.asset`, TZM content build/runtime state, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records.
- Base: `3f594659235cdde212cd728dda1d6a472d6fec54` plus current uncommitted TZM Choose implementation.
- Validation: rebuilt TZM editor bundle contains the Choose prefab and fresh Novels runtime uses the story-local screen.
- Result: confirmed missing authoring-chunk registration as the fallback cause, added the Choose prefab GUID, rebuilt TZM editor content successfully, and reopened Novels for visual confirmation.
