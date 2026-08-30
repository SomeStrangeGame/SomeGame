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

## Команды

### `context`

Объединяет task-plan и chat-resume. Read-only возвращает branch/HEAD, bounded
dirty paths, FIFO/write-lock, незавершённые work/handoff summaries, минимальный
набор документов, changed-path plan и следующую команду:

```bash
Tools/somegame context --task <docs|code|unity|content|art|integration>
```

### `verify`

`--explain` только показывает лестницу gates. Без него требуется точный lock;
дешёвые проверки выполняются первыми, content targets — последовательно.
Editor/Player/manual gates без точных параметров остаются явным `pending`, а не
считаются пройденными. Полностью успешное evidence кэшируется по входам,
tooling, Unity/package/config и platform fingerprint в ignored
`Library/SomeGameValidationCache`; `--release` и `--no-cache` обходят кэш.

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
останавливается через `am force-stop`, AVD остаётся запущенным.

### `licensing-preflight`

Без `--recover` только фиксирует Editor/Hub/Licensing PID, свежие conflict
markers и точные sockets. `--recover` требует lock, отсутствие Editor и
подтверждённый конфликт; отправляет только `TERM` exact main Hub/Licensing
Client PID, явно перечисленным через `--confirm-pid`. Runner никогда не удаляет
sockets, license или caches автоматически. Обнаруженный stale socket только
попадает в evidence; его ручное удаление требует отдельного подтверждения по
[licensing-протоколу](UnityLicensingTroubleshooting.md).
После recovery исходную Unity-команду разрешено повторить ровно один раз.

## Lock policy

`verify`, `finish-check`, `git-publish`, `content-gate`, `editor-gate`,
`player-build`, `android-smoke` и licensing
recovery сравнивают `--agent-id` с
`Docs/AI/CoordinationRuntime/active/write-lock/owner.md`. Несовпадение или
отсутствие lock завершаются до запуска внешнего процесса.

Реальный live gate выполняется только когда этого требует задача. Unit/static
проверка runner не запускает Unity, build, ADB install или recovery.
