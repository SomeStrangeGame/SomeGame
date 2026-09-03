# Agent: `somegame-story-design-acceptance-skills`

- Status: completed
- Task: extract story design and final acceptance into independent skills while keeping historical integrity conditional inside the story orchestrator.
- Scope: `.agents/skills/somegame-design-story/**`, `.agents/skills/somegame-accept-story/**`, `.agents/skills/somegame-create-story/**`, own coordination records, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and `Docs/AI/CoordinationRuntime/archive/reports/CoordinationHandoffHistory-2026-09-03.md` for required rotation.
- Compatibility: preserve mandatory author genre, factual-basis separation, guided/auto-approve checkpoints, existing production-skill ordering, catalog position, manual gates, and publication boundaries; do not create a historical-research skill.
- Validation: skill quick validation or equivalent YAML/frontmatter checks, routing/reference audit, scoped whitespace checks, and `Tools/somegame docs-check`.
- Base: `a9ff1e1344599ecc16ff4df11409f479e6603085`
- Requested UTC: `2026-09-03T14:43:09Z`.
- Result: added `$somegame-design-story` and `$somegame-accept-story`, moved their references to the owning skills, and routed `$somegame-create-story` through the complete lifecycle while retaining conditional historical integrity internally.
- Validation: frontmatter and UI YAML parsed; default prompts, names, descriptions, TODOs, references, routing, and absence of a historical-research skill checked; scoped whitespace checks and `Tools/somegame docs-check` passed. Bundled quick validator could not import PyYAML.
- Completed UTC: `2026-09-03T14:45:52Z`.
