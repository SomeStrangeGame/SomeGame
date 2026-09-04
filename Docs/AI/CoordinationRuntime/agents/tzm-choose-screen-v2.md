# Agent: `tzm-choose-screen-v2`

- Status: completed
- Task: add canonical story-local Choose prefab support and author the TZM Choose screen from the approved reference while preserving shared fallback behavior.
- Scope: `Packages/NovelsContentSdk/Runtime/ContentAddressing/ContentAddressConvention.cs`, `Packages/NovelsContentSdk/Runtime/ContentAddressing/ContentAddresses.cs`, `Packages/NovelsContentSdk/Runtime/Features/Choose/ChooseController.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Novels/Assets/Novels/EpisodeAssetLoader.cs`, `Novels/Assets/Novels/NovelRuntime.Presentation.cs`, `Projects/novels-tzm/Assets/Presentation/choose/**`, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records.
- Base: `3f594659235cdde212cd728dda1d6a472d6fec54`.
- Validation: scoped static checks, TZM editor content build, Novels Unity compile, and portrait multi-object visual review.
- Result: canonical story-local Choose prefab loading and the first TZM authored variant are implemented. TZM editor content build and fresh Novels compile passed; portrait runtime review remains.
