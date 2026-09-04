# Agent: `somegame-create-character-skill`

- Status: ready-for-integration
- Task: выделить создание персонажей SomeGame в отдельный discoverable skill и встроить его в end-to-end story workflow.
- Scope: `.agents/skills/somegame-create-character/**`, `.agents/skills/somegame-create-story/SKILL.md`, `.agents/skills/somegame-design-story/SKILL.md`, `.agents/skills/somegame-produce-story-art/SKILL.md`, `.agents/skills/somegame-produce-story-art/references/art-and-emotions.md`, `.agents/skills/somegame-author-story-content/SKILL.md`, `.agents/skills/somegame-accept-story/SKILL.md`, `Docs/AI/CoordinationRuntime/HANDOFF.md`, `Docs/AI/archive/reports/CoordinationHandoffHistory-2026-09-03.md`, own coordination records.
- Contract: новый skill владеет character brief, identity master, используемыми outfit/emotion/pose variants, runtime selectors и visual evidence; общий art skill принимает character handoff и не дублирует эту ответственность.
- Validation: skill quick validation, scoped diff review, `git diff --check`, repository docs check where applicable.
- Base: `1daa8129`
- Requested UTC: `2026-09-03T15:29:16Z`.
- Acquired UTC: `2026-09-03T15:37:39Z`.
- Completed UTC: `2026-09-03T15:42:23Z`.
- Result: создан и встроен `$somegame-create-character`; character-specific ответственность отделена от общего story-art этапа.
- Evidence: six skill quick validations passed; routing/TODO/reference audit, scoped `git diff --check`, and `Tools/somegame docs-check` passed.
- Pending: commit/publication not requested.
