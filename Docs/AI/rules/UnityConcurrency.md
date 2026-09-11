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

## Task-owned Unity и emulator

Каждая задача вправе запустить и использовать один собственный Unity Editor и
один собственный Android emulator, даже если Editor или emulator другой задачи
уже работает. Наличие чужого процесса само по себе не блокирует запуск.

До запуска задача фиксирует точные project/worktree path и Editor PID, а для
эмулятора — уникальные AVD name и ADB serial. Два Editor не открывают один и тот
же project path; две задачи не используют один emulator serial, package/cache
scope или build output. Общие Catalog, SDK, integration и совпадающие output
resources остаются последовательными под соответствующими locks. Подтверждённый
licensing IPC conflict также останавливает новый запуск до recovery.

Official Unity MCP и fallback helper принадлежат тому же task-owned Editor и не
создают отдельное право записи. Чужие Editor/emulator/helper процессы нельзя
останавливать или переиспользовать как свои.

При нескольких worktree независимые истории не используют один глобальный
`unity` lock. Каждая получает `story:<storyId>` и точные collision locks:
`unity-project:<canonical-project-id>`, `emulator:<serial>` и
`build-output:<canonical-output-id>`. Значения выводятся из разрешённых точных
path/serial, фиксируются в agent record; совпавший resource обслуживается своей
FIFO-очередью.

Общий `unity` lock применяется только к доказанно общей Unity-инфраструктуре,
например destructive licensing recovery. Для Catalog, общего SDK и записи в
main используются `catalog`, `shared-sdk` и `integration`. Их нельзя заменять
story lock. Финальная интеграция кандидатов последовательна, но их
Editor/build/emulator gates могут идти одновременно.

Для live Editor предпочтителен один persistent helper и один `editor-check`.
Запуск/остановка Editor, Play Mode, compile, tests и write-tools требуют lock и
точного `--agent-id`. Atomic project использует общий
`--coordination-root .` из корня репозитория.

Одновременно одна задача держит соединение максимум с одним Unity MCP target.
Persistent helper живёт только внутри одного bounded Unity-шага и закрывается
сразу после последнего требуемого вызова. Перед сменой target и перед release
lock владелец обязан остановить все созданные им helper/client/relay/server
процессы и проверить отсутствие собственных остатков. Пользовательский Editor
можно оставить открытым, но созданное задачей MCP connection к нему закрывается.
Чужой или неопределённый PID не завершается без точной идентификации и явного
разрешения человека. Пока остаточные MCP процессы мешают клиенту, новые
connections не создаются.

## Классы Unity/MCP-операций

Требования определяются фактическим side effect операции, а не названием tool
или тем, что вызов выглядит как чтение. Если provider не гарантирует отсутствие
refresh, import, compile, domain reload, Play Mode, save или изменения
serialized/Editor state, операция относится к следующему, более строгому
классу.

| Класс | Примеры | Checkout/FIFO lock | Shared `unity` lock | Отдельное актуальное разрешение человека |
| --- | --- | --- | --- | --- |
| Репозиторное чтение | `git status`, чтение `Assets`/`Packages`/`ProjectSettings`, MCP config и сохранённых логов | нет | нет | нет |
| Live read-only probe уже открытого Editor | гарантированно read-only `editor_status`, active scene/dirty flag, hierarchy, Console delta; проверка transport без запуска Editor | нет | нет | нет |
| Editor state/heavy operation | запуск/остановка Editor или helper с write-capable manifest, refresh/import, recompile/domain reload, Play Mode, tests, save, изменение scene/prefab/settings/assets, content/Player build | story/resource FIFO для atomic story либо checkout FIFO для общего scope | точные `unity-project`/`build-output`; общий `unity` только для общей инфраструктуры | не дополнительно, если это прямо входит в обычную текущую задачу; для новой истории — только внутри отдельно разрешённого финального слота |
| Защищённая операция | финальный/релизный слот истории, destructive recovery, завершение чужого процесса, действие с неясной license-tier зависимостью | применимая story/resource FIFO; checkout FIFO только для общего scope | точные collision locks; общий `unity` для recovery | да; неизвестная license-tier зависимость не разрешается согласием и требует Personal-совместимого маршрута или подтверждения совместимости |

Read-only probe теряет исключение сразу после обнаружения необходимости
изменить state: агент не продолжает тем же MCP-сеансом, а входит в FIFO и
получает требуемые locks. Серия отдельных status/Console/hierarchy вызовов не
используется как polling; для связанной проверки применяется один bounded
`editor-check`. Само наличие lock не является разрешением на финальный,
релизный, destructive или license-sensitive шаг.

## Уровни проверки

Проверка выбирается заранее и не повышается скрыто из-за наличия Unity-проекта:

1. **Быстрая** — режим по умолчанию для промежуточной законченной правки:
   scoped `git diff --check`, проверка затронутых форматов, shell syntax и
   адресные static/unit tests. Unity, content build и Player не запускаются.
2. **Финальная** — только после завершения всего кандидата истории и отдельного
   явного разрешения человека: быстрая проверка плюс один объединённый bounded
   слот для необходимых MCP live-probe, import, content build, compile/tests,
   Player/APK, emulator smoke и visual gates.
3. **Релизная** — отдельный явно разрешённый выпускной прогон, только когда
   финальное evidence устарело либо release требует дополнительных платформ или
   gates. Он не повторяется автоматически после успешного финального слота.

Для новой истории промежуточного стандартного Unity compile/build уровня нет.
Ни наличие Unity-файлов, ни `auto-approve`, ни общий end-to-end запрос, ни
write-lock не заменяют отдельное актуальное разрешение непосредственно перед
финальным тяжёлым слотом. До него разрешены только read-only/static операции,
не запускающие Unity, build или ADB. Если разрешение не дано, зафиксировать
`ready-for-final-validation`, освободить lock и остановиться без ошибки.

Changed-path plan может понизить объём внутри выбранного уровня, но не должен
самовольно запускать более дорогой уровень. Широкие `verify`, `finish-task`,
`story-check` и `android-dev-cycle` presets не запускаются до человеческого
разрешения, если способны косвенно вызвать Unity, content build, Player или
ADB. APK, эмулятор и визуальный smoke не являются частью промежуточной
разработки.

## Ожидание и stale lock

- Ожидать без write-lock и без блокирующего `sleep`/polling внутри активного
  turn. Если среда поддерживает heartbeat текущего чата, поставить один
  heartbeat с обычным интервалом пять минут и завершить turn; каждое пробуждение
  выполняет одну короткую проверку FIFO/write-lock и при необходимости ждёт
  следующего.
- Heartbeat прикрепляется именно к текущему чату. Фактический current thread id
  нужно получить из app context или списка задач; запрещено угадывать или
  вручную изобретать `target_thread_id`. После создания проверить сохранённые
  `kind=heartbeat`, `status=ACTIVE`, пятиминутный interval и точное совпадение
  `target_thread_id` с текущим чатом.
- Перед созданием проверить, нет ли уже активного heartbeat для этой request;
  дубли не создавать. После получения lock, отмены задачи или состояния,
  требующего решения пользователя, heartbeat отключить или поставить на паузу.
  Возобновлять его только когда причина паузы устранена и ожидание ещё нужно.
- Если heartbeat текущего чата недоступен, fallback polling проверяет очередь
  не чаще раза в пять минут. При явном «дождись и продолжай» следующие
  ограниченные периоды начинаются автоматически, пока есть прогресс или
  корректный heartbeat владельца. Минутный интервал допустим только для
  краткого ожидаемого release handoff, а не как постоянный режим.
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

Если Unity-команда не показывает прогресс 1–2 минуты, до дальнейшего ожидания
выполняется read-only `licensing-preflight`. Подтверждённые повторяющиеся
`Failed to acquire global mutex` или `Another instance ... is already running`
считаются licensing conflict: текущую команду останавливают, а не ждут её
долгого timeout. Recovery и завершение чужого процесса по-прежнему требуют
точного PID, write-lock и явного разрешения разработчика.

## Release

После проверки обновить handoff/agent status, затем удалить только собственные
request и write-lock. Не держать lock при ожидании пользователя, разрешения или
внешнего сервиса.

Диагностика Unity: [UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md) и
[UnityLicensingTroubleshooting.md](../guides/UnityLicensingTroubleshooting.md).
Неописанный concurrency edge case сначала фиксируется в handoff и добавляется
в этот действующий протокол; исторические правила не дают право на takeover.
