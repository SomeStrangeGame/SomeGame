# gpl-bundled-font

- Status: `ready-for-integration`
- Base: `4bfd64af41d3`
- Scope: `Projects/novels-gpl/Assets/Presentation/**`; GPL local content; own coordination records/handoff.
- Task: bundle a real font asset for the GPL setting screen.
- Validation: GPL validate/build, fresh Novels compile, manual runtime gate.

- Result: bundled Liberation Sans (350200 bytes) under GPL Presentation and
  restored both Text references to its project asset GUID. Release
  `7fe86a6182d53fee66b4750426a29259dfbe865cab804649c6eb89e8c14e7283` includes
  both font and setting prefab in chunk 0; validate/build and fresh compile passed.
- Pending: manual GPL card click to confirm rendered title and button.
