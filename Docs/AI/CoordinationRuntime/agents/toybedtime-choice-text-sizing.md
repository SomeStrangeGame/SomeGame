# Agent: `toybedtime-choice-text-sizing`

- Status: ready-for-visual-validation
- Task: вернуть подписи и нормальную ширину illustrated choice-кнопок toybedtime.
- Scope: `Projects/novels-toybedtime/Assets/Presentation/bubble/screen-variant.prefab`, generated toybedtime editor release, own coordination records and handoff.
- Constraint: shared Bubble/runtime and other stories remain unchanged.
- Validation: prefab binding/layout audit, toybedtime validate/build, fresh Novels compile, portrait replay, scoped diff-check.
- Base: `1daa8129`.
- Requested UTC: `2026-09-03T16:03:30Z`.
- Acquired UTC: `2026-09-03T16:16:27Z`.
- Completed UTC: `2026-09-03T16:17:49Z`.
- Result: story-local illustrated choice template now uses its root Image as the sole background, keeps `ChoiceText` above it, and has a compact authored width of 300 px; shared Bubble/runtime remain unchanged.
- Validation: toybedtime validate/build, release-address audit, attached Novels compile and scoped diff-check passed.
- Pending: restart Play Mode and visually replay `s01e01.ink:43`.
