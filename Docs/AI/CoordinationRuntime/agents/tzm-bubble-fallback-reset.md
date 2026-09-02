# Agent: `tzm-bubble-fallback-reset`

- Status: ready-for-manual-validation
- Task: исключить story-specific TZM bubble prefab из editor release, чтобы проверить общий authored fallback bubble.
- Scope: `Projects/novels-tzm/Assets/tzm.asset`, generated TZM editor release, Novels Editor lifecycle, own coordination records and shared handoff.
- Contract: TZM bubble prefab и sprites сохранить в source; Ink, runtime и другие presentation variants не менять; существующий dirty tree не затрагивать.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: 2026-09-01T11:15:49Z
- Completed UTC: 2026-09-01T11:30:13Z
- Evidence: current Mac release contains `story/presentation/bubble/screen-variant.prefab`; its GUID `ea739ade0e5234bc48b0405870a1f9a5` is assigned to chunk 0.
- Result: moved the TZM bubble prefab plus six local PNG dependencies from chunk 0 to `authoring-unused`; source files remain intact. The rebuilt TZM Mac release now has no `presentation/bubble` address and must use the common game fallback bubble.
- Validation: TZM validate/build editor passed; exact release and build-log audit found no custom bubble; fresh Novels compile passed with `compilerErrors: []`. Unity PID `59162` remains open for manual replay.
