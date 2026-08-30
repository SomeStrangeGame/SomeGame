# Current cross-chat handoff

Только актуальное незавершённое состояние. Предыдущий полный snapshot сохранён
в Git на commit `b910242f`; завершённые результаты сведены в
[`CoordinationHandoffHistory-through-2026-08-30.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-30.md).

## Ready for integration or validation

- `publish-all-main`: весь dirty tree проверен и опубликован в `origin/main`;
  content gates и Novels compile passed. Local/remote SHA подтверждены как
  `0900ca8a26cc8b5520bd8bbdd1861dd04f356cbf`.
- `free-wardrobe-equipped-index`: свободная категория теперь открывается на
  фактически equipped значении из save, поэтому подпись compact carousel сразу
  соответствует волосам/одежде персонажа. Initial preview остаётся выключен:
  открытие вкладки не меняет образ. При отсутствии equipped-записи используется
  эффективный explicit/default стиль CharacterController, а не item с индексом
  0; для TZM это `Распущенные`. Fresh Novels compile passed; нужен visual check.
- `free-wardrobe-custom-presentation-guard`: свободный гардероб остаётся
  доступным поверх кастомного сюжетного образа, но рендерит отдельный neutral
  target-aware preview; при закрытии восстанавливаются исходные story request,
  target, visibility и position. API принимает героя для будущего multi-character
  UI. `Wardrobe`-рендер дополнительно изолирован от story appearance cache,
  поэтому эмоция/поза сбрасываются в preview и возвращаются после закрытия.
  Fresh Novels compile passed; нужен visual open/close на строке с ночнушкой.
- `wardrobe-unlock-all-options`: scripted wardrobe теперь разблокирует все
  показанные варианты текущей категории, сохраняя выбранный надетым; обычные
  choices без wardrobe action остаются исключены, asset-фильтрация сохранена.
  Fresh Novels compile passed; нужен replay сюжетного и свободного гардероба.
- `wardrobe-choice-filter`: обычные `Выбор предмета` без явного wardrobe action
  больше не разблокируют вещи; при загрузке свободной категории старые записи
  сверяются с реальными assets и удаляются из save. Fresh Novels compile passed;
  нужен replay завтрака и повторное открытие свободного гардероба.
- `ink-line-overlay`: возвращён development-only overlay `Ink: файл:строка`;
  source map снова проходит через simple-layout release и runtime. TZM editor
  content rebuilt, Mac release содержит map, fresh Novels compile passed;
  Editor открыт. Нужен visual replay истории.

### UI and character runtime

- `tzm-hide-empty-wardrobe-tabs`: scripted wardrobe скрывает категории, которых
  нет в доступном наборе вкладок; временно заблокированные, но применимые
  категории остаются видимыми. Tooling/helper tests 43/43 и live Unity compile
  passed; нужен visual gate в уже открытом Editor.
- `wardrobe-tab-preview`: переключение вкладки и начальное позиционирование
  карусели больше не применяют вариант автоматически; preview остаётся для
  явного тапа и реального перехода на другую карточку. Tests 43/43 и live Unity
  compile passed; нужен visual gate в открытом Editor.
- `tzm-compact-wardrobe`: первая компактная итерация скрывает карточки в
  wardrobe-layout, оставляет название варианта, стрелки и confirm, уменьшает
  нижнюю панель и показывает количества вариантов на доступных scripted tabs.
  Иконок в assets нет, поэтому tabs пока текстовые. Tests 43/43 и live Unity
  compile passed; нужен visual gate в открытом Editor.
- `tzm-wardrobe-location-background`: отдельный fullscreen WardrobeBackdrop и
  glow удалены; гардероб теперь сохраняет текущую сюжетную локацию за
  персонажем и компактной панелью. Первый attach gate не нашёл старый Pipeline
  port; fresh Editor compile затем passed без compiler errors. Нужен visual
  gate в оставленном открытым Editor.
- `tzm-initial-wardrobe-preview`: первый scripted wardrobe page теперь явно
  применяет стартовый item один раз, поэтому main-character view задаётся до
  показа; последующие tab switches остаются non-mutating. Первый attach gate
  подтвердил compile, но увидел старый fallback.used из repro; после fresh
  Editor restart полный gate passed без compiler errors. Нужен visual replay.
- `free-wardrobe-empty-tabs`: свободный гардероб теперь вычисляет доступные
  категории из unlocked items конкретного персонажа, выбирает валидную
  начальную вкладку и скрывает/отклоняет пустые категории. Tests 43/43 passed;
  stale Editor transport failed, fresh Editor compile затем passed. Нужен
  visual replay отдельного гардероба.
- `gpl-fullbody-character-offset`: добавлен локальный GPL character-screen
  variant со сдвигом дочернего `Viewport` на `Y = -220`; первая попытка на root
  Screen Space Overlay Canvas не влияла на рендер и заменена. Общий SDK, TZM и ZDM
  не менялись. GPL editor release `33ca6c97…e0c7` собран и composed в LocalContent;
  fresh Novels Editor compile passed, Editor оставлен открытым для visual replay.
- `fallback-bubble-runtime-rebuild`: глобальный rebuild, сломавший геометрию custom
  TZM bubble, удалён. `BubbleScreen` теперь имеет opt-in флаг; layout overrides и
  rebuild включены только в game fallback `Novels/Fallbacks/.../bubble/screen-variant`.
  Shared base и TZM prefab не изменены. TZM release `48adc0d9…c192`, GPL release
  `33ca6c97…e0c7` собраны; fresh Novels compile passed. Нужен visual replay обоих кадров.
## Blocked / limitations

- `gpl-catalog-registration`: `gpl` добавлен третьей story; catalog editor build
  и GPL validation passed, composed registry содержит `tzm/zdm/gpl`. Исправлен
  catalog dispatch в `AtomicContentBuild`. Устранён `CONTENT_PREPARATION_FAILED`:
  обязательный GPL setting `screen.prefab` и реальный Liberation Sans добавлены
  в chunk 0, release `7fe8…7283`; built-in font rejected by bundle, fresh compile passed.
  `CharacterController` теперь начинает с base view `view`, устраняя Lea fallback
  без стартового appearance-choice. Ошибочный TZM bubble удалён из GPL; общий
  fallback теперь учитывает высоту dialogue перед choices, GPL release снова
  `7fe8…7283`; fresh compile passed. Нужен Play replay.

- `tzm-wardrobe-runtime`: implementation compiles and TZM content-gate passes
  without Ink changes. The initial consecutive appearance/hair/clothes choices
  now open as one tabbed scripted transaction containing only Ink-provided
  options; selections persist while switching tabs and unrelated tabs stay
  hidden. Одиночный scripted wardrobe также показывает только текущую категорию;
  multi-page/free режимы сохранены. Fresh Unity compile passed; Editor открыт.
  Pending: user visual check, then scoped integration/commit.

- `gpl-lea-layered-rework` — ready: общий head/hair и facial patches отделены от
  четырёх clothes-слоёв точной мягкой маской по нижней челюсти; вся шея/ворот в
  одежде. Jaw dark/light proof, GPL validate/editor build passed; нужен visual gate.
- `gpl-mark-integration`: 8 цельных вариантов Марка импортированы как station
  и polar whole-наборы; 7 station selectors привязаны к episode 1. GPL
  validate/editor build passed; нужен bounded visual gate в игре.
- `gpl-vera-integration`: Вера импортирована как layered head + 2 clothes + 3 facial
  emotions; `urgent`, `hides_hands`, `pain_pose` остаются whole-вариантами.
  GPL validate/editor build passed; нужен bounded visual gate в игре.
- `catalog-playmode-review`: paused до явного продолжения ручного visual review.
- WebGL prototype остаётся только в `prototype/webgl-local-platform`, commit
  `cfb92896`; compilation и browser smoke не выполнены.
## Runtime rules

- Перед работой использовать `Tools/somegame context --task <type>` и проверить
  полный Git/FIFO; архив читать только для конкретной регрессии.
- Затем выполнить `Tools/somegame verify --explain`; тяжёлые gates запускать
  только по плану и под точным write-lock.
- Успешные результаты передавать компактно; полные логи читать при failure.
