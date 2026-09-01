# Agent: wardrobe-reference-parity

- Status: ready-for-integration
- Task: привести функциональность гардероба к показанным TZM-референсам без изменения Ink.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListPresentation.cs`, `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/**`, `Packages/NovelsContentSdk/Runtime/Features/Character/{CharacterController,CharacterSpriteResolver}.cs`, `Novels/Assets/Novels/NovelRuntime.StoryQueue.cs`, `Novels/Assets/Novels/Save/{SaveSystem,SaveDataCodec}.cs` only if persistence requires it, focused tests, own coordination records and shared handoff.
- Contract: preserve story wardrobe semantics and existing saves; add reference-style icon tabs/header actions, full outfit confirmation/cancel transaction, and visible switching among wardrobe-capable characters. Empty categories remain hidden and current story location remains the background.
- Base commit: `8d3512787e6e`.
- Validation: scoped `git diff --check` passed; fresh Unity compile passed twice with no compiler errors. Full-tree `verify` stopped only on unrelated GPL `.meta` trailing whitespace. User portrait visual replay remains.
- Requested UTC: `2026-08-31T10:19:41Z`.
- Acquired UTC: `2026-08-31T11:55:03Z`.
