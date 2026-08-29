# Official Unity MCP: рабочий протокол

Этот документ задаёт канонический порядок использования Official Unity MCP в
репозитории `SomeGame`. MCP дополняет, но не заменяет Git-проверки, контентный
pipeline, ручной visual smoke и правила общей Unity-очереди.

Нормативным источником FIFO, heartbeat и права записи остаётся
[UnityConcurrency.md](../rules/UnityConcurrency.md). Этот guide только применяет
его к Editor/MCP и не создаёт вторую lock policy.

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

## Обязательный порядок

1. Прочитать `AGENTS.md`, индекс `Docs/AI/README.md`, этот документ, текущий
   `CoordinationRuntime/HANDOFF.md` и проверить `git status --short`.
2. Убедиться, что target project точный и содержит `Assets`,
   `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` и ровно один
   выбранный MCP provider.
3. Для уже открытого Editor разрешён лёгкий read-only probe без захвата lock,
   если он не меняет Unity state и не запускает новый тяжёлый процесс.
4. Перед запуском/остановкой Editor, Play Mode, compile, tests или любой
   write-командой войти в общую FIFO, получить `write-lock` и проверить реальные
   Unity-процессы. Один Editor/build остаётся эксклюзивным ресурсом репозитория.
5. Проверить transport командой `unity pipeline list`; target path должен
   совпасть буквально, Pipeline server должен быть reachable.
6. Сначала выполнить малый read-only probe: `editor_status`, затем при
   необходимости hierarchy и Console.
7. Предпочитать native namespace из таблицы текущего охвата, когда Codex Desktop
   его экспортирует. Пока namespace отсутствует, использовать checked-in
   persistent fallback из `Tools/unity-mcp-helper`.
8. Write-tools разрешены только checked-in manifest и только helper, запущенному
   с `--agent-id`, совпадающим с владельцем текущего lock. Временный расширенный
   manifest не считается проектным протоколом и не используется для штатной
   работы.
9. После операции дождаться `compiling=false`, `domainReloadInProgress=false`,
   повторно прочитать сцену/Console и проверить Git delta.
10. Остановить helper, освободить собственные request/lock и записать точное
    evidence в `HANDOFF.md`. Editor оставлять открытым только по явной задаче
    пользователя, без удержания write-lock.

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

Даже при установленном package нельзя открывать несколько Editor одновременно:
все live проверки, content builds и write-команды остаются общим эксклюзивным
Unity-ресурсом и выполняются через FIFO/write-lock.

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
