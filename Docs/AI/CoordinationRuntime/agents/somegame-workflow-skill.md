# Agent: `somegame-workflow-skill`

- Status: completed
- Task: создать проектный Codex skill `somegame-workflow`.
- Scope: `.agents/skills/somegame-workflow/**`, собственные coordination records и shared handoff.
- Contract: компактно маршрутизировать задачи к `Tools/somegame`, `Docs/AI` и применимым Unity skills; не дублировать нормативную документацию и не включать build-server workflow.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` с сохранением существующего dirty tree.
- Requested UTC: 2026-09-01T14:40:13Z
- Acquired UTC: 2026-09-01T14:51:45Z
- Validation: skill quick validation, scoped content review and `git diff --check`.
- Result: создан компактный project-local skill с автоматической маршрутизацией задач к каноническим context, coordination и validation workflows; build-server инструкции отсутствуют.
- Validation result: frontmatter/name/TODO manual validator passed; scoped `git diff --check` passed. Bundled `quick_validate.py` не запустился из-за отсутствующего в обоих Python runtimes модуля `yaml`; эквивалентные проверки выполнены через Ruby YAML.
