# maincharacter-default-view

- Status: `ready-for-integration`
- Base: `4bfd64af41d3`
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterController.cs`; own coordination records/handoff.
- Task: initialize the main character base view for stories without an appearance-selection command.
- Validation: fresh Novels compile and GPL runtime replay without `fallback.used`.

- Result: `CharacterController` initializes `_mainCharacterView` to the profile's
  `ViewRoot`; stories without a starting appearance choice now resolve
  `maincharacter/view/main.png` instead of an address with a null view segment.
- Evidence: failed GPL run emitted `fallback.used` with
  `required_character_assets_missing`; release contains body, default clothes and
  `dazed`, leaving the uninitialized view as the first invalid state.
- Validation: clean Novels restart and compile passed. Pending manual GPL replay.
