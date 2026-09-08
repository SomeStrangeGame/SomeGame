# Agent: `catalog-media-priority`

- Status: ready-for-review
- Base commit: c5a431e30ff8
- Requested UTC: 2026-09-08T09:55:26Z
- Task: Correct card media priority to episode video, available episode cover, story video, story cover.
- Scope: Novels/Assets/Novels/CatalogFlow.cs; Novels/Assets/Editor/CatalogVideoValidation.cs; Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs (episode video Tooltip only); Docs/AI/guides/ContentPipeline.md; Docs/AI/memory/Architecture.md; own coordination and HANDOFF/history rotation.
- Contract: Use a successfully loaded own cover before inheriting story video. Existing metadata/prefab/player unchanged; optional video failure keeps the existing image fallback. No content/Player builds, asset assignments, saves, commits or publish.
- Validation: actual resolver priority matrix, loaded card cover fallback, Editor compile and live catalog. Existing Unity slot continued under the user's auto-approval instruction.
- Result: production CatalogFlow now resolves episode video first; a loaded own episode cover suppresses inherited video; only absent/failed own cover permits story video, otherwise story image. Updated Inspector Tooltip and canonical guide/memory without changing metadata fields, content outputs or prefab.
- Evidence: Unity recompile completed failed=false/errors=[]; editor-gate-20260908T094847Z-editor.log has new MEDIA_PRIORITY marker at 2645 (8 combinations plus whitespace/missing-image), EPISODE_COVERS at 3276 and LIVE at 3307. Scoped diff check passed. Editor remains playing at catalog start, helper stopped; saves/config/production media untouched. Device/remote video validation remains pending from the previous task; no content or Player builds needed for resolver/Tooltip-only changes.
