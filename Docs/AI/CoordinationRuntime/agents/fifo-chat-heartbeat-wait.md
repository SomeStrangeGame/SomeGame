# Agent: `fifo-chat-heartbeat-wait`

- Status: completed
- Task: заменить блокирующее ожидание FIFO на минутные heartbeat-пробуждения текущего чата.
- Scope: `Docs/AI/rules/UnityConcurrency.md`, own coordination records and shared handoff.
- Contract: ожидание не удерживает write-lock или активный turn; минутный heartbeat возобновляет тот же чат, создаётся с фактическим current thread id, проверяется после создания и прекращается при получении lock, отмене или необходимости решения пользователя.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: `2026-09-01T15:17:25Z`
- Changed: `Docs/AI/rules/UnityConcurrency.md` now uses current-chat minute heartbeats with verified target binding, duplicate prevention, stop/pause conditions and bounded polling fallback.
- Validation: `git diff --check` and `Tools/somegame docs-check` passed.
