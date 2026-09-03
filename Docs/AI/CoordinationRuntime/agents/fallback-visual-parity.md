# Agent: `fallback-visual-parity`

- Status: ready-with-limitations
- Task: separate fallback bubble text from choice buttons and lower fallback characters to GPL parity.
- Scope: existing ready diff in `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs` and `Packages/NovelsContentSdk/BaseUI/Base/bubble/screen.prefab`; `Packages/NovelsContentSdk/BaseUI/Base/character/screen.prefab`; narrow tests; own coordination records and handoff.
- Contract: dialogue body and any choice buttons never overlap in portrait layout; shared fallback character viewport uses the proven GPL vertical offset; GPL remains visually unchanged through its matching variant override.
- Boundaries: preserve the existing `tzm-responsive-bubble` diff, do not edit Ink, character art, story-specific assets, or unrelated dirty files.
- Base commit: `7e8cf0b30f0fa2c754dbb09524ced4a2f77ef584` on canonical branch `codex/episode-launch-actions`; no worktree.
- Evidence: user screenshots at Sobibor Ink lines 75 and 91; GPL character variant overrides base Viewport `m_AnchoredPosition.y` and `m_LocalPosition.y` to `-220`, while shared fallback uses `0`.
- Planned validation: scoped prefab audit/diff-check, fresh Novels compile, Sobibor validation/build if available, portrait visual replay.
- Request: `20260903T082000Z-fallback-visual-parity`.
- Result: the existing responsive text-height work now places every active choice button below the rendered dialogue text with a 12 px gap after layout; multiple choices stack with the same gap. The shared character Viewport now inherits GPL's `-220` vertical offset. GPL's matching override remains unchanged.
- Validation: scoped prefab/runtime `git diff --check` passed; exact Viewport fileID audit passed; fresh Unity 6000.3.11f1 Novels compile passed with no compiler errors after one transient MCP transport retry.
- Limitation: portrait Play Mode replay of Sobibor lines 75 and 91 remains for user visual confirmation.
- Heartbeat UTC: `2026-09-03T08:38:07Z`
