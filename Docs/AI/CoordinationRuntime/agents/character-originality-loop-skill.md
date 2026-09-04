# Agent: `character-originality-loop-skill`

- Status: ready-for-integration
- Task: дополнить все downstream story-production skills согласованными originality gates после сюжетного originality review.
- Scope: `.agents/skills/somegame-create-character/SKILL.md`, `.agents/skills/somegame-create-character/references/character-package.md`, `.agents/skills/somegame-produce-story-art/SKILL.md`, `.agents/skills/somegame-author-story-content/SKILL.md`, `.agents/skills/somegame-accept-story/SKILL.md`, `.agents/skills/somegame-accept-story/references/acceptance-checklist.md`, `.agents/skills/somegame-create-story/SKILL.md`, own coordination records and handoff/archive only if required by line limit.
- Contract: максимум пять visual reviews персонажей, пять visual reviews остального art и пять text reviews полного Ink; ранний pass без material risk; после пятой неустранённой проверки downstream workflow blocked с источниками/change log. Acceptance повторно не ищет, а fail-closed проверяет evidence всех gates; orchestrator запрещает пропуск этапов.
- Validation: six skill quick validations, cross-skill routing/requirement audit, scoped `git diff --check`, `Tools/somegame docs-check`.
- Base: `1daa8129`
- Requested UTC: `2026-09-03T16:04:50Z`.
- Scope expanded UTC: `2026-09-03T16:06:40Z` by explicit user request within the same queued task.
- Acquired UTC: `2026-09-03T16:18:42Z`.
- Completed UTC: `2026-09-03T16:21:10Z`.
- Result: character, non-character art and full-text Ink получили bounded five-review originality gates; story orchestrator требует их pass, acceptance fail-closed проверяет evidence и stale changes.
- Evidence: six skill quick validations, originality/routing audit, scoped `git diff --check`, and `Tools/somegame docs-check` passed.
- Pending: commit/publication not requested.
