# Agent: `tzm-choose-screen`

- Status: queued
- Task: simplify and restructure the shared sprite-free grayscale Choose fallback using the approved reference hierarchy, preserving multi-object carousel selection.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/OptionListScreen.prefab`, cleanup of the untracked `Projects/novels-tzm/Assets/Presentation/choose.meta` left by the abandoned local-prefab experiment, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records.
- Base: `f691f613`.
- Validation: scoped diff checks, Unity compile, and portrait visual evidence for both one-object and multi-object states if the active Editor permits it.
