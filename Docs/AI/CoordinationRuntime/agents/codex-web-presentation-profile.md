# Agent: `codex-web-presentation-profile`

- Status: ready-with-limitations
- Task: Enable shared authored presentation in media-free web wrapper
- Scope: Packages/NovelsContentSdk/Runtime/Features/Location; Packages/NovelsContentSdk/Runtime/Features/Audio/Novels.Audio.asmdef; Packages/NovelsContentSdk/Runtime/Catalog/Novels.Catalog.asmdef; Packages/NovelsContentSdk/Editor/Novels.ContentSdk.Editor.asmdef; Projects/web-story-player; Docs/AI/architecture/UnityProjectContext.md; Docs/AI/memory/Architecture.md
- Base commit: `6e3dae6f84f2d541d4fdef81fe6c256cdd192005`.
- Requested UTC: `2026-09-11T16:28:56Z`.
- Contract: opt-in NOVELS_MEDIA_FREE excludes audio/video execution; NOVELS_STORY_PLAYER_ONLY excludes catalog and content-authoring Editor assembly. Only web wrapper opts in; native/story projects retain behavior and asset GUIDs. Existing web bundle prefab compatibility still needs live validation.
- Validation plan: scoped static checks, fresh wrapper and native compiles; no content rebuild/publication in this step because serialized native content schema and authoring behavior are unchanged.
- Unity resources: canonical projects /Users/iantonishin/Fork/SomeGame/Projects/web-story-player and /Users/iantonishin/Fork/SomeGame/Novels, one at a time; exact unity-project locks, runner-owned Editor/helper PID and logs captured by editor-gate.
- Validation: web fresh compile editor-gate-20260911T163124Z and native compile editor-gate-20260911T163219Z passed; scoped diff check/review. No Player/browser run for this profile yet.
- Pending: shared queue/composition, bundle prefab compatibility, IL2CPP dynamic type preservation, actual reading/save roundtrip. Source media and mobile behavior retained; no publication or commits.
