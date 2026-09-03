# Agent: `choice-horizontal-layout`

- Status: ready-with-limitations
- Task: fix choice buttons shifted and clipped to the right in TZM and fallback bubbles.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs`, narrow validation, own coordination records and handoff.
- Contract: runtime choice buttons are centered relative to dialogue text and remain below it.
- Boundaries: preserve all existing dirty bubble work and unrelated changes.
- Base commit: `7e8cf0b30f0fa2c754dbb09524ced4a2f77ef584` on canonical branch `codex/episode-launch-actions`; no worktree.
- Evidence: user screenshots at TZM `s01e01.ink:136` and Sobibor `sobibor.ink:163`; buttons retain prefab X offset after `ignoreLayout`.
- Request: `20260903T093839Z-choice-horizontal-layout`.
- Requested UTC: `2026-09-03T09:38:39Z`
- Acquired UTC: `2026-09-03T09:41:00Z`
- Result: active choice buttons now copy the dialogue text's horizontal anchors and pivot and reset their X position to the dialogue center before applying the calculated below-text Y position.
- Validation: scoped `git diff --check` passed; fresh Unity 6000.3.11f1 Novels compile passed with zero compiler errors. The first gate attempt hit an MCP startup timeout; the immediate bounded retry succeeded.
- Limitation: user will visually replay TZM `s01e01.ink:136` and Sobibor `sobibor.ink:163`.
