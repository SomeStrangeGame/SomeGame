# Agent: gpl-vera-pavel-face-left

- Status: ready-for-integration
- Task: correct Vera and Pavel so every current GPL supporting-character variant faces screen-left.
- Scope: `Projects/novels-gpl/Art/Vera/**`, `Projects/novels-gpl/Art/Pavel/**`, `Projects/novels-gpl/Assets/Characters/вера/**`, `Projects/novels-gpl/Assets/Characters/павел/**`, generated GPL editor content, own coordination/handoff.
- Constraints: deterministic whole-image horizontal mirroring only; mirror all technical layers consistently; preserve alpha, canvas, identity, pose, clothing, GUID/meta and runtime registration; do not touch Lea, Mark, Sigrid, code, shared runtime, other stories or foreign dirty work.
- Requested UTC: `2026-09-01T15:31:24Z`.
- Result: every Vera and Pavel source/proof/runtime PNG was mirrored
  deterministically; ordered contact-sheet cells retained their original order.
- Validation: key full-body and layered contacts visually face screen-left;
  runtime PNGs remain 1024x1536 RGBA with transparent corners; Pavel
  source/runtime byte parity passed; GPL validation and editor build passed;
  own Markdown diff-check passed (pre-existing Unity `.meta` whitespace was
  preserved and excluded from this PNG-only scope).
- Pending: bounded in-game visual gate with the combined GPL episode smoke.
