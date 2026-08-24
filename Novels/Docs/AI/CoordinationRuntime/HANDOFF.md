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
