# Android choice acceptance — 2026-09-10 — v001

Story: `les-zabyvshiy-tropy`

Candidate APK SHA-256: `3cf56aefc377f9f6e5078dcd26ba080993d64f396d12765af78a1a6315ab8d05`

Run `7652336f15334178a56e6700e5e8026a` reached the first choice in `s01e01` through the real catalog flow. Runtime marker sequence 46 reported `choiceCount=3`, but the three settled choice cards contained no readable labels. The run was stopped rather than selecting an unknown branch.

Evidence:

- `Art/Acceptance/20260910-route-matrix/route-a/choice-labels-missing.png`
- `Art/Acceptance/20260910-route-matrix/route-a/runtime-logcat.txt`

Result: `blocked`. Return the story-local Choice/Bubble presentation to its production owner, rebuild a fresh APK, then restart the three-route matrix and save/resume checks.

Archive adoption gap: earlier creative drafts, prompts, candidate variants, feedback, approvals and generation metadata were not retained in a story archive before this contract was adopted. They are unavailable and have not been recreated or backdated.
