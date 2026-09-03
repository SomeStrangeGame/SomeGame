# Agent: `somegame-story-production-skills`

- Status: completed
- Task: extract story art production and story content authoring into independent skills, then route the story orchestrator through them.
- Scope: `.agents/skills/somegame-produce-story-art/**`, `.agents/skills/somegame-author-story-content/**`, `.agents/skills/somegame-create-story/**`, own coordination records, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and `Docs/AI/CoordinationRuntime/archive/reports/CoordinationHandoffHistory-2026-09-03.md` if rotation is required.
- Compatibility: preserve brief, historical-integrity, project creation, approval mode, catalog ordering, and publication boundaries; move reusable art/content responsibilities without duplicating canonical repository rules.
- Validation: skill quick validation or equivalent YAML/frontmatter checks, routing/reference audit, scoped whitespace checks, and `Tools/somegame docs-check`.
- Base: `a9ff1e1344599ecc16ff4df11409f479e6603085`
- Requested UTC: `2026-09-03T14:36:56Z`.
- Result: added `$somegame-produce-story-art` and `$somegame-author-story-content`; moved the art reference to its new owner and routed `$somegame-create-story` through project creation, art production, and playable-content authoring.
- Validation: frontmatter and UI YAML parsed, default prompts/names/descriptions/TODOs/references/routing checked, scoped whitespace checks and `Tools/somegame docs-check` passed. Bundled quick validator could not import PyYAML.
- Completed UTC: `2026-09-03T14:40:25Z`.
