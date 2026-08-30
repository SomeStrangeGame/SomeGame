# Agent: wardrobe-tab-preview

- Status: waiting-user-validation
- Task: убрать резкую автоматическую смену образа при переключении вкладок гардероба.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`; focused tests/Unity compile; own coordination records and shared handoff.
- Base commit: `4bfd64af41d3`.
- Validation: scoped diff, tooling tests, fail-closed Unity compile; visual verification by user.
- Result: initial selection updates highlight/label without preview; explicit
  card tap, including the already highlighted card, and carousel movement still
  preview. Tooling/helper tests 43/43 and live Unity compile passed.
