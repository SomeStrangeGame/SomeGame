# Agent: wardrobe-reference-sizing

- Status: complete
- Task: correct fallback wardrobe internal proportions against the supplied reference and produce a visual overlay of reference versus the resulting runtime frame.
- Scope: `Packages/NovelsContentSdk/Editor/OptionListFallbackPrefabBuilder.cs`, shared `OptionListScreen.prefab`, `OptionListScreen.cs`, bounded Unity helper capture/play allowlist, Novels Unity validation, comparison image artifact, own coordination records and handoff.
- Contract: keep outer panel near its already matching 80% × 33% proportions; enlarge internal controls, reduce center whitespace, lower top controls, preserve hidden empty tabs and all behavior.
- Base commit: `8f89f27b0f01` plus preserved shared dirty tree.
- Requested UTC: `2026-09-01T07:31:00Z`.
- Completed UTC: `2026-09-01T08:47:08Z`.
- Result: rebuilt the authored sprite-less fallback wardrobe prefab with reference-matched panel width, tab height, selection/arrow placement and confirm-button geometry. Runtime only keeps the confirm/cancel arrangement synchronized with those authored dimensions; wardrobe selection and visibility behavior were preserved.
- Evidence: batch prefab rebuild exited successfully; fresh `Novels` compile passed with zero compiler errors (`editor-gate-20260901T084622Z.log`). A real 640x1114 Play Mode frame was captured and compared against the normalized supplied reference; panel edges and main vertical block boundaries align within a few pixels.
- Comparison artifacts: `wardrobe-result-final.png` and `wardrobe-reference-overlay.png` in the task visualization directory.
