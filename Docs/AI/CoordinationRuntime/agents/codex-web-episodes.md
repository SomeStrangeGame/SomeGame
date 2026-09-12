# Agent: `codex-web-episodes`

- Status: paused
- Task: Shared episode progression and durable WebGL next-episode launch
- Scope: Novels/Assets/Novels/NovelProgress.cs; Novels/Assets/Novels/NovelProgress.cs.meta; Novels/Assets/Novels/Novels.asmdef; Packages/NovelsStoryRuntime/Progress; Packages/NovelsStoryRuntime/Progress.meta; Packages/NovelsStoryRuntime/README.md; Projects/web-story-player/Assets/WebPlayer/Runtime; Projects/web-story-player/README.md; Projects/web-story-player/Tools/serve-smoke.py; Docs/AI/memory/Architecture.md; Docs/AI/architecture/UnityProjectContext.md; Docs/AI/CoordinationRuntime/HANDOFF.md
- Base commit: `6e3dae6f84f2d541d4fdef81fe6c256cdd192005`.
- Requested UTC: `2026-09-11T17:10:15Z`.
- Contract: move NovelProgress with original GUID/native NPR1 format and Cache constructors to Novels.Progress; add strict callback backend for web. IndexedDB browser envelope atomically holds native boundary blob plus completion markers. Web Launch resumes latest unlocked episode; NextEpisode is allowed only after durable completion. Main and web compile before local browser smoke; no content-authoring changes, publication or commits.
- Validation resources: shared-sdk, unity-project:novels (/Users/iantonishin/Fork/SomeGame/Novels), unity-project:projects-web-story-player (/Users/iantonishin/Fork/SomeGame/Projects/web-story-player), build-output:projects-web-story-player-build-webgl.
- Result: shared/native/web compile and development WebGL build passed; premature NextEpisode rejection and safe return event passed in-app. Boundary smoke blocked by existing story end-marker mismatch, NOT a validated multi-episode pass. Exact evidence and next step in HANDOFF; source story and saves preserved. Awaiting approval for story-definition correction/local rebuild; no publication or commits.
