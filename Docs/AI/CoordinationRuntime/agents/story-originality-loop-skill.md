# Agent: `story-originality-loop-skill`

- Status: ready-for-integration
- Task: дополнить `$somegame-design-story` пятиитерационным циклом черновик → проверка оригинальности → исправление риска.
- Scope: `.agents/skills/somegame-design-story/SKILL.md`, `.agents/skills/somegame-design-story/references/story-design.md`, own coordination records and handoff/archive only if required by line limit.
- Contract: максимум пять проверок; ранний выход при отсутствии существенного риска; после пятой неустранённой проверки — blocked handoff пользователю с совпадениями, источниками и историей исправлений, без ложного юридического заключения.
- Validation: skill quick validation, routing/wording audit, scoped `git diff --check`, `Tools/somegame docs-check`.
- Base: `1daa8129`
- Requested UTC: `2026-09-03T15:52:47Z`.
- Acquired UTC: `2026-09-03T16:02:04Z`.
- Completed UTC: `2026-09-03T16:03:26Z`.
- Result: добавлен обязательный originality gate максимум из пяти review-итераций с ранним pass и downstream block после пятого неустранённого material finding.
- Evidence: skill quick validation, risk/iteration/routing audit, scoped `git diff --check`, and `Tools/somegame docs-check` passed.
- Pending: commit/publication not requested.
