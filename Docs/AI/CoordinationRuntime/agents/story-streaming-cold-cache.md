# Agent: story-streaming-cold-cache

- Status: completed
- Task: make experimental Cold App clear cached remote payloads before restart
- Branch: `experiment/story-preview-streaming`
- Worktree: `/Users/iantonishin/Documents/Codex/SomeGame-story-preview-experiment`
- Scope: `Packages/Cache/Entity.cs`, `Novels/Assets/Novels/EntryPoint.cs`
- Expected result: Cold App preserves saves but removes `RemoteContent` and staging payloads
- Result: committed as `372f7423`; Cold App clears remote payload and staging caches while preserving saves
- Updated UTC: 2026-08-25T10:45:00Z
