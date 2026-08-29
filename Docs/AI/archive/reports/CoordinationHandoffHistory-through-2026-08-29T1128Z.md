# Current cross-chat handoff

## 2026-08-29T11:19:09Z — ink-domain-reload-fix — ready-for-integration

Task: устранить ложный `Ink Compiler timed out` при повторном запуске Unity и
проверить исправление на тяжёлых TZM/ZDM Ink-проектах.

Changed:
- Ink Unity Integration v1.2.2 закреплён локально в
  `Packages/InkUnityIntegration` с provenance исходного upstream commit.
- `InkCompiler` после domain reload переводит сохранённый `Compiling` item
  обратно в `Queued`, сбрасывает устаревший Progress id и продолжает очередь
  только после завершения Unity script compilation.
- Все шесть Unity manifests и package locks переведены на один checked-in
  local package; отдельной миграции на несовместимый Ink 2.0 нет.

Validation:
- TZM: два полных запуска Unity 6000.3.11f1, Pipeline reachable, Editor ready,
  compile/reload false; в Console нет Ink timeout и Progress API error.
- ZDM: два полных запуска с тем же результатом; первые циклы включали package
  resolution/import и соответствующий domain reload.
- Все 13 затронутых JSON-файлов разобраны успешно; шесть manifest/lock пар
  указывают на локальный package; scoped `git diff --check` успешен.
- Новые тесты не добавлялись; проверка выполнена повторным реальным запуском
  проектов, в которых дефект воспроизводился.

Pending / risks:
- При обновлении upstream Ink локальный patch нужно переносить осознанно и
  повторять минимум два startup cycle для TZM и ZDM.
- В Console остаются только штатные предупреждения automated mode/Input Manager,
  не связанные с Ink.

Suggested next step:
- Интегрировать local package и все шесть manifest/lock пар атомарно.

## 2026-08-29T11:12:00Z — runtime-smoke-telemetry — ready-for-integration

Task: покрыть runtime стабильными machine-readable маркерами для дешёвых ADB smoke-тестов и отлавливать показ fallback-персонажа.

Changed:
- `Novels/Assets/Novels/Diagnostics/SmokeTelemetry.cs(.meta)`: компактный однострочный `[NOVELS_SMOKE]` JSON emitter с `v`, `seq`, `runId`, `event` и экранированием значений.
- Runtime integration points: `app`, catalog, story release, episode, dialogue, choice, completion и structured `NovelError` события.
- `CharacterController` + `NovelRuntime.Presentation`: фактический `_missingCharacter` публикует `fallback.used` с content/episode/character/reason.
- `ContentPipeline.md`: событие character fallback объявлено блокирующим failure; screenshot/logcat/activity собираются только в failure branch.

Validation:
- Unity 6000.3.11f1 PID 28568: Pipeline reachable, bounded compile `up_to_date`, Console errors 0→0, Editor ready, compile/reload false, сцена `Assets/Novels/Novels.unity` clean.
- Scoped `git diff --check` и required-event audit: успешно.
- Editor/helper после проверки закрыты; `unity pipeline list` пуст.
- Licensing recovery: неуспешный Editor PID 28234 штатно завершён; stale Licensing Client PID 25447 с подтверждённым mutex conflict завершён через TERM; лицензии/cache/hosts не изменялись; повторный handshake успешен.

Pending / risks:
- Новые события ещё не проверены в Android Player/logcat; следующий APK smoke должен подтвердить фактический JSON flow и failure artifact collection.
- Файлы `ApplicationRuntime`, `CatalogFlow`, `EntryPoint`, `ChoiceSelectionHandler` и `CharacterController` содержат сохранённые соседние dirty changes; интегрировать scoped.

Suggested next step:
- Пересобрать Android Embedded APK и прогнать ADB runner по `[NOVELS_SMOKE]`; при `fallback.used` немедленно собрать failure-only screenshot/logcat/activity.

## 2026-08-29T10:57:24Z — gpl-clothes-only-story — completed-with-limitations

Task: адаптировать первый эпизод GPL к фиксированной внешности и выбору только одежды.

Changed:
- `s01e01.ink`: внешность Леи закреплена как знакомое лицо и короткие тёмные волосы; интерактивного выбора лица, волос или цвета нет.
- После первого контакта с голосом добавлен один сюжетно встроенный гардероб: медицинская форма, термокомплект или станционный комбинезон.
- Одеждо-зависимое «хватает за куртку» заменено на нейтральное «хватает за ворот».
- `gpl.ink.json` и source map пересобраны через установленный `Ink.Compiler`; карта содержит 740 записей.

Validation:
- Ink compile — успешно; generated JSON проходят `jq empty`.
- Поиск команд выбора внешности, причёски и цвета волос — совпадений нет.
- Scoped `git diff --check` — успешно.
- Unity content validation не завершилась: Licensing Client теряет соединение и не выдаёт `com.unity.editor.headless`; две зависшие попытки штатно прерваны.

Pending / risks:
- Три цельных clothing sprite ещё должны быть импортированы под точными именами из Ink.
- После восстановления Unity Licensing повторить `Tools/novels-tools/novels-content validate gpl`.

Suggested next step:
- Создать утверждённые цельные варианты Леи для трёх комплектов, затем подключить выбор одного полного sprite address без слоёв волос/лица.

## 2026-08-29T10:01:50Z — unity-mcp-all-projects-smoke — completed-with-observations

Task: проверить checked-in JSON-RPC Official Unity MCP helper на всех шести Unity-приложениях.

Changed:
- Production/project files задачей намеренно не изменялись; добавлены только собственные coordination records.

Validation:
- Последовательно открыты `Novels`, catalog, content-template, GPL, TZM и ZDM; одновременно работал только один Editor/helper.
- Во всех шести: Pipeline `0.5.0-exp.1` reachable, Unity `6000.3.11f1`, `editor_status=ready`, compile/reload false, Play Mode stopped, active scene not dirty.
- Helper успешно выполнил 18/18 read-only calls: по одному `editor_status`, `get_scene_hierarchy` и полной `console` на проект.
- `Novels`, catalog, template и GPL: Console без error entries. В `Novels` есть informational запись о завершении дублирующего ADB server из другого SDK.
- TZM и ZDM: воспроизводятся Ink Compiler timeout и `(Remove) Cannot get non-existing progress id 0`, после timeout следует `Ink compilation completed`; MCP calls при этом успешны.
- После smoke все Editor/helper процессы закрыты; `unity pipeline list` не содержит instances; coordination diff check успешен.

Pending / risks:
- TZM/ZDM Ink startup errors являются отдельной стабильной проблемой project health и требуют отдельного расследования.
- Native namespaces Codex Desktop всё ещё не экспортируются; этот smoke проверял зафиксированный persistent JSON-RPC fallback.

Suggested next step:
- Использовать helper как штатный MCP transport; отдельно диагностировать общий Ink postprocessor/progress lifecycle для TZM и ZDM.

## 2026-08-29T09:58:00Z — android-integration-test-protocol — ready-for-integration

Task: закрепить экономный протокол ADB integration smoke без бесполезных success screenshots.

Changed:
- `Docs/AI/guides/ContentPipeline.md`: success gate основан на PID/activity, структурированных Unity-маркерах, отсутствии блокирующих ошибок и тайм-аутах; screenshot, полный logcat и activity dump собираются только при failure.
- Собственные coordination/parallel records: scope передан в `ready-for-integration`.

Validation:
- Scoped `git diff --check`: успешно.
- Required-phrases audit: success screenshot исключён, failure-only screenshot/logcat/activity diagnostics присутствуют.

Pending / risks:
- Для стабильных сценариев runtime должен выдавать однозначные маркеры ожидаемых шагов; visual regression без эталонного сравнения этим smoke не покрывается.

Suggested next step:
- При реализации ADB integration tests сохранять screenshot только в failure branch до `am force-stop`.

## 2026-08-29T09:51:00Z — android-emulator-protocol — ready-for-integration

Task: закрепить немедленный Android Embedded build/install/launch/adb smoke в каноническом протоколе.

Changed:
- `ContentPipeline.md`: добавлен полный release-set → Embedded development APK → adb install/launch → PID/activity/logcat/screenshot → force-stop lifecycle.
- Зафиксированы debug-vs-release signing, Android `jar:file://` StreamingAssets и evidence/handoff требования.
- Emulator-only EGL/ASTC ограничения отделены от блокирующих runtime failures и physical-device gate.

Validation:
- Полный logcat PID 7347: 2785 строк, crash/ANR/FATAL и Unity error-level сообщений нет.
- Catalog release и TZM release активированы, episode runtime дошёл до `No save file`/инициализации истории.
- `TrySetException` относится к ожидаемому cache-miss → download → release activation маршруту, не к необработанной ошибке.
- EGL fallback и ASTC 6×6/8×8 software decompression классифицированы как ограничения текущего AVD.
- Игра остановлена через `am force-stop`; PID исчез, foreground вернулся к NexusLauncher, `emulator-5554` остался `device`.
- Scoped `git diff --check`, required-phrases и README link audit: success.

Pending / risks:
- Эмулятор не подтверждает ASTC visual quality, GPU memory, FPS, нагрев и physical-device compatibility.
- Documentation change не закоммичен; интегрировать scoped, сохранив соседний dirty tree.

Suggested next step:
- При следующем Android APK build выполнять новый раздел целиком; compression/performance gate отдельно повторять на физическом ASTC-capable устройстве.

## 2026-08-29T09:47:52Z — character-whole-variants — ready-for-integration

Task: закрепить одобренную на пилотном наборе Леи схему цельных вариантов персонажей.

Changed:
- `CharacterLayeringRules.md`: новая production-схема — один цельный полнофигурный PNG на сочетание позы, одежды и эмоции; канонический мастер и причёска фиксируются.
- Запрещены независимые головы/шеи/подбородки, радикальный дрейф позы и исправление артефактов наложением второго слоя.
- Добавлены close-up/full-height/alpha gates и правило единственной отрисовки цельного варианта в Unity.
- По уточнению автора legacy-исключения полностью удалены: правило действует для всех персонажей и ассетов без исключений; существующих старых ассетов нет.
- `README.md`: описание канонического контракта обновлено; параллельные MCP-добавления сохранены.

Validation:
- Scoped `git diff --check` — успешно.
- Старые обязательные формулировки о порядке слоёв и обратной сборке в каноническом правиле и README отсутствуют.

Pending / risks:
- Для импорта цельных гардеробных вариантов runtime потребуется адаптер, выбирающий ровно один полный sprite address без наложения отдельных частей.

Suggested next step:
- Весь character art создавать по этому протоколу; runtime-адаптер проектировать отдельной задачей до импорта цельного гардероба.

## 2026-08-29T09:43:00Z — android-embedded-emulator — ready-for-integration

Task: собрать Android Embedded Player, установить и запустить на AVD.

Changed:
- `PlayerBuildAutomation.cs`: Embedded Android development build временно использует Unity debug keystore; release signing сохранён.
- `EntryPoint.cs`, `StreamingAssetsContentSource.cs(.meta)`: Android Embedded читает APK StreamingAssets по `jar:file://` через UnityWebRequest.
- Generated Android content/player artifacts находятся только в ignored `Build`/cache paths.

Validation:
- Catalog, TZM, ZDM Android builds: success; GPL пропущен как WIP schema 1, отсутствующий в catalog stories.
- Embedded development APK: success, version `2026.08.29` (`3502656`), 1.7 GiB.
- `adb install -r -d`: success; package foreground activity `UnityPlayerGameActivity`, PID 7347.
- Первый smoke воспроизвёл filesystem-vs-jar failure; после fix повторный build/install показывает каталог, свежих catalog/init/version/schema failures в logcat нет.
- Scoped `git diff --check`: success.

Pending / risks:
- APK development-signed и очень большой из-за Embedded content; это ожидаемо для текущего smoke, не release artifact.
- Source changes не закоммичены и должны интегрироваться отдельно от большого соседнего dirty tree.

Suggested next step:
- Ручной touch smoke: перелистывание, Open TZM/ZDM, запуск эпизода; затем scoped commit build-automation/content-source fix.

## 2026-08-29T09:22:43Z — unity-mcp-all-projects — completed-with-observations

Task: распространить Official Unity MCP на все atomic Unity projects и закрепить результат в общем MCP-протоколе.

Changed:
- Во все пять atomic manifests/locks установлен `com.unity.pipeline` `0.5.0-exp.1` официальной CLI-командой.
- В локальный Codex config добавлены отдельные optional servers: `unity_novels_catalog`, `unity_novels_content_template`, `unity_novels_gpl`, `unity_novels_tzm`, `unity_novels_zdm`.
- `Tools/unity-mcp-helper`: добавлен явный `--coordination-root` для общей очереди атомарных проектов; default standalone behavior сохранён, write-policy fail-closed.
- `Docs/AI/guides/UnityMcpWorkflow.md`: обновлены coverage matrix, server names, helper invocation, validation evidence и правило подключения будущих проектов.

Validation:
- Helper syntax/unit tests: 21/21 passed, включая exact common-lock owner и wrong-owner denial.
- `codex mcp list` видит все пять новых enabled server entries.
- Каждый atomic project реально открыт отдельно: Pipeline `0.5.0-exp.1` reachable, Unity `6000.3.11f1`, Editor ready, compile/reload false, active scene clean, hierarchy и Console читаются.
- Все Editor/helper процессы после smoke закрыты; `unity pipeline list` пуст; scoped `git diff --check` успешен.

Pending / risks:
- Для появления новых native namespaces в Codex Desktop может понадобиться полный restart приложения; checked-in fallback работает независимо.
- Первичное открытие GPL/TZM/ZDM показало Ink compiler/Progress API Console errors, хотя Ink compilation затем завершилась. Это отдельный project health issue, не MCP transport failure.
- Ранее открытый `Novels` Editor PID 8705 был штатно закрыт для соблюдения exclusive Unity protocol; android build task может продолжить со своего следующего FIFO scope.

Suggested next step:
- После restart Codex выполнить один native `editor_status` для нужного server; Ink startup errors расследовать отдельной задачей, не смешивая с MCP integration.

## 2026-08-29T09:18:00Z — android-embedded-emulator — yielded

Task: собрать Android Embedded Player, установить APK и запустить на AVD.

Changed:
- Production files не изменялись; созданы только собственные coordination records.

Validation:
- Unity 6000.3.11f1 Android Build Support установлен.
- AVD `Novels_Pixel_7_API_34` ранее полностью загрузился и доступен через adb.
- Unity Editor PID 8705 всё ещё открыт для `Novels`; build не запускался.

Pending / risks:
- Штатное закрытие Editor остановлено до явного разрешения пользователя из-за возможного несохранённого состояния.

Suggested next step:
- После явного разрешения снова войти в FIFO, штатно закрыть Editor, дождаться process/IPC barrier, затем последовательно build content → Embedded APK → adb install → launch/smoke.

## 2026-08-29T09:13:47Z — unity-mcp-protocol — completed

Task: закрепить Official Unity MCP как канонический инструмент работы с Unity-проектами и проверить его распространение внутри Fork.

Changed:
- `Docs/AI/guides/UnityMcpWorkflow.md`: добавлен полный operational protocol, fallback/native transport, lock-policy, Console caveat, validation evidence и порядок подключения следующего проекта.
- `Docs/AI/README.md`: MCP guide добавлен в маршрутизацию и список действующих руководств.
- `Docs/AI/rules/ParallelRefactoringCoordination.md`: MCP явно подчинён общему эксклюзивному Unity-ресурсу и FIFO/write-lock.

Validation:
- Фактический audit шести project manifests: `com.unity.pipeline 0.5.0-exp.1` установлен только в `Novels`; catalog/template/gpl/tzm/zdm не подключены.
- Codex config: `unity_novels` указывает только на `/Users/iantonishin/Fork/SomeGame/Novels`.
- `unity pipeline list`: только живой `Novels`, Pipeline reachable.
- Links/required phrases audit и scoped `git diff --check`: успешно.

Pending / risks:
- Repo-level helper доступен другим проектам как код, но без установленного Pipeline endpoint не подключает их автоматически.
- Write-helper пока ищет coordination lock внутри target project; для атомарных проектов нужен отдельный `--coordination-root` scope до разрешения write-tools.
- Открытый Unity PID 8705 не изменялся и остаётся пользователю в Play Mode.

Suggested next step:
- Подключать MCP к отдельному atomic project только при реальной пользе, отдельным scope; не устанавливать во все проекты автоматически.

## 2026-08-29T09:08:44Z — android-version-editor-restart — completed

Task: проверить повторное появление client-version failure после правки Android Build Profile.

Changed:
- Project files не изменялись; выполнен полный restart Unity для перечитывания уже сохранённого `bundleVersion: 0.2.0`.

Validation:
- До restart повторный Play Mode действительно продолжал использовать cached `Application.version = 0.1.0`, несмотря на `0.2.0` в обоих settings-файлах.
- После полного restart Unity выполнены два последовательных цикла Play Mode.
- Первый свежий лог с позиции 615: catalog release активирован, `INITIALIZATION_FAILED`, version/schema failures отсутствуют.
- Второй свежий лог с позиции 784: catalog release активирован, `INITIALIZATION_FAILED`, version/schema failures отсутствуют.
- Unity PID 8705 оставлен в состоянии `playing`, compile/reload false.

Pending / risks:
- Старые Info-level ошибки остаются в накопленной Console как сообщения до restart; новые запуски чисты по проверяемому маршруту.
- AssetImportWorker дочерние процессы принадлежат открытому Editor и не являются вторым Editor.

Suggested next step:
- Продолжить ручное тестирование текущего второго запуска; версия теперь устойчива между Play Mode циклами.

## 2026-08-29T09:03:20Z — android-client-version — completed

Task: исправить runtime compatibility failure `content 0.2.0 / client 0.1.0`.

Changed:
- `Novels/Assets/Settings/Build Profiles/Android.asset`: только `bundleVersion` override изменён с `0.1.0` на `0.2.0`; существующие чужие cloud-поля сохранены.

Validation:
- Android Build Profile и `ProjectSettings.asset` теперь оба задают `0.2.0`.
- Unity импортировал asset; Editor status ready, compile/reload false.
- Новый Play Mode после правки: status `playing`; свежий участок `Editor.log` не содержит `INITIALIZATION_FAILED`, `Content requires client` или schema failure.
- Catalog release `c27d36aa...` успешно активирован после штатного восстановления отсутствующего cache-файла.
- Scoped `git diff --check`: успешно.

Pending / risks:
- Unity PID 7371 оставлен открытым в исправленном Play Mode для пользователя.
- Старая Info-level запись `INITIALIZATION_FAILED` остаётся в накопленной Console как история предыдущего запуска.

Suggested next step:
- Продолжить ручную проверку каталога; оценивать только сообщения с timestamp после 2026-08-29T09:02:36Z.

## 2026-08-29T08:57:48Z — catalog-playmode-retest — yielded

Task: перезапустить Novels после schema 2 fix и открыть каталог пользователю для тестирования.

Changed:
- Project files не изменялись; только собственные coordination records.

Validation:
- Unity PID 7371: Editor ready, Unity `6000.3.11f1`, сцена `Assets/Novels/Novels.unity` clean.
- До Play Mode Console errors: 0.
- После 10 секунд Play Mode: status `playing`, compile/reload false, Console errors: 0.
- Прежний `Unsupported story card schema version: 1` не повторился.

Pending / risks:
- Unity оставлен открытым в Play Mode и выведен на передний план для ручного тестирования пользователя.
- Пока Editor открыт, не запускать второй Editor/build для этого репозитория.

Suggested next step:
- Получить от пользователя результат visual/input проверки; затем отдельным заходом остановить Play Mode или внести точечные исправления.

## 2026-08-29T08:52:38Z — catalog-schema2-localcontent — completed-with-limitations

Task: исправить `INITIALIZATION_FAILED` из-за story-card schema 1 в Editor LocalContent.

Changed:
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`: story projects теперь валидируются по schema 2, как runtime contract.
- Generated Editor releases и `Novels/Build/LocalContent` пересобраны последовательно для catalog, TZM и ZDM.

Validation:
- Catalog/TZM/ZDM Unity batch builds: успешно; все три завершились `Atomic content ... built for editor` и exit 0.
- Composed registry и обе story cards: schema 2, minimum client `0.2.0`, genres присутствуют.
- `git diff --check` для контрактного блока: успешно.

Pending / risks:
- Повторный Play Mode smoke не завершён: два новых GUI-запуска Unity зависли до загрузки проекта на UPM IPC; зависшие процессы и helper остановлены.
- Runtime schema exception устранён по данным и единственному расходившемуся Editor validator path, но Console evidence после фикса пока отсутствует.

Suggested next step:
- После восстановления нормального GUI-старта открыть Novels и повторить Play Mode; ожидается каталог без `Unsupported story card schema version`.

## 2026-08-29T08:35:30Z — catalog-playmode-review — yielded

Task: подготовить каталог Novels в Play Mode для ручной проверки пользователя.

Changed:
- Project files не изменялись; только собственные coordination records.

Validation:
- Editor ready, сцена `Assets/Novels/Novels.unity` clean, до запуска 0 errors.
- Play Mode `playing`; после runtime startup Console также 0 errors.
- Unity PID 4595 оставлен открытым и выведен на передний план; helper остановлен.

Pending / risks:
- Ожидается ручной результат mouse/touch, phone/tablet, Safe Area, indicator,
  neighbour snap и Open→Continue.
- Пока Editor открыт, другим Unity-задачам нельзя запускать второй Editor/build.

Suggested next step:
- После ответа пользователя снова войти в FIFO: при замечаниях исправить точечно,
  при approval выйти из Play Mode и закрыть/оставить Editor по указанию.

## 2026-08-29T08:31:15Z — unity-mcp-editmode-tests — completed-with-limitations

Task: добавить bounded EditMode test workflow в Official Unity MCP fallback.

Changed:
- Helper manifest/source/tests/README: read-only list/status, lock-gated run,
  filter support, compact failures, Console/Git guards и empty-suite policy.

Validation:
- Syntax/unit tests: 19/19 passed.
- Без agent owner: немедленный `lock_not_owned`.
- Реальный Editor: `list_tests` нашёл 0 EditMode tests; run completed за 1 poll
  и корректно вернул non-success `outcome=no_tests`, без Console errors/Git delta.
- Сцена clean; helper/Editor/Hub/Licensing закрыты.

Pending / risks:
- В `Novels` сейчас нет EditMode tests, поэтому workflow готов, но ещё не может
  дать настоящий passing quality gate.

Suggested next step:
- Отдельным scope добавить минимальные тесты Catalog contracts/carousel logic,
  затем повторить `editmode-tests` до ненулевого `outcome=passed`.

## 2026-08-29T08:16:43Z — unity-mcp-compile-workflow — completed

Task: продолжить интеграцию Official Unity MCP fallback безопасным compile workflow.

Changed:
- `Tools/unity-mcp-helper/manifest.json`: добавлены lock-gated `recompile` и
  read-only `recompile_status`.
- Helper/README/tests: bounded `compile`, pre/post Console errors, Git-state
  guard и немедленная передача policy errors.

Validation:
- Syntax/unit tests: 14/14 passed.
- Без `--agent-id`: немедленный `lock_not_owned`, recompile не выполняется.
- С владельцем lock: `up_to_date`, 1 poll, 0 Console errors, no Git delta.
- Сцена clean; helper/Editor/Hub/Licensing закрыты.

Pending / risks:
- Проверен безопасный up-to-date путь; реальный domain reload/reconnect уже
  подтверждён предыдущим benchmark scope.
- Test runner и build commands всё ещё не allowlisted.

Suggested next step:
- Отдельным минимальным scope добавить bounded EditMode test workflow, затем
  использовать compile+tests как автоматический gate каталога.

## 2026-08-29T08:10:13Z — unity-mcp-fallback-benchmark — completed

Task: добавить воспроизводимый benchmark эффективности Official Unity MCP fallback
и проверить persistent/recovery свойства на реальном Editor.

Changed:
- `Tools/unity-mcp-helper/**`: команда `benchmark`, handshake instrumentation,
  character-size proxy, Git-state guard, тест и README.
- Только собственные coordination/parallel records; transient C# marker для
  domain reload удалён с восстановлением исходного content/diff hash.

Validation:
- Syntax/unit tests: 9/9 passed.
- 30 циклов × 3 tools: 90/90, median 97.30 ms, p95 100.86 ms, max 119.88 ms.
- Один persistent handshake; 3961 raw chars против 1661 summary chars,
  reduction proxy 58.07%; Git state во время benchmark не изменился.
- Domain reload и полный Editor restart (`99125` -> `884`) пережиты тем же
  daemon; после обоих `editor_status=ready`, handshake count остался 1.
- Финальная сцена clean, Console без errors; helper/Editor/Hub/Licensing закрыты.

Pending / risks:
- Token reduction является оценкой по символам, а не точным tokenizer count.
- Native namespace Codex Desktop по-прежнему не проверяется этим fallback benchmark.

Suggested next step:
- Использовать одинаковые 30 циклов как regression baseline; тревога при любом
  failure, новом handshake без recovery-события, p95 > 250 ms или Git delta.

## 2026-08-29T07:47:48Z — unity-mcp-fallback — completed

Task: реализовать безопасный persistent JSON-RPC fallback для Official Unity MCP,
который Codex Desktop пока не экспортирует как native namespace.

Changed:
- `Tools/unity-mcp-helper/**`: dependency-free Python client/daemon, manifest
  allowlist, compact summaries, timeout/reconnect/error policy, coordination
  guard, JSONL logging, fake MCP tests и README.
- Только собственные parallel/coordination records; Unity runtime, сцена и assets
  не менялись.

Validation:
- Unit tests: 8 passed, включая timeout, malformed JSON, process crash,
  unknown-tool deny и write-without-lock deny.
- Реальный persistent smoke одной MCP-сессией: `editor_status` ready,
  `get_scene_hierarchy` для чистой `Assets/Novels/Novels.unity`, `console` без
  errors и с двумя прежними warning.
- Compact Console summary подтверждён без stack traces; raw JSON доступен явно.
- Helper и Editor остановлены; scoped `git diff --check` успешен.

Pending / risks:
- Native `unity_novels` namespace остаётся проблемой Codex Desktop.
- Unity CLI/Pipeline experimental: при слишком старом descriptor CLI может его
  удалить; чистый Editor restart и быстрый старт helper подтверждены как recovery.
- Write-tools намеренно не добавлены: каждый требует отдельного scope, schema,
  dry-run/post-check и project write-lock.

Suggested next step:
- Использовать helper для read-only Editor проверок; расширять manifest только
  отдельными проверенными write-командами по мере реальной необходимости.

## 2026-08-28T17:43:16Z — unity-novels-mcp-verification — completed

Task: проверить появление `unity_novels` в новой Codex tool-сессии и выполнить
read-only MCP probe Editor status, текущей сцены и Console.

Changed:
- Только собственные runtime coordination records; сцена, assets, package files
  и project settings задачей не изменялись.

Validation:
- `~/.codex/config.toml` содержит `mcp_servers.unity_novels` с точным project path.
- Native callable namespace `unity_novels` в этой tool-сессии не экспортирован;
  MCP resources/templates его также не показывают, хотя Codex запускает stdio server processes.
- Прямой MCP stdio `initialize` (`2025-06-18`) и `tools/list` прошли; server
  `unity-mcp` версии `1.0.0-beta.5`.
- MCP `editor_status`: `ready`, Unity `6000.3.11f1`, compile/reload false,
  Play Mode stopped, точный project path `Novels`.
- MCP `get_scene_hierarchy`: `Assets/Novels/Novels.unity`, active, not dirty;
  пять roots: MainCamera, DirectionalLight, EventSystem, EntryPoint, CodeStrippingFix.
- MCP `console`: две warning-записи (не automated mode; deprecated Input Manager),
  errors отсутствуют.
- Запущенные задачей Editor PID 91211 и Hub PID 91231 штатно закрыты;
  Pipeline server больше не reachable, активных Editor/Hub/Licensing процессов нет.

Pending / risks:
- Причина остаётся на стороне Codex tool registration: config и MCP transport
  исправны, но native namespace в новом task не зарегистрирован.
- Unity Pipeline/CLI остаются experimental/beta.

Suggested next step:
- Проверить перезапуск всего Codex desktop host/app, а не только создание нового task;
  после появления namespace повторить один native `unity_novels.editor_status`.

## 2026-08-28T17:27:00Z — official-unity-mcp — completed

Task: установить официальный Unity CLI MCP/Pipeline в `Novels`, подключить
Codex и проверить reconnect после domain reload и перезапуска Editor.

Changed:
- `Novels/Packages/manifest.json`, `Novels/Packages/packages-lock.json`:
  установлен `com.unity.pipeline` `0.5.0-exp.1`.
- `/Users/iantonishin/.codex/config.toml`: добавлен отдельный MCP server
  `unity_novels`, существующий server другого Unity-проекта сохранён.
- Unity CLI analytics prompt: выбран opt-out.

Validation:
- `unity pipeline list`: Novels reachable на localhost:7800.
- Read-only probe: Unity `6000.3.11f1`, сцена `Assets/Novels/Novels.unity`,
  hierarchy из пяти root objects, сцена не dirty.
- Реальная C# recompilation/domain reload: Pipeline восстановился из
  `compiling` в `ready`; временная строка удалена, content diff отсутствует.
- Полный Editor restart: новый PID, Pipeline автоматически снова reachable.
- MCP stdio initialize `2025-06-18` и `tools/list`: успешно.
- Console после restart: новых errors нет; остаётся прежнее предупреждение о
  deprecated Input Manager.
- Scoped `git diff --check`: успешно.
- Editor, Hub и Licensing Client после проверки штатно завершены.

Pending / risks:
- Текущий Codex task не подхватит новый `unity_novels` tool до перезапуска
  Codex/нового task; транспорт проверен отдельным stdio handshake.
- Unity Pipeline и CLI имеют experimental/beta версии.

Suggested next step:
- Перезапустить Codex, выбрать `unity_novels` и выполнить один read-only вызов
  `editor_status` или `get_scene_hierarchy`.

## 2026-08-28T17:08:00Z — gpl-character-layers — yielded

Task: восстановить baked-master-first процесс и показать точные alpha-слои Леи.

Changed:
- Локально из одного утверждённого цельного master без resize/shift получены `master.png`, `hair.png`, `clothes.png` на холсте `1024x1536`.
- Hair/clothes содержат только исходные пиксели master; фон заменён настоящим alpha. Удалены skin fragments у лица, ушей и кистей.

Validation:
- Размеры всех слоёв: `1024x1536`; hair bbox `423,56–580,221`, clothes bbox `310,287–675,1470`.
- Трёхпанельный proof визуально проверен на нейтральной подложке.

Pending / risks:
- Project import не выполнялся; требуется visual approval пользователя.

Suggested next step:
- После approval импортировать exact master/hair/clothes и сделать reverse-composite proof в Unity scope.

## 2026-08-28T17:02:00Z — gpl-character-layers — yielded

Task: создать новый base-first master Леи с пропорциональной головой и проверить причёску.

Changed:
- Локально создан новый bald-base master с уменьшенной головой.
- Первая hair-only генерация имела правильный холст, но неверный content bbox; вторая стала хуже и отклонена.
- Первая компактная причёска зарегистрирована детерминированно без смены холста: `1024x1536`, bbox `430,108–575,300`; собрано preview base+hair.

Validation:
- Фактические размеры base и hair source: `1024x1536`.
- Визуальная сборка показывает корректный масштаб головы и отсутствие прежнего oversized-эффекта.

Pending / risks:
- Нужен пользовательский visual approval формы причёски до генерации одежды и импорта.

Suggested next step:
- После approval сохранить base/hair как project-bound alpha assets и создать одежду строго на этом master.

## 2026-08-28T16:56:00Z — gpl-character-layers — yielded

Task: продолжить послойную Лею через встроенный imagegen без API-ключа.

Changed:
- В локальном каталоге Codex созданы три технические bald-base итерации и детерминированные exact-pixel hair/clothes cutouts; игровых assets не импортировано.

Validation:
- Reverse composite на общем холсте выявил выступы базового тела за утверждённые рукава и обувь даже после targeted correction; набор отклонён.
- Установлена причина: baked-first референс не содержит скрытую анатомию, а встроенное редактирование не гарантирует её точную регистрацию.

Pending / risks:
- Продолжать исправлять baked-first основу локальными масками нельзя: это создаст одеждо-зависимое тело.

Suggested next step:
- Создать и утвердить новый канонический `base-first` master Леи, затем генерировать волосы и одежду строго как производные общего холста и проверять reverse composite.

## 2026-08-28T16:47:00Z — texture-compression-protocol — completed

Task: закрепить обязательные правила сжатия story textures.

Changed:
- `ContentAuthoringGuide.md`: добавлен production-контракт для location и
  character textures — единый postprocessor, ASTC 8×8, importer flags,
  Max Size, alpha, размеры вне block multiple, versioned reimport,
  последовательные builds, bundle/GPU measurements и visual gate.
- Choices, Presentation UI и desktop-профили явно исключены из механического
  наследования mobile story-art профиля.

Validation:
- Контракт сопоставлен с текущим `NovelContentTexturePostprocessor`:
  `GetVersion() = 6`, Android/iOS ASTC 8×8, Max Size 4096.
- Scoped `git diff --check` — успешно.

Pending / risks:
- Ручной visual quality gate текущего ASTC 8×8 остаётся отдельной задачей.

Suggested next step:
- Использовать новый раздел при любом добавлении арта или изменении texture
  profile; экспериментальные цифры вести только в size plan/baseline.

## 2026-08-28T16:41:00Z — official-unity-mcp — yielded

Task: установить официальный Unity CLI MCP/Pipeline в `Novels`, подключить
Codex и проверить reconnect после domain reload и перезапуска Editor.

Changed:
- `Novels/Packages/manifest.json`: добавлен `com.unity.pipeline` версии
  `0.5.0-exp.1` штатной командой `unity pipeline install`.

Validation:
- `unity auth status`: вход подтверждён.
- `unity pipeline list`: package обнаружен, server пока не reachable.

Pending / risks:
- Импорт пакета и `packages-lock.json` не завершены, потому что Unity Editor
  занят другим проектом `/Users/iantonishin/Kids/.worktrees/skazbuka-unity-mcp-pilot`.
- Codex MCP configuration, connection probe, domain reload и Editor restart
  ещё не выполнены.

Suggested next step:
- После освобождения единственного Unity Editor снова войти в FIFO, открыть
  только `Novels`, дождаться reachable Pipeline server и продолжить пилот.

Этот файл содержит только актуальное незавершённое состояние. Завершённая
история до ротации 2026-08-28 находится в
[`CoordinationHandoffHistory-through-2026-08-28.md`](CoordinationHandoffHistory-through-2026-08-28.md).

Перед работой прочитайте этот файл полностью, затем проверьте утверждения по
текущим файлам, `git status --short`, runtime FIFO и write-lock. Архивный
handoff читается только при расследовании конкретного прежнего решения.

## Текущее состояние

- Runtime FIFO-заявок нет.
- Активного write-lock нет после завершения текущей cleanup-задачи.
- Единственная незавершённая архитектурная работа — WebGL local prototype:
  branch `prototype/webgl-local-platform`, commit `cfb92896` отсутствует в
  `main`; Unity compilation и browser smoke ещё не выполнены.
- Windows Player уже интегрирован в `main` через `f849ff22`; прежний статус
  `ready-for-integration` закрыт как устаревший.
- Story preview merge уже интегрирован через `60a13762`; прежний статус
  `active` закрыт.
- Bundle audit присутствует в `main`; прежний статус
  `ready-for-integration` закрыт.
- Локальный `main` содержит cleanup commit `6d15bec6`, который ещё не был
  отправлен в `origin/main` на момент создания этого снимка.
- В рабочем дереве сохраняются посторонние пользовательские изменения
  `.DS_Store`, `Novels/ProjectSettings/ProjectSettings.asset` и
  `Projects/novels-tzm/ProjectSettings/PackageManagerSettings.asset`; не
  включать и не откатывать их без отдельного запроса.

## Pending / risks

- WebGL prototype требует восстановления стабильного Unity Licensing,
  последовательной Unity-компиляции и browser smoke перед интеграцией.
- Перед публикацией локальных documentation commits повторно проверить
  `origin/main` и точный staged diff.

## Suggested next step

- Для обычной работы следовать `Docs/AI/README.md` и не читать архивную
  историю без конкретной причины.
- Для WebGL продолжить единственную запись из `work/parallel/` после проверки
  лицензии.

## 2026-08-28T14:51:08Z — fallback-wardrobe-background — yielded

Task: добавить собственный фон и обновить системный fallback гардероба по
утверждённому мокапу без переключения персонажей.

Changed:
- `OptionSelection`: добавлен отдельный wardrobe-layout поверх прежнего
  универсального экрана выбора; обычный Choose не изменён визуально.
- Гардероб получил собственный фон между location и character canvases,
  nameplate персонажа, категории, стрелки вариантов и сворачивание панели.
- `WardrobePresentation` теперь явно передаёт имя персонажа и активную
  категорию; live preview и подтверждение используют прежние callbacks.
- Ink и контентные проекты не изменялись этой задачей.

Validation:
- Scoped `git diff --check`: успешно.
- Fallback `dotnet build Novels/Novels.csproj --no-restore`: успешно,
  0 warnings, 0 errors.
- Unity batch compile остановлен после повторяемого licensing IPC
  `Unsupported protocol version '1.18.0'`; до визуальной проверки не дошёл.

Pending / risks:
- После восстановления Unity Licensing нужны Editor compilation и ручная
  проверка layout в развёрнутом и свёрнутом состояниях.
- Кнопка снятия элемента намеренно не реализована без безопасного Ink-choice
  контракта пустого выбора.

Suggested next step:
- Открыть Novels после исправления лицензии, перейти к первому гардеробу TZM и
  проверить фон, перекрытие персонажа, категории, стрелки и collapse.

## 2026-08-28T13:31:00Z — character-hairstyle-protocol — completed

Task: дополнить общий протокол правилами проектирования причёсок.

Changed:
- `Docs/AI/rules/CharacterLayeringRules.md`: добавлены утверждение
  причёски на собранном персонаже, контроль естественного объёма, обязательное
  проектирование front/back и общее превью состава эпизода.

Validation:
- `git diff --check -- Docs/AI/rules/CharacterLayeringRules.md` — успешно.
- Commit `1671fd66` содержит только канонический документ правил.

Pending / risks:
- Публикация `main` в `origin/main` выполняется в этой же задаче.

Suggested next step:
- Использовать обновлённый порядок до производства игровых alpha-слоёв.

## 2026-08-28T13:48:00Z — gpl-project-bootstrap — completed

Task: создать атомарный Unity-проект истории «Голос подо льдом».

Changed:
- `Projects/novels-gpl/**`: Unity 6000.3.11f1 story-project с ID `gpl`,
  карточкой, обложкой, definition, Ink первого эпизода и пустыми каталогами
  для последовательного наполнения.
- `Docs/AI/work/parallel/ParallelWork.gpl-project-bootstrap.md`:
  зафиксированы scope и проверки проекта.

Validation:
- `Tools/novels-tools/novels-content doctor` — успешно.
- Unity import и компиляция runtime/editor assemblies — успешно.
- `Tools/novels-tools/novels-content validate gpl` — успешно.
- `git diff --check -- Projects/novels-gpl` — успешно.

Pending / risks:
- В Ink пока включён только пролог без ссылок на незавершённый арт; дальнейшие
  сцены добавлять синхронно с утверждённым episode asset list.
- Локальный commit нового проекта потребуется опубликовать отдельно, если это
  не будет сделано в текущем пользовательском запросе.

Suggested next step:
- Начать с полного переноса текста эпизода 1 в Ink либо с первого утверждённого
  фона и затем персонажных alpha-слоёв.

## 2026-08-28T13:58:00Z — gpl-episode1-ink — completed

Task: перенести полный текст эпизода 1 «Нулевая глубина» в GPL Ink.

Changed:
- `Projects/novels-gpl/Assets/Ink/s01e01.ink`: полный эпизод, четыре выбора и
  четыре переносимых строковых состояния.
- `Projects/novels-gpl/Assets/Ink/gpl.ink.json`: пересобранный runtime Ink.
- `Projects/novels-gpl/Assets/Ink/gpl.ink.json.source-map.json`: актуальная
  связь runtime-путей с исходными строками.

Validation:
- Ink compilation — успешно.
- `Tools/novels-tools/novels-content validate gpl` — успешно.
- Все 12 вариантов четырёх выборов имеют стабильные labels и assignments.

Pending / risks:
- В Ink намеренно ещё нет команд `Локация`: фоны подключаются только после
  добавления утверждённых PNG.
- Реплики Леи, Марка и Веры уже канонические; runtime использует fallback до
  появления их персонажных слоёв.
- Изменения validated, но ещё не закоммичены: после освобождения lock в FIFO
  уже ожидала независимая задача `android-astc8`; не включать её файлы в GPL
  commit.

Suggested next step:
- Добавить первый утверждённый фон медблока и затем по одному персонажному
  набору с обратной сборкой.

## 2026-08-28T14:23:00Z — android-astc8 — ready-with-limitations

Task: перевести Android story art на ASTC 8×8 через общий postprocessor.

Changed:
- `NovelContentTexturePostprocessor`: Android ASTC 8×8 вместо 6×6, версия
  postprocessor 6 для принудительного реимпорта.
- Authoring и content-size документы синхронизированы с новым профилем.

Validation:
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` по scope задачи — успешно.
- TZM Android build запускался как единственная тяжёлая Unity-задача.

Pending / risks:
- Unity Licensing Client не завершил batchmode: `Unsupported protocol version
  '1.18.0'`, затем отсутствует entitlement `com.unity.editor.headless`.
- Размеры TZM/ZDM и визуальное качество ещё не измерены; после восстановления
  лицензии последовательно пересобрать обе истории и проверить лица, волосы,
  одежду, alpha-края и градиенты.

Suggested next step:
- Восстановить Unity headless entitlement/совместимость Licensing Client, затем
  выполнить `novels-content build tzm android` и `zdm android` последовательно.

## 2026-08-28T15:02:00Z — gpl-episode1-art — ready-with-limitations

Task: добавить фоны и персонажей первого эпизода GPL.

Changed:
- `Projects/novels-gpl/Assets/Locations/**`: добавлены ровно четыре утверждённых фона — медблок, коридор, столовая и нижний уровень/гермодверь буровой — со стабильными Unity GUID.
- `Projects/novels-gpl/Assets/Ink/s01e01.ink`: добавлены пять команд `Локация`; сцена решения повторно использует столовую.

Validation:
- Размер всех фонов 1672×941, alpha не требуется.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` по GPL scope — успешно.
- Unity `validate gpl` запущен и остановлен после повторной ошибки Licensing Client: unsupported protocol `1.18.0`, затем отсутствует `com.unity.editor.headless`.

Pending / risks:
- `gpl.ink.json` и source map ещё не перекомпилированы с командами локаций.
- Последний утверждённый дизайн Леи, Марка и Веры существует только на слитном превью; старый layered-набор содержит отвергнутые причёски.
- Две попытки background-extraction создали PNG без настоящего alpha и были отклонены, в проект не импортированы.

Suggested next step:
- После восстановления Unity Licensing выполнить импорт/компиляцию и `validate gpl`.
- Затем подготовить зарегистрированные слои персонажей с настоящим alpha и доказанной обратной сборкой.

## 2026-08-28T15:14:00Z — android-astc8-license — completed

Task: восстановить Unity Licensing IPC, завершить Android ASTC 8×8 builds и
усилить канонический recovery-протокол.

Changed:
- Licensing guide теперь требует один автономный recovery-loop: evidence,
  штатное закрытие Hub, точные stale sockets, повтор исходной Unity-команды и
  автоматическое продолжение при успехе.
- Android ASTC baseline дополнен фактическими текущими totals: TZM 27 792 318 B
  в 12 chunks, ZDM 56 445 255 B в 25 chunks.
- Android ASTC status переведён в `ready-for-integration`.

Validation:
- Подтверждён конфликт Hub Licensing Client PID 51721 со входящим LocalIPC 1.18.
- Hub закрыт штатно; удалены только два точных stale socket без владельца:
  `Unity-LicenseClient-Luwu4XyaHQ3HNH0tGlt-W.sock` и notifications socket.
- `novels-content build tzm android` и `build zdm android` — успешно и
  последовательно; новых protocol/headless licensing ошибок нет.
- `novels-content doctor` и scoped `git diff --check` — успешно.

Pending / risks:
- Старый Android baseline монолитный, а текущий pipeline chunked и исключает
  unused content; общую delta нельзя считать чистым ASTC A/B.
- Нужен ручной visual quality gate лиц, волос, одежды, alpha-краёв и градиентов.

Suggested next step:
- Выполнить visual quality gate и интегрировать scoped Android ASTC/licensing
  изменения, не включая соседний dirty tree.

## 2026-08-28T15:22:00Z — gpl-episode1-art — backgrounds-completed

Task: продолжить GPL art после восстановления очереди и Unity Licensing.

Changed:
- `Projects/novels-gpl/Assets/Locations/**`: Unity импортировал четыре утверждённых фона и нормализовал texture `.meta`.
- `Projects/novels-gpl/Assets/Ink/gpl.ink.json` и source map: явно перекомпилированы; содержат пять команд `Локация`.

Validation:
- `Tools/novels-tools/novels-content validate gpl` — успешно после явной компиляции.
- `git diff --check` по GPL scope — успешно.

Pending / risks:
- Персонажи ещё не добавлены. Последний утверждённый состав существует на слитном превью; старый layered-набор не соответствует утверждённым причёскам.
- Встроенный image generation дважды создал фон без настоящего alpha. Для точного продолжения требуется явное разрешение применить детерминированное локальное маскирование/сегментацию, затем выполнить послойную обратную сборку.

Suggested next step:
- После разрешения подготовить Lea/Mark/Vera layers на общем холсте, проверить `hasAlpha`, reverse composite и импортировать в GPL.
## 2026-08-28T15:13:00Z — fallback-wardrobe-background — ready-for-integration

Task: добавить собственный фон и обновить системный fallback гардероба по
утверждённому мокапу.

Changed:
- Wardrobe использует отдельную компоновку существующего OptionListScreen;
  обычный Choose не изменён.
- Добавлены системный нейтральный фон между location и character canvases,
  имя персонажа, категории с активным состоянием, стрелки вариантов и
  сворачивание нижней панели.
- Ink и content assets не изменялись.

Validation:
- `git diff --check` по scope — успешно.
- Unity 6000.3.11 batch compile — успешно после восстановления Licensing IPC;
  C# ошибок нет, выход штатный.

Pending / risks:
- Нужен ручной visual check в Play Mode на 1080×1920.
- Кнопка снятия элемента не добавлена: текущий Ink-choice contract не имеет
  безопасного отдельного решения для подтверждения пустого выбора.

## 2026-08-28T15:42:00Z — catalog-mockup-parity — ready-with-limitations

Task: реализовать пункты 1–5 catalog mockup: progress-aware CTA, отдельную
кнопку, индикатор страниц, жанры и Safe Area без валют и нижней навигации.

Changed:
- Catalog SDK/Game: центральный focus обновляет page indicator и отдельный CTA;
  первый запуск истории создаёт started marker, существующие save/progress files
  распознаются как legacy progress; статус становится «Продолжить».
- Story-card contract: schema 2 с обязательным `genre`; TZM и ZDM получили
  канонические жанры из Ink metadata.
- Catalog prefab: добавлены `SafeArea`, `PageIndicator`, `OpenButton`; background
  остаётся полноэкранным; Catalog/Game/story minimum client синхронизирован на
  `0.2.0`, Game `bundleVersion` также `0.2.0`.

Validation:
- `Tools/novels-tools/novels-content doctor` — успешно.
- Unity Bee Roslyn: `Novels.Catalog.Contracts`, `Novels.Catalog`, `Novels` —
  успешно, без compiler errors.
- JSON schema/version assertions, prefab local fileID audit и scoped
  `git diff --check` — успешно.
- Unity batch `validate catalog` повторён дважды вне sandbox, но Licensing Client
  потерял IPC/headless entitlement; второй запуск завершился disposed
  `IServiceProvider`, поэтому validate/build не подтверждены.

Pending / risks:
- После стабильного Licensing нужны `validate catalog`, `build catalog editor`,
  Game compile и ручной Play Mode gate: mouse/touch, phone/tablet, Safe Area,
  page indicator, neighbour centering, Open→Continue после возврата.
- Интегрировать SDK + Game + Catalog + TZM/ZDM cards атомарно из-за schema 2.

Suggested next step:
- Восстановить один стабильный Licensing Client и выполнить перечисленный
  последовательный quality gate без параллельного Unity процесса.

## 2026-08-28T15:45:00Z — licensing-serialization-rule — completed

Task: закрепить профилактическое правило последовательного lifecycle Unity.

Changed:
- Licensing guide запрещает одновременный запуск и завершение Hub, Editor и
  batch-mode.
- Добавлен обязательный процессный и IPC-барьер: готовность подтверждается
  повторной проверкой процессов и владельца канала, а не фиксированной паузой.
- Hub запускается только после успешного handshake Editor/batch-mode либо для
  явного входа или активации.

Validation:
- Scoped `git diff --check` — успешно.

## 2026-08-28T17:00:00Z — gpl-character-layers — blocked

Task: продолжить точное API/mask-редактирование и импорт слоёв персонажей GPL episode 1.

Changed:
- `Docs/AI/rules/CharacterLayeringRules.md`: ранее добавлен резервный chroma/masking-контракт; новых игровых assets в этой попытке не импортировано.
- В локальном рабочем каталоге Codex собраны и проверены edit-mask и контрольная красная накладка для Леи.

Validation:
- Bundled image CLI `edit --dry-run` — успешно, payload использует `gpt-image-2`, исходник 1024×1536 и alpha-mask.
- Визуальная проверка overlay — волосы и одежда покрыты; перед финальным вызовом требуется дополнительно защитить пальцы.
- Проверка окружения — `OPENAI_API_KEY` отсутствует, пакет `openai` в bundled runtime отсутствует.

Pending / risks:
- Реальный API edit не запускался; нельзя импортировать прежний независимо сгенерированный base из-за несовпадения анатомии и регистрации.
- Lock и собственная FIFO-заявка освобождены на время ожидания доступа к API.

Suggested next step:
- После доступности `OPENAI_API_KEY` установить/предоставить `openai`, создать новую FIFO-заявку, защитить пальцы в маске и выполнить подготовленный CLI edit.

## 2026-08-29T10:35:00Z — wardrobe-interaction-fix — ready-for-integration

Task: исправить невидимого персонажа и неработающие вкладки нового fallback-гардероба.

Changed:
- Wardrobe backdrop вынесен из верхнего OptionList canvas в отдельный canvas
  между location и character; персонаж больше не перекрывается фоном.
- Четыре вкладки стали интерактивными и читают доступные face/hair/clothes/
  accessory варианты из sprite-trim manifest истории.
- Варианты показываются live preview. Только исходная сюжетная категория
  подтверждает Ink-choice; Ink и обычный Choose не менялись.

Validation:
- Unity 6000.3.11f1 batch compile — успешно, exit code 0.
- Licensing handshake — успешно; `git diff --check` по scope — успешно.

Pending / risks:
- Нужен ручной Play Mode check на 1080×1920 для визуального и input gate.
