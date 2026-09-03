# Current cross-chat handoff

Только актуальное незавершённое состояние. Предыдущий snapshot: Git commit `b910242f`; история: [`CoordinationHandoffHistory-through-2026-08-30.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-30.md), [`CoordinationHandoffHistory-2026-09-01.md`](../archive/reports/CoordinationHandoffHistory-2026-09-01.md), [`CoordinationHandoffHistory-2026-09-02.md`](../archive/reports/CoordinationHandoffHistory-2026-09-02.md).
## Ready for integration or validation
- `publish-all-local-main`: весь локальный dirty tree опубликован в `origin/main` как атомарные tooling/docs, runtime/shared packages и production content commits с последующим non-force merge четырёх remote-коммитов. `git diff --check`, automation tests, editor content builds для catalog и всех 12 stories, fresh Novels Editor compile и finish-check passed; EditMode test assemblies отсутствуют. После одного штатного licensing recovery-loop (TERM только конфликтующего PID 57057; license/cache/socket не удалялись) Unity gates стабильны. `localSha == remoteSha == 8a75e39e9bb0f976311d18d472b2762d221c07a7`; существующие manual visual gates остаются в handoff.
- `tzm-s01e01-smoke-fixes`: устранён источник массового fallback Салли — несуществующие в каноническом и переданном read-only арт-наборе `Сандали` больше не применяются как аксессуар; исправлена опечатка эмоции `снисхождение`. Финальный экран получил уменьшенный end-of-episode текст, сдвиг единственной кнопки и принудительный layout rebuild в TZM bubble variant. `novels-content validate tzm`, `git diff --check` и два Android Embedded player build passed. Первый контрольный проход дошёл до `s01e01.ink:1905` без `fallback.used`; финальный APK запущен без crash/error/fallback, но повторный полный проход остановился у сохранённого последнего кадра `s01e01.ink:169`: video layer перекрывает следующий dialogue и не принимает продолжение. Это отдельный runtime-дефект `Кат-сцена (стоп)`, поэтому визуальная проверка нового финального layout остаётся pending. Evidence: `Novels/Build/Players/automation/Android/Embedded/Novels.apk`, `Novels/Build/Logs/automation/tzm-s01e01-smoke-fixes-20260902.log`, `Novels/Build/Logs/automation/tzm-s01e01-smoke-fixes-blocked-stop-frame-20260902.png`.
- `gpl-episode2-art-integration-smoke`: commit `a92ba3f0` завершил арт эпизода: три фона и четыре цельных Павла (`main`, раненый, с рычагом, двойник), Ink selectors, meta и full/face dark/light contact sheets. Alpha/dimensions/GUID и визуальные края passed. Повторный GPL validate заблокирован Unity licensing (`LicenseClient-iantonishin` channel отсутствует, headless license не найден); own batch остановлен, lockfile удалён. После восстановления Hub нужны validate, compiled Ink, editor/android content, Embedded APK и episode-two smoke.
- `free-wardrobe-equipped-index`: свободная категория теперь открывается на фактически equipped значении из save, поэтому подпись compact carousel сразу соответствует волосам/одежде персонажа. Initial preview остаётся выключен: открытие вкладки не меняет образ. При отсутствии equipped-записи используется эффективный explicit/default стиль CharacterController, а не item с индексом 0; для TZM это `Распущенные`. Fresh Novels compile passed; нужен visual check.
- `free-wardrobe-custom-presentation-guard`: свободный гардероб остаётся доступным поверх кастомного сюжетного образа, но рендерит отдельный neutral target-aware preview; при закрытии восстанавливаются исходные story request, target, visibility и position. API принимает героя для будущего multi-character UI. `Wardrobe`-рендер дополнительно изолирован от story appearance cache, поэтому эмоция/поза сбрасываются в preview и возвращаются после закрытия. Fresh Novels compile passed; нужен visual open/close на строке с ночнушкой.
- `wardrobe-unlock-all-options`: scripted wardrobe теперь разблокирует все показанные варианты текущей категории, сохраняя выбранный надетым; обычные choices без wardrobe action остаются исключены, asset-фильтрация сохранена. Fresh Novels compile passed; нужен replay сюжетного и свободного гардероба.
- `wardrobe-choice-filter`: обычные `Выбор предмета` без явного wardrobe action больше не разблокируют вещи; при загрузке свободной категории старые записи сверяются с реальными assets и удаляются из save. Fresh Novels compile passed; нужен replay завтрака и повторное открытие свободного гардероба.
- `ink-line-overlay`: возвращён development-only overlay `Ink: файл:строка`; source map снова проходит через simple-layout release и runtime. TZM editor content rebuilt, Mac release содержит map, fresh Novels compile passed; Editor открыт. Нужен visual replay истории.
- `tzm-ink-typos`: в `s01e01–s01e06.ink` исправлены 27 строк с орфографией, 41 строка с пунктуацией и 13 безопасных alias-source ссылок приведены к canonical video/character targets. Строки с `TODO`, semantic fallback aliases, `tzm.asset`, Ink structure и generated JSON не менялись. 21 alias теперь source-unused и является кандидатом на отдельное удаление после compile/validation и решения по save compatibility. Scoped `git diff --check` passed.
### UI and character runtime

- `tzm-wardrobe-prefab-variant`: TZM asset пересохранён Unity как настоящий `PrefabAssetType.Variant`, наследующий общий Resources fallback GUID `c70fbd96d8d6443329e9d10a73f0428a`; в нём остались только style/layout overrides, без root-transform overrides. Светлая панель высотой 600, синие sliced controls, Liberation Sans и разнесённые tabs/title/item/buttons ближе к референсу; fallback сохраняет тёмный layout. После визуального воспроизведения найдено, что prefab был импортирован, но не назначен content chunk и потому отсутствовал в release. GUID `4bb1c2193bcf4b238a510594d07de05f` добавлен в bootstrap chunk TZM; свежий editor build и composed `release.json` теперь содержат `story/presentation/wardrobe/screen-variant.prefab`, attach compile Novels passed. Нужен новый Play/episode restart для portrait visual replay.
- `wardrobe-reference-parity`: свободный гардероб приведён к функциональной структуре референса без смены сюжетного фона: icon-like tabs с counts, верхний nameplate/heart/crown, отдельные боковые стрелки героев, item arrows, collapse, `Готово` и `×`. Preview теперь транзакционный: `Готово` одним save-снимком фиксирует весь комплект всех просмотренных героев, `×` возвращает исходные appearance/hair/clothes/accessory; простое открытие вкладки ничего не меняет. Герои берутся из реально unlocked wardrobe state, их выбранный образ восстанавливается между сессиями и применяется при обычном сюжетном рендере. Сердце пока неактивный relationship badge: отдельной системы отношений в runtime нет; crown и центральная стрелка сворачивают панель. Scoped diff-check и fresh Unity compile passed. Нужен portrait visual replay; общий `verify` отдельно блокируют чужие GPL `.meta` с trailing whitespace.
- `tzm-hide-empty-wardrobe-tabs`: scripted wardrobe скрывает категории, которых нет в доступном наборе вкладок; временно заблокированные, но применимые категории остаются видимыми. Tooling/helper tests 43/43 и live Unity compile passed; нужен visual gate в уже открытом Editor.
- `wardrobe-tab-preview`: переключение вкладки и начальное позиционирование карусели больше не применяют вариант автоматически; preview остаётся для явного тапа и реального перехода на другую карточку. Tests 43/43 и live Unity compile passed; нужен visual gate в открытом Editor.
- `tzm-compact-wardrobe`: первая компактная итерация скрывает карточки в wardrobe-layout, оставляет название варианта, стрелки и confirm, уменьшает нижнюю панель и показывает количества вариантов на доступных scripted tabs. Иконок в assets нет, поэтому tabs пока текстовые. Tests 43/43 и live Unity compile passed; нужен visual gate в открытом Editor.
- `tzm-wardrobe-location-background`: отдельный fullscreen WardrobeBackdrop и glow удалены; гардероб теперь сохраняет текущую сюжетную локацию за персонажем и компактной панелью. Первый attach gate не нашёл старый Pipeline port; fresh Editor compile затем passed без compiler errors. Нужен visual gate в оставленном открытым Editor.
- `tzm-initial-wardrobe-preview`: первый scripted wardrobe page теперь явно применяет стартовый item один раз, поэтому main-character view задаётся до показа; последующие tab switches остаются non-mutating. Первый attach gate подтвердил compile, но увидел старый fallback.used из repro; после fresh Editor restart полный gate passed без compiler errors. Нужен visual replay.
- `free-wardrobe-empty-tabs`: свободный гардероб теперь вычисляет доступные категории из unlocked items конкретного персонажа, выбирает валидную начальную вкладку и скрывает/отклоняет пустые категории. Tests 43/43 passed; stale Editor transport failed, fresh Editor compile затем passed. Нужен visual replay отдельного гардероба.
- `gpl-fullbody-character-offset`: добавлен локальный GPL character-screen variant со сдвигом дочернего `Viewport` на `Y = -220`; первая попытка на root Screen Space Overlay Canvas не влияла на рендер и заменена. Общий SDK, TZM и ZDM не менялись. GPL editor release `33ca6c97…e0c7` собран и composed в LocalContent; fresh Novels Editor compile passed, Editor оставлен открытым для visual replay.
- `fallback-bubble-runtime-rebuild`: глобальный rebuild, сломавший геометрию custom TZM bubble, удалён. `BubbleScreen` теперь имеет opt-in флаг; layout overrides и rebuild включены только в game fallback `Novels/Fallbacks/.../bubble/screen-variant`. Shared base и TZM prefab не изменены. TZM release `48adc0d9…c192`, GPL release `33ca6c97…e0c7` собраны; fresh Novels compile passed. Нужен visual replay обоих кадров.
## Blocked / limitations
- `gpl-catalog-registration`: `gpl` добавлен третьей story; catalog editor build и GPL validation passed, composed registry содержит `tzm/zdm/gpl`. Исправлен catalog dispatch в `AtomicContentBuild`. Устранён `CONTENT_PREPARATION_FAILED`: обязательный GPL setting `screen.prefab` и реальный Liberation Sans добавлены в chunk 0, release `7fe8…7283`; built-in font rejected by bundle, fresh compile passed. `CharacterController` теперь начинает с base view `view`, устраняя Lea fallback без стартового appearance-choice. Ошибочный TZM bubble удалён из GPL; общий fallback теперь учитывает высоту dialogue перед choices, GPL release снова `7fe8…7283`; fresh compile passed. Нужен Play replay.
- `tzm-wardrobe-runtime`: implementation compiles and TZM content-gate passes without Ink changes. The initial consecutive appearance/hair/clothes choices now open as one tabbed scripted transaction containing only Ink-provided options; selections persist while switching tabs and unrelated tabs stay hidden. Одиночный scripted wardrobe также показывает только текущую категорию; multi-page/free режимы сохранены. Fresh Unity compile passed; Editor открыт. Pending: user visual check, then scoped integration/commit.
- `gpl-lea-layered-rework` — ready: общий head/hair и facial patches отделены от четырёх clothes-слоёв точной мягкой маской по нижней челюсти; вся шея/ворот в одежде. Jaw dark/light proof, GPL validate/editor build passed; нужен visual gate.
- `gpl-mark-integration`: 8 цельных вариантов Марка импортированы как station и polar whole-наборы; 7 station selectors привязаны к episode 1. GPL validate/editor build passed; нужен bounded visual gate в игре.
- `gpl-vera-integration`: Вера импортирована как layered head + 2 clothes + 3 facial emotions; `urgent`, `hides_hands`, `pain_pose` остаются whole-вариантами. GPL validate/editor build passed; нужен bounded visual gate в игре.
- `catalog-playmode-review`: paused до явного продолжения ручного visual review.
- WebGL prototype остаётся только в `prototype/webgl-local-platform`, commit `cfb92896`; compilation и browser smoke не выполнены.
## Active validation handoff
- `tzm-episode1-android-smoke`: Android Embedded APK собран и установлен на `Novels_Pixel_7_API_34`; полный `s01e01` дошёл до `episode.completed` и `catalog.returned` без crash/ANR. Ограничения: 35 `fallback.used` для Салли (`required_character_assets_missing`), перекрытие текста/кнопки финального экрана; повторный `novels-content validate tzm` завис после старта Unity и остановлен через 3 минуты (content compile в составе player-build прошёл). APK и smoke-артефакты в `Novels/Build`; приложение остановлено, AVD оставлен запущенным.
- `gpl-story-continuity-fixes`: все пять GPL Ink логически согласованы без смены knot/choice IDs; `validate gpl` и `build gpl editor` passed, шесть bundle audits passed, compiled JSON/source map (3356 mappings) и `gpl.asset` проверены. Код, арт, `.meta` и другие истории не менялись.
- `fallback-prefab-wrapper-removal`: `Novels.unity` теперь напрямую использует shared base prefab для loading/location/character/notification, как уже делал bubble; четыре пустых game wrapper variants и metadata опустевших папок удалены. Notification font asset сохранён, потому что shared prefab реально ссылается на его GUID. Removed-GUID audit, shared root fileID audit, scoped diff-check и attached Novels compile passed; story-specific variants не менялись.
- `tzm-wardrobe-reference-parity`: approved softened sprite kit remains integrated into the real TZM prefab variant, still inheriting shared fallback GUID `c70fbd96d8d6443329e9d10a73f0428a`. Item arrows now follow their click direction: the authored left chevron is used unchanged for previous and mirrored only for next. Expanded collapse position is inherited through an optional serialized Y: fallback keeps 635, TZM overrides 700, leaving a 27 px gap above the 625 px panel with its 96 px control. TZM validate/editor build, release asset audit, scoped diff-check and attached Novels compile passed; Editor remains open for portrait visual replay.
- `gpl-episode3-full-smoke`: paused at a safe emulator checkpoint by user request so higher-priority `com.zebrainy.skazbuka` smoke can run. Episode-three Ink/art integration and full Embedded APK are complete; GPL content validation passed. Emulator smoke completed `gpl/s01e01` and `gpl/s01e02` with catalog returns. `gpl/s01e03` reached its third decision (`s01e03.ink:257`, run `9e34be64f3a146ef995a70dcc4c4606b`, seq 93) before the other app took foreground; GPL PID 16869 was still alive. Resume only after re-entering FIFO and acquiring emulator/write scope; finish episode 3 and final evidence.
- `android-memory-full-smoke`: paused because the running Embedded APK contains stale content and does not recognize the episode end marker. Preliminary emulator stress data: GPL ~335 MiB PSS; TZM plateau ~374 MiB PSS / 464 MiB RSS; swap negligible. Emulator lacks ASTC8x8 and expands textures to RGBA. Resume after a full Embedded content rebuild and final episode pass begins.
## 2026-09-01T11:05:30Z — tzm-fallback-prefabs-reset — manual gate

TZM Mac/editor release пересобран из уже настроенного fallback-state: custom
wardrobe prefab и девять его sprites отсутствуют в release, исходники сохранены
как authoring-unused. Novels compile clean; Editor PID `56234` оставлен открытым.

## 2026-09-01T11:30:13Z — tzm-bubble-fallback-reset — manual gate

Предыдущий reset затронул только wardrobe. Теперь TZM bubble prefab и шесть
локальных PNG также переведены из chunk 0 в authoring-unused; свежий Mac release
не содержит `presentation/bubble`. Compile clean; Editor PID `59162` открыт.

## 2026-09-01T11:38:56Z — fallback-bubble-size-parity — manual gate

Fallback bubble больше не включает forced rebuild и пять layout overrides,
которые растягивали текст и choices отдельными полосами. Он снова наследует
фиксированную геометрию base/TZM prefab. Compile clean; нужен Play Mode restart.

## 2026-09-01T12:00:33Z — bubble-fallback-inheritance — manual gate

Bubble приведён к той же схеме, что wardrobe: `Novels.unity` использует общий
package-prefab `BaseUI/Base/bubble/screen.prefab` как готовый fallback, а TZM
наследуется непосредственно от него. Пустая game-only wrapper-variant удалена;
её старый GUID больше не используется. Scoped diff-check и live Novels compile
passed; нужен Play Mode restart и визуальная проверка fallback bubble.

## 2026-09-01T15:51:18Z — gpl-vera-pavel-face-left — ready-for-integration
Task: закрепить экранное направление всех второстепенных персонажей GPL без runtime-кода.
Changed: правило GPL сохранено в `CharacterLayeringRules`; source/proof/runtime Сигрид, Веры и Павла детерминированно отражены влево, порядок ячеек contact sheets и Лея не менялись.
Validation: визуальные контакты, runtime alpha/dimensions, Pavel source/runtime parity и GPL validate/editor build passed.
Pending / risks: только bounded in-game visual gate вместе с интеграцией эпизодов 4–5; next step — включить этот gate в общий GPL smoke.

## 2026-09-01T15:33:00Z — tzm-ink-todos-video-phil — ready-for-integration

В Ink добавлен русский аргумент кат-сцены `стоп`; «Вид из окна» удерживает
последний кадр. Выполнены согласованные TODO камеры, позиции Фила, видео пляжа и
причёски Салли. У Фила neutral body теперь `main`, angry body — `злость`; оба
PNG/GUID сохранены, старый адрес `основной` совместимо указывает на `main`.
Scoped diff/GUID/hash audit, визуальная сверка двух тел, TZM validation и fresh
Novels compile passed. Остальные TODO не менялись; нужен только in-game replay.

## 2026-09-01T15:55:00Z — tzm-ink-accessory-splashes-bun — ready-for-integration
TZM Ink: Салли снимает аксессуар после сна в катере; `брызги` оставлены без runtime-правок; под водой добавлен `пучок`. Три TODO удалены, остальные не менялись.
Validation: scoped diff/TODO audit clean; `novels-content validate tzm` passed.

## 2026-09-01T16:31:00Z — tzm-visual-source-todos — ready-for-integration

Выполнены десять согласованных TODO первой серии TZM: ветка водоворота учитывает
первоначальную внешность, исправлены локации, камеры, вдох и видео метеорита.
Добавлены четыре 1-секундные MP4 водоворота и канонические `Статуя Атлана` и
`Дворец снаружи` с видео; все восемь assets включены в chunk 0. Source/hash,
media/spec, TODO/diff audits и `novels-content validate tzm` passed.

## 2026-09-01T16:54:00Z — tzm-charon-clothed-scene — ready-for-integration
Task: показать Харона одетым в сцене первого появления.
Changed: во всех четырёх репликах сцены закреплён существующий вид `основной`; удалён только TODO `в одежде`.
Validation: scoped line/TODO audit, `git diff --check` и `novels-content validate tzm` passed.
Pending / risks: none; TODO `актуализировать с текстом` сохранён.
## 2026-09-02T10:44:06Z — mamkin-story — ready-with-limitations

Создана отдельная атомарная история `Projects/novels-mamkin`: один завершённый документально-художественный эпизод с рамкой воспоминаний Владимира Шашкова, девятью утверждёнными фонами, обложкой и production-набором персонажей. В проект не перенесены сидящий Мамкин, prealpha/checkerboard, ранние позы и старые варианты второстепенных персонажей; Валентина Степановна, Толик и солдат используют утверждённые whole sprites со взглядом влево. Мамкин имеет GPL-compatible base view, слой `flight` и whole poses `main`/`raises_hand`; Ink содержит явные clothes/pose selectors, остальные персонажи оформлены whole-only по действующему контракту. `mamkin` добавлен четвёртым элементом каталога.

Validation: после одного ограниченного recovery-loop Unity Licensing IPC `novels-content validate mamkin`, `build mamkin editor`, `validate catalog` и `build catalog editor` прошли; Ink compiled с source map на 700 записей, release `b646e88c0c01507cb184a695f74e71cb3785000571a02b51f0862d242cac6b9d` содержит 3 chunks и все заявленные location/character addresses. JSON, alpha/dimensions, GUID duplicate audit и scoped diff-check прошли. Ручной визуальный runtime gate не выполнялся по прямому указанию пользователя «всё, кроме пункта 6»; это единственное заявленное ограничение.
## 2026-09-02T13:15:57Z — mamkin-emotions — ready-with-limitations

Для `mamkin` identity-preserving editing утверждённых whole-мастеров добавил три сюжетные эмоции: Валентина Степановна `alarmed`, Толик `frightened`, солдат `urgent`. Все сохраняют одежду, позу, масштаб и взгляд влево; source с нарисованной checkerboard был отклонён, production alpha получен из повторных цельных вариантов на однотонном chroma key. Ink selectors обновлены на строках 368/408/452. Full-body и face contact sheets лежат в `Projects/novels-mamkin/Art/EmotionProofs`.

Validation: alpha/corners/dimensions 1024x1536 passed; `novels-content validate mamkin` и `build mamkin editor` passed; composed release содержит все три новые whole addresses; scoped diff-check passed. Ручной in-game runtime gate не запускался, визуальное сравнение выполнено на светлой/тёмной подложках.
