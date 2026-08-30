# Agent: tzm-wardrobe-location-background

- Status: waiting-user-validation
- Task: оставить текущую сюжетную локацию фоном гардероба ТЗМ.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`; focused tests/Unity compile; own coordination records and shared handoff.
- Base commit: `4bfd64af41d3`.
- Validation: scoped diff, tooling tests, fail-closed Unity compile; visual verification by user.
- Result: removed the separately created fullscreen wardrobe canvas and glow;
  current story location now remains visible. Tooling/helper tests 43/43 passed.
  An attach attempt correctly failed on missing stale Pipeline port; a fresh
  Editor launch then compiled with `compilerErrors: []` and remains open.
