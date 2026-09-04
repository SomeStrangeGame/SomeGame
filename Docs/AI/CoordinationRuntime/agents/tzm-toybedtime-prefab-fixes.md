# Agent: `tzm-toybedtime-prefab-fixes`

- Status: ready-for-visual-validation
- Task: исправить унаследованный TZM character offset и фактический fallback Bubble в toybedtime.
- Scope: `Projects/novels-tzm/Assets/Presentation/character/screen-variant.prefab`, `Projects/novels-toybedtime/Assets/Presentation/bubble/screen.prefab` rename to `screen-variant.prefab` with preserved meta/GUID, generated TZM/toybedtime releases, own coordination records and related handoff lines.
- Evidence: shared character Viewport is `Y=-220`, TZM variant has no Viewport override; episode runtime requests Bubble `screen-variant`, while toybedtime release only contains `screen.prefab`.
- Validation: Unity-authored TZM override, preserved toybedtime prefab identity, validate/build both stories, release address audits, Novels compile, portrait replay, scoped diff-check.
- Base: `1daa8129`
- Requested UTC: `2026-09-03T15:40:02Z`.
- Acquired UTC: `2026-09-03T15:54:10Z`.
- Completed UTC: `2026-09-03T16:01:01Z`.
- Result: TZM locally overrides the inherited character Viewport offset to `Y=0`; toybedtime's authored Bubble was renamed to the runtime contract `screen-variant.prefab` with GUID `f8ebfd5654c6d419cb091e741234bc5c` preserved.
- Validation: `validate toybedtime`, `validate tzm`, editor builds for both stories, release-address audits and fresh Novels compile passed; scoped diff-check clean.
- Pending: user portrait replay for the TZM character position and toybedtime illustrated choice buttons.
