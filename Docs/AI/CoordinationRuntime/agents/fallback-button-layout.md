# Agent: `fallback-button-layout`

- Status: ready-with-limitations
- Task: prevent fallback choice buttons from being repositioned over dialogue after the next Unity layout pass.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs`, narrow validation, own coordination records and handoff.
- Contract: runtime choice buttons remain below the rendered dialogue text with stable spacing across subsequent layout passes.
- Boundaries: preserve all existing dirty bubble work and unrelated changes.
- Base commit: `7e8cf0b30f0fa2c754dbb09524ced4a2f77ef584` on canonical branch `codex/episode-launch-actions`; no worktree.
- Evidence: user screenshot Sobibor Ink line 163 after the first fix; root `VerticalLayoutGroup` still controls instantiated buttons after their manual position is set.
- Planned fix: mark pooled choice buttons `ignoreLayout` before positioning, then compile and replay the reported frame.
- Request: `20260903T090018Z-fallback-button-layout`.
- Requested UTC: `2026-09-03T09:00:18Z`
- Result: every instantiated pooled choice button now receives a runtime `LayoutElement` with `ignoreLayout = true`; subsequent `VerticalLayoutGroup` passes cannot overwrite the explicit below-text position.
- Validation: scoped `git diff --check` passed; fresh Unity 6000.3.11f1 Novels compile passed with zero compiler errors. The first helper attempt timed out on a stale MCP socket; a bounded retry succeeded. Direct generated-csproj fallback is not authoritative and failed because Unity's generated project references an obsolete package mirror path.
- Limitation: user will perform the requested visual replay of Sobibor line 163.
