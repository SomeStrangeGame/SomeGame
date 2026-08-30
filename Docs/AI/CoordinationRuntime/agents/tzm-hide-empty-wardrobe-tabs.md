# Agent: tzm-hide-empty-wardrobe-tabs

- Status: waiting-user-validation
- Task: скрыть вкладку аксессуаров и другие категории без вариантов в scripted wardrobe.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`; focused tests/Unity compile; own coordination records and shared handoff.
- Base commit: `4bfd64af41d3`.
- Validation: static diff, relevant tests, fail-closed Unity compile; visual verification by user.
- Result: tabs omitted from `InteractableTabs` are hidden; a null availability
  list preserves all tabs, including temporarily locked categories. Tooling and
  helper tests 43/43 passed; live Unity compile passed without reported errors.
