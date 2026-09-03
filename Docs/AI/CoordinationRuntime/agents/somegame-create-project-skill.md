# Agent: `somegame-create-project-skill`

- Status: completed
- Task: create a reusable SomeGame Unity-project creation skill with mandatory Unity MCP setup and make story creation delegate project scaffolding to it.
- Scope: `.agents/skills/somegame-create-unity-project/**`, `.agents/skills/somegame-create-story/SKILL.md`, `.agents/skills/somegame-create-story/agents/openai.yaml`, own coordination records, `Docs/AI/CoordinationRuntime/HANDOFF.md`, and `Docs/AI/CoordinationRuntime/archive/reports/CoordinationHandoffHistory-2026-09-03.md` for required rotation.
- Compatibility: preserve the story brief and authoring workflow; move only reusable project scaffolding and MCP readiness into the new prerequisite contract.
- Validation: skill quick validation, YAML/frontmatter checks, scoped link/reference checks, `Tools/somegame docs-check`, and scoped diff review.
- Base: `a9ff1e134459`
- Requested UTC: `2026-09-03T14:27:03Z`.
- Result: added `$somegame-create-unity-project` with mandatory per-project Official Unity MCP configuration and live/restart proof; `$somegame-create-story` delegates scaffolding to it before project-bound authoring.
- Validation: frontmatter and `openai.yaml` parsed; names/default prompts/TODOs/references checked; scoped whitespace checks and `Tools/somegame docs-check` passed. Bundled quick validator could not import PyYAML.
- Completed UTC: `2026-09-03T14:32:28Z`.
