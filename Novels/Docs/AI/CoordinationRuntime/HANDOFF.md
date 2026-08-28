# Cross-chat handoff log

Перед работой прочитайте этот журнал полностью и проверьте относящиеся к задаче
утверждения по текущим файлам и `git status`. Перед освобождением write-lock
добавьте новую запись в конец файла по формату из
`../ParallelRefactoringCoordination.md`.

## 2026-08-26T17:30:00Z — ink-tools-window-merge — completed

Task: Объединить отдельные Ink compiler и asset-order окна.

Changed:
- Единственное окно `Novels/Content/Ink Tools` принимает root `.ink` через
  drag-and-drop.
- В нём находятся `Скомпилировать` (JSON + source map) и `Рассчитать ассеты`
  (по соседнему compiled JSON), а также прежняя таблица/фильтр.
- Отдельный `StoryInkCompilerWindow` и его menu entries удалены.
- Commit: `bfee19aa`.

Validation:
- Unity Editor restart: `Tundra build success`, 4 items updated, новых C#
  errors нет; Editor PID 97689.
- `git diff --check`: успешно.
- Удалённые пользователем TZM generated JSON/map и legacy JSON не
  восстанавливались и в commit не включались.

Pending / risks:
- Пользователь должен перетащить `tzm.ink`, нажать `Скомпилировать`, затем
  `Рассчитать ассеты`; это заново создаст отсутствующие generated artifacts.

Suggested next step:
- Выполнить первый compile из единого окна и проверить result box/Console.

## 2026-08-26T17:05:00Z — ink-compiler-drag-drop — completed

Task: Упростить выбор корневого Ink в отдельном compiler window до явного
drag-and-drop.

Changed:
- Удалены filesystem scan, INCLUDE/sibling эвристики, popup и refresh action.
- Добавлено одно сериализуемое `DefaultAsset`-поле; принимается только реальный
  project asset с расширением `.ink`.
- Компиляция и безопасная парная запись JSON/source map не менялись.
- Commit: `a665877a`.

Validation:
- Unity 6000.3.11f1 content build exit code 0, новых C# errors нет; TZM release
  скомпонован.
- Экспериментальный Editor открыт, PID 97120.
- `git diff --check`: успешно; пользовательские ProjectSettings не включены.

Pending / risks:
- Нужен ручной drag `tzm.ink` и первый клик `Скомпилировать`.

Suggested next step:
- В `Novels/Content/Ink Compiler` перетащить `tzm.ink`, нажать кнопку и
  проверить созданные пути в result box.

## 2026-08-26T16:40:00Z — ink-compiler-window — completed

Task: Добавить отдельное окно компиляции корневого Ink в compiled JSON и
source map рядом с исходником.

Changed:
- Добавлен `StoryInkCompilerWindow`: `Novels/Content/Ink Compiler` и context
  menu `Assets/Novels/Open Ink Compiler`.
- Корневые Ink определяются по `INCLUDE` или существующему sibling `.ink.json`;
  при нескольких кандидатах отображается popup.
- Одна кнопка компилирует официальный Ink Compiler один раз и создаёт
  `<root>.json` плюс `<root>.json.source-map.json`.
- Оба temporary-файла готовятся до обновления; при ошибке прежние артефакты
  восстанавливаются из backup.
- Commit: `920bb263`.

Validation:
- TZM discovery возвращает ровно `tzm.ink`.
- Unity 6000.3.11f1 content build exit code 0, новых C# errors нет; release
  скомпонован.
- Экспериментальный Editor открыт, PID 96705.
- `git diff --check`: успешно; пользовательские ProjectSettings не включены.

Pending / risks:
- Нужен первый ручной клик `Скомпилировать` и смысловое сравнение новых
  артефактов с загруженными пользователем перед дальнейшей перепаковкой.

Suggested next step:
- Открыть `Novels/Content/Ink Compiler`, убедиться, что выбран `tzm.ink`, и
  нажать `Скомпилировать`; затем проверить Console/result box.

## 2026-08-26T16:10:00Z — story-source-map-builder — completed

Task: Вернуть генерацию Ink source map и добавить действие в asset-order
EditorWindow.

Changed:
- В `NovelsContentSdk.Editor` восстановлен `StorySourceMapBuilder`: sibling
  root `.ink` компилируется Ink Compiler с INCLUDE handler, runtime containers
  обходятся по `debugMetadata`, затем map атомарно заменяется через `.tmp`.
- В `Story Asset Order` добавлена кнопка `Рассчитать source map`, notification
  с количеством записей и Console log с путём результата.
- Editor assembly явно ссылается на Ink Libraries и StoryProcessor.
- Commit: `50863b27`.

Validation:
- Первая batch-компиляция выявила и зафиксировала конфликт `Path` типов.
- Финальная Unity 6000.3.11f1 content build завершилась с exit code 0, новых C#
  errors нет; TZM release скомпонован.
- Экспериментальный TZM Editor снова открыт, PID 95335.
- `git diff --check`: успешно; пользовательские ProjectSettings не включены.

Pending / risks:
- Нужен ручной клик кнопки для сравнения вновь рассчитанной карты с текущей;
  при ошибке старая карта сохраняется.

Suggested next step:
- Открыть `Story Asset Order`, нажать `Рассчитать source map` и проверить
  notification/Console, затем использовать карту для человекочитаемой строки
  вместо offset `Первое`.

## 2026-08-26T15:35:00Z — story-asset-order-compiled-ink — completed

Task: Переключить asset-order window с authoring `.ink` на compiled
`*.ink.json`.

Changed:
- Автопоиск окна использует маску `*.ink.json`.
- Legacy `tzm.json` и `*.source-map.json` в selector не попадают.
- UI явно называет выбранный файл `Compiled Ink`.
- Commit: `2c3d2107`.

Validation:
- В TZM маска возвращает ровно `tzm.ink.json`; source map найден отдельно и
  исключён из выбора.
- `git diff --check`: успешно; пользовательские ProjectSettings не включены.

Pending / risks:
- Открытый Editor не обновил внешний local-package автоматически; нужен
  `Assets/Refresh` или перезапуск перед визуальной проверкой.

Suggested next step:
- Обновить Assets, открыть окно и нажать `Рассчитать` для `tzm.ink.json`.

## 2026-08-26T15:15:00Z — story-asset-order-window-ux — completed

Task: Убрать зависимость asset-order window от Project selection и запускать
расчёт только явной кнопкой.

Changed:
- Окно само ищет все `.ink` под `Assets`; один выбирается автоматически,
  несколько доступны через popup.
- Расчёт запускается только кнопкой `Рассчитать`; список можно пересканировать
  кнопкой `Обновить`.
- Окно доступно через `Novels/Content/Story Asset Order` и Project context menu
  `Assets/Novels/Open Story Asset Order`.
- Report анализирует именно выбранный Ink-файл, без объединения остальных.
- Commit: `aa148123`.

Validation:
- В TZM автоматически обнаруживаются 8 Ink-файлов.
- Unity 6000.3.11f1: content build exit code 0, новых C# errors нет.
- `git diff --check`: успешно; ProjectSettings пользователя не включён.

Pending / risks:
- Нужна ручная визуальная проверка popup и таблицы в открытом Editor.

Suggested next step:
- Открыть окно, выбрать `s01e01.ink`, нажать `Рассчитать` и проверить первые
  результаты перед подключением отчёта к chunk planner.

## 2026-08-26T14:55:00Z — story-asset-order-window — completed

Task: Добавить отдельное Unity-окно линейного порядка первого использования
ассетов без моделирования развилок.

Changed:
- `ExperimentalStreamingPlan` предоставляет единый first-use report для art,
  video и audio, используя тот же линейный Ink-анализ, что streaming planner.
- Добавлен `StoryAssetOrderWindow`, доступный по правому клику на Ink-файле или
  папке истории: `Assets/Novels/Show Linear Asset Order`.
- Окно показывает порядок, тип, позицию первого использования, source size,
  путь, поиск и отдельно помечает `Not found`.
- Commit: `2726e973`.

Validation:
- Unity 6000.3.11f1: TZM Editor content build exit code 0, новых C# errors нет.
- TZM release успешно скомпонован; `git diff --check` успешно.
- Пользовательский ProjectSettings не включён.

Pending / risks:
- Позиция первого использования сейчас является детерминированным offset в
  линейно склеенном Ink; ветки намеренно не моделируются.
- Нужна ручная визуальная проверка окна в открытом TZM Editor.

Suggested next step:
- Открыть окно на `noveltexts/tzm`, проверить первые строки и затем использовать
  этот report как вход для новой стратегии нарезки чанков.

## 2026-08-26T14:30:00Z — runtime-ink-whitelist — completed

Task: Упростить novel text release filter до явного whitelist.

Changed:
- В `noveltexts/` публикуются только `.ink.json` и `.source-map.json`.
- Удалены проверки типа authoring-файла, поиск sibling и чтение/сравнение JSON.
- Commit: `416bbde3`.

Validation:
- TZM Editor streaming release успешно пересобран.
- Manifest содержит ровно `tzm.ink.json` и
  `tzm.ink.json.source-map.json`; исходники `.ink` и `tzm.json` сохранены в
  authoring-проекте, но не опубликованы.
- `git diff --check`: успешно; пользовательский ProjectSettings не включён.

Pending / risks:
- Whitelist является строгим контрактом: новый runtime-файл другого типа в
  `noveltexts/` потребуется явно добавить в pipeline.

Suggested next step:
- Продолжить оптимизацию размера art chunks по фактическому manifest и
  временной близости использования assets.

## 2026-08-26T14:10:00Z — runtime-ink-payload-filter — completed

Task: Не публиковать authoring Ink и подтверждённый legacy JSON-дубликат в
runtime release, сохранив compiled Ink и source map для аналитики.

Changed:
- `ContentPipeline` исключает `.ink` только внутри `noveltexts/`.
- Обычный `<story>.json` исключается лишь при наличии идентичного
  `<story>.ink.json`; compiled JSON и `.source-map.json` всегда сохраняются.
- Commit: `68351108`.

Validation:
- TZM Editor streaming build завершился успешно, release `11bd2918...`.
- В release под `noveltexts/` ровно `tzm.ink.json` и
  `tzm.ink.json.source-map.json`; source map присутствует.
- Все восемь исходных `.ink` остаются в authoring-проекте.
- Text payload уменьшен с 2,542,580 до 1,579,177 bytes: −963,403 bytes.
- `git diff --check`: успешно; пользовательский ProjectSettings не включён.

Pending / risks:
- Нет. Правило намеренно консервативно сохраняет любой plain JSON, если его
  содержимое отличается от sibling `.ink.json`.

Suggested next step:
- При следующей сборке других историй проверить их manifest; общий pipeline
  применит то же правило без project-specific hardcode.

## 2026-08-26T13:10:00Z — player-build-publish — completed

Task: Зафиксировать и отправить в main систему автоматических Player-сборок.

Changed:
- Commit `f849ff22` содержит только функциональные файлы Remote/Embedded
  матрицы, build identity, встроенного content source и Windows content target.
- Тяжёлые артефакты `Build/` и чужие streaming-изменения не включались.

Validation:
- `git diff --cached --check`: успешно до commit.
- `origin/main` подтверждён на `f849ff229eb1e56b89af2bac9e4fd12cbb927e83`.

Pending / risks:
- В рабочем дереве остаются независимые координационные изменения других задач.

Suggested next step:
- Продолжать работу от `origin/main` / `f849ff22`.

## 2026-08-26T11:49:00Z — macos-embedded-build — completed

Task: Собрать macOS Player со встроенными бандлами всех историй.

Changed:
- Пересобраны LocalContent Catalog, TZM и ZDM для Mac bundle key.
- Собран versioned universal macOS Embedded Player.
- `PlayerBuildAutomation.cs`: автоматический build number теперь также
  записывается в `PlayerSettings.macOS.buildNumber` и восстанавливается после
  сборки.

Validation:
- Unity build: exit code 0, `Embedded Player build completed`, 2100.4 MiB.
- Mach-O: universal `arm64` + `x86_64`.
- Info.plist: version `2026.08.26`, build `3498463`.
- В StreamingAssets найдены Catalog, TZM и ZDM Mac releases.
- `git diff --check`: успешно; `.gitignore` не менялся.

Pending / risks:
- Приложение подписано ad-hoc, не notarized; ручной runtime smoke не выполнялся.

Suggested next step:
- Запустить `.app`, выбрать обе истории и проверить первый экран каждой.

## 2026-08-26T11:35:30Z — player-build-matrix — completed

Task: Автоматизировать версию и дать Android, iOS, Windows и macOS одинаковые
Remote/Embedded варианты Player-сборки, не меняя `.gitignore`.

Changed:
- `PlayerBuildAutomation.cs`: build identity применяется к PlayerSettings на
  время сборки и восстанавливается после неё; Embedded принимает release/dev.
- `build-player.sh`: единая реализация четырёх платформ и двух режимов.
- `build-player-matrix.sh`: сборка всех восьми артефактов под одной версией.
- Старые Remote/Embedded скрипты оставлены как совместимые оболочки.

Validation:
- `zsh -n` и `git diff --check`: успешно.
- Unity 6000.3.11f1 batch compile: `Tundra build success`, exit code 0.
- `.gitignore` не менялся; артефакты остаются намеренно игнорируемыми.

Pending / risks:
- На текущей установке найден только MacStandaloneSupport. Для полной матрицы
  Unity Hub должен установить Android, iOS и Windows Build Support.

Suggested next step:
- После установки модулей запустить `Novels/Tools/build-player-matrix.sh`.

## 2026-08-26T11:10:43Z — story-streaming-chunks — yielded parallel media fix

Task: Устранить трёхсекундное ожидание бабла на фоне и безопасно загружать
ближайшие art/media параллельно.

Changed:
- `ContentDeliveryCoordinator`: одинаковые release/group используют одну
  preserved download-operation с несколькими progress subscribers; каждый
  потребитель сохраняет независимый storage lease. Общий SemaphoreSlim
  ограничивает все группы runtime-настройкой MaximumParallelDownloads.
- `StoryStreamingController`: art chunk и соответствующая media group одного
  шага готовятся параллельно. Commit `217d44d8`.
- `BackgroundPresentationController`: обычное looping video разрешается,
  подготавливается и crossfade-ится после возврата из background operation;
  статический poster и следующий bubble больше не ждут видео. Cut-scenes
  сохраняют блокирующую семантику. Commit `cc569ae2`.

Validation:
- Исходный Editor.log подтвердил `Already continuation registered` при
  одновременном predictive/on-demand запросе media-1.
- Unity 6000.3.11f1: финальные две компиляции `Tundra build success`, новых C#
  errors нет.
- `git diff --check`: успешно; ProjectSettings пользователя не включён.

Pending / risks:
- Нужен повторный Cold App replay до `s01e01.ink:87` при 5 Mbit/s: бабл должен
  появиться сразу на poster, video — плавно позже; предупреждение double-await
  не должно повториться.

Suggested next step:
- В открытом Editor нажать Cold App и повторить сцену номера в отеле, затем
  проверить Console на отсутствие `Predictive story streaming stopped`.

## 2026-08-26T10:31:51Z — story-streaming-chunks — yielded download-all controls

Task: Добавить единое действие `Скачать всю историю` в demand-wait overlay и
экран выбора эпизодов.

Changed:
- `Packages/Bundles/Entity.cs`: чтение размера delivery group для общего
  byte-progress.
- Catalog runtime: `CatalogAction`, optional secondary action в controller и
  runtime-кнопка в `CatalogScreen`. Commit `18374908`.
- Game runtime: `StoryStreamingController` агрегирует art+video+audio groups,
  публикует состояния `Скачать` / `Загрузка N%` / `История загружена` /
  `Продолжить загрузку`; тот же action подключён к выбору эпизодов и fallback
  download screen. Commit `4de440c9`.

Validation:
- Unity 6000.3.11f1: после исправления nullable method group получен
  `Tundra build success`, 113 items updated, новых C# errors нет.
- `git diff --check`: успешно; ProjectSettings пользователя не включён.
- Bundled prefabs не менялись: обе кнопки создаются runtime, content rebuild не
  требуется.

Pending / risks:
- Нужен визуальный Cold App smoke обоих размещений. Предиктивная очередь как и
  раньше стартует автоматически; явное нажатие включает общий пользовательский
  прогресс и retry после временной остановки.

Suggested next step:
- Cold App → TZM: проверить кнопку выбора эпизода; затем на demand miss проверить
  ту же кнопку, общий процент и финальное `История загружена`.

## 2026-08-26T10:07:00Z — story-streaming-chunks — yielded smooth backdrop

Task: Убрать крупноблочную пикселизацию размытого фона demand-wait overlay.

Changed:
- `StoryDownloadOverlay.cs`: snapshot повышен с 1/12 до 1/4 разрешения экрана
  и уменьшается двумя bilinear-ступенями через временный half-resolution RT.
- Commit: `a3e2039e`.

Validation:
- Unity 6000.3.11f1: `Tundra build success`, новых C# errors нет.
- `git diff --check`: успешно; ProjectSettings пользователя не включён.

Pending / risks:
- Нужен визуальный replay следующего demand wait; transient RT освобождается в
  `finally`.

Suggested next step:
- Повторить Cold App или дождаться следующего отсутствующего чанка и проверить,
  что фон остаётся мягким без крупных квадратов.

## 2026-08-26T10:03:00Z — story-streaming-chunks — yielded UI/video fixes

Task: Исправить наложение строк fallback download screen и искажение
пропорций фонового видео.

Changed:
- `StoryDownloadFallbackPrefabBuilder.cs` и fallback `screen.prefab`: title,
  progress, details и remaining получили непересекающиеся вертикальные rect.
  Commit `37947469`.
- `LocationScreen.cs`: RawImage использует centered aspect-fill UV crop и
  пересчитывается при назначении texture/изменении rect. Commit `22fb5877`.

Validation:
- Unity 6000.3.11f1: `Tundra build success`, 9 items updated, новых C# errors
  нет.
- PrefabImporter успешно импортировал fallback prefab.
- `git diff --check`: успешно; пользовательский ProjectSettings не включён.

Pending / risks:
- Нужен ручной визуальный replay demand wait и видео Santorini; aspect-fill
  сохраняет геометрию ценой симметричного crop по длинной оси.

Suggested next step:
- Повторить Cold App, проверить читаемые строки окна; затем дойти до видео и
  подтвердить отсутствие сплющивания.

## 2026-08-26T09:49:00Z — story-streaming-chunks — yielded rebuilt release

Task: Пересобрать TZM после bootstrap dependency closure и подготовить ручную
проверку штатного Loading screen.

Changed:
- Experimental TZM Editor output и composed Game LocalContent пересобраны.
- Release ID: `22a45a544f9a9ae4429a6a4f7c4d532712fe5798e80fe44e43c49cc3d153c594`.

Validation:
- `NOVELS_STREAMING_EXPERIMENT=1 novels-content build tzm editor`: успешно.
- `loading/screen-variant.prefab`, `loading/background.png` и
  `loading/header.png` находятся только в chunk-0.
- 82 art chunks, 51 media groups; chunk-0 — 7,621,847 bytes / 73 explicit
  assets; полный art bundle payload — 244,239,004 bytes.
- Game Editor PID 63847 открыт с 5 Mbit/s, latency 120 ms, jitter 30 ms.
- Рабочее дерево содержит только прежний пользовательский
  `Novels/ProjectSettings/ProjectSettings.asset`.

Pending / risks:
- Нужен ручной Cold App smoke: каталог → TZM → New Game; проверить штатный
  Loading screen и затем первое fallback demand-wait окно.

Suggested next step:
- Нажать Cold App и пройти старт истории; прислать screenshot, если Loading
  screen снова отображается некорректно.

## 2026-08-26T09:40:30Z — story-streaming-chunks — yielded planner fix

Task: Исправить белый штатный Loading screen, чьи PNG-зависимости были
разнесены с prefab по разным streaming chunks.

Changed:
- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`: bootstrap
  теперь включает dependency closure bootstrap-ассетов, отфильтрованный по
  ассетам текущей истории; обязательные sprites стартовых UI-prefab попадут с
  ними в chunk-0.
- Commit: `246c02c8`.

Validation:
- Текущий release подтвердил причину: `loading/screen-variant.prefab` находился
  в chunk-0, а `loading/background.png` и `loading/header.png` — в chunk-51.
- Unity 6000.3.11f1: `Tundra build success`, новых C# errors нет.
- `git diff --check`: успешно; пользовательский
  `Novels/ProjectSettings/ProjectSettings.asset` не включён.

Pending / risks:
- TZM ещё не пересобран: основной Game Editor открыт пользователем, второй
  Unity запускать нельзя.

Suggested next step:
- После закрытия Editor снова получить FIFO/write-lock, выполнить TZM Editor
  streaming build и проверить по release.json, что loading prefab/background/
  header находятся в chunk-0; затем повторить Cold App smoke.

## 2026-08-26T09:35:00Z — story-streaming-chunks — yielded fallback prefab

Task: Заменить нестабильное OnGUI-окно demand wait на штатный fallback uGUI
prefab.

Changed:
- `StoryDownloadOverlay.cs`: оставлен controller прогресса, ETA и размытого
  snapshot; OnGUI и runtime color textures удалены.
- `StoryDownloadScreen.cs`: отдельный uGUI view для CanvasGroup, RawImage,
  progress fill и двух строк состояния.
- `Resources/Fallbacks/StoryDownload/screen.prefab`: fallback Canvas с
  затемнением, размытой подложкой, тёмной панелью, progress bar и текстом.
- `StoryDownloadFallbackPrefabBuilder.cs`: детерминированная пересборка prefab
  через меню `Novels/Rebuild Story Download Fallback` и создание при отсутствии.
- Временный Setting/Canvas probe и не подтвердившийся font workaround удалены.
- Commit: `3be8eb6e`.

Validation:
- Unity 6000.3.11f1: `Tundra build success`, новых C# errors нет.
- PrefabImporter успешно импортировал `screen.prefab`; все пять serialized
  view references ненулевые.
- `git diff HEAD^ --check`: успешно.
- Пользовательский `Novels/ProjectSettings/ProjectSettings.asset` не включён.

Pending / risks:
- Нужен ручной Cold App visual smoke на первом реальном art demand miss.

Suggested next step:
- Cold App → TZM → пройти до недостающего art chunk; ожидается тёмное
  масштабируемое окно поверх размытого текущего кадра с progress и ETA.

## 2026-08-26T09:12:00Z — story-streaming-chunks — yielded UI probe

Task: Различить Setting screen, Loading screen и demand overlay для белой
плашки без текста.

Changed:
- `Packages/Setting/View/Screen.cs`, `Packages/Setting/Entity.cs`: временный
  snapshot active Graphic, rect, alpha, material и shader.
- `Novels/Assets/Novels/StorySourceOverlay.cs`: HUD показывает Setting snapshot
  и активные Canvas/sorting order/CanvasGroup alpha.
- Не подтвердившаяся подмена шрифта из `dead3aa8` удалена.
- Commit: `cc0e849c`.

Validation:
- Первая версия probe поймала и затем исправила compile boundary между
  `Novels` и `Setting.View`; вызов перенесён через `Setting.Entity`.
- Scoped `git diff --check`: успешно.

Pending / risks:
- Unity не выполнил второй Refresh после исправления source; Editor.log всё ещё
  содержит устаревшую ошибку первой версии probe. Нужен ручной Assets/Refresh
  либо перезапуск Editor/Play Mode, затем новый screenshot HUD.

Suggested next step:
- После Refresh прислать HUD со строками `Setting` и `Canvas`; по ним определить
  конкретный Graphic/Canvas и заменить probe финальной правкой.

## 2026-08-26T09:00:00Z — story-streaming-chunks — yielded

Task: Исправить белый прямоугольник без текста на старте TZM streaming smoke.

Changed:
- `Packages/Setting/View/Screen.cs`: legacy UI стартового экрана теперь
  использует встроенный `LegacyRuntime.ttf`, а не десериализованный из story
  AssetBundle шрифт; исправление зафиксировано commit `dead3aa8`.

Validation:
- Сопоставление HUD `s01e01.ink:30` и Ink: кадр сделан до выбора `Играть`, то
  есть видимый прямоугольник принадлежит Setting screen, не demand overlay.
- Состав release: setting prefab и Liberation Sans находятся в `chunk-0`.
- `git diff --check -- Packages/Setting/View/Screen.cs`: успешно.

Pending / risks:
- Открытый Unity Editor не выполнил refresh после изменения (Editor.log не
  обновлялся); компиляция и визуальный replay остаются за следующим запуском.

Suggested next step:
- Перезапустить Play Mode после компиляции и проверить наличие заголовка и
  кнопки `Новая игра`, затем пройти до реального demand wait.

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

## 2026-08-27T17:14:07Z — tzm-shared-presentation-font — integrated

Task: Объединить три идентичные копии Liberation Sans Regular TZM в один общий
font asset.

Changed:
- Canonical TTF с GUID `0125029842d6d993020c25af5bf725f6` перенесён из
  `Presentation/setting` в `Presentation/Fonts`; две копии из bubble и
  notification удалены.
- Все 14 ссылок трёх Presentation-prefab используют canonical GUID.
- `tzm.asset`: два удалённых font GUID исключены из первого chunk; assignments
  416 → 414, число chunks осталось 12.
- `ContentAuthoringGuide.md`: общие шрифты закреплены за `Presentation/Fonts`.

Validation:
- Static serialization audit: один font-файл, 14 canonical references, старые
  GUID отсутствуют.
- `novels-content validate tzm`, `build tzm editor`, `doctor`: успешно.
- Bundle audit: 555 roots / 183902.2 KiB, одна запись Liberation Sans; release
  bundle 188 315 864 байт, на 312 218 байт меньше baseline.
- 12 chunks, 414 GUID, 51/51 video и 56 unused posters сохранены.

Pending / risks:
- Визуальный UI smoke через Game не выполнялся; prefab references, Unity import
  и content bundle проверены.

Suggested next step:
- При следующем playback проверить bubble, notification и settings text; новых
  authoring-шагов не требуется.

## 2026-08-27T17:46:23Z — tzm-exclude-unused-bundle — ready-for-integration

Task: Исключить authoring-группу `Не используется` из обычного story bundle
без удаления или дополнительного сжатия арта.

Changed:
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`: story build вычитает
  `StoryChunkAuthoring.UnusedPaths` из bundle roots и проверяет точное число
  исключений.
- `Novels/Docs/AI/ContentAuthoringGuide.md`: зафиксировано, что группа не
  публикуется среди roots обычного story bundle.
- Собственный status-файл содержит scope, результат и ограничения проверки.

Validation:
- Static TZM audit: roots 555 → 499; 56/56 unused GUID разрешены; missing и
  overlap — 0; serialized dependencies на них вне `tzm.asset` — 0.
- Unity Bee/Roslyn: `Novels.ContentSdk.Editor.dll` скомпилирован успешно.
- `novels-content doctor` и `git diff --check`: успешно.
- Полный `validate tzm` остановлен после повторяющегося Licensing handshake
  `Unsupported protocol version '1.18.0'`; `build tzm editor` не запускался.

Pending / risks:
- После восстановления Unity Licensing Client нужно выполнить последовательные
  `validate tzm` и `build tzm editor`, подтвердить 499 roots и измерить
  фактический bundle size.
- Арт, import settings, `tzm.asset` и ZDM не изменялись.

Suggested next step:
- Завершить отложенный TZM build, затем отдельной безопасной партией
  канонизировать exact duplicates Choices и очевидные опечатки/синонимы
  персонажей; неоднозначные character/layer duplicates оставить на ручную
  проверку.

## 2026-08-28T06:53:16Z — tzm-art-aliases — ready-for-integration

Task: Добавить story-level Art Aliases и канонизировать только безопасные
exact-byte дубликаты TZM без изменения Ink и без неоднозначного пункта 7.

Changed:
- `NovelContentAsset` / `NovelDefinition`: добавлены 17 story-relative Art
  Aliases, нормализация, duplicate/self/cycle checks и runtime resolver.
- `ContentAddresses`, `NovelRuntime` и Character runtime: Choice, Location и
  Character sprite разрешают alias до фактической загрузки.
- Editor pipeline/validation: bundle roots оставляют только canonical target;
  отсутствующий или unused конечный target останавливает validation.
- `Projects/novels-tzm/Assets/tzm.asset`: добавлены aliases, chunk assignments
  переведены 414 → 408.
- Удалено 17 exact-byte alias-source PNG и 17 `.meta` (3 Choices, 14
  Characters); source PNG уменьшены на 2 772 637 байт.
- Character trim manifest: после доказанного совпадения geometry/hash удалены
  14 alias-записей, 435 → 421.
- Authoring guide документирует формат `story/...`, отсутствие физического
  alias-source и требования к character trim metadata.

Validation:
- Static TZM audit: 17 уникальных aliases, все конечные targets существуют,
  все sources удалены; 408 уникальных chunk GUID и 56 unused GUID разрешаются.
- SHA-256 snapshot всех 20 файлов `Assets/Ink`: идентичен до и после работы.
- Неоднозначные different-character, `front/back`, `main/emotion` и
  `main/view` exact pairs физически сохранены.
- `Novels.ContentAddressing.csproj` и `Novels.Content.csproj`: build succeeded,
  0 warnings / 0 errors.
- `novels-content doctor` и `git diff --check`: успешно.

Pending / risks:
- Unity, `validate tzm` и `build tzm editor` не запускались по прямому
  ограничению пользователя после падения Licensing Client.
- Restore generated Character/Editor projects зависал и был отменён; требуется
  штатная Unity compile/validation после восстановления лицензии.
- Static expectation: 538 source roots, 482 published roots после вычитания 56
  unused; подтвердить фактической сборкой.

Suggested next step:
- После восстановления Unity последовательно выполнить refresh/compile,
  `validate tzm` и `build tzm editor`; пункт 7 оставлять нетронутым до отдельного
  решения пользователя.

## 2026-08-28T07:26:54Z — tzm-hair-bundle — ready-for-integration

Task: Исправить расчёт и состав hair assets TZM; для exact front/back пар
оставить только front.

Changed:
- `ExperimentalStreamingPlan.cs`: дополнительные Ink gather-ветки гардероба
  входят в usage report, а дефолтные character clothes/hair/accessory получают
  первое взрослое появление персонажа.
- `Projects/novels-tzm/Assets/tzm.asset`: `Ободок`, `Бант`, `Мальвинка` и
  defaults назначены ранним чанкам; 19 неиспользуемых hair PNG перенесены в
  `Не используется`; итог 12 чанков / 410 GUID и 75 unused GUID.
- `Projects/novels-tzm/Assets/Characters/**`: удалены три exact-byte
  maincharacter hair back PNG/meta, front сохранены; trim manifest 421 → 418.
- `ContentAuthoringGuide.md`: описаны ветки/defaults и правило не создавать
  alias между совпадающими front/back слоями.

Validation:
- Три пары: одинаковые SHA-256, размеры, importer settings, crop и trim hash.
- Static content audit: 35 hair PNG = 16 chunk + 19 unused, unassigned 0;
  missing/duplicate/unused overlap 0; source/published roots 535/460.
- Unity Roslyn изменённого planner и `Novels.Content.csproj`: успешно.
- `novels-content doctor`, SHA-256 всех 20 Ink-файлов и `git diff --check`:
  успешно; Ink побайтово не изменён.

Pending / risks:
- Unity/validate/build не запускались по прямому ограничению после сбоя
  Licensing Client; фактический новый bundle size не измерен.
- Статическая оценка опубликованных волос: BC3 6,87 MiB, Android ASTC 6×6
  3,06 MiB, iOS ASTC 8×8 1,72 MiB; экономия против прежних 38 roots — примерно
  9,81 / 4,37 / 2,47 MiB до bundle-level compression.

Suggested next step:
- После восстановления лицензии выполнить Unity refresh, `validate tzm` и
  `build tzm editor`; проверить начало s01e01 и первое появление Алексы.

## 2026-08-28T08:30:21Z — tzm-semantic-art-aliases — ready-for-integration

Task: Канонизировать оставшиеся шесть exact-byte Character duplicates TZM
через aliases и убрать повторную отрисовку совпадающих main/emotion.

Changed:
- `CharacterSpriteSetLoader.cs`: сохраняет разрешённые адреса main/emotion,
  проверяет fallback на полном наборе и не возвращает второй emotion-слой,
  когда адрес совпадает с main.
- `Projects/novels-tzm/Assets/tzm.asset`: добавлены шесть aliases; canonical
  GUID перенесены в ранние чанки, assignments 410 → 406.
- `Projects/novels-tzm/Assets/Characters/**`: удалены шесть exact-byte
  alias-source PNG/meta (8 925 949 байт), пустая папка `царь` и её folder
  meta; trim manifest 418 → 412.
- `ContentAuthoringGuide.md`: зафиксировано явное смысловое решение для
  different-character/main-emotion/main-view aliases и runtime de-dup слоя.

Validation:
- Шесть пар: SHA-256, importer settings, original size, crop и trimmed hash
  совпадали до удаления.
- Static aliases/chunks: 23 aliases, targets 23/23, sources 0/23; 12 чанков /
  406 GUID, unused 75; duplicates, missing, overlap, self и cycles — 0.
- Unity Roslyn `Novels.ContentAddressing`, `Novels.Content`,
  `Novels.Character`: успешно; `novels-content doctor` и
  `git diff --check`: успешно.
- Все 20 файлов `Assets/Ink` имеют исходные SHA-256; Ink не изменён.

Pending / risks:
- Unity Editor, `validate tzm`, bundle build и Play Mode не запускались по
  прямому ограничению после сбоя Licensing Client.
- После восстановления лицензии нужно подтвердить 529 source / 454 published
  roots и визуально проверить Царя, Атлана, Фила и подростка Алексу.

Suggested next step:
- Выполнить Unity refresh/compile, `validate tzm`, `build tzm editor`, затем
  короткий playback smoke указанных персонажей.

## 2026-08-28T08:43:43Z — zdm-art-aliases — ready-for-integration

Task: Актуализировать ZDM по схеме exact-art aliases и удалить проверенные
пустые каталоги TZM.

Changed:
- `Projects/novels-zdm/.../definition/zdm.asset`: добавлены 58 Art Aliases,
  ведущих в 45 canonical targets.
- `Projects/novels-zdm/.../story/character/**`: удалены 58 exact-byte
  alias-source PNG/meta (18 256 737 байт), пустые деревья Анпу и Стражников;
  trim manifest 455 → 397.
- `Projects/novels-tzm/Assets/Characters/maincharacter/hairs/back/**`:
  удалены три ранее проверенных пустых каталога, ставший пустым `back` и четыре
  неиспользуемых folder meta.

Validation:
- Все 58 ZDM-пар совпадают по SHA-256, importer settings и trim geometry;
  sources отсутствуют, targets существуют, deleted GUID refs/self/cycles — 0.
- 397 character PNG = 397 trim entries; safe exact duplicates после миграции —
  0. Сохранены 25 exact-byte пар с отличающимся `wrap mode`.
- Пустых каталогов Assets TZM/ZDM нет; Ink snapshots TZM/ZDM не изменились.
- `novels-content doctor` и `git diff --check`: успешно.

Pending / risks:
- Unity, `validate zdm` и bundle build не запускались по ограничению после сбоя
  Licensing Client.

Suggested next step:
- После восстановления лицензии выполнить Unity refresh/compile,
  `validate zdm`, `build zdm editor` и playback smoke Гора/Анпу/Стражников.

## 2026-08-28T09:03:44Z — zdm-wrap-mode-normalization — ready-for-integration

Task: Привести ZDM character texture wrap mode к эталону TZM и завершить
alias-канонизацию оставшихся exact duplicates.

Changed:
- 194 ZDM character PNG importer records с `0/0/0` нормализованы в
  `wrapU/V/W = 1/1/1` (`Clamp`); в итоговом дереве все 448 PNG единообразны.
- `zdm.asset`: добавлены ещё 25 Art Aliases; итог 83 aliases / 69 targets.
- Удалены 25 alias-source PNG/meta ещё на 2 173 892 байта; trim manifest
  397 → 372.
- `ContentAuthoringGuide.md`: эталон TZM Clamp закреплён для story art всех
  content-проектов; Presentation-specific настройки исключены.

Validation:
- В 188 surviving изменённых PNG meta меняются только wrapU/V/W; отличающихся
  от `1/1/1` текущих ZDM PNG нет.
- Exact-byte duplicate groups: 0; 83 alias-пары совпадают по SHA-256,
  нормализованным importer settings и trim geometry.
- 372 character PNG = 372 trim entries; missing targets, sources, deleted GUID
  refs, self/cycles и пустые каталоги — 0.
- Ink snapshots TZM/ZDM неизменны; `novels-content doctor` и
  `git diff --check` успешны.

Pending / risks:
- Unity/reimport, `validate zdm` и bundle build не запускались из-за сбоя
  Licensing Client.

Suggested next step:
- После восстановления лицензии выполнить Unity refresh/reimport,
  `validate zdm`, `build zdm editor` и playback smoke внешности главной
  героини.

## 2026-08-28T09:14:56Z — bundle-size-rebuild — ready-with-limitations

Task: Последовательно пересобрать TZM/ZDM Editor/Mac bundles после текущей
серии content-оптимизаций и измерить фактическую экономию.

Changed:
- Штатные build/composed artifacts TZM и ZDM обновлены командами
  `novels-content build <story> editor`.
- `ContentSizeBaseline.md`: добавлена фактическая Editor/Mac delta bundles и
  полного delivery.
- `ParallelWork.bundle-size-rebuild.md`: сохранены версии, хеши и ограничения
  проверки.

Validation:
- Обе Unity batchmode-команды: exit code 0; content bundle audit прошёл для
  454 TZM и 452 ZDM root assets.
- TZM bundle: 188 315 864 B → 130 621 560 B, −57 694 304 B (−30,637%).
- ZDM bundle: 149 133 366 B → 132 260 907 B, −16 872 459 B (−11,314%).
- Итого bundles: 337 449 230 B → 262 882 467 B, −74 566 763 B
  (−71,112 MiB; −22,097%).
- Project output и composed story output совпадают по размеру и SHA-256;
  `novels-content doctor` и `git diff --check` успешны.
- Ink snapshot, проверенный до и после Unity build, не изменился; Unity-only
  whitespace в 51 TZM video `.meta` возвращён к исходному тексту.

Pending / risks:
- Пересобран только Editor/Mac target; Android и iOS в этой итерации не
  обновлялись.
- Runtime/Play Mode smoke не выполнялся. В Unity log есть неблокирующий
  `Curl error 60`, но batchmode завершился успешно и audit прошёл.

Suggested next step:
- Перед мобильным release отдельно и последовательно пересобрать Android/iOS;
  затем выполнить короткий playback smoke TZM/ZDM.

## 2026-08-28T09:22:35Z — tzm-streaming-throttle-launch — ready-with-limitations

Task: Пересобрать актуальный TZM streaming release и открыть Novels Editor с
симуляцией канала 20 Мбит/с для ручной проверки доставки чанков.

Changed:
- Generated TZM Editor/Mac streaming release пересобран с
  `NOVELS_STREAMING_EXPERIMENT=1` и скомпонован в `Novels/Build/LocalContent`.
- `ParallelWork.tzm-streaming-throttle-launch.md`: зафиксированы актуальный
  состав release и параметры запуска.
- Production source, Ink и authoring assets намеренно не изменялись.

Validation:
- Build exit code 0; все 12 chunk bundle audits успешны.
- Release содержит `chunk-0..11`, 51 media entry в 12 media-группах;
  отдельного preview-бандла в актуальном контракте нет, стартовый — chunk 0.
- Project/composed release совпадают; Ink snapshot до/после идентичен;
  `git diff --check` успешен.
- Unity Editor 6000.3.11f1 открыт на `Novels`; процесс подтвердил
  `NOVELS_SIMULATED_MBITS=20`, latency/jitter 0 и streaming flag 1.
- Import/compile завершены, сцена `Assets/Novels/Novels.unity` загружена,
  C# compilation errors отсутствуют.

Pending / risks:
- Ручной Play Mode сценарий `TZM -> Cold App/Warm` ещё не выполнен.
- Неблокирующие Unity Services TLS/403 warnings остаются в Editor log.

Suggested next step:
- В открытом Editor нажать Play, войти в TZM и сравнить Cold App/Warm по HUD.

## 2026-08-28T09:27:58Z — novels-throttle-5-relaunch — ready-with-limitations

Task: Переоткрыть основной Novels Unity Editor, изменив симуляцию канала с
20 на 5 Мбит/с.

Changed:
- Предыдущий Novels Unity process завершён; новый Editor открыт с тем же
  streaming release и новым process environment.
- `ParallelWork.novels-throttle-5-relaunch.md`: записаны параметры и проверки.
- Content release, production source и Ink не изменялись.

Validation:
- Новый Unity PID 23418 открыл проект `Novels` и сцену
  `Assets/Novels/Novels.unity`.
- Process environment: `NOVELS_SIMULATED_MBITS=5`, latency/jitter 0,
  `NOVELS_STREAMING_EXPERIMENT=1`.
- Initial refresh/compile завершён; C# compilation errors отсутствуют;
  `git diff --check` успешен.

Pending / risks:
- Ручной Play Mode сценарий остаётся пользователю.
- Неблокирующие Unity Services TLS/403 warnings остаются в Editor log.

Suggested next step:
- В открытом Editor нажать Play и повторить TZM Cold App/Warm на 5 Мбит/с.

## 2026-08-28T09:43:48Z — debug-hud-initial-chunk-progress — ready-with-limitations

Task: Восстановить meaningful delivery-данные debug HUD во время стартовой
загрузки chunk-0.

Changed:
- `Novels/Assets/Novels/ContentDeliveryFlow.cs`: начальный progress callback
  теперь обновляет и bootstrap text, и `StreamingExperimentDiagnostics`;
  перед загрузкой выставляются `Preparing` и `chunk-<index>`.
- `ParallelWork.debug-hud-initial-chunk-progress.md`: записаны причина,
  реализация и проверки.
- Content release, Ink, bundles и authoring assets не изменялись.

Validation:
- Unity Editor 6000.3.11f1 initial refresh/compile: C# errors отсутствуют.
- Editor открыт на Novels с 5 Мбит/с, latency/jitter 0 и streaming flag 1.
- `novels-content doctor` и `git diff --check`: успешно; focused source diff
  содержит только `ContentDeliveryFlow.cs`.

Pending / risks:
- Визуальный runtime smoke требует ручного `Play -> TZM -> Cold App` в
  оставленном открытым Editor.
- `dotnet --no-restore` неприменим без Unity-generated NuGet assets; это не
  Unity compile failure.

Suggested next step:
- На стартовой загрузке TZM проверить строку
  `Preparing · tzm-chunk-0 · <percent> · <MiB/s>` и `Queue · chunk-0`.

## 2026-08-28T09:55:18Z — video-solid-color-null-fix — ready-with-limitations

Task: Исправить падение TZM после гардероба при подготовке видео «Причал».

Changed:
- `LocationLayout.cs`: добавлена безопасная очистка sprite без layout-расчёта.
- `LocationScreen.cs`: добавлена явная операция `ClearImage`.
- `BackgroundPresentationController.cs`: `ShowSolidColor` использует
  `ClearImage` вместо запрещённого `SetImage(null)`.
- Ink, story assets, bundles и delivery-разметка не изменялись.

Validation:
- Unity Roslyn по актуальному `Novels.Location.rsp`: успешно.
- В runtime больше нет `SetImage(null)`; scoped `git diff --check`: успешно.
- `dotnet --no-restore` не стартует без Unity-generated `project.assets.json`;
  это ограничение fallback-проверки, а не C# compile failure.

Pending / risks:
- Открытый Editor PID 29694 не выполнил refresh внешнего package после
  падения; программное меню заблокировано macOS Assistive Access.
- Нужен ручной `Exit Play Mode -> refresh/recompile -> TZM -> Гардероб ->
  Причал`; второй Unity не запускался.

Suggested next step:
- Повторить начало TZM в уже открытом Editor; ожидается однотонный переход,
  подготовка `причал.mp4` и продолжение истории без `ArgumentNullException`.

## 2026-08-28T10:07:24Z — video-camera-without-poster-fix — ready-with-limitations

Task: Исправить второе падение на «Причале» при camera action поверх видео без
PNG-постера.

Changed:
- `LocationLayout.cs`: video RenderTexture теперь настраивает геометрию общего
  visual container; camera travel считается по фактической ширине контейнера,
  а доступность visual учитывает Sprite или видео.
- `LocationScreen.cs`: video texture передаётся layout; dialogue alignment
  использует общий visual-контракт.
- Ink, story assets, prefab и bundles не изменялись.

Validation:
- Stack trace и Editor log: видео успешно показано, падение происходило на
  следующей Ink-команде `Камера: слева направо`.
- `причал.mp4` имеет размер 2160×1920; новые расчёты используют размеры его
  RenderTexture.
- Unity Roslyn по `Novels.Location.rsp`: успешно.
- Открытый Unity Editor PID 29694 скомпилировал `Novels.Location.dll` и
  выполнил Domain Reload без новых C#-ошибок; scoped `git diff --check` успешен.

Pending / risks:
- Нужен повторный Play Mode smoke исходного маршрута; управление Play Mode
  остаётся ручным.

Suggested next step:
- В открытом Editor снова пройти `TZM -> Гардероб -> Причал` и проверить
  `слева направо`, затем две команды `справа налево -> слева направо`.

## 2026-08-28T10:34:00Z — tzm-child-character-streaming-fix — yielded

Task: Вернуть маленькую Салли на репликах `Салли (маленькая)` в TZM.

Changed:
- `ExperimentalStreamingPlan.cs`: child body главной героини теперь получает
  first use по первой детской реплике независимо от выбранного до истории view.
- `tzm.asset`: четыре child body GUID для вариантов внешности возвращены во
  второй authoring chunk перед первыми детскими эмоциями.
- Ink не изменялся.

Validation:
- Unity Roslyn по актуальному `Novels.ContentSdk.Editor.rsp`: успешно.
- Все четыре child body GUID присутствуют в `tzm.asset` ровно по одному разу;
  scoped `git diff --check` успешен.

Pending / risks:
- Локальные bundles ещё не пересобраны: Play Mode остановлен, но основной
  Editor не выполняет auto-refresh внешних package/authoring changes; macOS
  также запрещает автоматическое управление меню через Assistive Access.
- Нужно закрыть основной Unity, выполнить последовательный batch build, затем
  снова открыть Editor на 5 Мбит/с и повторить TZM до `s01e01.ink:421`.

Suggested next step:
- Пользователь закрывает Unity; задача снова входит в FIFO, выполняет batch
  build и повторно открывает проект без параллельного второго экземпляра.

## 2026-08-28T10:41:14Z — tzm-child-character-streaming-fix — ready-with-limitations

Task: Завершить сборку исправления маленькой Салли и открыть тестовый Editor.

Changed:
- Штатный `novels-content build tzm editor` успешно пересобрал и скомпоновал
  TZM streaming release `dadf0907cad0ac641810e21c3d8ef5d16ee68ed93accf7508088bf308d82ec19`.
- Четыре child body всех внешностей находятся в `tzm-chunk-1` перед детскими
  эмоциями; project/composed release идентичны.
- Unity-generated trailing whitespace в 51 video `.meta` удалён без изменения
  содержательных importer settings.

Validation:
- 12 bundle audits и batchmode: success, exit code 0.
- Новый `chunk-1`: 11 509 814 B, version
  `00edc30fc881466debcfd65a48c8f424`.
- Основной Unity Editor PID 40245 открыт с 5 Мбит/с, latency/jitter 0 и
  streaming experiment 1; initial refresh/domain reload без C# errors.
- `git diff --check`: успешно.

Pending / risks:
- Финальный визуальный smoke строки `s01e01.ink:421` остаётся пользователю.

Suggested next step:
- В Editor запустить TZM Cold App и убедиться, что на строке 421 показана
  маленькая Салли выбранной внешности, а не взрослый fallback.

## 2026-08-28T10:48:00Z — child-character-adult-layers-fix — yielded

Task: Убрать взрослую одежду с уже корректно загруженной маленькой Салли.

Changed:
- `CharacterSpriteSetLoader.cs`: child presentation теперь немедленно
  возвращает пустые clothes, hair и accessory layers после очистки взрослого
  appearance state; взрослые defaults больше не подставляются.
- Ink, story authoring и content bundles не изменялись.

Validation:
- Unity Roslyn по актуальному `Novels.Character.rsp`: успешно.
- `git diff --check`: успешно.

Pending / risks:
- Открытый Editor находится в Play Mode и использует старый domain; нужен
  ручной Exit/Close, затем повторный запуск на 5 Мбит/с.

Suggested next step:
- Пользователь закрывает Unity; задача повторно открывает основной Editor и
  оставляет финальный визуальный smoke строки 409 пользователю.

## 2026-08-28T11:00:22Z — child-character-adult-layers-fix — ready-with-limitations

Task: Подтвердить применение runtime-исправления без закрытия Editor.

Validation:
- Открытый Unity автоматически обновил `Novels.Character.dll` в 13:48 local,
  выполнил два успешных domain reload и не сообщил C# errors.
- Полный перезапуск и пересборка content bundles не потребовались.

Pending / risks:
- Финальный визуальный smoke детской реплики остаётся пользователю.

Suggested next step:
- Повторить строку 409 в текущем Editor; детская Салли должна отображаться без
  adult clothes/hair/accessory layers.

## 2026-08-28T11:35:54Z — remove-streaming-test-harness — ready-with-limitations

Task: Удалить временную тестовую обвязку streaming, сохранив production
доставку чанков и ранее исправленное поведение контента.

Changed:
- `Novels/Assets/Novels/**`: удалены debug HUD, Cold/Warm restart, поколения
  кэша, source-location callback и диагностические streaming hooks; production
  bootstrap/story download progress сохранены.
- `Packages/Bundles/ThrottledFileSystemContentSource.*`: удалён Editor-only
  эмулятор скорости, latency и jitter.
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`: story streaming стал
  штатным для всех Story-сборок без environment flag; runtime больше не
  публикует source map, который использовался только HUD.
- `ExperimentalStreamingPlan.*` переименован в `StoryStreamingPlan.*` без
  изменения алгоритма chunk usage/layout.
- `LocationScreen.cs`: удалён только глобальный debug snapshot; исправления
  video layout/camera и ClearImage сохранены.
- Ink, authoring assets и content bundles не изменялись этой задачей.

Validation:
- Unity Roslyn: `Bundles`, `Novels.Location`, `Novels` и
  `Novels.ContentSdk.Editor` успешно, C# errors отсутствуют.
- Поиск production C#: ссылок на HUD, diagnostics, throttle/env flags,
  cache-generation и experimental planner name нет.
- `git diff --check`: успешно.
- Unity batch refresh не дошёл до импорта: Licensing Client IPC завершился
  таймаутом; зависший процесс остановлен.

Pending / risks:
- После восстановления Unity Licensing нужен обычный refresh/domain reload.
- Следующая Story content build впервые проверит штатный streaming без flag и
  создаст release без runtime source-map payload.

Suggested next step:
- Открыть Novels обычным способом без специальных env vars, дождаться compile,
  затем при следующем изменении контента выполнить `build tzm editor`.

## 2026-08-28T12:10:00Z — tzm-default-editor-launch — yielded

Task: Пересобрать TZM Editor content и открыть Novels Editor без ограничения
сети.

Changed:
- Production source и Ink не изменялись.
- Запущенная без `NOVELS_*` env vars TZM build выполнила полный initial import,
  успешно скомпилировала scripts и начала bundle compression.
- Batch PID 46007 остановлен после подтверждённого бесконечного licensing
  reconnect; stale `Projects/novels-tzm/Temp/UnityLockfile` удалён.
- Novels Editor PID 51354 открыт обычным запуском без network throttle.

Validation:
- TZM import: 576 actual imports, C# compile errors в логе отсутствуют.
- Unity Package Manager IPC вне sandbox подключается успешно.
- Старый целостный composed release остаётся в
  `Novels/Build/LocalContent/stories/tzm/Remote/Mac/release.json`.

Pending / risks:
- Новая bundle-сборка не завершена: Unity Licensing Client отклоняет handshake
  с `Unsupported protocol version '1.18.0'` и теряет entitlement
  `com.unity.editor.headless`.
- Пользователь исправляет Unity/Hub licensing; до этого повторять build нельзя.

Suggested next step:
- После подтверждения исправленной лицензии заново войти в FIFO, закрыть
  оставшийся Editor при необходимости, повторить `build tzm editor`, затем
  открыть Novels Editor без специальных env vars.

## 2026-08-28T12:13:00Z — tzm-default-editor-launch — completed

Task: После исправления лицензии завершить TZM Editor build и открыть Novels
Editor без ограничения сети.

Changed:
- `novels-content build tzm editor` успешно пересобрал и скомпоновал release
  `236af7315c7d977ad8d575ec01c732a9cd0211e6744f202a7f548be8ef32de57`.
- Release содержит 12 bundles / 12 chunks / 51 media entries и не содержит
  runtime source-map payload.
- Unity-generated trailing whitespace в 51 video `.meta` механически удалён;
  содержательные importer settings не менялись.
- Novels Editor PID 52402 открыт обычным запуском без `NOVELS_*` env vars.

Validation:
- Content build exit code 0; composed output находится в
  `Novels/Build/LocalContent`.
- Novels initial refresh/domain reload завершены, сцена
  `Assets/Novels/Novels.unity` загружена, C# compile errors отсутствуют.
- Licensing access token успешно обновлён; остаются прежние неблокирующие
  Unity Services TLS/403 warnings.
- `git diff --check`: успешно.

Pending / risks:
- Runtime/visual smoke выполняет пользователь в открытом Editor.

Suggested next step:
- Нажать Play и проверить TZM на обычной локальной скорости без debug HUD и
  Cold/Warm controls.
## 2026-08-25T08:43:50Z — story-streaming-experiment — completed

Task: Подготовить в отдельной ветке тест preview/full streaming, симуляцию сети
и диагностический HUD, затем собрать TZM Editor-контент.

Changed:
- Worktree `/Users/iantonishin/Documents/Codex/SomeGame-story-preview-experiment`,
  branch `experiment/story-preview-streaming`, commit `949587ec`.
- Экспериментальный build flag создаёт preview/full-бандлы и отдельную
  `<story>-media` delivery-группу; production contract без flag не меняется.
- Runtime запускает историю на preview и догружает full-арт в фоне.
- Editor поддерживает `NOVELS_SIMULATED_MBITS`; OnGUI HUD показывает Ink,
  FPS, frame time, RAM, tier, прогресс/скорость и даёт `Cold App`/`Warm`.

Validation:
- `novels-content doctor`: passed.
- TZM Editor build: passed; release `c74e1cd2...` composed locally.
- Preview group 36 297 428 B; full-art 246 683 737 B; media 287 268 760 B.
- Main Novels Unity batch compile: passed; C# errors absent.
- TZM authoring art/meta unchanged; `git diff --check`: passed.

Pending / risks:
- Preview пока уменьшает весь текущий art-бандл, а не только будущий chunk 0.
- Уже показанный Sprite не обновляется мгновенно; full применяется при следующих
  загрузках визуальных ассетов.
- Нужен ручной Play Mode замер Legacy/Warm/Cold с выбранной скоростью сети.

Suggested next step:
- Открыть экспериментальный worktree с `NOVELS_SIMULATED_MBITS=20`, пройти TZM
  и сравнить первый вход, `Warm` и `Cold App` по HUD.

## 2026-08-25T09:56:32Z — story-streaming-experiment — completed hotfix

Task: Исправить `FileNotFoundException` на `stories/zdm/card.json` при запуске
экспериментальной Editor-версии.

Changed:
- В изолированный worktree добавлены проверенные Editor outputs ZDM.
- Catalog Editor bundle пересобран; локальная композиция повторно собрана как
  Catalog + TZM experiment + ZDM.

Validation:
- Все catalog release, TZM/ZDM card, cover и Mac release существуют и непусты.
- Catalog содержит `tzm` и `zdm`; итоговая локальная композиция 854 МБ.
- Catalog Unity build завершён успешно, compile/build errors отсутствуют.

Pending / risks:
- Требуется только повторный Play Mode запуск в уже открытом Editor.

Suggested next step:
- Выйти из Play Mode и войти снова; перезапуск Unity не требуется.

## 2026-08-25T10:20:42Z — background-quality-crossfade — completed

Task: Плавно заменить уже показанный preview-фон на full после завершения
фоновой загрузки, без пересборки контента.

Changed:
- Commit `be60e0f3` в `experiment/story-preview-streaming`.
- `NovelRuntime` уведомляет активный Location о готовности full-бандла.
- Location хранит идентификатор текущего фона, повторно загружает full Sprite и
  применяет его только если фоновая команда всё ещё актуальна.
- `LocationScreen` создаёт временный runtime Image и выполняет crossfade.
- Во время видео обновляется скрытый poster; после cutscene возвращается full.

Validation:
- Открытый Unity Editor автоматически пересобрал `Novels.Location.dll` и
  `Novels.dll`: Tundra build success, C# errors absent.
- Scoped `git diff --check`: passed.
- Content bundles, releases, prefabs и serialized fields не изменялись.

Pending / risks:
- Нужен ручной Play Mode visual smoke; live-upgrade персонажей остаётся
  отдельным блоком.

Suggested next step:
- Повторить сценарий Preview → Full на статическом фоне и на cutscene-video.

## 2026-08-25T10:27:41Z — native-video-render-texture — completed

Task: Устранить размытие видео, вызванное RenderTexture размером с preview
poster (~256 px) вместо native MP4.

Changed:
- Commit `fbbf846c` в `experiment/story-preview-streaming`.
- `VideoPlayback` теперь сначала выполняет `Prepare()`, читает native
  `VideoPlayer.width/height`, затем создаёт RenderTexture.
- Размер безопасно ограничивается `SystemInfo.maxTextureSize`; fallback на
  poster используется только при отсутствии video metadata.
- Depth-buffer RenderTexture уменьшен с 16 до 0 как ненужный для UI-видео.

Validation:
- Исходный `номер в отеле.mp4`: 2160x1920, H.264, ~4.6 Mbps.
- Scoped `git diff --check`: passed.
- Unity compile ожидает возврата фокуса открытому Editor; content rebuild не
  требуется.

Pending / risks:
- Нужен повторный визуальный Play Mode smoke после автоматической recompilation.

Suggested next step:
- Вернуться в Unity, дождаться compile, снова запустить Play Mode и проверить
  `Номер в отеле` на preview tier.

## 2026-08-25T09:30:22Z — webgl-local-prototype — ready-for-integration

Task: Реализовать в отдельной ветке строго локальный persistent WebGL prototype.

Changed:
- Worktree `/Users/iantonishin/Documents/Codex/SomeGame-webgl-local-prototype`,
  branch `prototype/webgl-local-platform`, commit `cfb92896`.
- WebGL content target, local account, local analytics и WebGL-safe storage.
- Одна Editor-команда строит контент/player, запускает localhost и открывает
  настоящий WebGL-клиент в браузере.

Validation:
- Content doctor, shell syntax и `git diff --check`: passed.
- Unity batch compile не состоялся из-за несовместимой версии локального Unity
  Licensing Client; C# compilation не начиналась.

Pending / risks:
- После восстановления Unity license требуется compile и первый полный WebGL
  build/browser smoke; Android/iOS production delivery не изменялась.

Suggested next step:
- Открыть worktree в Unity и выполнить
  `Novels > Prototype > Build & Preview WebGL`.

## 2026-08-25T10:45:00Z — story-streaming-cold-cache — completed

Task: Сделать кнопку Cold App настоящей симуляцией чистого payload-кеша.

Changed:
- `Packages/Cache/Entity.cs`: добавлена безопасная очистка относительного
  каталога внутри `CachedFiles`.
- `Novels/Assets/Novels/EntryPoint.cs`: после завершения текущей сессии Cold
  App удаляет `RemoteContent` и `ContentStaging`, сохраняя игровые сейвы.

Validation:
- Unity Editor автоматически перекомпилировал и перезагрузил assemblies;
  C#-ошибок в новых строках лога нет.
- `git diff --check -- Packages/Cache/Entity.cs
  Novels/Assets/Novels/EntryPoint.cs`: успешно.
- Commit: `372f7423`.

Pending / risks:
- Финальное подтверждение сетевого сценария требует один раз нажать Cold App:
  preview и full TZM должны появиться в логе как новые downloads.
- Автогенерируемый `Novels/Novels.slnx` остался чужим незакоммиченным
  изменением и не включён в commit.

Suggested next step:
- В Play Mode нажать Cold App и проверить повторную загрузку preview с
  установленным `NOVELS_SIMULATED_MBITS`.

## 2026-08-25T11:16:00Z — story-streaming-chunks — completed block 1

Task: Ввести экспериментальный контракт predictive delivery plan и сборку
именованных `preview`, `chunk-0..N` без отрицательных номеров.

Changed:
- `Packages/Bundles/ContentRelease*`: optional streaming plan, fingerprint и
  validation.
- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`: build-time
  порядок арта и media по первому упоминанию в Ink.
- `ContentPipeline.cs`: feature-flag сборка preview только из chunk-0 и
  последовательных полноразмерных chunk bundles.
- `ContentPackageConvention.cs`: стабильные имена chunk/media payload groups.

Validation:
- Открытый Unity Editor: Tundra build success, assemblies reloaded, C# errors
  отсутствуют.
- Scoped `git diff --check`: passed.
- Commit: `67c9d862`.

Pending / risks:
- Runtime scheduler и asset routing ещё не подключены; экспериментальный
  контент этим контрактом пока не пересобирался.

Suggested next step:
- Отдельным lock-блоком подключить runtime scheduler и загрузку chunk-0..N.

## 2026-08-25T11:30:00Z — story-streaming-chunks — completed block 2

Task: Подключить непрерывную runtime-очередь чанков, predictive media prefetch
и live-upgrade текущего арта.

Changed:
- `StoryStreamingController.cs`: очередь `chunk-0 → media → chunk-1`,
  приоритетная догрузка запрошенного чанка и asset-to-bundle routing.
- `NovelRuntime*`: scheduler запускается до окна выбора эпизода; фоны, choose
  и character sprites получают правильный chunk bundle.
- Character runtime: сброс preview sprite cache и повторное разрешение текущих
  слоёв после готовности очередного full-чанка.
- Media scope принимает предрассчитанные individual media delivery groups.

Validation:
- Unity Editor: Tundra build success, assemblies reloaded, новых C# errors нет.
- Scoped `git diff --check`: passed.
- Commit: `0ea26631`.

Pending / risks:
- Нужна экспериментальная TZM rebuild, после которой runtime контракт можно
  проверить в Play Mode.
- Diagnostics пока показывает активную группу, но не всю ожидающую очередь.

Suggested next step:
- Отдельным lock-блоком расширить HUD/network simulation, затем собрать TZM.

## 2026-08-25T11:40:00Z — story-streaming-chunks — completed block 3

Task: Сделать Editor-сеть и HUD ближе к реальному streaming-клиенту.

Changed:
- `ThrottledFileSystemContentSource`: bandwidth + configurable latency/jitter.
- `EntryPoint`: defaults 120 ms latency and 30 ms jitter при включённом
  `NOVELS_SIMULATED_MBITS`; overrides через environment.
- HUD показывает ближайшую очередь `chunk/media`, а не только active group.

Validation:
- Unity Editor assemblies reloaded, новых C# errors нет.
- Scoped `git diff --check`: passed.
- Commit: `3c91f2ef`.

Pending / risks:
- Требуется TZM content rebuild и end-to-end cold test.

Suggested next step:
- Получить эксклюзивный Unity/build slot и собрать TZM Editor experiment.

## 2026-08-25T11:45:00Z — story-streaming-chunks — ready-for-integration

Task: Реализовать приближённый к production прототип `preview` + последовательных
art-чанков и предиктивной догрузки отдельных video/audio файлов.

Changed:
- Commits `67c9d862`, `0ea26631`, `3c91f2ef`, `8aeed0f1` в ветке
  `experiment/story-preview-streaming`.
- `Packages/Bundles/**`, `Packages/NovelsContentSdk/Editor/**`: optional
  streaming plan, build-time Ink analysis, preview/chunk/media release contract.
- `Novels/Assets/Novels/**`: непрерывный scheduler, priority demand load,
  background/character live upgrade, latency/jitter simulation и HUD queue.
- `Novels/Docs/AI/ParallelWork.story-streaming-experiment.md`: результаты.

Validation:
- `novels-content doctor`: успешно.
- `NOVELS_STREAMING_EXPERIMENT=1 novels-content build tzm editor`: успешно.
- Release `b3cddd9d...`: preview 1.71 MiB, preview group 4.14 MiB,
  13 art-чанков и 51 media delivery group.
- Unity bundle build скомпилировал assemblies без C# ошибок.

Pending / risks:
- Ручной cold/warm PlayMode smoke остаётся за пользователем.
- Повторный standalone `validate` завис на локальном Unity Licensing Client:
  `Unsupported protocol version '1.18.0'`; это не ошибка release/content build.
- Не включать эксперимент в production без `NOVELS_STREAMING_EXPERIMENT=1` и
  сравнительных замеров.
- Пользовательские изменения `Novels.slnx` и `ProjectSettings.asset` сохранены
  и не включены в commits.

Suggested next step:
- Открыть `Novels` этого worktree с `NOVELS_SIMULATED_MBITS=5`, нажать
  `Cold App`, проверить старт через preview, очередь и live upgrade.

## 2026-08-25T12:14:00Z — story-streaming-chunks — completed hotfix

Task: Исправить отказ Catalog на optional `streamingPlan: null` и чёрный экран
TZM, вызванный попаданием первого фона в `chunk-1`.

Changed:
- `ContentReleaseCodec` нормализует только полностью пустой optional plan в
  `null`; частично повреждённые планы по-прежнему отклоняет validator.
- `ExperimentalStreamingPlan` распознаёт любое числовое имя variant-файла как
  имя родительского ассета и NFC-нормализует токен.
- Commit: `9ee0f93d`.

Validation:
- Unity assemblies скомпилированы в ходе TZM build без C# errors.
- `NOVELS_STREAMING_EXPERIMENT=1 novels-content build tzm editor`: успешно.
- Release `c5dbae5c...`: 13 chunks, 51 media groups.
- Первый Ink-фон `гардероб суша день.png` подтверждён в `chunk-0` и preview.
- Preview bundle 9 496 365 B; preview delivery group 12 038 945 B.

Pending / risks:
- Требуется ручное подтверждение первого кадра в PlayMode на 5 Мбит/с.
- Увеличение preview — осознанная цена покрытия ранних candidate-наборов.

Suggested next step:
- Выполнить Cold App и открыть TZM; первый фон должен появиться до загрузки
  `chunk-1`.

## 2026-08-25T12:21:00Z — story-streaming-chunks — completed runtime hotfix

Task: Исправить падение загрузки многослойного персонажа при конкурентном
ожидании одного streaming chunk.

Changed:
- `StoryStreamingController` заменил single-await preserved task на общий
  `UniTaskCompletionSource` с безопасным fan-out результата, ошибки и cancel.
- Commit: `051267a3`.

Validation:
- Unity Tundra compile после изменения: success, C# errors отсутствуют.
- Новая PlayMode-сессия запущена; старые `Already continuation registered`
  находятся до domain reload и после него не повторялись.

Pending / risks:
- Пользователь повторяет исходный маршрут выбора внешности/причёски/одежды.

Suggested next step:
- Подтвердить показ персонажа и отсутствие `QUEUE_EXECUTION_FAILED`.

## 2026-08-25T12:58:00Z — story-streaming-chunks — completed planner hotfix

Task: Убрать блокировку первого гардероба на позднем `chunk-6`.

Changed:
- Для `maincharacter/view/<variant>/main.png` planner использует authored view
  name как Ink-token вместо отсутствующего в сценарии `maincharacter`.
- Commit: `afad5963`.

Validation:
- TZM Editor streaming build: success, release `8d48546d...`.
- Все четыре стартовых тела героини подтверждены в `chunk-0`.
- Preview bundle 9 660 424 B; preview group 12 203 004 B.
- C# compile errors отсутствуют.

Pending / risks:
- Требуется ручной replay первого гардероба на 5 Мбит/с.

Suggested next step:
- Cold App → TZM → первый выбор внешности; UI не должен ждать `chunk-6`.

## 2026-08-25T13:11:00Z — story-streaming-chunks — completed path-token fix

Task: Устранить позднюю загрузку стартовых hair/clothes/body слоёв без
добавления отдельных эвристик для каждого типа арта.

Changed:
- Planner ранжирует арт по всем содержательным сегментам пути и игнорирует
  технические сегменты дерева.
- Commit: `7a101010`.

Validation:
- TZM Editor build: success, release `a8616482...`.
- В `chunk-0` подтверждены 4 тела, 3 стартовых наряда и существующие front/back
  слои причёсок `за плечами`, `пучок`, `афрокосички`.
- Preview bundle 13 189 767 B; preview group 15 732 347 B.
- C# compile errors отсутствуют.

Pending / risks:
- Ручной replay первого гардероба на 5 Мбит/с.
- Более полное candidate-покрытие увеличило cold preview примерно до 25 секунд.

Suggested next step:
- Cold App → TZM → пройти три последовательных wardrobe выбора.

## 2026-08-25T13:23:00Z — story-streaming-chunks — completed defaults fix

Task: Включить не упомянутые в Ink, но обязательные runtime default assets в
preview.

Changed:
- Planner принудительно относит default hair `распущенные/блонд` главной
  героини к стартовому чанку.
- Commit: `eb25f4c1`.

Validation:
- TZM Editor build: success, release `8956b712...`.
- Существующий front-слой default hair подтверждён в `chunk-0`; back-слоя в
  authoring tree нет.
- Preview bundle 13 237 389 B; preview group 15 779 969 B.
- C# compile errors отсутствуют.

Pending / risks:
- Ручной replay первого гардероба на 5 Мбит/с.

Suggested next step:
- Cold App → TZM → выбрать внешность; запрос позднего чанка не должен
  блокировать UI.

## 2026-08-25T13:38:00Z — story-streaming-chunks — completed runtime quality hotfix

Task: Исправить ошибку CanvasGroup при live-upgrade фона и оставшийся мутным
персонаж после загрузки полноразмерного чанка.

Changed:
- `LocationScreen.CrossfadeImage`: переиспользует CanvasGroup, уже склонированный
  вместе с Image, вместо попытки добавить запрещённый второй компонент.
- Character runtime: отдельный full-quality sprite provider; после готовности
  чанка текущие слои повторно разрешаются из полноразмерного bundle.
- Commit: `14d7e004`.

Validation:
- Scoped `git diff --check`: passed.
- Компиляция отложена до следующего выхода/входа в Play Mode открытого Editor.

Pending / risks:
- Повторить текущую сцену после domain reload: фон и персонаж должны стать
  резкими, ошибки CanvasGroup/ArgumentNullException не должны повториться.

Suggested next step:
- Остановить и снова запустить Play Mode, затем пройти до того же кадра.

## 2026-08-25T14:05:00Z — story-streaming-chunks — completed preview handoff hotfix

Task: Исправить конфликт одновременно загруженных preview и chunk-0
AssetBundle с одинаковым набором authored assets.

Changed:
- `Bundles.Scope`: точечное освобождение одного принадлежащего scope бандла.
- `StoryStreamingController`: после скачивания chunk-0 выгружает контейнер
  preview через `Unload(false)`, сохраняя уже созданные preview-спрайты, затем
  открывает full-quality chunk и переключает маршрутизацию запросов.
- Commit: `dde2e8fb`.

Validation:
- Release `8956b712...`: preview `3ccc2975...` и chunk-0 `47c7f9f3...`
  подтверждены как разные payload с одинаковым authored asset set.
- Scoped `git diff --check`: passed.
- Runtime replay требует domain reload открытого Unity Editor.

Pending / risks:
- Повторить Cold App после выхода/входа в Play Mode; сообщение `same files is
  already loaded` не должно повториться, фон и персонаж должны обновиться.

Suggested next step:
- Перезапустить Play Mode и проверить переход preview → chunk-0.

## 2026-08-25T14:18:00Z — story-streaming-chunks — completed Unicode routing hotfix

Task: Исправить запрос персонажа к выгруженному preview после успешной
загрузки chunk-0.

Changed:
- `StoryStreamingController.Canonicalize`: streaming plan и runtime asset paths
  приводятся к Unicode NFC перед сопоставлением.
- Commit: `f8eba6ca`.

Validation:
- Стек подтвердил ошибочную unknown-asset ветку для full-quality main body.
- Release содержит macOS NFD-путь `европейская`, а runtime addressing
  канонизирует selector в NFC.
- Scoped `git diff --check`: passed.

Pending / risks:
- Требуется новый domain reload и ручной replay текущего кадра.

Suggested next step:
- Перезапустить Play Mode; после готовности chunk-0 персонаж должен обновиться
  без обращения к `novels_content_tzm_preview`.

## 2026-08-25T14:35:00Z — story-streaming-chunks — completed poster/HUD hotfix

Task: Устранить чёрный экран во время ожидания фонового видео и сделать
экспериментальную OnGUI-панель адаптивной к разрешению.

Changed:
- `BackgroundPresentationController`: сначала показывает доступный poster,
  затем ждёт/готовит видео.
- `LocationScreen`: готовое видео плавно проявляется поверх poster и только
  после fade отключает статический Image.
- `StorySourceOverlay`: масштабирует шрифты, отступы, ширину и кнопки по
  reference 465x1024; высота панели рассчитывается по полному тексту.
- Commit: `951d0762`.

Validation:
- Лог и Ink line 84 подтвердили ожидание `номер в отеле.mp4` после скрытия
  предыдущего фона как причину чёрного экрана на line 87.
- Scoped `git diff --check`: passed.
- Runtime replay требует domain reload открытого Unity Editor.

Pending / risks:
- Проверить poster → video transition и HUD на 1920x1080 Portrait после
  остановки/повторного запуска Play Mode.

Suggested next step:
- Warm restart после domain reload; пройти от гардероба к дисклеймеру.

## 2026-08-25T14:43:00Z — story-streaming-chunks — completed HUD compile hotfix

Task: Исправить `CS0117` для недоступного `GUIContent.Temp`.

Changed:
- `StorySourceOverlay`: один кэшируемый `GUIContent` обновляется вместе с
  текстом и используется для расчёта высоты.
- Commit: `0ec26ede`.

Validation:
- Исходная ошибка локализована в `StorySourceOverlay.cs:102`.
- Scoped `git diff --check`: passed.
- Unity Editor ещё не записал новый compile cycle после внешней правки.

Pending / risks:
- Дождаться автоматического refresh/compile Editor и проверить Play Mode.

Suggested next step:
- Вернуть фокус Unity или повторно войти в Play Mode для refresh.

## 2026-08-25T15:05:00Z — story-streaming-chunks — completed first-frame hotfix

Task: Устранить чёрный RenderTexture между готовностью VideoPlayer и первым
декодированным кадром.

Changed:
- `VideoPlayback`: включает frame-ready events, ждёт первый реальный кадр до
  статуса Ready; при десятисекундном таймауте сохраняется poster fallback.
- Commit: `5eff606c`.

Validation:
- Свежий лог подтвердил успешный `ShowStatic` и последующую загрузку chunk-1;
  chunk-1 не является зависимостью стартовой локации.
- Runtime flow подтвердил, что прежний `Ready` возвращался сразу после
  `VideoPlayer.Play()`, до появления данных в RenderTexture.
- Scoped `git diff --check`: passed.

Pending / risks:
- Требуется domain reload и ручная проверка poster → first frame → crossfade.

Suggested next step:
- Перезапустить Play Mode и повторить переход после гардероба.

## 2026-08-25T15:07:00Z — story-streaming-chunks — completed render publication hotfix

Task: Устранить сохраняющийся чёрный экран после стартового гардероба.

Changed:
- `VideoPlayback`: после `VideoPlayer.frameReady` ждёт фазу
  `LastPostLateUpdate`, чтобы декодированный кадр успел попасть в целевой
  `RenderTexture` до начала UI crossfade.
- Commit: `0a6334ca`.

Validation:
- Первый кадр исходного `номер в отеле.mp4` извлечён через ffmpeg и не является
  чёрным.
- Scoped `git diff --check`: passed.
- Unity batch compile не начался: локальный LicensingClient не создал IPC
  channel за 60 секунд; процесс остановлен, write-lock не удерживается.

Pending / risks:
- Требуется обычный запуск Editor, domain reload и ручной replay перехода
  гардероб → строка 84 → дисклеймер.

Suggested next step:
- Открыть Unity и повторить Cold App; контентные бандлы пересобирать не нужно.

## 2026-08-25T15:19:00Z — story-streaming-chunks — yielded manual replay

Task: Запустить экспериментальный Unity Editor с ограничением сети 5 Мбит/с.

Changed:
- Рабочее дерево не изменялось.
- Editor запущен с `NOVELS_SIMULATED_MBITS=5`, latency 120 ms и jitter 30 ms.

Validation:
- Процесс Unity успешно стартовал и продолжает работать.

Pending / risks:
- Editor занят ручной пользовательской проверкой; другим Unity/build задачам
  необходимо дождаться его закрытия.

Suggested next step:
- Выполнить Cold App и пройти стартовый гардероб до дисклеймера.

## 2026-08-25T15:37:00Z — story-streaming-chunks — yielded stale-frame replay

Task: Исправить повторный чёрный экран и неверную скорость HUD.

Changed:
- `VideoPlayback`: first-frame completion создаётся после `Prepare()` и
  принимает callbacks только между `Play()` и `Stop()`; commit `aefce6a7`.
- `StreamingExperimentDiagnostics`: throughput хранится отдельно для каждой
  параллельной delivery group; commit `62075ea9`.

Validation:
- Scoped `git diff --check`: passed.
- Unity Editor выполнил два domain reload без `CS`/compilation errors.
- Editor повторно запущен с 5 Mbit/s, 120 ms latency, 30 ms jitter.

Pending / risks:
- Требуется ручной Cold App replay стартового перехода.

Suggested next step:
- Пройти стартовый гардероб; ожидается poster, затем video crossfade без
  чёрного кадра, HUD около 0.6 MiB/s на одной активной загрузке.

## 2026-08-25T15:41:00Z — story-streaming-chunks — yielded location diagnostics

Task: Инструментировать повторяющийся чёрный экран без изменения presentation flow.

Changed:
- `LocationScreen`: read-only snapshot текущих sprite/Image/CanvasGroup,
  RawImage/RenderTexture и VideoPlayer.
- `StorySourceOverlay`: показывает snapshot четвёртой строкой HUD.
- Commit: `d8935b90`.

Validation:
- Scoped `git diff --check`: passed.
- Unity выполнил domain reload без compilation errors.
- Editor открыт с 5 Mbit/s, 120 ms latency, 30 ms jitter.

Pending / risks:
- Диагностика временная и должна быть удалена после подтверждения причины.

Suggested next step:
- Cold App, пройти до чёрного экрана и передать снимок полной строки Location.

## 2026-08-25T15:46:00Z — story-streaming-chunks — yielded load-before-hide replay

Task: Исправить подтверждённый чёрный экран между гардеробом и новой локацией.

Changed:
- `BackgroundPresentationController`: для обычной локации сначала разрешает
  следующий sprite, сохраняя текущий фон активным, затем выполняет hide/show.
- Solid-color переходы сохраняют прежнюю семантику.
- Commit: `031f1587`.

Validation:
- Диагностический снимок подтвердил старый sprite при `alpha=0`, `go=false`,
  отключённом video и отсутствующем RenderTexture.
- Scoped `git diff --check`: passed.
- Unity выполнил domain reload без compilation errors.
- Editor открыт с профилем 5 Mbit/s.

Pending / risks:
- Требуется ручной Cold App replay; diagnostic HUD удалить после подтверждения.

Suggested next step:
- До разрешения `номер в отеле` должен оставаться виден фон гардероба, затем
  произойти короткий переход на новый poster/video без чёрного ожидания.

## 2026-08-26T08:27:00Z — story-streaming-chunks — yielded preview-free waits

Task: Удалить preview bundle, сильнее раздробить TZM art и показывать понятное
блокирующее ожидание при demand miss.

Changed:
- Commit `ff918449`: streaming release содержит только `chunk-0..N`; preview
  DTO/fingerprint/validator/addressing/build/runtime path удалены.
- Default source target уменьшен с 96 до 16 MiB; startup art снова подчиняется
  лимиту, bootstrap и Ink остаются в `chunk-0`.
- `StoryDownloadOverlay`: после порога 0,7 с показывает уменьшенный размытый
  снимок текущего кадра, байтовый progress и сглаженный ETA; после 100% —
  `Подготавливаем продолжение…`.
- Overlay подключён только к обязательным art-запросам; background prefetch сам
  игру не перекрывает.

Validation:
- Unity 6000.3.11f1: `Tundra build success`, новых C# errors нет.
- TZM Editor build: release `a4307e72140063fe3abae81f68d2bc6090c10006e8b320ffc0c80cbe712c31c3`.
- Manifest: 82 art bundles, preview=0, median 2,1 MiB, max 12,7 MiB,
  `noveltexts/**` находится в `tzm-chunk-0`.
- Scoped `git diff --check`: passed.
- Пользовательский `Novels/ProjectSettings/ProjectSettings.asset` сохранён и
  не включён в commit.

Pending / risks:
- Нужен ручной Cold App smoke: визуально проверить blur/progress/ETA и
  продолжение очереди после обязательного чанка.
- Автоматический Play Mode не включён: macOS запретил synthetic keystroke.
- Временная строка location diagnostics остаётся до подтверждения нового smoke.

Suggested next step:
- В открытом Editor нажать Play → Cold App → TZM, пройти первый гардероб и
  проверить окно на первом demand miss.

## 2026-08-26T08:45:00Z — story-streaming-chunks — yielded overlay contrast hotfix

Task: Исправить белое нечитаемое окно обязательной загрузки.

Changed:
- `StoryDownloadOverlay`: затемнение, panel, track и fill используют собственные
  RGBA textures вместо глобального `GUI.color`; content color задаётся явно.
- Safe area явно преобразуется из bottom-left координат Unity в top-left
  координаты OnGUI.
- Commit: `10be5be3`.

Validation:
- Исходный screenshot подтвердил белый panel при корректном размере окна и
  отсутствии Console exception.
- Unity: `Tundra build success`, domain reload завершён, C# errors нет.
- Scoped `git diff --check`: passed.

Pending / risks:
- Требуется повторный Play Mode visual smoke на первом demand miss.

Suggested next step:
- Снова запустить Play Mode/Cold App; ожидается тёмное центральное окно с
  белым текстом и синей шкалой.

## 2026-08-26T10:57:14Z — windows-player-build — ready-for-integration

Task: Собрать с main полностью автономную тестовую Windows-версию.

Changed:
- Content SDK/CLI поддерживают Windows bundle target с platform key `Win`.
- `EntryPoint` при `NOVELS_EMBEDDED_CONTENT` читает локальный
  `StreamingAssets/NovelContent` без HTTP.
- Добавлен `Novels/Tools/build-embedded-test-player.sh`; remote build script
  также теперь понимает Windows/Win64.

Validation:
- Catalog/TZM/ZDM Windows content builds: passed.
- Unity Windows development Player: Success, PE32+ x86-64, 2139,3 MiB.
- В Player встроено 1,9 ГБ контента и три `Remote/Win/release.json`.
- `git diff --check` и shell syntax: passed.

Pending / risks:
- Нужен launch smoke на реальной Windows; macOS `.exe` не запускает.

Suggested next step:
- Перенести целиком `Novels/Build/Players/WindowsOffline` на Windows и
  запустить `Novels.exe` с отключённой сетью.

## 2026-08-26T11:43:00Z — story-streaming-chunks — yielded awaiting Unity import

Task: Убрать ручную загрузку всей истории и заменить её пассивным нижним
индикатором автоматической последовательной загрузки.

Changed:
- Catalog action API и обе runtime-кнопки удаления всей истории удалены.
- `StoryStreamingController` публикует общий byte-progress art/media в новый
  `StoryStreamingProgressOverlay`.
- Добавлены view/controller и deterministic editor-builder отдельного нижнего
  prefab overlay.

Validation:
- `rg` подтвердил отсутствие `CatalogAction`, `DownloadAll` и
  `SecondaryAction` в C# runtime.
- `git diff --check`: успешно.

Pending / risks:
- Unity открыт в Play Mode и не импортировал новые скрипты; macOS запретил
  UI automation для Assets/Refresh. Prefab и `.meta` ещё не сгенерированы,
  компиляция и commit отложены до остановки Play Mode.

Suggested next step:
- Остановить Play Mode; затем этому потоку повторно получить FIFO/write-lock,
  дождаться импорта, проверить prefab references и Unity compile.

## 2026-08-26T11:51:00Z — story-streaming-chunks — yielded awaiting refresh

Task: Завершить импорт нижнего prefab-индикатора загрузки истории.

Changed:
- Unity импортировал новые C# scripts и `.meta`; первая компиляция прошла.
- Builder дополнен созданием отсутствующей Resources folder chain.
- Физическая папка fallback создана, чтобы prefab path был валиден.

Validation:
- Unity: `Tundra build success`, 12 items updated.
- Первичный `[DidReloadScripts]` выявил только отсутствующую папку; исправление
  ещё не подхвачено Editor, поскольку он не выполняет Refresh без фокуса.

Pending / risks:
- Нужен ручной `Assets → Refresh`; затем проверить prefab и повторную compile.

Suggested next step:
- В открытом Unity выполнить `Assets → Refresh`, после чего возобновить поток.

## 2026-08-26T11:55:00Z — story-streaming-chunks — completed

Task: Убрать ручную полную загрузку истории и показывать автоматический общий
прогресс отдельным нижним prefab overlay.

Changed:
- Удалены `CatalogAction`, secondary Catalog button и кнопка из demand-wait
  overlay; эпизодный экран снова содержит только выбор эпизода.
- `StoryStreamingController` продолжает автоматическую последовательную
  art/media очередь и агрегирует её byte-progress.
- Добавлены `StoryStreamingProgressOverlay`, screen view, Resources prefab и
  deterministic editor-builder. Commit `357d9705`.

Validation:
- Unity 6000.3.11f1: финальный `Tundra build success`, новых C# errors нет.
- PrefabImporter успешно импортировал prefab; `_canvasGroup`, `_progressFill`
  и `_label` ненулевые, sorting order 90, raycasts выключены.
- `git diff --cached --check`: успешно.
- Пользовательский `Novels/ProjectSettings/ProjectSettings.asset` не включён.

Pending / risks:
- Нужен только визуальный Play Mode smoke: компактная нижняя полоса должна
  показывать `Загрузка истории · N%`, а после завершения — кратко
  `История доступна офлайн` и скрыться.

Suggested next step:
- Запустить Play Mode/Cold App и визуально проверить нижнюю полосу на нескольких
  разрешениях; bundle rebuild не требуется.

## 2026-08-26T12:19:00Z — tzm-video-30fps — completed

Task: Уменьшить FPS TZM-видео до 30, не меняя разрешение и пропорции.

Changed:
- 37 исходных роликов с 60 FPS перекодированы в H.264, 30 FPS, CRF 18,
  `yuv420p`, faststart, без аудио. 14 уже 30-FPS файлов не изменялись.
- Commit: `a5dbfd2b`.
- TZM Editor streaming release пересобран и composed; release ID
  `3ac58d77fdbc8eff66c4f6dabc84d4f2e7c559b9e81b88dbad5e4d610b991ab5`.

Validation:
- Все 51 исходных и release media-файла: 30 FPS, один видеопоток, ни одной
  аудиодорожки; ширина и высота совпадают с исходными.
- Media payload: 287,268,760 -> 263,354,915 bytes, экономия 23,913,845 bytes
  (8.3%, около 22.8 MiB).
- Unity content build и compose: успешно.
- Пользовательский `Novels/ProjectSettings/ProjectSettings.asset` не включён.

Pending / risks:
- Рекомендуется короткий визуальный smoke динамичных сцен; CRF 18 сохранён,
  но плавность теперь намеренно ограничена 30 FPS.

Suggested next step:
- Открыть Game Editor и проверить `причал.mp4`, `мотоцикл в движении.mp4` и
  `метеорит.mp4` как наиболее динамичные ролики.

## 2026-08-28T00:00:00Z — character-layering-rules — completed

Task: Сделать правила модульной отрисовки персонажей доступными всем чатам
репозитория.

Changed:
- `AGENTS.md`: добавлено обязательное чтение character-art спецификации.
- `Novels/Docs/AI/CharacterLayeringRules.md`: зафиксированы правила общей базы,
  волос, одежды, эмоций, регистрации слоёв и обратной сборки.

Validation:
- `git diff --check -- AGENTS.md Novels/Docs/AI/CharacterLayeringRules.md` —
  успешно.

Pending / risks:
- Нет.

Suggested next step:
- Использовать документ как quality gate для всех новых character-art задач.

## 2026-08-28T01:00:00Z — ai-docs-index — completed

Task: Сделать общую AI-документацию доступной всем чатам по типу задачи.

Changed:
- `AGENTS.md`: общий индекс стал обязательной первой точкой входа.
- `Novels/Docs/AI/README.md`: документы разделены на обязательное ядро,
  действующие руководства, планы/измерения и историю; добавлена маршрутизация
  по типам задач и правила поддержания структуры.

Validation:
- `git diff --check -- AGENTS.md Novels/Docs/AI/README.md` — успешно.
- Все относительные Markdown-ссылки из нового индекса существуют.

Pending / risks:
- Физическое перемещение исторических документов не выполнялось, чтобы не
  ломать ссылки; кандидаты на архив явно обозначены в индексе.

Suggested next step:
- Новым чатам начинать с `Novels/Docs/AI/README.md` и читать только обязательный
  набор для своей задачи.

## 2026-08-28T02:00:00Z — ai-docs-physical-layout — completed

Task: Физически структурировать AI-документацию и обновить действующие ссылки.

Changed:
- `Novels/Docs/AI/rules/`: обязательные правила.
- `Novels/Docs/AI/guides/`: тематические authoring/pipeline/Ink/checklist
  руководства.
- `Novels/Docs/AI/architecture/`: обзор и архитектурные границы.
- `Novels/Docs/AI/plans/`: текущие планы и датированные измерения.
- `Novels/Docs/AI/work/parallel/`: статусы `ready-for-integration` и очередь.
- `Novels/Docs/AI/archive/parallel-work/`: integrated/completed статусы.
- `Novels/Docs/AI/archive/reports/`: исторические отчёты.
- `AGENTS.md`, `Novels/Docs/AI/README.md` и внутренние ссылки обновлены.

Validation:
- 38 документов физически распределены по новым каталогам.
- Проверка всех относительных Markdown-ссылок: 0 отсутствующих целей.
- Поиск старых действующих путей вне исторического `CoordinationRuntime`: 0.
- Scoped `git diff --check` для документации: успешно.
- Общий `git diff --check` по-прежнему видит только прежние trailing spaces в
  пользовательском `Novels/ProjectSettings/ProjectSettings.asset`; файл не
  изменялся этой задачей.

Pending / risks:
- Исторические пути внутри `HANDOFF.md` и завершённых runtime agent-записей
  намеренно не переписывались: они описывают состояние на момент записи.

Suggested next step:
- Использовать `Novels/Docs/AI/README.md` как единственную точку маршрутизации
  документации.

## 2026-08-28T03:00:00Z — parallel-work-archive-audit — completed

Task: Проверить старые `ready-for-integration` документы и архивировать
подтверждённо завершённые.

Changed:
- 17 статусов, чья реализация присутствует в `main` (`f849ff22`) или его
  истории, помечены `integrated` и перенесены в
  `Novels/Docs/AI/archive/parallel-work/`.
- `Novels/Docs/AI/work/parallel/ParallelWork.queue.md` сокращён до одной
  реальной незавершённой интеграции.
- `Novels/Docs/AI/README.md` уточняет различие work и archive.

Validation:
- Текущий `main`: `f849ff22`.
- Реализация audit, Catalog carousel, trim manifest, platform Library cache,
  ContentBuildPlan, story-global layout и Player/Windows tooling подтверждена
  текущими файлами и историей `main`.
- `cfb92896` не является предком HEAD и остаётся отдельным WebGL prototype.
- Проверка Markdown-ссылок: 0 отсутствующих целей.
- Scoped `git diff --check` документации: успешно.

Pending / risks:
- `ParallelWork.webgl-local-prototype.md`: Unity compilation и browser smoke не
  выполнены; commit находится только в `prototype/webgl-local-platform`.

Suggested next step:
- Восстановить Unity license, проверить WebGL prototype и принять решение об
  интеграции либо закрытии эксперимента.

## 2026-08-28T04:00:00Z — ai-docs-publish-main — yielded

Task: Опубликовать новую структуру AI-документации в `main`.

Changed:
- Создан локальный commit `da753b93` (`docs(ai): structure shared project guidance`).
- В commit включены только `AGENTS.md` и структурированная документация
  `Novels/Docs/AI` без `CoordinationRuntime` и пользовательских Unity settings.

Validation:
- Удалённый `main` перед commit подтверждён на `f849ff22` через HTTPS.
- Staged `git diff --check`: успешно.

Pending / risks:
- Push не выполнен: защитный механизм требует отдельного подтверждения
  публикации внутренних архитектурных и координационных документов в GitHub
  `SomeStrangeGame/SomeGame`, branch `main`.

Suggested next step:
- После явного подтверждения повторить HTTPS push `da753b93` в `main`.

## 2026-08-28T04:10:00Z — ai-docs-publish-main — yielded credentials

Task: После явного разрешения пользователя отправить `da753b93` в GitHub
`SomeStrangeGame/SomeGame`, branch `main`.

Changed:
- Рабочее дерево не изменялось; локальный commit остаётся `da753b93`.

Validation:
- Перед push удалённый `main` подтверждён на `f849ff22`.
- Пользователь явно разрешил публикацию внутренних документов.

Pending / risks:
- HTTPS push завершился `could not read Username for 'https://github.com'`.
- SSH ранее завершился `Permission denied (publickey)`.

Suggested next step:
- Настроить GitHub write credentials (SSH key или credential helper/token),
  затем повторить push `da753b93:refs/heads/main`.

## 2026-08-28T04:20:00Z — ai-docs-publish-main — completed

Task: Опубликовать новую структуру AI-документации в GitHub main.

Changed:
- Commit `da753b93` отправлен в `SomeStrangeGame/SomeGame`, branch `main`.

Validation:
- `~/.ssh/SomeGame_ssh` успешно авторизуется как `MisterPureshechka`.
- Удалённый `refs/heads/main` после push равен
  `da753b93981e00a529801736a3c40d7b473196b4`.
- Чужие локальные изменения остались вне commit.

Pending / risks:
- Нет для публикации документации.

Suggested next step:
- При желании добавить `~/.ssh/config` для автоматического выбора
  `SomeGame_ssh` при следующих командах Git.

## 2026-08-28T12:05:00Z — asset-scope-protocol — completed

Task: Уточнить протокол комплектации арта после ошибочного расширения списка
фонов эпизода.

Changed:
- `guides/ContentAuthoringGuide.md`: утверждённый asset list закреплён как
  граница задачи; промежуточные точки, ракурсы и состояния не создают новые
  обязательные ассеты без явного согласования.
- `guides/ManualContentChecklist.md`: добавлена проверка пропусков, лишних
  обязательных ассетов и основания для каждого самостоятельного фона.

Validation:
- `git diff --check` для двух изменённых руководств: успешно.
- Ручная сверка: правило распространяется на фоны, персонажные слои, аудио и
  видео; черновики отделены от обязательной комплектации.

Pending / risks:
- Существующий экспериментальный набор из девяти фонов не переклассифицирован
  автоматически; его обязательная часть должна быть приведена к утверждённому
  списку отдельно.

Suggested next step:
- Перед следующей генерацией зафиксировать таблицу `asset list -> файл` для
  первого эпизода и согласовать её с автором.

## 2026-08-28T12:20:00Z — asset-scope-publish — completed

Task: Опубликовать правки протокола комплектации в `main`.

Changed:
- Commit `77adad49` содержит только `ContentAuthoringGuide.md` и
  `ManualContentChecklist.md`.
- Commit отправлен в `origin/main`.

Validation:
- Перед commit локальный `HEAD` совпадал с `origin/main` на `60a13762`.
- `git diff --cached --check`: успешно; в commit два файла и 34 добавленные
  строки.
- Push: `60a13762..77adad49 main -> main`.

Pending / risks:
- Чужие локальные изменения рабочего дерева сохранены и не включены в commit.

Suggested next step:
- Использовать утверждённый asset list перед следующей генерацией арта.

## 2026-08-28T12:40:00Z — unity-licensing-protocol — completed

Task: Добавить общий протокол диагностики и восстановления Unity Licensing.

Changed:
- `guides/UnityLicensingTroubleshooting.md`: добавлены evidence-first baseline,
  классификация IPC/license/network причин, безопасное восстановление,
  критерии проверки и профилактика.
- `README.md`: добавлены маршрут для licensing-сбоев и ссылка среди действующих
  руководств.

Validation:
- `git diff --check` для индекса и нового руководства: успешно.
- Ссылки и ключевые симптомы проверены через `rg`; новый файл непустой.
- Исторический конфликт protocol `1.17.4` / `1.18` отмечен как пример, а не
  универсальная причина.

Pending / risks:
- Протокол не автоматизирует завершение процессов и удаление sockets: точные
  PID/пути должны подтверждаться заново при каждом инциденте.

Suggested next step:
- При следующем licensing-сбое следовать новому руководству и записать свежие
  версии, логи и результат в handoff.

## 2026-08-28T13:00:00Z — unity-licensing-publish — completed

Task: Опубликовать Unity Licensing troubleshooting protocol в `main`.

Changed:
- Commit `a737fa3d` содержит индекс AI-документации и новый
  `UnityLicensingTroubleshooting.md`.
- Commit отправлен в `origin/main`.

Validation:
- Перед commit локальный `HEAD` совпадал с `origin/main` на `77adad49`.
- `git diff --cached --check`: успешно после удаления лишней пустой строки.
- В commit ровно два файла и 148 добавленных строк.
- Push: `77adad49..a737fa3d main -> main`.

Pending / risks:
- Чужие локальные изменения рабочего дерева сохранены и не включены в commit.

Suggested next step:
- Использовать новый guide при следующем licensing-инциденте.

## 2026-08-28T13:40:00Z — parallel-root-dedup — completed

Task: Убрать вторую корневую структуру `ParallelWork.*.md`.

Changed:
- 30 корневых status-файлов перемещены в `archive/parallel-work/` после
  подтверждения, что все упомянутые в них commits входят в текущий `main`.
- Самоссылки внутри перемещённых записей обновлены на архивные пути.
- `work/parallel/` оставлен единственным местом для незавершённой работы.

Validation:
- Корневых `ParallelWork.*.md`: 0.
- `work/parallel`: только queue и WebGL prototype; совпадающих имён с архивом
  нет.
- `archive/parallel-work`: 51 запись.
- Старых корневых путей в действующих rules/guides/plans/architecture и самих
  status-файлах нет; исторические упоминания в runtime-журнале сохранены.
- `git diff --check`: успешно для содержательных файлов после освобождения
  runtime lock.

Pending / risks:
- WebGL prototype остаётся единственной незавершённой parallel-задачей.

Suggested next step:
- Зафиксировать перемещения вместе с проверенными completed coordination-
  записями отдельным commit, не включая устаревший Windows agent-status.

## 2026-08-28T14:05:00Z — ai-docs-runtime-cleanup — completed locally

Task: Зафиксировать очищенную ParallelWork-структуру и завершённые runtime-
записи.

Changed:
- Подготовлены к одному commit 30 переносов из корня в
  `archive/parallel-work/`, completed agent-записи, полный handoff и штатное
  завершение runtime-состояния `story-streaming-experiment`.
- WebGL и устаревший Windows agent-status намеренно исключены.

Validation:
- Корневых `ParallelWork.*.md`: 0; в `work/parallel` только queue и WebGL.
- Старых путей в действующей документации нет; `git diff --check` успешно.
- Staging проверяется по точному списку перед commit.

Pending / risks:
- WebGL остаётся ready-for-integration и не входит в cleanup commit.
- Windows agent-status требует отдельного исправления владельцем или
  подтверждённого архивного решения.

Suggested next step:
- После commit при явном запросе пользователя отправить его в `main`.
