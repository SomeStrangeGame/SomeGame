# Official Unity MCP: рабочий протокол

Этот документ задаёт канонический порядок использования Official Unity MCP в
репозитории `SomeGame`. MCP дополняет, но не заменяет Git-проверки, контентный
pipeline, ручной visual smoke и правила общей Unity-очереди.

Нормативным источником FIFO, heartbeat и права записи остаётся
[UnityConcurrency.md](../rules/UnityConcurrency.md). Этот guide только применяет
его к Editor/MCP и не создаёт вторую lock policy.

MCP и fallback helper подчиняются базовому требованию
[Unity Personal](ContentPipeline.md#базовый-уровень-лицензии-unity). Нельзя
вызывать, включать или использовать для диагностики Editor-команды, окна,
настройки, сервисы и API, требующие Unity Pro или иной платной license
entitlement. Неизвестная license-tier зависимость останавливает операцию до
подтверждения совместимости или выбора Personal-совместимого маршрута.

## Текущий охват

На 2026-08-29 Official Unity Pipeline `0.5.0-exp.1` установлен во всех текущих
Unity-проектах репозитория. Каждому проекту соответствует отдельный локальный
Codex server:

| Project | MCP server |
| --- | --- |
| `Novels` | `unity_novels` |
| `Projects/novels-catalog` | `unity_novels_catalog` |
| `Projects/novels-content-template` | `unity_novels_content_template` |
| `Projects/novels-gpl` | `unity_novels_gpl` |
| `Projects/novels-tzm` | `unity_novels_tzm` |
| `Projects/novels-zdm` | `unity_novels_zdm` |

Atomic server entries имеют `required=false`: закрытый content Editor не должен
мешать запуску Codex. Pipeline endpoint существует только пока открыт точный
target Editor. После изменения `~/.codex/config.toml` Codex Desktop может
потребовать полный restart, прежде чем новые native namespaces появятся в
следующих задачах.

`required=false` не является lifecycle-гарантией: клиент всё равно может
поднять несколько relay/server процессов и перегрузить либо подвесить чат.
Одна задача выбирает ровно один target project и не активирует, не probe-ит и
не удерживает соединения с MCP servers остальных проектов.

Общий fallback helper находится в `Tools/unity-mcp-helper`. Для атомарного
проекта write-capable daemon обязательно запускается с target `--project`,
общим `--coordination-root .` из корня `SomeGame` и `--agent-id` владельца lock. Без явного
coordination root сохраняется standalone-поведение: lock ищется внутри target
project. В обоих режимах отсутствие точного owner даёт fail-closed.

## Когда использовать MCP

При работе с `Novels` MCP является предпочтительным интерфейсом для состояния,
которое принадлежит живому Editor:

- готовность Editor, compile и domain reload;
- Play Mode state;
- активная сцена, hierarchy и scene dirty state;
- Unity Console, включая stack trace по запросу;
- bounded recompile и EditMode test workflow из checked-in helper;
- единый `editor-check`, агрегирующий status, scene, Console delta, compile и
  опциональный filtered EditMode suite в одном локальном цикле;
- повторная проверка Editor после изменения scripts/assets/settings.

Файлы, Git diff, конфиги, generated releases и большие логи по-прежнему
проверяются обычными репозиторными инструментами. Успешный MCP probe не
доказывает успешную content/player build или визуальную корректность.

## Как определить требования операции

Перед MCP-вызовом классифицировать его по таблице в
[UnityConcurrency.md](../rules/UnityConcurrency.md#классы-unitymcp-операций).
Решает фактический side effect, а не имя команды:

- чтение файлов проекта, Git, MCP config и уже сохранённых логов не является
  Unity-операцией и не требует lock;
- гарантированно read-only status/scene metadata/hierarchy/Console delta уже
  открытого Editor допускаются как один короткий probe без lock;
- вызов, способный запустить refresh/import/compile/domain reload, Play Mode,
  test, save или изменить selection, scene, prefab, asset, setting либо другой
  Editor state, требует FIFO/write-lock до первого вызова;
- helper с write-capable manifest запускается только под lock, даже если
  планируется сначала вызвать в нём read-tool; для probe без lock используется
  native read-only surface либо явно read-only manifest;
- отдельное актуальное разрешение человека требуется не для каждого MCP write,
  а для защищённых случаев из concurrency-контракта: финального/релизного слота
  истории, destructive recovery, завершения чужого процесса и иных явно
  оговорённых действий. Lock сам по себе это разрешение не заменяет.

Если read-only probe выявил необходимость исправления, recompile или иной
mutation, остановиться на evidence, затем отдельно войти в FIFO и получить
locks. Нельзя расширять probe до write-сеанса молча.

## Lifecycle и обязательный cleanup

Persistent означает только один bounded Unity-шаг, а не весь чат. После
последнего требуемого MCP-вызова поток немедленно:

1. Закрывает созданные им helper, client session и relay/server process точного
   target, не откладывая cleanup до конца длинного текстового этапа.
2. Если Editor был запущен runner'ом, закрывает и его, кроме явного
   `--no-stop-editor`; пользовательский уже открытый Editor без явной просьбы
   не закрывает, но своё MCP-соединение с ним завершает.
3. Проверяет отсутствие собственных оставшихся helper/relay/server процессов и
   только затем освобождает lock. В итоговом evidence указывает результат
   cleanup.
4. Перед переключением на другой Unity project сначала полностью завершает
   MCP lifecycle предыдущего target; два активных target MCP в одном чате
   запрещены.

Обнаруженный процесс без доказанного ownership не завершается вслепую. Сначала
фиксируются command line, PID, target project и владелец; завершение чужого или
неопределённого процесса требует явного разрешения человека. Если такие
остаточные процессы уже мешают чату, новые Unity MCP connections не создаются
до cleanup или решения пользователя.

## Обязательный порядок

1. Прочитать `AGENTS.md`, индекс `Docs/AI/README.md`, этот документ, текущий
   `CoordinationRuntime/HANDOFF.md` и проверить `git status --short`.
2. Убедиться, что target project точный и содержит `Assets`,
   `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` и ровно один
   выбранный MCP provider; выбранная MCP-операция доступна на Unity Personal.
3. Классифицировать операцию по нормативной матрице. Для уже открытого Editor
   разрешён один лёгкий read-only probe без захвата lock только когда provider
   гарантирует отсутствие side effects и не запускается write-capable helper.
4. Перед запуском/остановкой Editor, Play Mode, compile, tests или любой
   write-командой войти в общую FIFO, получить `write-lock` и проверить реальные
    Unity-процессы. Чужой Editor другого project/worktree path не блокирует
    задачу; совпадающий target path или общий mutable output блокирует.
5. Проверить transport командой `unity pipeline list`; target path должен
   совпасть буквально, Pipeline server должен быть reachable.
6. Сначала выполнить малый read-only probe: `editor_status`, затем только
   необходимые hierarchy и Console delta. Если эти чтения составляют одну
   проверку, выполнить их одним bounded `editor-check`, а не отдельными
   модельными polling-циклами.
7. Предпочитать native namespace из таблицы текущего охвата, когда Codex Desktop
   его экспортирует. Пока namespace отсутствует, использовать checked-in
   persistent fallback из `Tools/unity-mcp-helper`.
8. Write-tools разрешены только checked-in manifest и только helper, запущенному
   с `--agent-id`, совпадающим с владельцем текущего lock. Временный расширенный
   manifest не считается проектным протоколом и не используется для штатной
   работы.
9. После операции дождаться `compiling=false`, `domainReloadInProgress=false`,
   повторно прочитать сцену/Console и проверить Git delta.
10. Выполнить lifecycle cleanup: остановить все созданные текущим потоком
    helper/client/relay/server процессы target, проверить отсутствие своих
    остатков, затем освободить собственные request/lock и записать точное
    evidence в `HANDOFF.md`. Запущенный runner'ом Editor оставлять открытым
    только по явной задаче пользователя, без удержания write-lock;
    пользовательский Editor можно оставить, но MCP connection к нему закрыть.

Для обычной проверки нельзя вызывать status/Console/hierarchy отдельными
модельными циклами. Используется один `editor-check`; его внутренний polling не
публикует неизменившиеся промежуточные состояния. Полные Console/Editor logs
читаются адресно только при non-success результате. При накопленной Console
передаётся сохранённый cursor, чтобы не возвращать старую историю.

Cold-start port-файл сам по себе не доказывает readiness: `editor-check`
bounded ждёт `editor_status=ready` без compile/domain reload. Если `recompile`
сразу возвращает `up_to_date`, это уже финальное состояние; последующий
`recompile_status=idle` не должен превращать успешный no-op в timeout.

## Console и логирование Novels

Нельзя ограничиваться фильтром Unity `level=error`. `Logs.Entity` может
показывать доменные ошибки красным цветом через `Debug.Log`, поэтому записи
вроде `[INITIALIZATION_FAILED]` технически имеют уровень `log`.

Для runtime smoke обязательно:

- прочитать все новые Console entries, а не только Unity errors;
- искать доменные коды `INITIALIZATION_FAILED` и другие failure markers;
- при накопленной Console зафиксировать позицию свежего `Editor.log` до запуска
  и анализировать только добавленный участок;
- отличать старую запись от нового воспроизведения по timestamp/позиции;
- после изменений Build Profile или других cached PlayerSettings выполнять
  полный Editor restart, если повторный Play Mode сохраняет старое значение.

## Проверка результата

Минимальный handoff после Unity-изменения содержит:

- target project path, Unity version и Editor PID;
- active scene и dirty state;
- compile/reload state;
- какие Console levels и какой свежий участок Editor log проверены;
- точный runtime-сценарий и число повторений;
- Git delta до/после;
- что осталось ручным или platform-specific gate.

Компиляция сама по себе не означает готовность. Для UI нужен Play Mode и ручная
визуальная проверка, для content release — штатный `novels-content build`, для
Player — целевая platform build/device проверка.

## Подключение нового проекта

Текущие шесть проектов уже подключены. Будущий Unity project не наследует MCP
автоматически и добавляется отдельным scope:

1. Создать узкий coordination scope и получить lock.
2. Подтвердить Unity 6 и отсутствие другого MCP provider.
3. Установить ровно один Official Unity Pipeline package в target project.
4. Добавить отдельное уникальное имя MCP server и точный `--project-path`, не
   перезаписывая `unity_novels`.
5. Для write-workflow использовать helper с явным общим
   `--coordination-root .` из корня `SomeGame`; fail-closed поведение сохраняется.
6. Проверить read-only status/hierarchy/Console, domain reload, полный restart и
   отсутствие Git delta.
7. Только после этого allowlist-ить необходимые write-tools отдельным scope с
   bounded polling и post-check.

Параллельные задачи могут открыть по одному Editor для разных точных
project/worktree paths. Один path нельзя открывать дважды; shared outputs,
Catalog/SDK/integration resources и подтверждённый licensing conflict требуют
применимой сериализации.

## Проверенная матрица

При подключении 2026-08-29 каждый atomic project был реально открыт отдельно.
Для всех пяти подтверждены Pipeline reachable, Unity `6000.3.11f1`,
`editor_status=ready`, отсутствие compile/domain reload, чистая active scene и
читаемые hierarchy/Console. После smoke каждый Editor и helper были закрыты.

Первичное открытие `novels-gpl`, `novels-tzm` и `novels-zdm` также показало
Ink/Progress API errors, несмотря на последующее сообщение о завершении Ink
compilation. Причина была в сохранённой очереди Ink v1: domain reload обрывал
несериализуемый worker, а восстановленный `Compiling` item сразу считался
просроченным; одновременно сбрасывался статический Progress id. На 2026-08-29
все шесть проектов используют checked-in `Packages/InkUnityIntegration` с
повторной постановкой такого item в очередь после reload. TZM и ZDM прошли по
два чистых запуска без `Ink Compiler timed out` и ошибки Progress API.
