# Agent: `remove-story-start-buttons`

- Status: ready-for-integration
- Task: remove redundant one-option start choices from every affected story episode and prevent new stories from adding them.
- Scope: the 28 affected source `.ink` files under `Projects/novels-{deti,devyataev,gpl,mamkin,maresyev,mmm,okt,poletaev,sobibor,toybedtime,tzm,zdm,zmt}`, `Docs/AI/guides/ContentAuthoringGuide.md`, `.agents/skills/somegame-author-story-content/SKILL.md`, `.agents/skills/somegame-create-story/SKILL.md`, generated compiled story/source-map/build evidence, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and own coordination records.
- Base: `f691f613`.
- Result: removed 28 empty start choices, rebuilt all affected compiled Ink/source maps, and passed all 13 scoped content gates plus corpus and whitespace checks.
