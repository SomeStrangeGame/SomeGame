# Cross-chat handoff log

Перед работой прочитайте этот журнал полностью и проверьте относящиеся к задаче
утверждения по текущим файлам и `git status`. Перед освобождением write-lock
добавьте новую запись в конец файла по формату из
`../ParallelRefactoringCoordination.md`.

## 2026-08-24 — coordination-runtime — completed

Task: Дополнить существующую координацию атомарной FIFO-очередью, handoff и
правилами безопасной последовательной работы.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлен операционный
  протокол.
- `Novels/Docs/AI/CoordinationRuntime/`: создано стартовое пространство
  очереди.

Validation:
- Ручная сверка с переносимым протоколом `skazbuka`: ключевые правила
  перенесены без замены проектной интеграционной очереди.

Pending / risks:
- Механизм остаётся кооперативным: чат должен открыть репозиторий от его корня,
  чтобы получить инструкции из `AGENTS.md`.

Suggested next step:
- Использовать runtime-очередь при следующей изменяющей или тяжёлой задаче.

## 2026-08-24T10:11:08Z — catalog-coordination-rules — completed

Task: Формализовать безопасное расширение области владения и межпроектные
атомарные scope.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлены правила
  временной передачи владения, отдельного межпроектного scope, приоритета
  активного владельца и атомарных блоков внутри расширенной задачи.

Validation:
- `git diff --no-index --check /dev/null
  Novels/Docs/AI/ParallelRefactoringCoordination.md`: whitespace-ошибок нет.
- Ручная сверка с существующей runtime FIFO-очередью: новый раздел использует
  `write-lock` как единственное разрешение записи и не создаёт второй механизм
  блокировки.

Pending / risks:
- Механизм кооперативный: временный scope должен быть объявлен до изменения
  shared-файлов.

Suggested next step:
- Для переноса Catalog size audit создать отдельный
  `ParallelWork.bundle-audit.md` с точными shared-файлами.

## 2026-08-24T10:25:30Z — bundle-audit — ready-for-integration

Task: Перенести контроль размера и состава Catalog bundle из локального
Editor-кода в общий Content SDK.

Changed:
- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs`: общий аудит root
  assets, фактического размера и Catalog dependencies/budget.
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`: аудит подключён после
  создания bundle и до записи `release.json`.
- `Projects/novels-catalog/Assets/Editor/**`: локальный audit удалён вместе с
  папкой и meta.
- `Projects/novels-catalog/README.md`: отдельное Unity-меню заменено описанием
  автоматической проверки.
- `Novels/Docs/AI/ParallelWork.bundle-audit.md`: записан scope и результат.

Validation:
- `novels-content doctor`: успешно.
- `novels-content validate catalog`: успешно.
- `novels-content build catalog editor`: успешно.
- Unity log: `Content bundle audit passed`; размер 6606 байт (6,5 КиБ).
- Catalog не содержит локальных C#-файлов.
- `git diff --check`: успешно.

Pending / risks:
- Для Story bundle audit пока только проверяет root assets, файл и размер без
  отдельного size budget; поведение сборки Story не менялось.

Suggested next step:
- Интеграционному координатору принять единым блоком общий audit и удаление
  локального Catalog audit.

## 2026-08-24T11:09:42Z — novels-simplification — completed

Task: Последовательно упростить Game runtime, Content SDK, UI, адресацию и
валидацию Novels, принять готовый bundle audit и пересобрать Editor-контент.

Changed:
- `Novels/Assets/Novels/**`: линейный application flow, предметный lifetime
  каталога, отдельный `ReplayValidator`, единый dialogue frame.
- `Packages/NovelsContentSdk/Runtime/**`: общие операции адресации и поиска
  слоёв; удалены три пустые contract assemblies; Choose и Wardrobe используют
  общий lifecycle через композицию, оставаясь самостоятельными фичами.
- `Packages/NovelsContentSdk/Editor/**`: неизменяемый результат инспекции
  проекта; принят общий `ContentBundleAudit` из предыдущей задачи.
- `Novels/Docs/AI/**`: актуализированы обзор, короткий план и история волны.

Validation:
- Unity 6000.3.11f1 batch compile Game runtime и Editor assemblies: успешно,
  C#-ошибок нет.
- `novels-content doctor`: успешно.
- `novels-content validate all`: Catalog, TZM и ZDM успешно.
- `novels-content build all editor`: application, TZM и ZDM успешно; свежая
  локальная композиция находится в `Novels/Build/LocalContent`.
- `git diff --check`: успешно; ссылок на удалённые contract assemblies нет.

Pending / risks:
- Play Mode, визуальные размеры и полный игровой маршрут автоматически не
  проверялись; пользователь выполнит ручной smoke test в Editor.
- Тесты намеренно не добавлялись и не запускались согласно правилам проекта.
- Editor-сборка не заменяет Android/iOS player build.

Suggested next step:
- Открыть Novels, пройти каталог → выбор истории → эпизод для TZM и ZDM и
  проверить Console; при успехе зафиксировать изменения отдельным коммитом.

## 2026-08-24T12:21:13Z — novels-simplification-2 — completed

Task: Выполнить шесть согласованных упрощений runtime, OptionList и Content SDK
validation, затем уточнить самостоятельное ожидание FIFO.

Changed:
- `NovelRuntime*`: удалены промежуточные bootstrap/session-типы, маршрут виден
  непосредственно в `Init` и preparation.
- `StoryExecution/**`, `StoryQueue/**`: простые операции переведены на
  `DelegateStoryOperation`; stateful выбор сохранён отдельными типами.
- `ContentProjectValidation.cs`: линейный validation без rule-интерфейса.
- `OptionSelection/**`: статическая разметка хранится в системном prefab,
  динамически создаются только карточки.
- Choose, Wardrobe и Catalog принимают малые зависимости напрямую.
- `ParallelRefactoringCoordination.md`: FIFO ожидается автоматически до
  таймаута; занятая очередь сама по себе не завершает ход.

Validation:
- Unity 6000.3.11f1 batch compile: успешно, C#-ошибок нет.
- `novels-content validate all`: Catalog, TZM и ZDM успешно.
- `git diff --check`: успешно.
- C# объём проверяемой области: 9628 → 9305 строк.

Pending / risks:
- Нужен ручной Play Mode smoke test Choose и Wardrobe, включая повторное
  открытие, прокрутку, preview и confirm.
- Android/iOS player build не запускался; форматы Ink/save/release не менялись.
- Тесты не добавлялись и не запускались согласно правилам проекта.

Suggested next step:
- Выполнить ручной UI smoke test, затем интегрировать
  `ParallelWork.simplification-wave-2.md`.

## 2026-08-24T11:12:00Z — story-global-content — completed block 0

Task: Разрешить ограниченное продолжительное ожидание FIFO по явному запросу
пользователя.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлен режим ожидания
  без write-lock с интервалом не менее 60 секунд, таймаутом 10 минут и не более
  чем 10 проверками.

Validation:
- `git diff --check` для изменённых coordination-файлов: успешно.
- Ручная сверка с FIFO/write-lock: ожидание не даёт права записи и не позволяет
  удерживать lock.

Pending / risks:
- Нет; режим включается только явным запросом пользователя или координатора.

Suggested next step:
- Освободить lock атомарного блока 0 и продолжить блок 1
  `story-global-content` через обычную FIFO.

## 2026-08-24T11:20:00Z — story-global-content — completed blocks 1-2

Task: Ввести единый story-global адресный контракт и мигрировать ZDM.

Changed:
- `Packages/NovelsContentSdk/Runtime/ContentAddressing/**`: Unity-assets теперь
  адресуются только через `content/<story>/story/**`.
- Character/runtime loaders: удалён episode/shared fallback.
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/**`: `shared` перенесён
  в `story`, локации дедуплицированы и собраны в story-global каталог.

Validation:
- `git diff --check`: успешно.
- `Tools/novels-tools/novels-content validate zdm`: успешно.
- `Tools/novels-tools/novels-content build zdm editor`: успешно.

Pending / risks:
- 12 исключённых ZDM PNG (64 409 963 байта) временно сохранены в
  `/tmp/novels-zdm-story-global-20260824T1115Z`.
- TZM ещё не мигрирован.

Suggested next step:
- Отдельным lock-блоком мигрировать и собрать TZM.

## 2026-08-24T11:40:00Z — story-global-content — ready-for-integration

Task: Завершить story-global миграцию двух историй и первую итерацию сжатия.

Changed:
- `Projects/novels-tzm/**` и `Projects/novels-zdm/**`: Unity-assets собраны в
  `content/<story>/story/**`; episode/shared каталоги удалены.
- `NovelContentTextureImporter.cs`: Android/iOS ASTC 6×6, Max Size 4096.
- Документация authoring/size: зафиксированы новый контракт и измерения.

Validation:
- `validate zdm`, `build zdm editor`, `validate tzm`, `build tzm editor`:
  успешно.
- Android build ZDM: 111 465 312 B, экономия 64,3%.
- Android build TZM: 304 933 451 B, экономия 37,6%.
- Суммарный Android bundle: 416 398 763 B вместо 800 659 912 B, экономия
  384 261 149 B (48,0%).
- `git diff --check`: успешно.

Pending / risks:
- Нужен ручной визуальный quality gate ASTC на устройствах.
- iOS override задан, но iOS build в этой итерации не запускался.
- Обратимые копии исключённых дубликатов временно находятся в
  `/tmp/novels-zdm-story-global-20260824T1115Z` и
  `/tmp/novels-tzm-story-global-20260824T1121Z`.

Suggested next step:
- Выполнить визуальный smoke test и затем iOS size build; видео оптимизировать
  отдельным блоком, не смешивая с Unity bundle.

## 2026-08-24T11:53:00Z — texture-postprocessor-rename — completed

Task: Сделать назначение Unity texture postprocessor очевидным новичкам.

Changed:
- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`:
  прежний AssetPostprocessor переименован без изменения поведения.
- `.meta` переименован вместе с C#-файлом, GUID сохранён.
- Size-документация использует новое имя.

Validation:
- `Tools/novels-tools/novels-content validate zdm`: успешно.
- `git diff --check`: успешно.
- Ссылок на старое имя в актуальном коде и документации нет; исторический
  handoff оставлен неизменным.

Pending / risks:
- Нет.

Suggested next step:
- Использовать `NovelContentTexturePostprocessor` как единственную общую точку
  автоматических import settings нового story art.

## 2026-08-24T13:00:00Z — tzm-video-crf18 — ready-for-integration

Task: Удалить звук и сжать все TZM-видео H.264 CRF 18.

Changed:
- 51 MP4 в `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/`:
  перекодированы `libx264 -preset slow -crf 18`, audio streams удалены.
- Size-документация и `ParallelWork.tzm-video-crf18.md`: добавлены измерения.
- TZM Android, Mac и iOS releases пересобраны.

Validation:
- 51/51: H.264/YUV420p, исходные resolution/FPS, duration delta ≤ 0,04 с.
- Audio streams: 0.
- Видео: 346 350 427 B → 287 268 760 B, −59 081 667 B (−17,06%).
- `validate tzm` и `build tzm android|editor|ios`: успешно.
- `git diff --check`: успешно.

Pending / risks:
- Требуется ручной visual/loop quality gate.
- iOS Unity bundle вырос до 305 236 981 B против baseline 212 013 875 B;
  это отдельная регрессия texture profile, не video payload.
- Оригиналы сохранены в `/tmp/novels-tzm-video-originals-20260824T1236Z`.

Suggested next step:
- Визуально принять видео, затем отдельным атомарным блоком исправить iOS
  texture profile и повторно измерить bundle.

## 2026-08-24T13:12:00Z — ios-texture-profile — ready-for-integration

Task: Устранить регрессию iOS story bundle после общего ASTC 6×6.

Changed:
- `NovelContentTexturePostprocessor.cs`: Android оставлен ASTC 6×6, iOS
  переведён на ASTC 8×8; importer version увеличена до 4.
- Size/status документация актуализирована.

Validation:
- `build zdm ios`: успешно, bundle 66 039 336 B вместо baseline 116 864 846 B.
- `build tzm ios`: успешно, bundle 181 345 930 B вместо baseline 212 013 875 B.
- Временная регрессия TZM до 305 236 981 B устранена.
- `git diff --check`: успешно.

Pending / risks:
- Нужен ручной iOS visual quality gate ASTC 8×8.

Suggested next step:
- Проверить лица, волосы, UI и градиенты на iOS-устройстве.

## 2026-08-24T11:43:46Z — continuous-wait-policy — completed

Task: Формализовать непрерывное ожидание и автоматическое возобновление.

Changed:
- `ParallelRefactoringCoordination.md`: для явного требования «не
  останавливаться» добавлены повторяемые ограниченные периоды ожидания без
  удержания `write-lock`.
- После освобождения ресурса поток обязан сам перечитать handoff, FIFO и
  состояние репозитория, затем продолжить исходную задачу без нового сообщения
  пользователя.
- Занятая очередь и долгая сборка явно не считаются самостоятельным блокером.

Validation:
- `git diff --no-index --check`: успешно.
- Правило сохраняет интервал не менее 60 секунд и не более 10 проверок за один
  период ожидания.

Pending / risks:
- Нет.

Suggested next step:
- Применять непрерывное ожидание только при явном терминальном требовании
  пользователя или координатора.

## 2026-08-24T12:30:57Z — catalog-simplification — ready-for-integration

Task: Упростить Catalog, кроме prefab.

Changed:
- `CatalogContracts.cs`, `ContentProjectValidation.cs`, `CatalogFlow.cs` и
  `Config/catalog.json`: registry schema 2 хранит упорядоченный массив строк;
  `order`, `enabled` и `CatalogRegistryEntry` удалены.
- `Projects/novels-catalog/Packages/**`: удалена только неиспользуемая прямая
  зависимость `com.unity.2d.sprite`.
- `Projects/novels-catalog/README.md`: добавлены отдельные сценарии изменения
  списка историй и внешнего вида.
- `ParallelRefactoringCoordination.md`: зафиксирован schema-2 контракт.

Validation:
- `novels-content validate catalog`: успешно.
- Unity 6000.3.11f1 batch compile `Novels`: успешно.
- `novels-content build catalog editor`: успешно; bundle audit пройден.
- Отрицательная проверка подтвердила обязательность JSON module; зависимость
  восстановлена до финальной успешной сборки.
- Prefab не изменялся; scoped `git diff --check` успешен.

Pending / risks:
- Schema 2 несовместима со старым registry reader; registry и обновлённый
  клиент должны интегрироваться и публиковаться вместе.
- Play Mode не запускался, поскольку визуальное поведение не менялось.

Suggested next step:
- Интегрировать SDK, Game и Catalog одним контрактным блоком, затем выполнить
  обычный ручной маршрут открытия каталога.

## 2026-08-24T13:31:09Z — validation-simplification — completed

Task: Упростить Content SDK validation, промежуточный план сборки и диагностические
логи, отделив автоматические проверки от ручной приёмки.

Changed:
- `ContentValidation.cs`: удалён неиспользуемый общий warning-слой; ошибки
  по-прежнему собираются и группируются одним сообщением.
- `ContentProjectValidation.cs`: `ContentProject`/inspector заменены линейной
  проверкой и компактным `ContentBuildPlan`; тип проекта задаёт JSON-маркер.
- `ContentBundleAudit.cs`, `ContentPipeline.cs`: успешный audit выдаёт одну
  сводку, подробные assets/dependencies показываются при ошибке.
- `ManualContentChecklist.md` и связанные документы: субъективная визуальная и
  смысловая приёмка явно передана человеку.

Validation:
- Unity 6000.3.11f1 batch compile Novels: успешно, C#-ошибок нет.
- `Tools/novels-tools/novels-content validate all`: Catalog, TZM, ZDM успешно.
- `Tools/novels-tools/novels-content build catalog editor`: успешно; audit
  `novels_catalog` — одна строка, 1 root asset, 6.5 KiB.
- `git diff --check`: успешно.

Pending / risks:
- Ручная визуальная проверка контента не выполнялась; для неё добавлен чек-лист.
- Тесты намеренно не добавлялись и не запускались.

Suggested next step:
- Выполнить обычный ручной маршрут Catalog → история → эпизод; следующая FIFO-
  заявка `catalog-carousel` может начинать работу после освобождения lock.

## 2026-08-24T13:42:24Z — catalog-carousel — ready-for-integration

Task: Заменить вертикальный список Catalog полной горизонтальной каруселью.

Changed:
- `CatalogCarousel.cs`: drag, snap ближайшей карточки, адаптивные отступы,
  масштаб/прозрачность и select-or-open click behavior.
- `Card.cs`, `CatalogScreen.cs`: минимальная интеграция focus и click без
  изменения загрузки данных.
- `screen.prefab`: горизонтальный ScrollRect/LayoutGroup, карточка 280×340 и
  полностью связанные сериализованные ссылки карусели.
- `README.md`: поведение, параметры и ручной device/aspect smoke checklist.

Validation:
- Unity Catalog import/validation: успешно, prefab импортирован без missing
  scripts или invalid references.
- `novels-content build catalog editor`: успешно; audit — 1 root asset,
  bundle 6,7 КиБ.
- Unity 6000.3.11f1 batch compile `Novels`: успешно.
- Scoped `git diff --check`, GUID и prefab file ID: успешно.

Pending / risks:
- Реальные mouse/touch gestures и визуальная плавность на телефоне/планшете
  требуют ручной проверки; batch mode этого не подтверждает.

Suggested next step:
- Пройти чек-лист README в Game Play Mode на узком и широком экране.
## 2026-08-24T14:08:00Z — character-alpha-trim — completed

Task: physically trim transparent canvas from character emotion/hair/accessory
sprites while preserving authored placement at runtime.

Changed:
- added one generated `sprite-trim-manifest.asset` per story and runtime layout
  restoration in `CharacterScreen`; character prefabs and asset addresses remain
  unchanged;
- added `CharacterSpriteAlphaTrim` and CLI command
  `novels-content trim-sprites <story|all> <report|apply> [padding]`;
- trimmed 305 TZM and 391 ZDM PNGs with 4 px padding; source files decreased by
  76 386 043 B, and recoverable originals are under each project's
  `Build/SpriteTrimBackup`;
- documented the authoring workflow and exact bundle deltas in
  `ParallelWork.character-alpha-trim.md` and size documentation.

Validation:
- main Novels Unity batch compile: passed;
- repeated trim report: 0 new trims, 696 already processed;
- TZM/ZDM content validation: passed;
- Android/iOS builds and bundle audits: passed;
- final `git diff --check`: passed.

Pending / risks:
- manual visual gate is still required for emotion, front/back hair and
  accessories on narrow and wide screens.

Suggested next step:
- run the representative character visual checklist before publishing the new
  bundles.

## 2026-08-24T14:15:00Z — platform-library-cache — completed

Task: Устранить повторный массовый импорт текстур при чередовании Android и
iOS content builds.

Changed:
- `Tools/novels-tools/novels-content`: перед сборкой активируется постоянный
  `Library` выбранной платформы; неактивные кэши хранятся в игнорируемом
  `<project>/Build/UnityLibraryCache`.
- `Tools/novels-tools/README.md`, `ContentPipeline.md`: описаны расположение,
  первый холодный прогрев, требование закрыть Unity и очистка кэшей.

Validation:
- shell syntax и `novels-content doctor`: успешно.
- Catalog Android → iOS → Android: все три bundle build/audit успешны, размер
  каждой платформы 6,7 КиБ.
- Повторный Android-запуск вывел `Activate cached android Library` и не содержал
  запусков `TextureImporter`.
- `git diff --check`: успешно.

Pending / risks:
- TZM и ZDM получат отдельные кэши при следующих штатных сборках; их холодный
  прогрев намеренно не запускался ради времени и памяти.
- Кэши увеличивают локальное использование диска, но лежат только в уже
  игнорируемых `Library` и `Build`.

Suggested next step:
- Обычной командой собрать нужные TZM/ZDM платформы; первая сборка прогреет
  кэш, последующие переключения будут использовать сохранённый.
## 2026-08-24T14:32:00Z — character-body-clothes-trim — completed

Task: extend the existing reversible character alpha trim to body and clothes.

Changed:
- `CharacterSpriteAlphaTrim.cs`: added clothes and narrowly classified body
  addresses; unknown nested view folders remain excluded;
- trimmed 130 additional TZM PNGs and 64 ZDM PNGs with 4 px padding, preserving
  original files and `.meta` in timestamped `Build/SpriteTrimBackup` folders;
- expanded story manifests to 435 TZM and 455 ZDM entries;
- updated size/status documentation with exact source and bundle deltas.

Validation:
- main Novels Unity batch compile: passed;
- repeated trim report: 0 new trims, all 890 entries recognized;
- TZM/ZDM validation: passed;
- Android/iOS builds and bundle audits: passed;
- final `git diff --check` and CLI shell syntax: passed.

Pending / risks:
- manual visual comparison of body/clothes alignment on narrow and wide screens
  is still required before publication.

Suggested next step:
- run the representative character visual checklist, then publish the rebuilt
  content if alignment matches the backup originals.

## 2026-08-24T14:56:27Z — editor-content-smoke-build — completed

Task: Подготовить актуальные bundles и локальную композицию для полного ручного
теста Novels в Editor.

Changed:
- Сгенерированы свежие Mac releases/bundles Catalog, TZM и ZDM.
- `Novels/Build/LocalContent` скомпонован для `FileSystemContentSource` игры.
- Unity-generated `Novels.slnx` нормализован обратно к LF без изменения состава.

Validation:
- Unity 6000.3.11f1 batch compile Game: успешно, C#-ошибок нет.
- `novels-content validate all`: Catalog, TZM, ZDM успешно.
- `novels-content build all editor`: все три проекта успешно, bundle audit
  прошёл; Catalog 6,7 КиБ, TZM 240902,1 КиБ, ZDM 145638,0 КиБ.
- Локально проверены existence, size и SHA-256 всех Mac payloads: 1 Catalog,
  63 TZM, 16 ZDM.
- `git diff --check`: успешно.

Pending / risks:
- Play Mode и визуальное поведение не автоматизировались; требуется ручной
  маршрут Catalog → TZM/ZDM, особенно carousel и trimmed character layers.
- Тесты не запускались; проект ранее договорённо проверяется ручным smoke test.

Suggested next step:
- Открыть `Novels/Assets/Novels/Novels.unity`, нажать Play и пройти обе истории
  по `ManualContentChecklist.md`.
## 2026-08-24T15:08:44Z — catalog-carousel-canvasgroup-hotfix — ready-for-integration

Task: Исправить MissingComponentException при первом показе карточки Catalog.

Changed:
- `Packages/NovelsContentSdk/Runtime/Catalog/View/Card.cs`: заменён несовместимый
  с Unity fake-null оператор `??` на две явные проверки `== null`, поэтому
  отсутствующий `CanvasGroup` действительно добавляется перед установкой alpha.

Validation:
- Scoped `git diff --check`: успешно.
- Unity batch compile: не запущен, поскольку проект уже открыт в Unity Editor;
  второй экземпляр Unity штатно отказался открывать тот же проект.

Pending / risks:
- Требуется дождаться компиляции открытого Editor и повторить запуск Catalog.

Suggested next step:
- В Unity выйти из Play Mode при необходимости, дождаться завершения compile и
  снова запустить игру; каталог должен открыться без MissingComponentException.
## 2026-08-24T15:18:47Z — catalog-card-sizing — ready-for-integration

Task: Сделать карточки Catalog адаптивными и размером около 80% viewport.

Changed:
- `CatalogCarousel.cs`: родительский layout пересчитывается до чтения viewport;
  карточкам назначается размер до 80% viewport с сохранением пропорций;
  изменение размеров viewport отслеживается по обеим координатам.

Validation:
- Scoped `git diff --check`: успешно.
- Новых сериализованных ссылок нет; коэффициент имеет безопасный default 0.8.
- Отдельный Unity compile не запускался: проект открыт в пользовательском Editor.

Pending / risks:
- Требуется ручной Play Mode smoke на текущем портретном разрешении и проверка
  свайпа между двумя карточками.

Suggested next step:
- Дождаться recompilation открытого Unity и снова открыть Catalog.
## 2026-08-24T15:27:40Z — catalog-content-height-hotfix — ready-for-integration

Task: Исправить оставшееся сжатие карточек Catalog.

Changed:
- `CatalogCarousel.cs`: content горизонтального layout получает высоту viewport
  перед расчётом карточек и rebuild.

Validation:
- Scoped `git diff --check`: успешно.
- Исходные cover PNG проверены визуально: 1360×1920, без видимых прозрачных
  полей; проблема находилась в нулевой высоте `StoryList`.
- Отдельный Unity compile не запускался: проект открыт пользователем.

Pending / risks:
- Открытый Editor ещё не перечитал последнюю правку; требуется выйти из Play
  Mode, дождаться recompilation и запустить Catalog заново.

Suggested next step:
- Повторить Play Mode smoke после recompilation; bundle rebuild не нужен.
## 2026-08-24T15:33:10Z — character-trim-manifest-hotfix — ready-for-integration

Task: Исправить падение story queue из-за повторного ожидания trim-manifest.

Changed:
- `CharacterSpriteSetLoader.cs`: `Preserve` заменён на `AsyncLazy`; готовый
  manifest загружается до `WhenAll`, а `GetSprite` выполняет только lookup.

Validation:
- Scoped `git diff --check`: успешно.
- Путь повторного ожидания одного `MemoizeSource` удалён статически.
- Отдельный Unity compile не запускался: проект открыт пользователем.

Pending / risks:
- Требуется выйти из Play Mode, дождаться recompilation и повторить запуск
  `tzm/s01e01`.

Suggested next step:
- Повторный Play Mode smoke; content bundle rebuild не требуется.

## 2026-08-26T16:55:00Z — story-authoring-defaults — ready-for-integration

Task: Добавить ручную JSON-разметку чанков в Ink Tools и перенести дефолтную
внешность персонажей из глобального hardcode в ассет истории.

Changed:
- `Packages/NovelsContentSdk/Editor/StoryAssetOrderWindow.cs` и
  `ExperimentalStreamingPlan.cs`: экспорт/чтение `*.ink.chunks.json` с art,
  video и audio в линейном порядке.
- `Packages/Bundles/ContentReleaseValidator.cs` и
  `Novels/Assets/Novels/StoryStreamingController.cs`: несколько media-файлов
  могут принадлежать одному логическому чанку и delivery group.
- `Packages/NovelsContentSdk/Runtime/Content/**` и Character loaders: дефолтные
  одежда, волосы, цвет и аксессуар читаются из story definition.
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`:
  настроены Салли и Алекса.

Validation:
- `dotnet build Novels/Novels.Content.csproj`: успешно.
- `git diff --check`: успешно.
- Старые глобальные default-hair identifiers удалены.

Pending / risks:
- Полная Unity-компиляция отложена: открытый TZM Editor не перечитал внешний
  package; второй Unity не запускался. Требуется refresh/restart и PlayMode
  smoke Салли/Алексы.
- JSON-разметка ещё не создана автором из окна, поэтому текущая сборка пока
  использует прежний fallback.

Suggested next step:
- Обновить открытый Editor, открыть `Novels/Content/Ink Tools`, рассчитать
  ассеты и сохранить разметку; затем проверить Inspector и первые появления
  Салли/Алексы.

## 2026-08-26T17:00:00Z — story-authoring-defaults — completed

Task: Уточнить заголовок колонки первого использования в Ink Tools.

Changed:
- `StoryAssetOrderWindow.cs`: `Первое` заменено на `Позиция в Ink`, колонка
  расширена.

Validation:
- `git diff --check`: успешно.

Pending / risks:
- none.

Suggested next step:
- none.

## 2026-08-26T17:55:00Z — story-authoring-defaults — ready-for-integration

Task: Убрать mixer из story definition, автозаполнять Episodes из root Ink и
показать границы чанков в Ink Tools.

Changed:
- `NovelContentAsset.cs`, TZM/ZDM definitions: story-level AudioMixer удалён.
- `EntryPoint`, application/novel runtime и `Novels.unity`: общий AudioMixer
  теперь принадлежит приложению.
- `StoryAssetOrderWindow.cs`: Episodes формируются по INCLUDE и
  сохраняют authoring metadata; таблица показывает chunk separators.

Validation:
- TZM Unity recompiled Content and ContentSdk.Editor without C# errors.
- `dotnet build Novels/Novels.Content.csproj`: success, 0 warnings/errors.
- `git diff --check`: success.
- Story/content search found no remaining mixer field usage.

Pending / risks:
- Main Novels Editor compile and PlayMode mixer smoke remain pending because TZM Editor
  currently owns the exclusive Unity resource.

Suggested next step:
- Close/refresh TZM as appropriate, compile main Novels, then click `Обновить эпизоды`
  once in each atomic story project and review the generated entries.

## 2026-08-27T09:29:13Z — story-authoring-defaults — ready-for-integration

Task: Отделить неизвестные ассеты от последнего чанка в таблице Ink Tools.

Changed:
- `Packages/NovelsContentSdk/Editor/StoryAssetOrderWindow.cs`: строки без
  назначения в chunk layout получают отдельный разделитель
  `Не входят в чанки`; состав и JSON-контракт чанков не менялись.

Validation:
- Scoped `git diff --check`: успешно.
- Статически подтверждено, что `CreateChunkLayout` по-прежнему включает только
  `IsReferenced`, а новый раздел рисуется только при отсутствии пути в
  `_chunksByPath`.

Pending / risks:
- Unity assembly ещё не пересобралась после правки; требуется обычный refresh
  открытого TZM Editor и визуальная проверка последней страницы таблицы.

Suggested next step:
- Нажать `Рассчитать ассеты` и убедиться, что после последнего чанка отображён
  самостоятельный раздел `Не входят в чанки`.

## 2026-08-27T10:18:57Z — story-authoring-defaults — ready-for-integration

Task: Перенести ручную разметку чанков из sidecar JSON в story asset и добавить
генератор непосредственно в Inspector контентного проекта.

Changed:
- `NovelContentAsset` хранит корневой Ink, целевой размер и чанки в скрытых
  authoring-полях; элементы чанков — GUID-строки без прямых Object references.
- Новый общий `NovelContentAssetEditor` рассчитывает разметку из Ink, проверяет
  её и позволяет вручную менять порядок чанков и art/video/audio внутри них.
- `ExperimentalStreamingPlan` читает authored layout из story asset; при пустой
  разметке сохраняется прежний автоматический fallback. Sidecar
  `*.ink.chunks.json` больше не читается.
- `tzm.asset` связан с корневым `tzm.ink` и настроен на 16 MiB; ZDM-файлы не
  изменялись, но общий Inspector будет доступен и в ZDM.

Validation:
- Открытый TZM Editor пересобрал `Novels.Content` и
  `Novels.ContentSdk.Editor` с новым Inspector без C#-ошибок.
- Editor assembly дополнительно скомпилирован актуальным Unity Bee rsp —
  успешно.
- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- GUID корневого Ink в `tzm.asset` совпадает с `tzm.ink.meta`.
- Scoped `git diff --check` и поиск старого JSON-reader — успешно.

Pending / risks:
- `_authoringChunks` в TZM намеренно пуст до явного действия автора; content
  build пока использует fallback.
- Существующий untracked `tzm.ink.chunks.json` оставлен без удаления, но код его
  игнорирует.

Suggested next step:
- Выбрать `tzm.asset`, нажать `Рассчитать по Ink`, проверить/подправить список,
  нажать `Проверить разметку`, затем выполнить content build и bundle audit.

## 2026-08-27T10:53:50Z — story-authoring-defaults — ready-for-integration

Task: Убрать дублирующий Ink Tools и оставить единый authoring-интерфейс в
Inspector story asset.

Changed:
- `NovelContentAssetEditor.cs`: в одном Inspector объединены compile Ink и
  source-map, обновление Episodes выбранного ассета, линейный список,
  генерация/валидация и ручное редактирование чанков.
- Линейный список адаптирован к ширине Inspector: вертикальные строки, фильтр,
  пагинация по 40 элементов и разделители фактических чанков.
- `StoryInkAuthoring.cs`: editor-only операции с Ink и Episodes отделены от UI;
  обновление Episodes использует выбранный `NovelContentAsset` и поддерживает
  Undo.
- `StoryAssetOrderWindow.cs` и его meta удалены; прежний GUID перенесён на
  `StoryInkAuthoring.cs`. Оба menu item старого окна удалены.

Validation:
- Финальный Editor source set скомпилирован Unity 6000.3.11f1 Roslyn по Bee rsp
  — 0 errors, 0 warnings.
- Поиск `StoryAssetOrderWindow`, `Novels/Content/Ink Tools` и
  `Assets/Novels/Open Ink Tools` в Editor C# — совпадений нет.
- Scoped whitespace/diff check — успешно.

Pending / risks:
- Открытый TZM Editor не подхватил внешний package refresh автоматически;
  standalone-компиляция успешна, но визуальный Inspector smoke требует
  `Assets/Refresh` или повторного открытия проекта.

Suggested next step:
- Обновить TZM Editor, выбрать `tzm.asset` и визуально проверить единый workflow
  сверху вниз; отдельного Ink Tools в меню больше быть не должно.

## 2026-08-27T11:10:17Z — story-authoring-defaults — ready-for-integration

Task: Устранить лаги единого Inspector при отображении ручной разметки чанков.

Changed:
- `NovelContentAssetEditor.cs`: весь ручной список закрывается верхним foldout;
  при закрытом разделе 44 чанка и 1027 GUID TZM не обходятся для отрисовки.
- Размер файла вычисляется и кэшируется только для раскрытого чанка.
- Одновременно раскрывается один чанк; его список разбит на страницы по 30
  объектов, чтобы ограничить число `AssetDatabase`-операций за кадр.

Validation:
- Editor assembly скомпилирована Unity 6000.3.11f1 Roslyn по актуальному Bee
  rsp — 0 errors, 0 warnings.
- Проверка trailing whitespace изменённого C#-файла — успешно.
- Общий `git diff --check` видит только ранее существующие trailing spaces в
  Unity YAML `tzm.asset`; этот блок контентный ассет не менял.

Pending / risks:
- Фактическое изменение времени кадра Editor не измерялось профайлером;
  визуальный smoke требует refresh открытого TZM Editor.

Suggested next step:
- Выполнить `Assets/Refresh`, выбрать `tzm.asset`, сравнить отзывчивость при
  закрытом разделе и проверить перелистывание самого крупного чанка.

## 2026-08-27T11:29:21Z — episode-schema-dedup — ready-for-integration

Task: Убрать дублирующий SourcePath из Episodes и распознавать ID в prefixed
Ink filenames.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: `_sourcePath` и публичный
  `EpisodeDefinition.SourcePath` удалены как неиспользуемый runtime-контракт.
- `StoryInkAuthoring.cs`: ID извлекается по `sXXeXX` из имён вроде
  `ZDMs01e01.ink`; добавлена проверка неоднозначных и повторных ID.
- `tzm.asset`, `zdm.asset`: удалены только сериализованные `_sourcePath`.
  Source-map и исходные Ink не менялись.

Validation:
- `Novels.Content.csproj` — 0 warnings, 0 errors.
- Unity Roslyn: `Novels.Content` и TZM `Novels.ContentSdk.Editor` — успешно.
- В изменённой области нет `_sourcePath|SourcePath`; ID сохранены: TZM 7,
  ZDM 11; source maps содержат реальные имена 7/11 файлов.

Pending / risks:
- `Novels.csproj --no-restore` не стартует без generated
  `Temp/obj/Novels/project.assets.json`; статический поиск подтверждает, что
  Game runtime удалённый контракт не использовал.
- Нужен визуальный Inspector smoke после package refresh, особенно ZDM.

Suggested next step:
- В ZDM нажать `Обновить эпизоды` и проверить IDs `s01e01`…`s02e01`, затем
  выполнить обычную последовательную content validation при интеграции.

## 2026-08-27T11:45:13Z — episode-schema-dedup — ready-for-integration

Task: Убрать оставшееся дублирование StoryPath из каждого Episodes.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: story path перенесён из
  `EpisodeEntry` / `EpisodeDefinition` на уровень истории.
- `StoryInkAuthoring.cs`, `NovelContentAssetEditor.cs`,
  `ContentProjectValidation.cs`: authoring, Inspector и проверка работают с
  единым story-level полем.
- `NovelRuntime.cs`, `NovelRuntime.NovelPreparation.cs`: выбор эпизода и
  загрузка Ink/source-map сохраняют и используют `NovelDefinition.StoryPath`.
- `tzm.asset`, `zdm.asset`: оставлено по одному корневому пути
  `tzm.ink.json` / `zdm.ink.json`; вложенные поля удалены.

Validation:
- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- Unity 6000.3.11f1 Roslyn: `Novels.Content`, TZM
  `Novels.ContentSdk.Editor` и основной `Novels` — успешно.
- В definitions найдено по одному корневому `_storyPath`, вложенных — 0;
  оба указанных Ink JSON существуют.
- Scoped `git diff --check` — успешно.

Pending / risks:
- Нужен только обычный package refresh и визуальный Inspector smoke; бандлы
  ради изменения сериализуемой схемы не пересобирались.

Suggested next step:
- После refresh открыть TZM/ZDM definition и проверить, что `Скомпилированный
  Ink` показан один раз над списком Episodes.

## 2026-08-27T11:59:51Z — story-content-version — ready-for-integration

Task: Перенести ContentVersion из каждого Episodes на уровень всей истории.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: единый story-level
  `ContentVersion`; episode-level поле и constructor parameter удалены.
- `NovelRuntime.cs`, `NovelRuntime.Content.cs`: playable definition сохраняет
  общую версию, а episode save использует её вместо версии эпизода.
- `NovelProgress.cs`: новая запись содержит общую версию; чтение совместимо с
  прежней агрегированной строкой одинаковых episode versions.
- `StoryInkAuthoring.cs`, `NovelContentAssetEditor.cs`: обновление Episodes не
  управляет версиями, Inspector показывает одно поле `Версия истории`.
- `tzm.asset`, `zdm.asset`: оставлено по одной корневой `_contentVersion: 1`,
  вложенные значения удалены.

Validation:
- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- Unity 6000.3.11f1 Roslyn: `Novels.Content`, TZM
  `Novels.ContentSdk.Editor` и основной `Novels` — успешно.
- TZM: 7 Episodes; ZDM: 11 Episodes; корневых версий по одной, вложенных — 0.
- Scoped `git diff --check` — успешно; общий check сообщает только о прежних
  trailing spaces в других строках большого Unity YAML `tzm.asset`.

Pending / risks:
- Для runtime-теста требуется package refresh и пересборка content bundles,
  поскольку сериализуемая схема definition изменилась.

Suggested next step:
- После refresh проверить единое поле `Версия истории` в TZM/ZDM Inspector,
  затем пересобрать бандлы перед следующим Play Mode smoke.

## 2026-08-27T12:42:41Z — episode-title-authoring — ready-for-integration

Task: Формировать Episode title из настоящего названия внутри Ink.

Changed:
- `StoryInkAuthoring.cs`: первая narrator-строка вида
  `... (История): Серия N: Название` становится title; при её отсутствии
  используется `Сезон N, эпизод M`.
- `tzm.asset`, `zdm.asset`: 17 найденных авторских названий записаны в
  Episodes; незавершённый TZM `s01e07` сохранил fallback.

Validation:
- Автоматическая read-only сверка root INCLUDE, 18 episode Ink и двух
  definitions — TZM 7/7, ZDM 11/11 совпадений.
- Unity 6000.3.11f1 Roslyn по TZM `Novels.ContentSdk.Editor.rsp` — успешно.
- Scoped whitespace/diff check — успешно; существующие Unity YAML trailing
  spaces вне title-строк не исправлялись.

Pending / risks:
- Нужен package refresh и визуальный Inspector smoke.
- `StoryCommandSyntax.MetadataNames` по-прежнему hardcoded. Он не содержит
  `Описание` и не распознаёт ZDM-форму `Серия 2/10` без двоеточия, поэтому эти
  строки могут стать обычными диалогами.

Suggested next step:
- Отдельным блоком добавить в story asset `_ignoredStoryLinePrefixes` со
  значениями `Название`, `Серия`, `Описание`, `Жанры`, `Аннотация`, `Статы`;
  передавать их и в live `StoryCommands.Entity`, и в `ReplayValidator`, с
  поддержкой разделителя `:` и пробела.

## 2026-08-27T14:05:26Z — story-path-convention — ready-for-integration

Task: Удалить ручную настройку compiled Ink path из единого story Inspector.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: сериализуемый `_storyPath` и
  constructor parameter удалены; runtime `StoryPath` выводится как
  `<story-id>.ink.json`.
- `NovelContentAssetEditor.cs`, `StoryInkAuthoring.cs`: поле больше не
  показывается и не записывается; authoring проверяет имя `<story-id>.ink`.
- `ContentProjectValidation.cs`: сообщение об отсутствии compiled Ink
  уточнено без изменения diagnostic ID.
- `NovelRuntime.cs`: создание playable definition использует упрощённый
  constructor.
- `tzm.asset`, `zdm.asset`: удалён только корневой `_storyPath`.

Validation:
- Статический поиск всех constructor calls и `StoryPath` consumers: успешно.
- `_storyPath` и label `Скомпилированный Ink` отсутствуют.
- Ожидаемые root/compiled Ink TZM и ZDM существуют; TZM root GUID сохранён.
- Scoped `git diff --check` C# и ZDM asset: успешно.

Pending / risks:
- Unity compile/refresh и bundle rebuild не запускались: открытый TZM Editor
  PID 97689 владеет Unity-ресурсом.
- После refresh требуется пересборка content bundles из-за изменения
  сериализуемой схемы definition.

Suggested next step:
- Обновить открытый TZM Editor, проверить отсутствие поля в Inspector,
  пересобрать TZM content bundle и выполнить обычный runtime smoke.

## 2026-08-27T14:19:39Z — story-episode-defaults — ready-for-integration

Task: Убрать дублирование EndMarker и SilentAudioIds в каждом Episode.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: общие marker и silent IDs
  перенесены на story level; `EpisodeDefinition` оставляет ID/title, а
  `EpisodeMediaDefinition` удалён.
- `NovelContentAssetEditor.cs`, `StoryInkAuthoring.cs`: настройки показываются
  один раз; обновление Episodes больше их не генерирует и не копирует.
- `NovelRuntime.cs`, `NovelRuntime.NovelPreparation.cs`,
  `NovelRuntime.EpisodeComposition.cs`: runtime использует общие настройки
  `NovelDefinition`.
- `tzm.asset`, `zdm.asset`: значения `КОНЕЦ СЕРИИ` и `тишина` сохранены один
  раз над Episodes; вложенные копии удалены.

Validation:
- Статический поиск всех constructor calls и старых episode-level consumers:
  успешно, старых ссылок нет.
- TZM 7 Episodes, ZDM 11 Episodes; в каждом asset по одному marker/list,
  вложенных полей нет.
- Scoped `git diff --check` C# и ZDM asset: успешно.

Pending / risks:
- Unity compile/refresh и bundle rebuild не запускались: открытый TZM Editor
  PID 97689 владеет Unity-ресурсом.
- После refresh требуется пересборка content bundles из-за изменения
  сериализуемой схемы definition.

Suggested next step:
- Обновить открытый TZM Editor, проверить общие поля над Episodes, затем
  пересобрать TZM content bundle и выполнить runtime smoke конца эпизода/тишины.

## 2026-08-27T14:27:38Z — episode-description — ready-for-integration

Task: Извлекать description эпизодов из Ink и показывать в каталоге выбора.

Changed:
- `NovelContentAsset.cs`, `NovelDefinition.cs`: добавлено episode-level поле
  `Description`.
- `StoryInkAuthoring.cs`: сначала читается `Описание:`, при отсутствии —
  `Аннотация:`; сохраняется текст после двоеточия.
- `CatalogFlow.cs`: описание передаётся существующему `CatalogItem` экрана
  выбора эпизода.
- `tzm.asset`, `zdm.asset`: записаны 7 и 11 авторских descriptions.

Validation:
- Все 18 episode Ink имеют `Описание:` или fallback `Аннотация:`.
- Автоматическая сверка извлечённых Ink-значений с asset values: полное
  совпадение.
- Статический поиск constructor/UI consumers: успешно.
- Scoped `git diff --check` C# и ZDM asset: успешно; TZM сообщает только ранее
  существующие trailing spaces вне изменённых description-строк.

Pending / risks:
- Unity compile/refresh и bundle rebuild не запускались: открытый TZM Editor
  PID 97689 владеет Unity-ресурсом.
- После refresh требуется пересборка content bundles из-за изменения
  сериализуемой episode schema.

Suggested next step:
- Обновить открытый TZM Editor, проверить descriptions в Episodes, затем
  пересобрать TZM bundle и открыть экран выбора эпизода в Play Mode.

## 2026-08-27T14:59:03Z — tzm-flat-layout — ready-for-integration

Task: Упростить физическую структуру папок пилотного TZM без изменения
runtime-адресов.

Changed:
- `Projects/novels-tzm/Assets/**`: definition, Ink, character/location/choice
  art, presentation и video перенесены в короткие корневые каталоги вместе с
  `.meta`; старые `RemoteAssets`/`StreamingAssets` и повторные `content/tzm` /
  `story` удалены.
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`: добавлено отображение
  физических TZM-путей в прежние AssetBundle addressable names и file keys с
  legacy fallback для ZDM/Catalog.
- Точечные Editor authoring/validation/streaming/import/trim файлы: работают с
  новым layout и сохраняют legacy-layout.
- `Projects/novels-tzm/README.md`, `ContentPipeline.md`,
  `ContentAuthoringGuide.md`: описана короткая структура и граница физических /
  runtime-путей.

Validation:
- Unity Roslyn `Novels.ContentSdk.Editor.rsp`: успешно, C#-ошибок нет.
- `novels-content doctor`: успешно.
- 1270/1270 bundle addresses и 61/61 payload paths совпадают со старыми
  логическими ключами, missing/extra 0/0.
- 1027/1027 GUID chunk layout разрешаются в `.meta`; дубликатов GUID и
  файлов/каталогов без meta нет.
- Scoped `git diff --check`: успешно.

Pending / risks:
- Открытый TZM Editor PID 97689 удерживает UnityLockfile. Второй Unity не
  запускался, поэтому полноценные `validate tzm`, bundle build/audit и runtime
  smoke отложены до refresh/закрытия Editor.
- ZDM намеренно не мигрирован и продолжает использовать legacy-layout.

Suggested next step:
- Выполнить refresh открытого TZM Editor, проверить `Assets/tzm.asset`, затем
  последовательно запустить `validate tzm`, `build tzm editor` и Play Mode
  smoke на свежем локальном контенте.

## 2026-08-27T15:28:44Z — character-trim-regeneration — ready-for-integration

Task: Добавить безопасную перегенерацию character sprite trim непосредственно
в Inspector манифеста без повторной обрезки существующих PNG.

Changed:
- `CharacterSpriteTrimManifest.cs`: generated entry хранит SHA-256 уже
  обработанного PNG; runtime layout остаётся прежним.
- `CharacterSpriteAlphaTrim.cs`: hash-aware полная перегенерация, миграция
  старых записей по crop-размеру, удаление устаревших записей, backup/rollback
  и Inspector-кнопки `Проверить арты` / `Перегенерировать трим`.
- `ContentAuthoringGuide.md`: новый Inspector-first workflow и правило
  добавлять заменяющий арт на исходном авторском холсте.

Validation:
- Unity Roslyn `Novels.Character` и `Novels.ContentSdk.Editor` — успешно.
- TZM 435/435 и ZDM 455/455 существующих manifest entries разрешаются в PNG,
  их физические размеры совпадают с crop; missing/mismatch 0/0.
- `novels-content doctor` и scoped `git diff --check` — успешно.

Pending / risks:
- Полный Unity refresh/build не запускался: TZM Editor уже открыт пользователем.
- Первый TZM apply добавит хеши в 435 записей manifest, но не должен изменить
  ни одного PNG; после этого нужен rebuild content bundle.

Suggested next step:
- После refresh выбрать `Assets/Characters/sprite-trim-manifest.asset`, нажать
  `Проверить арты`, затем `Перегенерировать трим` и проверить Console/diff.

## 2026-08-27T15:42:03Z — character-trim-tool-safety — ready-for-integration

Task: Устранить двусмысленную «перегенерацию» trim manifest и явно отделить
безопасное обновление индекса от физической записи PNG.

Changed:
- `Packages/NovelsContentSdk/Editor/CharacterSpriteAlphaTrim.cs`: Inspector
  получил три шага — read-only preview, manifest-only index update и отдельную
  подтверждаемую обрезку только показанных новых/заменённых PNG; добавлена
  повторная проверка плана, SHA-256 и crop-геометрии до записи.
- `Novels/Docs/AI/ContentAuthoringGuide.md`: workflow и гарантии записи
  актуализированы без термина «перегенерировать».
- `Novels/Docs/AI/ParallelWork.character-trim-tool-safety.md`: записаны scope,
  результат и проверки.

Validation:
- Unity 6000.3.11f1 Roslyn: `Novels.Character` и финальная
  `Novels.ContentSdk.Editor` assemblies скомпилированы без ошибок.
- Текущий TZM: 435 PNG и 435 записей с хешами; SHA-256 совпадают для 435/435,
  missing/hash mismatch 0/0.
- `novels-content doctor` и scoped `git diff --check`: успешно.

Pending / risks:
- Открытый TZM Editor удерживает UnityLockfile, поэтому второй Unity, полный
  content validate/build и визуальный Inspector smoke не запускались.
- PNG и `sprite-trim-manifest.asset` в рамках этого исправления не изменялись.

Suggested next step:
- Выполнить refresh открытого TZM Editor и нажать только
  `1. Проверить изменения (без записи)`: на текущем TZM ожидается 0 PNG к
  обрезке и disabled-кнопки обоих записывающих действий.

## 2026-08-27T16:02:12Z — tzm-video-posters-unused — ready-for-integration

Task: Пометить постеры видеолокаций TZM как неиспользуемые, сохранив статические
PNG для локаций без видео.

Changed:
- `NovelContentAsset.cs`, `NovelContentAssetEditor.cs`: добавлена скрытая
  authoring-группа GUID `Не используется`, Inspector-редактор, исключение из
  повторного расчёта чанков и проверки missing/duplicate/chunk overlap.
- `BackgroundPresentationController.cs`: видео разрешается до PNG; при наличии
  видео первый кадр готовится за однотонным переходным экраном, а PNG
  загружается только при отсутствии video URL.
- `Projects/novels-tzm/Assets/tzm.asset`: 56 PNG с 51 прямым MP4 и 5 aliases
  перенесены из чанков в `Не используется`; 9 статических PNG оставлены в
  чанках; 44 чанка сведены к 39 без media-only chunks.
- `ContentAuthoringGuide.md`: описаны правила видеопостеров и новой группы.

Validation:
- Unity 6000.3.11f1 Roslyn: `Novels.Content`, `Novels.Location` и
  `Novels.ContentSdk.Editor` скомпилированы без ошибок.
- Идемпотентная статическая проверка TZM: 56 unused-постеров, 9 статических
  локаций, overlap/duplicates/missing GUID/media-only chunks 0.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` tracked diff — успешно; новый Editor-файл чист. В
  исходном Unity YAML `tzm.asset` остаются четыре прежних trailing spaces.

Pending / risks:
- Полный Unity `validate tzm`, bundle build и Play Mode smoke не запускались:
  уже открытый TZM Editor удерживает `Temp/UnityLockfile`; второй Unity не
  запускался.
- Изменены сериализуемая authoring-схема и chunk layout, поэтому TZM bundle
  нужно пересобрать после refresh.

Suggested next step:
- В открытом TZM Editor выполнить refresh, проверить `Не используется (56)`,
  затем последовательно запустить `validate tzm`, `build tzm editor` и smoke:
  видеолокация ждёт первый кадр на чёрном переходе, статическая локация без MP4
  продолжает показывать PNG.

## 2026-08-27T16:22:37Z — tzm-precise-usage-order — ready-for-integration

Task: Исправить порядок TZM chunks по точному первому использованию команд Ink.

Changed:
- `ExperimentalStreamingPlan.cs`: location/video/audio используют общий
  `Novels.StoryCommands` parser; video aliases разрешаются через definition;
  character art сопоставляется с точным speaker/candidate и wardrobe use вместо
  широкого совпадения имени папки.
- `StoryInkAuthoring.cs`, `Novels.ContentSdk.Editor.asmdef`: usage report
  получает runtime definition и явные зависимости parser/contracts.
- `Projects/novels-tzm/Assets/tzm.asset`: разметка пересчитана с 24 до 18
  чанков; все 51 MP4 и существующие 56 unused poster GUID сохранены.
- `ContentAuthoringGuide.md`: зафиксирован command-aware порядок чанков.

Validation:
- Unity 6000.3.11f1 Roslyn `Novels.ContentSdk.Editor`: успешно.
- Детерминированный пересчёт: 18 чанков, 700 назначенных GUID, 565 неизвестных
  ассетов исключены, duplicate/unused overlap/media-only chunks 0; повторный
  запуск сохраняет SHA-256 `796dceff…a78a96c`.
- MP4 51/51: `номер в отеле`, `вид из окна`, `кафе` находятся в chunk 1;
  `атлантида`, ранее ложно найденная в аннотации, находится в chunk 9.
- Scoped `git diff --check`: успешно.

Pending / risks:
- Полный Unity `validate tzm`, bundle build и Inspector smoke не запускались:
  второй Unity не стартовал при существующем TZM `Temp/UnityLockfile`.
- Изменены Editor algorithm и chunk layout, поэтому после refresh требуется
  пересобрать TZM content bundle.

Suggested next step:
- В TZM Inspector проверить первые чанки, затем последовательно выполнить
  `validate tzm`, `build tzm editor` и короткий playback smoke начала s01e01.

## 2026-08-27T16:28:29Z — tzm-unused-video-poster-label — ready-for-integration

Task: Визуально отличить video posters от прочих ассетов в группе
`Не используется`.

Changed:
- `NovelContentAssetEditor.cs`: unused location PNG получает бейдж
  `Постер видео`, если его ID соответствует direct или alias-resolved MP4;
  вычисляемый набор кэшируется и инвалидируется при project/alias changes.
- `ContentAuthoringGuide.md`: описана вычисляемая Inspector-метка без нового
  serialized-поля.

Validation:
- Статическая TZM-сверка: метку получают 56/56 unused GUID по 51 MP4 и
  5 video aliases.
- Unity 6000.3.11f1 Roslyn `Novels.ContentSdk.Editor`: успешно.
- SHA-256 `tzm.asset` не изменился: `796dceff…a78a96c`.
- Scoped whitespace check: успешно.

Pending / risks:
- Визуальный Inspector smoke не выполнялся; нужен refresh уже открытого TZM
  Editor. Serialized schema, chunks и runtime не менялись.

Suggested next step:
- Раскрыть `Не используется` в `Assets/tzm.asset` и проверить бейджи на обеих
  страницах списка; content bundle пересобирать из-за этой UI-правки не нужно.

## 2026-08-27T16:42:53Z — tzm-exclude-legacy-presentation-art — ready-for-integration

Task: Исключить старые character/location PNG внутри `Presentation` из
расчёта TZM streaming chunks.

Changed:
- `ExperimentalStreamingPlan.cs`: character roots ограничены
  `Assets/Characters/**` и legacy `*/story/character/characters/**`; вложенные
  legacy Presentation character/location art не проходят и generic fallback.
- `Projects/novels-tzm/Assets/tzm.asset`: текущая разметка пересчитана с 14
  чанков / 547 GUID до 12 чанков / 416 GUID, удалены ровно 131 legacy
  Presentation character roots.
- `ContentAuthoringGuide.md`: `Presentation` закреплён как UI/prefab область,
  а физическое удаление legacy-папок отложено до проверки свежего bundle.

Validation:
- Unity 6000.3.11f1 Roslyn `Novels.ContentSdk.Editor`: успешно.
- Детерминированный повторный расчёт: SHA-256 `tzm.asset`
  `2e15c96abc33245f824f7e8184e6b7565db55862f50adc2a33d5a8fd0cb5e71b`.
- 51/51 MP4, 56 unused video posters и 9 статических locations сохранены;
  duplicate GUID, unused overlap и media-only chunks — 0.
- Presentation audit: direct legacy roots 131 → 0; `novels-content doctor` и
  scoped `git diff --check` успешны.

Pending / risks:
- Полный Unity `validate tzm`, bundle build и Inspector smoke не запускались:
  открытый пользовательский Unity PID 97689 владеет
  `Projects/novels-tzm/Temp/UnityLockfile`; второй Unity не стартовал.
- 713 legacy PNG и их `.meta` намеренно не удалялись.

Suggested next step:
- После refresh открытого TZM Editor последовательно выполнить `validate tzm`,
  `build tzm editor` и проверить bundle audit; только затем отдельно решать об
  удалении legacy Presentation art.

## 2026-08-27T16:50:38Z — story-preview-integration-commit — integrated

Task: Проверить и разложить накопленное story-preview дерево на логические
Git-коммиты без LFS.

Changed:
- `3c1a2e7d`: runtime/Game contracts и обновлённый ZDM definition.
- `a81f79fb`: единый Content SDK Editor authoring/streaming pipeline.
- `05dc9494`: плоская TZM-структура; Git распознал 2955 перемещений контента.
- `62bcf9e3`: authoring-документация и завершённые coordination-записи.
- Финальная status/handoff-запись фиксируется отдельным coordination-коммитом.

Validation:
- Из 3040 исходных untracked файлов 2953 переиспользовали существующие blobs
  на 1 506 031 045 байт; реально новых untracked blobs было 351 420 байт,
  максимальный файл — 18 354 220 байт. Git LFS не использован и не требуется.
- Unity Roslyn `Novels.ContentSdk.Editor`, `novels-content doctor`, scoped
  layout checks и `git diff bfee19aa..HEAD --check`: успешно.
- TZM: 12 чанков, 416 GUID, 51/51 video, 56 unused posters; SHA-256
  `tzm.asset` после whitespace-нормализации —
  `95032a7a122c6bf2c0159e76ae86f51ab96e76c0d992ee448297ff73ce3e4882`.

Pending / risks:
- Полный `validate tzm`, `build tzm editor` и Inspector smoke не запускались:
  пользовательский Unity PID 97689 всё ещё владеет UnityLockfile; второй Unity
  не стартовал.

Suggested next step:
- После refresh/закрытия TZM Editor последовательно выполнить `validate tzm`,
  `build tzm editor` и короткий playback smoke, затем push текущей ветки.

## 2026-08-27T17:03:47Z — tzm-remove-legacy-presentation-art — integrated

Task: Удалить физически уже исключённый legacy character/location art TZM.

Changed:
- Удалены `Assets/Presentation/character/characters` и
  `Assets/Presentation/location/locations` с folder meta: 713 PNG,
  711 938 459 байт; всего 1551 tracked-файл с Unity meta.
- `ContentAuthoringGuide.md`: legacy Presentation roots TZM помечены как
  удалённые и не подлежащие восстановлению.

Validation:
- Baseline и regression `novels-content validate tzm` / `build tzm editor`:
  успешно.
- TZM layout неизменен: 12 чанков, 416 GUID, 51/51 video и 56 unused posters;
  SHA-256 `tzm.asset` остался `95032a7a…e3e4882`.
- После удаления все 10 Presentation images являются сохранёнными prefab
  dependencies; direct/unreachable legacy и внешние ссылки на 838 удалённых
  GUID отсутствуют.
- Composed TZM tree уменьшился с 704828 до 648100 KiB.

Pending / risks:
- Визуальный playback smoke через Game не выполнялся; content validation и
  bundle build прошли полностью.

Suggested next step:
- При необходимости выполнить короткий playback smoke начала TZM через Game;
  сами legacy-каталоги больше не восстанавливать.
