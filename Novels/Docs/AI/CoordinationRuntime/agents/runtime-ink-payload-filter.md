# Agent: runtime-ink-payload-filter

- Status: completed
- Branch: `experiment/story-preview-streaming`
- Worktree: `/Users/iantonishin/Documents/Codex/SomeGame-story-preview-experiment`
- Scope: `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`, TZM generated release outputs, own coordination records
- Contract: keep authoring files in source; omit `.ink` from releases; omit `<story>.json` only when identical sibling `<story>.ink.json` exists; always retain source maps
- Result: commit `68351108`; TZM release retains only `tzm.ink.json` and `tzm.ink.json.source-map.json` under `noveltexts/`
- Result: chunk-0 text payload reduced by 963,403 bytes (about 0.92 MiB); all eight source `.ink` files remain in the authoring project
- Updated UTC: 2026-08-26T14:10:00Z
