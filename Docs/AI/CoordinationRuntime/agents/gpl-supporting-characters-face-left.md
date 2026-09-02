# Agent: gpl-supporting-characters-face-left

- Status: ready-for-integration
- Task: enforce the current GPL composition rule that every character except main hero Lea faces screen-left, by correcting source/proof/runtime art rather than code.
- Scope: `Projects/novels-gpl/Art/**` and `Projects/novels-gpl/Assets/Characters/{вера,марк,павел,сигрид}/**` where orientation is wrong; `Docs/AI/rules/CharacterLayeringRules.md`; generated GPL content; own coordination/handoff.
- Constraints: preserve alpha, canvas, identity, pose, clothing and Unity registration; use identity-preserving source-art edits only where a frontal master cannot be corrected by mirroring; do not touch Lea, shared runtime, other stories or foreign dirty work.
- Requested UTC: `2026-09-01T14:48:32Z`.
- Result: GPL-specific screen-left rule recorded; Sigrid `main`, `alarmed` and
  `frost_double` source, proof and runtime PNGs mirrored deterministically with
  unchanged 1024x1536 registration and transparent corners.
- Validation: visual full/face contact review passed; source/runtime byte parity
  passed; `novels-content validate gpl` and `build gpl editor` passed.
- Pending: bounded in-game visual gate when the broader GPL episode 4-5 scope is integrated.
