# Unity and FIFO concurrency

Читайте этот документ перед Unity Editor/batch, MCP write, compile, tests,
import, build, генерацией или ожиданием общей очереди.

## Runtime queue

```text
Docs/AI/CoordinationRuntime/
  requests/<timestamp>-<agent>/request.md
  active/write-lock/owner.md
  agents/<agent>.md
  HANDOFF.md
```

Порядок получения lock:

1. Создать agent record и timestamped request.
2. Дождаться первой лексикографической позиции и отсутствия write-lock.
3. Атомарно создать `active/write-lock` и `owner.md`.
4. Повторно подтвердить первую позицию и совпадение owner/request/agent.
5. Обновлять heartbeat не реже одного раза в пять минут и перед долгой командой.

## Эксклюзивный ресурс

Одновременно допускается один тяжёлый процесс на весь SomeGame. Перед запуском
проверяются реальные Unity/build процессы. Official Unity MCP и fallback helper
используют ту же очередь; они не создают отдельное право записи.

Для live Editor предпочтителен один persistent helper и один `editor-check`.
Запуск/остановка Editor, Play Mode, compile, tests и write-tools требуют lock и
точного `--agent-id`. Atomic project использует общий
`--coordination-root .` из корня репозитория.

## Ожидание и stale lock

- Ожидать без write-lock; событийный механизм предпочтительнее polling.
- Без событий проверять очередь не чаще раза в 60 секунд, максимум 10 раз за
  десятиминутный период, если пользователь не задал другое terminal condition.
- При явном «дождись и продолжай» начинать следующие ограниченные периоды
  автоматически, пока есть прогресс или корректный heartbeat.
- Heartbeat старше десяти минут делает lock подозрительным, но не разрешает
  takeover. Проверить owner, request, agent status, процессы и Git; удаление
  чужого lock требует подтверждения разработчика.
- Не публиковать одинаковые сообщения о неизменившемся ожидании.

## Release

После проверки обновить handoff/agent status, затем удалить только собственные
request и write-lock. Не держать lock при ожидании пользователя, разрешения или
внешнего сервиса.

Диагностика Unity: [UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md) и
[UnityLicensingTroubleshooting.md](../guides/UnityLicensingTroubleshooting.md).
Неописанный concurrency edge case сначала фиксируется в handoff и добавляется
в этот действующий протокол; исторические правила не дают право на takeover.
