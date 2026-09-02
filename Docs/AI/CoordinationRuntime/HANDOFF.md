# Current cross-chat handoff

Только актуальное незавершённое состояние. Предыдущий полный snapshot сохранён
в Git на commit `b910242f`; завершённые результаты сведены в
[`CoordinationHandoffHistory-through-2026-08-30.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-30.md).

## Ready for integration or validation

- `gpl-episode2-art-integration-smoke`: commit `a92ba3f0` завершил арт эпизода: три фона и четыре цельных Павла (`main`, раненый, с рычагом, двойник), Ink selectors, meta и full/face dark/light contact sheets. Alpha/dimensions/GUID и визуальные края passed. Повторный GPL validate заблокирован Unity licensing (`LicenseClient-iantonishin` channel отсутствует, headless license не найден); own batch остановлен, lockfile удалён. После восстановления Hub нужны validate, compiled Ink, editor/android content, Embedded APK и episode-two smoke.
- `free-wardrobe-equipped-index`: свободная категория теперь открывается на фактически equipped значении из save, поэтому подпись compact carousel сразу соответствует волосам/одежде персонажа. Initial preview остаётся выключен: открытие вкладки не меняет образ. При отсутствии equipped-записи используется эффективный explicit/default стиль CharacterController, а не item с индексом 0; для TZM это `Распущенные`. Fresh Novels compile passed; нужен visual check.
- `free-wardrobe-custom-presentation-guard`: свободный гардероб остаётся доступным поверх кастомного сюжетного образа, но рендерит отдельный neutral target-aware preview; при закрытии восстанавливаются исходные story request, target, visibility и position. API принимает героя для будущего multi-character UI. `Wardrobe`-рендер дополнительно изолирован от story appearance cache, поэтому эмоция/поза сбрасываются в preview и возвращаются после закрытия. Fresh Novels compile passed; нужен visual open/close на строке с ночнушкой.
- `wardrobe-unlock-all-options`: scripted wardrobe теперь разблокирует все показанные варианты текущей категории, сохраняя выбранный надетым; обычные choices без wardrobe action остаются исключены, asset-фильтрация сохранена. Fresh Novels compile passed; нужен replay сюжетного и свободного гардероба.
- `wardrobe-choice-filter`: обычные `Выбор предмета` без явного wardrobe action больше не разблокируют вещи; при загрузке свободной категории старые записи сверяются с реальными assets и удаляются из save. Fresh Novels compile passed; нужен replay завтрака и повторное открытие свободного гардероба.
- `ink-line-overlay`: возвращён development-only overlay `Ink: файл:строка`; source map снова проходит через simple-layout release и runtime. TZM editor content rebuilt, Mac release содержит map, fresh Novels compile passed; Editor открыт. Нужен visual replay истории.

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

- `tzm-wardrobe-generated-ui`: whole-atlas iteration was visually rejected and removed. Approved reference art remains as a source sheet; runtime now uses separate transparent/sliced panel, tab, two buttons, character arrow and four category icons, so active/hidden controls remain dynamic and no baked empty button remains. TZM validate/editor build passed and contains all assets. Fresh Novels editor-check could not reach readiness because Unity Licensing returned `Access token is unavailable`; visual replay waits for Hub sign-in/license.

- `gpl-episode3-full-smoke`: paused at a safe emulator checkpoint by user request so higher-priority `com.zebrainy.skazbuka` smoke can run. Episode-three Ink/art integration and full Embedded APK are complete; GPL content validation passed. Emulator smoke completed `gpl/s01e01` and `gpl/s01e02` with catalog returns. `gpl/s01e03` reached its third decision (`s01e03.ink:257`, run `9e34be64f3a146ef995a70dcc4c4606b`, seq 93) before the other app took foreground; GPL PID 16869 was still alive. Resume only after re-entering FIFO and acquiring emulator/write scope; finish episode 3 and final evidence.

- `android-memory-full-smoke`: paused because the running Embedded APK contains stale content and does not recognize the episode end marker. Preliminary emulator stress data: GPL ~335 MiB PSS; TZM plateau ~374 MiB PSS / 464 MiB RSS; swap negligible. Emulator lacks ASTC8x8 and expands textures to RGBA. Resume after a full Embedded content rebuild and final episode pass begins.

## 2026-09-01T06:49:10Z — tzm-wardrobe-fallback-isolation — manual gate

TZM wardrobe variant и девять UI sprites временно помечены unused без удаления
sources. Editor release пересобран без `presentation/wardrobe`; Novels Editor
перезапущен, compile clean. Следующий кадр гардероба показывает общий fallback.

## 2026-09-01T07:25:31Z — fallback-wardrobe-authored-prefab — manual gate

Общий fallback wardrobe больше не строит UI при открытии. В shared
`OptionListScreen.prefab` сохранён отдельный `WardrobeRoot`: пропорции нижнего
окна соответствуют референсу, tabs/стрелки/confirm/cancel/collapse и верхние
действия заранее авторены из sprite-less цветных `Image`. Runtime только
заполняет текст, counts, видимость, active tab и listeners; save/Ink/wardrobe
selection logic не менялись. Builder, scoped diff-check и fresh Novels compile
passed; Editor оставлен открытым для portrait visual replay.

## 2026-09-01T09:53:20Z — full-tree-publish — integrated

Task: publish the complete current tree as atomic commits. Changed: automation/protocols, shared wardrobe runtime, TZM wardrobe presentation, GPL episode three and runtime handoffs are separated into atomic commits. The experimental mobile character-chat branch, bundled Qwen model and LlamaLib binaries are intentionally excluded and preserved only on local backup branch `codex/full-tree-with-character-chat`. Validation: docs-check, automation tests, catalog/TZM/ZDM/GPL editor content builds and fresh Novels compiles passed; wardrobe Play Mode visual gate was completed immediately before integration. Publication: content commit `8aaae082934d402a45a0ae957ef372430ed67047` was confirmed on `origin/main`; this entry is the final coordination follow-up. Pending / risks: no affected EditMode test assembly exists; no publication blocker remains.

## 2026-09-02T14:17:29Z — zmt-project-integration — completed

Task: полностью интегрировать документальную историю ZMT как atomic content project. Changed: создан scaffold/card/portrait cover/definition, шесть локаций и 18 selector pairs подключены к Ink, Unity сгенерировала 109 уникальных `.meta`, compiled Ink и source map; 15 character identities получили defaults, layered Зинаида — шестизаписный trim manifest; `zmt` добавлен четвёртой историей каталога. Validation: initial and post-trim ZMT content gates, catalog content gate and final changed-path verify passed; final trim report 6/6 unchanged, 42 release assets in 3 chunks, selector/location/meta/GUID/JSON audits and bounded visual gate passed. Pending / risks: none for atomic editor content; Player/Android runtime smoke was not part of this integration scope. Suggested next step: optional in-game episode replay.

## 2026-09-02T16:00:10Z — zmt-main-publish — completed

Task: перенести завершённую ZMT-интеграцию из изолированного worktree в актуальный `main`. Changed: story project, catalog registration and validation evidence опубликованы тремя атомарными коммитами из чистого integration-checkout; грязный канонический checkout и его посторонние изменения не затрагивались. Validation: final diff-check and JSON parsing passed; prior ZMT/catalog editor gates and bounded visual gate remained applicable; redundant Unity rerun was stopped after detecting a foreign batch in another worktree. Publication: `f838cad1ce0c1128ab45eee2f668b443613cd9a5` confirmed on `origin/main`. Pending / risks: none; local canonical checkout remains intentionally untouched because it contains unrelated uncommitted work.
