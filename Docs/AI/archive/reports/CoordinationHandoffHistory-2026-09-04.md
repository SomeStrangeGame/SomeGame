# Coordination handoff history — 2026-09-04

## 2026-09-04T11:07:04Z — tzm-choose-screen — paused-for-visual-approval

Task: redesign the shared Choose fallback before adding any TZM-local override.
Changed: no source or prefab changes retained; generated a preview based on the supplied reference, the actual TZM `детский рисунок` asset, and existing TZM panel/button styling.
Validation: confirmed all experimental shared/runtime/TZM edits were fully reverted; preview rendered successfully.
Pending / risks: user visual approval or requested adjustments before implementing the shared fallback prefab.
Suggested next step: after approval, reacquire FIFO/write-lock and implement the agreed fallback structure.

## 2026-09-04T10:55:00Z — remove-story-start-buttons — ready-for-integration

Task: remove redundant one-option episode start choices.
Changed: removed 28 `Начать`/`Начать историю`/`Играть` choices from source Ink, rebuilt generated Ink JSON/source maps, and prohibited these empty start choices in the authoring guide and story skills.
Validation: exact corpus search and scoped diff-check passed; after recovering stale Unity Licensing IPC, scoped `Tools/somegame verify` passed all 13 affected content gates. No Unity Editors were open; the earlier Editor diagnosis was incorrect. Save compatibility was intentionally not preserved because the app is unpublished.
Pending / risks: none for this change; commit/publication was not requested.
Suggested next step: integrate with the other working-tree changes when ready.

Только актуальное незавершённое состояние. Предыдущий snapshot: Git commit `b910242f`; история: [`CoordinationHandoffHistory-through-2026-08-30.md`](CoordinationHandoffHistory-through-2026-08-30.md), [`CoordinationHandoffHistory-2026-09-01.md`](CoordinationHandoffHistory-2026-09-01.md), [`CoordinationHandoffHistory-2026-09-02.md`](CoordinationHandoffHistory-2026-09-02.md), [`CoordinationHandoffHistory-2026-09-03.md`](CoordinationHandoffHistory-2026-09-03.md).
## Ready for integration or validation
- `catalog-prefab-inheritance`: `Assets/RemoteAssets/catalog/fallback.prefab` is now the complete authored neutral grayscale Catalog base with no heading/navigation, enlarged carousel, Safe Area and dot indicator. The runtime-addressed `screen.prefab` is a genuine serialized Unity Prefab Variant of that base and carries only the current blue/white catalog color overrides; no visual hierarchy is created in code. Both assets remain in the single `Projects/novels-catalog` project. Fresh uncached catalog Editor build imported both prefabs and passed bundle audit; scoped diff-check passed.
- `toybedtime-story-bubble-art`: toybedtime now has story-local Bubble artwork: a wide nine-sliced cream/golden dialogue panel with small corner motifs and no separate top indicator. Illustrated choices use dedicated near-square `choice-card.png`; because its aspect matches the button, it is proportionally scaled as a simple image rather than nine-sliced, preserving a thin outer rim and large cream interior. Icons use a 36 px inset. Only the story-local NoCharacter Bubble changed; fallback, shared runtime and TZM remain unchanged. Toybedtime validation/Android content build and Embedded APK `Novels/Build/Players/toybedtime-thin-choice-frame/Novels.apk` passed; final line-43 evidence is `Novels/Build/Logs/toybedtime-thin-choice-frame.png`; Unity error-only log is empty. Ready for integration; publication not requested.
- `toybedtime-light-dialogue-bubble`: the story-local NoCharacter bubble now has a warm cream background, dark-brown text, tighter padding and lower placement; shared/fallback Bubble presentation is unchanged and the existing horizontal image-only choices remain intact. Toybedtime validation, Android content build and full Embedded APK build passed. Fresh emulator replay reached ordinary line `s01e01.ink:21`; `Novels/Build/Logs/toybedtime-normal-bubble-3.png` confirms the light dialogue bubble, and `toybedtime-light-dialogue-bubble.png` confirms the light question bubble with both illustrated choices. Unity error-only log is empty. Ready for integration; publication was not requested in this task.
- `toybedtime-choice-publish`: commit `f691f61313acb3b7d42d6d833015ec582c25fc43` published to `origin/main`; canonical publisher confirmed equal local/remote SHA. A separate post-push Android replay reached `s01e01.ink:43`; `Novels/Build/Logs/toybedtime-choice-image-only-post-push.png` confirms two large horizontal image-only choices, and the Unity error-only log is empty. App stopped; AVD remains running. No pending work.
- `toybedtime-choice-image-only`: toybedtime now presents its two illustrated choices as large `184x160` image-only cards in one horizontal row. Shared `BubbleScreen` gained two opt-in serialized flags; both default off, so fallback and other stories keep their existing vertical labeled choices. Toybedtime validate/Android build and full Embedded player build passed. Pixel runtime screenshot `Novels/Build/Logs/toybedtime-choice-image-only-line43.png` confirms both images and layout at `s01e01.ink:43`; Unity error log is empty. App stopped, AVD left running. Ready for integration.
- `coordination-queue-resilience`: `Tools/somegame queue-status` now reports FIFO position, request/lease ages, owner consistency, long waits, recoverable terminal orphans and process-probe availability; `queue-prune --request <exact-id>` removes only a terminal orphan not referenced by the current lock. Long bounded runner commands refresh only their exact owner's heartbeat every minute, while stale/ambiguous foreign locks remain fail-closed. Protocol now requires request release on inactive waits. Automation tests 34/34, `docs-check`, scoped `verify`, syntax/diff review passed; no Unity gate required. Pending: commit/publication not requested.
- `toybedtime-choice-runtime-repro`: root cause of all missing Bubble text was a font GUID that existed only in `Novels/Assets` and resolved null in the atomic story bundle. Toybedtime now embeds and references story-local Liberation Sans; its editor/android bundle includes the 344.5 KB font, buttons are 260 px wide, both content builds and Novels compile passed. After the approved `Novels_Pixel_7_API_34` reset, the Embedded APK reached `s01e01.ink:43`; `Novels/Build/Logs/toybedtime-choice-runtime-line43.png` confirms the question, both labels, both icons, wrapping and compact centered buttons. App stopped after validation; AVD remains running. Ready for integration.
- `coordination-speed-paths`: общий workflow теперь пропускает project init для чистых meta-вопросов, поддерживает `inspect`, document fingerprints для `--resume` и task-owned `context/verify --paths`; полный dirty snapshot сохраняется для collision detection, но чужие изменения не расширяют plan или scoped diff-check. Targeted tests 29/29, `docs-check` и scoped `verify` passed; pre-existing Unity Editor PID 63245 не принадлежит этому docs/tooling scope и не останавливался. Pending: commit/publication отдельно не запрашивались.
- `tzm-toybedtime-prefab-fixes`: TZM locally cancels the shared character Viewport `Y=-220` with `Y=0`; toybedtime's authored Bubble now uses the runtime-required name `screen-variant.prefab` while preserving GUID `f8ebfd5654c6d419cb091e741234bc5c`. Both story validations/builds, release-address audits, scoped diff-check and fresh Novels compile passed. Pending: portrait replay of the TZM character and toybedtime illustrated choices.
- `toybedtime-bubble-prefab`: branch `codex/toybedtime-bubble-prefab`; fallback снова text-only и игнорирует `choice_icon`, потому что `BubbleScreen` больше не создаёт icon UI динамически. Unity-authored story-local `Assets/Presentation/bubble/screen.prefab` содержит встроенный `IllustratedChoiceButton` с authored background, `ChoiceIcon` и `ChoiceText`; временный authoring utility удалён. Toybedtime validate/build passed, release `418df27d...` содержит prefab и обе sprites, fresh Novels compile clean, diff-check passed. Pending: portrait replay первого choice.
- `somegame-story-design-acceptance-skills`: созданы `$somegame-design-story` и `$somegame-accept-story`; первый выдаёт утверждённый brief/narrative package/scene matrix/choice-state graph, второй владеет catalog gate, end-to-end audit, changed-path validation, runtime/manual evidence и итоговым readiness status. `$somegame-create-story` теперь оркестрирует design → MCP-ready project → art → playable content → acceptance. Historical integrity оставлена условным внутренним этапом, отдельный research skill не создан. Validation: YAML/frontmatter/default prompts/TODO/reference/routing/negative historical-skill checks, scoped diff-check и `Tools/somegame docs-check` passed; штатный quick validator blocked только отсутствующим PyYAML. Pending: commit/publication отдельно не запрашивались.
- `somegame-create-character-skill`: создан `$somegame-create-character`, владеющий character brief, identity master, только используемыми outfit/emotion/pose variants, runtime selectors, provenance и visual evidence. Story workflow теперь явно разделяет character package и остальной art manifest; старый character-reference из `$somegame-produce-story-art` перенесён и удалён. Все шесть затронутых skills прошли штатный `quick_validate.py`, scoped diff-check, routing/TODO audit и `Tools/somegame docs-check`. Pending: commit/publication отдельно не запрашивались.
- `story-originality-loop-skill`: `$somegame-design-story` теперь выполняет максимум пять review-итераций `draft/revision → evidence-backed originality check → targeted correction`, завершается раньше без material findings и блокирует весь downstream workflow после пятой неуспешной проверки. Итог содержит источники, risk/confidence, change log и ограничения; жанровые тропы/исторические факты не считаются плагиатом сами по себе, legal clearance не заявляется. Skill quick validation, requirement/routing audit, scoped diff-check и `Tools/somegame docs-check` passed. Pending: commit/publication отдельно не запрашивались.
- `production-originality-gates`: story workflow теперь требует passed originality evidence на четырёх уровнях: narrative, каждый character package, non-character art и полный source Ink. Каждый production gate выполняет максимум пять evidence-backed review/revision итераций с ранним pass и downstream block после пятого material finding; `$somegame-accept-story` fail-closed проверяет evidence и возвращает изменившийся материал владельцу вместо повторного поиска или waiver. Все шесть затронутых skills прошли quick validation, cross-skill requirement audit, scoped diff-check и `Tools/somegame docs-check`. Pending: commit/publication отдельно не запрашивались.
- `child-story-bubbles-skill`: создан `$somegame-create-child-story-bubbles` для story-local светлых Bubble и крупных иллюстрированных choice-кнопок дошкольных историй; закреплены отдельный aspect-matched choice-card sprite, безопасный label fallback, сохранение shared fallback/других историй и обязательный Player visual gate реплики и выбора. YAML/TODO/default-prompt/scoped diff checks passed; штатный quick validator недоступен без PyYAML, а общий `docs-check` сообщает существующее превышение HANDOFF 122/120 строк. Pending: commit/publication не запрашивались; rotation — отдельная задача.
- `toybedtime-bubble-lower`: story-local Bubble Viewport toybedtime опущен с `Y=50` до `Y=-50`; реплика и choice сохраняют внутреннюю геометрию, но занимают нижнюю треть. После восстановления конфликтовавшего licensing IPC editor/android content gates и fresh Embedded APK прошли. Emulator evidence: `Novels/Build/Logs/toybedtime-bubble-lower-runtime.png` (choice line 43) и `toybedtime-bubble-lower-aftertap.png` (ordinary line 48); карточки целиком видны, снизу есть запас, runtime failure/fallback markers отсутствуют. App stopped, AVD running. Pending: commit/publication не запрашивались.
- `fallback-button-layout`: подтверждённый повторный layout-pass больше не возвращает choice-кнопки поверх текста: pooled buttons получают `LayoutElement.ignoreLayout = true`, после чего сохраняют вычисленное положение ниже реплики. Scoped diff-check и fresh Novels compile passed. Pending: пользовательский visual replay Sobibor line 163.
- `fallback-visual-parity`: поверх готовой responsive bubble-базы choice-кнопки теперь явно ставятся ниже фактического нижнего края текста с зазором 12 px и корректно стекаются; shared character `Viewport` опущен на `-220`, как в GPL, поэтому новые fallback-истории наследуют ту же посадку, а GPL визуально не меняется. Scoped diff/prefab audit и fresh Novels compile passed. Pending: portrait replay Sobibor lines 75/91.
- `episode-launch-actions`: story запуск больше не требует `<story>/application/setting/screen.prefab`. На экране эпизодов без save показывается `Новая игра`, а с save — `Продолжить` и `Начать заново`; restart очищает только выбранный эпизод. Старый публичный `CatalogController.Select` сохранён, двухкнопочный результат доступен через `SelectAction`; вторичная кнопка создаётся из существующей UI-кнопки, поэтому prefab-миграция не нужна. Fresh Novels compile, catalog/Sobibor validation, scoped diff-check и setting-reference audit passed. Pending: ручной portrait UI pass.
- `tzm-responsive-bubble`: базовый bubble prefab теперь по умолчанию пересчитывает высоту тела по фактическому тексту и затем штатно раскладывает choice-кнопки; высота узкого Header больше не меняется, поэтому `Салли`/`Дисклеймер` не раздувают синюю плашку. Удалены semantic special cases `КОНЕЦ СЕРИИ`, отдельные размеры шрифта и смещение финальной кнопки; TZM variant больше не хранит поведенческий override и остаётся визуальным. `git diff --check`, `novels-content validate tzm`, `build tzm editor` и fresh Novels compile passed. Targeted EditMode suite отсутствует (`0 tests`). Pending: portrait visual replay `s01e01.ink` lines 68, 94, 171 и финального экрана.
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
- `fallback-hint-placement`: fallback `Hint` bubble lowered from Y `100` to `-80`, clearing the face band of foot-aligned historical characters; TZM explicitly preserves its authored Y `100`. Scoped prefab diff-check and fresh Novels compile passed; user visual gate remains at Sobibor `sobibor.ink:297`.
- `fallback-choice-contrast`: fallback choice background now uses a lighter/cooler slate (`0.25/0.29/0.35`, alpha `0.98`) instead of the dialogue panel color; TZM keeps its authored blue sprite override. Scoped diff-check and fresh Novels compile passed; user visual gate remains.
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

## 2026-09-01T16:31:00Z — tzm-visual-source-todos — ready-for-integration

Выполнены десять согласованных TODO первой серии TZM: ветка водоворота учитывает
первоначальную внешность, исправлены локации, камеры, вдох и видео метеорита.
Добавлены четыре 1-секундные MP4 водоворота и канонические `Статуя Атлана` и
`Дворец снаружи` с видео; все восемь assets включены в chunk 0. Source/hash,
media/spec, TODO/diff audits и `novels-content validate tzm` passed.

## 2026-09-02T10:44:06Z — mamkin-story — ready-with-limitations

Создана отдельная атомарная история `Projects/novels-mamkin`: один завершённый документально-художественный эпизод с рамкой воспоминаний Владимира Шашкова, девятью утверждёнными фонами, обложкой и production-набором персонажей. В проект не перенесены сидящий Мамкин, prealpha/checkerboard, ранние позы и старые варианты второстепенных персонажей; Валентина Степановна, Толик и солдат используют утверждённые whole sprites со взглядом влево. Мамкин имеет GPL-compatible base view, слой `flight` и whole poses `main`/`raises_hand`; Ink содержит явные clothes/pose selectors, остальные персонажи оформлены whole-only по действующему контракту. `mamkin` добавлен четвёртым элементом каталога.

Validation: после одного ограниченного recovery-loop Unity Licensing IPC `novels-content validate mamkin`, `build mamkin editor`, `validate catalog` и `build catalog editor` прошли; Ink compiled с source map на 700 записей, release `b646e88c0c01507cb184a695f74e71cb3785000571a02b51f0862d242cac6b9d` содержит 3 chunks и все заявленные location/character addresses. JSON, alpha/dimensions, GUID duplicate audit и scoped diff-check прошли. Ручной визуальный runtime gate не выполнялся по прямому указанию пользователя «всё, кроме пункта 6»; это единственное заявленное ограничение.
## 2026-09-02T13:15:57Z — mamkin-emotions — ready-with-limitations

Для `mamkin` identity-preserving editing утверждённых whole-мастеров добавил три сюжетные эмоции: Валентина Степановна `alarmed`, Толик `frightened`, солдат `urgent`. Все сохраняют одежду, позу, масштаб и взгляд влево; source с нарисованной checkerboard был отклонён, production alpha получен из повторных цельных вариантов на однотонном chroma key. Ink selectors обновлены на строках 368/408/452. Full-body и face contact sheets лежат в `Projects/novels-mamkin/Art/EmotionProofs`.

Validation: alpha/corners/dimensions 1024x1536 passed; `novels-content validate mamkin` и `build mamkin editor` passed; composed release содержит все три новые whole addresses; scoped diff-check passed. Ручной in-game runtime gate не запускался, визуальное сравнение выполнено на светлой/тёмной подложках.

## 2026-09-03T16:38:30Z — accept-story-editor-gate — ready-for-integration
Task: заменить интерактивную Editor-приёмку истории обязательным Android emulator gate.
Changed: `$somegame-accept-story` и checklist требуют свежий Embedded APK, точную APK/emulator identity, реальный catalog flow, покрытие эпизодов/значимых веток/концовок, runtime/visual evidence; stale или неполное evidence блокирует acceptance.
Validation: skill quick validation, scoped `git diff --check` и `Tools/somegame docs-check` passed.
Pending / risks: реальный APK/emulator не запускался, потому что эта задача меняет только skill contract.
Suggested next step: применять gate при следующей фактической приёмке истории.

## 2026-09-04T09:25:00Z — toybedtime-choice-style-parity — ready-for-integration

Story-local `choice-card.png` приведён к визуальному языку детского dialogue
panel: тот же тёплый кремовый фон, тонкая золотая внешняя линия, тонкий
пунктирный inset и редкие игрушечные угловые акценты. Размер 464x400,
прозрачный exterior и существующий GUID сохранены; prefab, shared fallback и
другие истории не менялись. Editor verify, Android content-gate, fresh Embedded
APK и emulator visual gate на `s01e01.ink:43` passed; evidence:
`Novels/Build/Logs/toybedtime-choice-style-parity-final.png`. Отдельно выявлен
старый дефект: стартовый choice на строке 11 без `choice_icon` пуст в image-only
режиме; нужен отдельный icon-or-label fallback.

## 2026-09-04T10:03:00Z — toybedtime-choice-white-rim — ready-for-integration

У story-local `choice-card.png` удалены белый внешний кант и checkerboard-ореол:
шумная унаследованная alpha заменена чистой rounded-rect маской, RGB прозрачной
зоны заполнен кремовым matte, Android import этой небольшой UI-текстуры переведён
на несжатый RGBA32. Prefab, dialogue panel, shared fallback и другие истории не
менялись. Editor verify, Android content-gate, fresh Embedded APK и emulator gate
на `s01e01.ink:43` passed; evidence:
`Novels/Build/Logs/toybedtime-choice-white-rim-final.png`.

## 2026-09-04T10:59:00Z — child-bubble-alpha-skill — ready-for-integration

`$somegame-create-child-story-bubbles` теперь требует проверять реальный alpha
channel, чистую связанную маску, matte-цвет скрытого RGB и platform compression;
built-runtime gate явно отклоняет белые канты, checkerboard, alpha speckles и
halos после пересборки atomic content и Player. Scoped verify и эквивалентная
Ruby YAML-валидация passed; системный quick_validate блокирует отсутствующий
PyYAML, docs-check — ранее превышенный лимит HANDOFF.
