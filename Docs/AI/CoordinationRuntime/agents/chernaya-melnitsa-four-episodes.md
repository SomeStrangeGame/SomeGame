# Agent: `chernaya-melnitsa-four-episodes`

- Status: ready-for-final-validation
- Task: Convert the approved four-episode Chyornaya Melnitsa revision into playable story content, archive prior revision, add episode covers, validate, and integrate to main
- Scope: Projects/novels-chernaya-melnitsa/Assets/Ink,Projects/novels-chernaya-melnitsa/Assets/chernaya-melnitsa.asset,Projects/novels-chernaya-melnitsa/Config/card.json,Projects/novels-chernaya-melnitsa/Config/EpisodeCovers,Projects/novels-chernaya-melnitsa/Art,Projects/novels-chernaya-melnitsa/archive,Docs/AI/CoordinationRuntime/HANDOFF.md
- Base commit: `eb7655d32ab6765bd96fcabb5a728a9a72ce407c`.
- Requested UTC: `2026-09-10T13:29:06Z`.

- Candidate prepared UTC: `2026-09-10T14:09:00Z`.
- Result: four-episode Ink candidate, content definition v2, card copy, three
  additional episode covers, and immutable source/archive manifest are present.
- Static validation: literary verifier passed all 72 routes, five decisions and
  three endings; archive manifest verified six SHA-256 hashes; four episode files,
  12 choice alternatives, JSON parsing, residual-Markdown scan and scoped
  `git diff --check` passed.
- Pending: real Ink compilation, Unity import-generated metadata, content bundle
  build and presentation checks require a fresh explicit human-approved final slot.
- Yield: own checkout lock/request released while awaiting that approval.
