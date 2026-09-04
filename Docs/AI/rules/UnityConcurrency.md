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

Состояние очереди читается одной командой:

```bash
Tools/somegame queue-status [--agent-id <agent>]
```

Она показывает позицию, возраст request/heartbeat, долгие ожидания, согласованность
owner/request/FIFO, реальные Unity/Hub/Licensing процессы и причину блокировки.
Это read-only диагностика; наличие `lockStale=true` само по себе не выполняет
takeover.

## Эксклюзивный ресурс

Одновременно допускается один тяжёлый процесс на весь SomeGame. Перед запуском
проверяются реальные Unity/build процессы. Official Unity MCP и fallback helper
используют ту же очередь; они не создают отдельное право записи.

Для live Editor предпочтителен один persistent helper и один `editor-check`.
Запуск/остановка Editor, Play Mode, compile, tests и write-tools требуют lock и
точного `--agent-id`. Atomic project использует общий
`--coordination-root .` из корня репозитория.

## Ожидание и stale lock

- Ожидать без write-lock и без блокирующего `sleep`/polling внутри активного
  turn. Если среда поддерживает heartbeat текущего чата, поставить один
  минутный heartbeat и завершить turn; каждое пробуждение выполняет одну
  короткую проверку FIFO/write-lock и при необходимости ждёт следующего.
- Heartbeat прикрепляется именно к текущему чату. Фактический current thread id
  нужно получить из app context или списка задач; запрещено угадывать или
  вручную изобретать `target_thread_id`. После создания проверить сохранённые
  `kind=heartbeat`, `status=ACTIVE`, минутный interval и точное совпадение
  `target_thread_id` с текущим чатом.
- Перед созданием проверить, нет ли уже активного heartbeat для этой request;
  дубли не создавать. После получения lock, отмены задачи или состояния,
  требующего решения пользователя, heartbeat отключить или поставить на паузу.
  Возобновлять его только когда причина паузы устранена и ожидание ещё нужно.
- Если heartbeat текущего чата недоступен, fallback polling проверяет очередь
  не чаще раза в 60 секунд, максимум 10 раз за десятиминутный период. При явном
  «дождись и продолжай» следующие ограниченные периоды начинаются автоматически,
  пока есть прогресс или корректный heartbeat владельца.
- Heartbeat старше десяти минут делает lock подозрительным, но не разрешает
  takeover. Выполнить `queue-status`, проверить owner/request/agent status,
  процессы и Git. Несогласованный owner считается отдельной ошибкой протокола.
  Удаление или перенос чужого lock/request требует подтверждения разработчика.
- Request без lock старше десяти минут помечается как долго ожидающий, но не
  считается stale только по возрасту. Если agent record уже имеет терминальный
  статус, точную orphaned-заявку разрешено удалить через
  `Tools/somegame queue-prune --request <exact-id>`; команда откажется менять
  активную заявку или request текущего lock. Остальные случаи требуют решения
  разработчика.
- Не публиковать одинаковые сообщения о неизменившемся ожидании.

Bounded-команды `Tools/somegame`, запущенные владельцем lock, автоматически
обновляют heartbeat не реже одного раза в минуту, пока дочерний процесс жив.
Это предотвращает ложный stale при долгих Unity/import/build/test операциях.
Ручные и внешние процессы по-прежнему требуют heartbeat владельца.

## Release

После проверки обновить handoff/agent status, затем удалить только собственные
request и write-lock. Не держать lock при ожидании пользователя, разрешения или
внешнего сервиса.

Диагностика Unity: [UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md) и
[UnityLicensingTroubleshooting.md](../guides/UnityLicensingTroubleshooting.md).
Неописанный concurrency edge case сначала фиксируется в handoff и добавляется
в этот действующий протокол; исторические правила не дают право на takeover.
