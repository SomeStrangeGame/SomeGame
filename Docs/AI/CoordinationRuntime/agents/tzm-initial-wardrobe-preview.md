# Agent: tzm-initial-wardrobe-preview

- Status: waiting-user-validation
- Task: убрать fallback-силуэт при первом открытии стартового гардероба.
- Scope: OptionList presentation/screen and WardrobeController; focused tests/Unity compile; own coordination records and shared handoff.
- Base commit: `4bfd64af41d3`.
- Root cause: initial presentation suppresses preview together with tab changes, leaving main-character appearance unset until explicit interaction.
- Validation: scoped diff, tooling tests, fail-closed Unity compile; visual verification by user.
- Result: added an explicit initial-preview presentation flag. The first
  scripted wardrobe page enables it; tab activation and free wardrobe do not.
  Tooling/helper tests 43/43 passed. Attach compile passed but correctly found
  the old reproduction fallback marker; a fresh Editor gate then passed with
  `compilerErrors: []` and remains open.
