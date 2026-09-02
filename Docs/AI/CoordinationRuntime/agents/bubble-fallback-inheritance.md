# Agent: `bubble-fallback-inheritance`

- Status: ready-for-manual-validation
- Task: привести наследование bubble к схеме общего рабочего fallback, от которого напрямую наследуется TZM.
- Scope: `Novels/Assets/Novels/Novels.unity`, `Novels/Assets/Novels/Fallbacks/EpisodeUI/bubble/screen-variant.prefab*`, own coordination records and shared handoff.
- Contract: общий SDK prefab и TZM style variant не менять; направить game runtime прямо на общий fallback и удалить только ставшую лишней game wrapper-variant; прочий dirty tree сохранить.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: 2026-09-01T11:57:46Z
- Completed UTC: 2026-09-01T12:00:33Z
- Result: `Novels.unity` references the shared package bubble directly; the redundant game-only prefab variant and its meta were removed. TZM remains a direct variant of the same shared prefab.
- Validation: old wrapper GUID has no remaining references; shared component fileID and TZM parent GUID match; scoped `git diff --check` passed; live Novels Editor compile passed with no compiler errors. Manual Play Mode visual gate remains.
