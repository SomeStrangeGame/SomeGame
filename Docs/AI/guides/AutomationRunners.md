# Bounded automation runners

Каноническая точка входа для повторяемых локальных операций:

```bash
Tools/somegame <workflow> [options]
```

Runner не создаёт отдельный coordination contract: право на тяжёлую операцию и
порядок ожидания определяет
[UnityConcurrency.md](../rules/UnityConcurrency.md), а канонические content-
команды и их семантику — [ContentPipeline.md](ContentPipeline.md).

Каждый workflow имеет конечный timeout, выполняет polling внутри процесса,
пишет полный timestamped лог в ignored `Novels/Build/Logs/automation` и
возвращает один компактный JSON. Чат ждёт завершения команды и не интерпретирует
неизменившиеся промежуточные состояния.

Ни один workflow не вправе включать или вызывать license-tier-зависимую
функциональность Unity. Канонический Personal-only контракт и fail-closed
трактовка неизвестной зависимости заданы в
[ContentPipeline.md](ContentPipeline.md#базовый-уровень-лицензии-unity).

## Команды

### Lifecycle и локальные пресеты

`start-task` создаёт собственные coordination records и атомарно получает lock
только для первой FIFO-заявки. `finish-task` запускает проверки точных
`--paths`, обновляет agent/handoff и освобождает только собственные request/lock;
pending runtime gates требуют явного `--allow-pending`.

```bash
Tools/somegame start-task --agent-id <id> --task <summary> --scope <exact-scope>
Tools/somegame tooling-tests
Tools/somegame story-check --agent-id <lock-owner> --target <story-id> [--build]
Tools/somegame android-dev-cycle --agent-id <lock-owner> --package-id <id>
Tools/somegame clean-generated --agent-id <lock-owner> --project <project> [--apply]
Tools/somegame finish-task --agent-id <lock-owner> --paths <owned-path...> \
  --summary <result> [--allow-pending]
```

`tooling-tests` не запускает Unity. `story-check` ограничен одним atomic story.
`android-dev-cycle` последовательно выполняет существующие Player build и ADB
smoke contracts. `clean-generated` fail-closed принимает только точный Unity
project внутри репозитория; без `--apply` это обязательный dry-run.

### `context`

Объединяет task-plan и chat-resume. Read-only возвращает branch/HEAD, bounded
dirty paths, FIFO/write-lock, незавершённые work/handoff summaries, минимальный
набор документов с fingerprint, changed-path plan и следующую команду:

```bash
Tools/somegame context --task <inspect|docs|code|unity|content|art|integration> \
  [--resume] [--paths <task-owned-path> ...]
```

`inspect` не добавляет тематических документов. Точные `--paths` строят plan
только по scope задачи, но полный dirty snapshot сохраняется для обнаружения
конфликтов. `--resume` разрешает в том же чате не перечитывать документ, если
его fingerprint, task type и scope не изменились.

### `queue-status`

Read-only снимок FIFO и process barrier:

```bash
Tools/somegame queue-status [--agent-id <agent>]
```

Возвращает позицию агента, возраст request и heartbeat, `lockStale`, долгие
ожидания и recoverable orphaned-заявки, проверку
совпадения первой заявки с owner, точную причину ожидания и Unity/Hub/Licensing
процессы. Команда ничего не очищает и не получает lock. При долгом запуске
любой bounded workflow с `--agent-id` автоматически обновляет heartbeat своего
lock не реже раза в минуту; чужой lock она изменить не может.

`Tools/somegame queue-prune --request <exact-id>` удаляет только точную заявку,
не связанную с текущим lock, чей agent record уже имеет терминальный статус.
Для active/queued владельца и неоднозначного состояния команда fail-closed.

### `verify`

`--explain` только показывает лестницу gates. `--paths` ограничивает planner и
`git diff --check` файлами владельца. Без него требуется точный lock;
дешёвые проверки выполняются первыми, content targets — последовательно.
Editor/Player/manual gates без точных параметров остаются явным `pending`, а не
считаются пройденными. Полностью успешное evidence кэшируется по входам,
tooling, Unity/package/config и platform fingerprint в ignored
`Library/SomeGameValidationCache`; `--release` и `--no-cache` обходят кэш.

Перед запуском runner агент выбирает уровень проверки из
[UnityConcurrency.md](../rules/UnityConcurrency.md#уровни-проверки):

- быстрый уровень является default и ограничивается scoped diff, форматами,
  shell/static/unit checks без запуска Unity;
- стандартный запускается один раз на завершённый логический блок и добавляет
  один compile с адресными tests;
- релизный запускается только по прямому запросу или перед выпуском и включает
  необходимые content/Player/device/visual gates.

Наличие Unity-файла в diff не является само по себе разрешением повышать
уровень. Связанные правки сначала пакетируются, затем получают один validation
slot. Если пользователь исключил APK, эмулятор или иной gate, runner не должен
запускать его косвенно через более широкий preset.

### `commit-plan` и `finish-check`

`commit-plan` read-only предлагает группы dirty paths и отделяет generated
artifacts; это рекомендация, не автоматический staging. `finish-check` требует
lock owner и fail-closed проверяет agent/handoff, commit plan и реальные Editor
процессы. Команды не создают commit и не освобождают lock.

### `docs-check`

Проверяет Markdown links/anchors, лимиты core/memory/handoff, `git diff --check`
и dependency-free tooling tests. Read-only относительно tracked sources и не
требует lock.

### `git-publish`

Безопасно публикует уже подготовленную последовательность атомарных commits:

```bash
Tools/somegame git-publish --agent-id <lock-owner> \
  [--ssh-key <local-key-path>]
```

По умолчанию цель — `origin/main`. Runner проверяет точного lock owner, текущую
ветку и чистоту дерева; разрешены только три собственных untracked runtime-
файла: agent, request и owner. Затем выполняются `fetch`, проверка divergence,
обычный push без force и сравнение SHA локального `HEAD` с remote branch.
Remote-ahead история, staged runtime record, любой посторонний dirty path,
ошибка авторизации или несовпадение SHA останавливают workflow. Runner не делает
автоматические pull/rebase/merge и не сохраняет ключ: `--ssh-key` лишь передаёт
существующий локальный путь в `ssh-add`.

### `content-gate`

Запускает существующий changed-path `novels-content verify` для одной платформы.
Требует `--agent-id` текущего lock owner. В очень грязном рабочем дереве
`--target <catalog|story-id>` запускает ровно один явный atomic build вместо
широкого changed-path plan.

Batch/Editor start при работающем Unity Hub fail-closed. Явный `--close-hub`
штатно отправляет `TERM` только main Hub PID и ждёт process barrier.

### `editor-gate`

Подключается к открытому Editor либо запускает ровно один через
`--start-editor`, поднимает persistent MCP helper и выполняет один
`editor-check`. Перед Console/hierarchy/compile helper bounded ждёт реального
`editor_status=ready`, а финальный `up_to_date` из `recompile` не требует
дополнительного status polling. При `--compile` gate не допускает ни новых, ни
уже существовавших Console errors; для свежего Editor дополнительно проверяет
принадлежащий этому запуску `Editor.log` на C# compiler errors. Поэтому
`up_to_date` само по себе больше не является доказательством успешной
компиляции. Compile и filtered EditMode suite включаются флагами. Запущенные
runner’ом helper и Editor завершаются в cleanup; `--no-stop-editor` применяется
только при явной необходимости оставить запущенный им Editor и отделяет его
process session от lifecycle самой команды.

### `player-build`

Последовательно собирает content для одной платформы и ровно один Player
`Remote|Embedded`, используя `Novels/Tools/build-player.sh`. Полная matrix не
является default. `--skip-content-build` допустим только для уже проверенного
актуального `Build/LocalContent`.

### `android-smoke`

Принимает точные APK, serial и package ID; install/launch выполняются через ADB.
Runner ждёт упорядоченные `[NOVELS_SMOKE]` events одного `runId`, foreground
activity и отсутствие blocking markers. Package ID не угадывается. Screenshot,
полный logcat и activity dump сохраняются только при failure; приложение всегда
останавливается через `am force-stop`, AVD остаётся запущенным. Повторные
соединения самого `qemu-system-aarch64` с `127.0.0.1:1970` классифицируются в
JSON как `android-emulator-sdk-controller-1970` с `affectsGate=false`: это
внешняя диагностика необязательного SDK Controller, а не ошибка приложения.

### `licensing-preflight`

Без `--recover` только фиксирует Editor/Hub/Licensing PID, свежие conflict
markers и точные sockets. `--recover` требует lock, отсутствие Editor и
подтверждённый конфликт; отправляет только `TERM` exact main Hub/Licensing
Client PID, явно перечисленным через `--confirm-pid`. Runner никогда не удаляет
sockets, license или caches автоматически. Обнаруженный stale socket только
попадает в evidence; его ручное удаление требует отдельного подтверждения по
[licensing-протоколу](UnityLicensingTroubleshooting.md).
После recovery исходную Unity-команду разрешено повторить ровно один раз.
Recovery восстанавливает только штатный Personal-совместимый запуск и не даёт
права активировать, запрашивать или имитировать платный entitlement.

Если bounded Unity-команда не показывает прогресс в течение 1–2 минут, сначала
запускается read-only `licensing-preflight`, не дожидаясь общего timeout.
Повторяющиеся mutex markers означают подтверждённый licensing conflict:
дочернюю Unity-команду останавливают, lock освобождают перед ожиданием решения,
а recovery выполняют только по правилам точных PID и явного подтверждения.

## Lock policy

`verify`, `finish-check`, `git-publish`, `content-gate`, `editor-gate`,
`player-build`, `android-smoke` и licensing
recovery сравнивают `--agent-id` с
`Docs/AI/CoordinationRuntime/active/write-lock/owner.md`. Несовпадение или
отсутствие lock завершаются до запуска внешнего процесса.

Реальный live gate выполняется только когда этого требует задача. Unit/static
проверка runner не запускает Unity, build, ADB install или recovery.
