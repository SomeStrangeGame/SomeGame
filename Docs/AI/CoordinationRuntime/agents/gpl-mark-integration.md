# Agent: gpl-mark-integration

- Status: ready-for-integration
- Task: импортировать восемь утверждённых цельных вариантов Марка в GPL episode 1.
- Scope: `Projects/novels-gpl/Assets/Characters/марк/**`; Mark default outfit in `Projects/novels-gpl/Assets/gpl.asset`; точечные Mark presentation selectors in `Projects/novels-gpl/Assets/Ink/s01e01.ink` and compiler-owned Ink outputs; own coordination records; compact shared handoff.
- Base commit: `4bfd64af41d3`.
- Contract: station and polar outfits remain whole variants; no cross-image head/body layering; episode selectors map only to authored story beats.
- Validation: 1024×1536 RGBA/source hashes, GPL validation/editor build, release addresses, compiled Ink selectors, scoped diff and docs checks.
- Result: imported seven station variants under `view/whole/station` and the polar outfit under `view/whole/polar`; Mark defaults to `station`; episode 1 uses `crowbar`, `hands_raised`, `main`, `avoids_gaze`, `suspicious`, `demanding` and `pale_fear` at authored beats.
- Evidence: all eight sources are 1024×1536 RGBA and imported SHA-256 values match; `novels-content validate gpl` and `build gpl editor` passed; Mac release lists all eight addresses; compiled Ink contains all seven station selectors; scoped `git diff --check` passed.
- Pending: bounded in-game visual gate after the GPL/Novels Editor is intentionally opened.
