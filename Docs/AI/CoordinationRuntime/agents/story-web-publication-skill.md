# Agent: `story-web-publication-skill`

- Status: completed
- Result: `somegame-create-story` now requires a story-owned website reading preview and routes deployment through a dedicated reference and `$somegame-release-app`. The reference distinguishes website preview from generated catalog preview, records the Remote manifest/catalog compatibility rule, requires immutable story upload before manifest visibility, preserves existing channel stories, and requires public/player verification.
- Validation: scoped `git diff --check` PASS; project `docs-check` PASS after compact handoff rotation. System `quick_validate.py` could not import PyYAML in available Python environments; project docs-check independently validates project-local skill frontmatter/contracts.
- Task: Add required web preview and compatible Remote content publication guidance to story creation skill
- Scope: .agents/skills/somegame-create-story/SKILL.md; .agents/skills/somegame-create-story/references/web-preview-publication.md; own coordination and compact handoff/archive only. No story/runtime/Website changes, build, server publication or Git mutation.
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T15:06:26Z`.

- Completed UTC: `2026-09-09T15:29:32Z`.
- Validation: finish-task passed; logs: 1; pending: none.
