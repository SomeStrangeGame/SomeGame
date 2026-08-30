# gpl-bubble-presentation

- Status: `ready-for-integration`
- Base: `4bfd64af41d3`
- Scope: `Projects/novels-gpl/Assets/Presentation/bubble/**`; GPL local content; own coordination records/handoff.
- Task: replace the generic fallback bubble with a story bubble that lays out long choices correctly.
- Validation: GPL validate/build, fresh Novels compile, portrait choice visual replay.

- Result: copied the proven story bubble prefab and its six sprite dependencies
  into GPL Presentation. Release
  `93d9f30a297a55f1a35022d1b7ffbc299cc49bbaf4316509d3a5b9f32d547078`
  contains `bubble/screen-variant.prefab` and all referenced sprites.
- Validation: GPL validate/build and clean Novels restart/compile passed.
- Pending: replay the three-choice portrait frame and confirm no overlap.
