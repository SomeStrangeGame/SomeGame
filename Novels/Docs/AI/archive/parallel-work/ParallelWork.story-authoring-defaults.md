# Parallel work: story authoring defaults

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: Ink chunk layout and character defaults
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/StoryAssetOrderWindow.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs.meta`
- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs.meta`
- точечные Editor-файлы authoring-разметки чанков
- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Runtime/Content/CharacterAssetProfile.cs`
- точечная интеграция в `Packages/NovelsContentSdk/Runtime/Features/Character/**`
- `Novels/Assets/Novels/StoryStreamingController.cs`
- `Novels/Assets/Novels/EntryPoint.cs`
- `Novels/Assets/Novels/ApplicationEnvironment.cs`
- `Novels/Assets/Novels/ApplicationRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.Content.cs`
- `Novels/Assets/Novels/Novels.unity`
- `Packages/Bundles/ContentReleaseValidator.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/archive/parallel-work/ParallelWork.story-authoring-defaults.md`

## Не изменять

- `Projects/novels-catalog/**`
- `Projects/novels-zdm/**`
- исходный Ink и арт TZM
- чужие status-файлы

## Изменённые контракты

- Планируется: ассет истории задаёт необязательные дефолтные слои внешности по имени персонажа.
- Разметка последовательных чанков хранится в story asset как GUID-списки без
  прямых Unity Object dependencies; общий Inspector генерирует её из Ink.

## Атомарные блоки

1. Экспорт JSON-разметки чанков из линейного отчёта (заменён блоком 5).
2. Дефолтная внешность персонажей из ассета истории и runtime-интеграция.
3. Компиляция и scoped validation.
4. Отдельный визуальный раздел для ассетов, не входящих в чанки.
5. Перенос source of truth разметки из sidecar JSON в story asset и Inspector.
6. Единый Inspector: перенос оставшихся Ink Tools и удаление старого меню.
7. Устранение повторных тяжёлых вычислений в `OnInspectorGUI`.

## Выполнено

- Ink Tools рассчитывает линейный список и предпросмотр чанков; source of truth
  больше не сохраняется в sidecar JSON.
- Art, video и audio остаются в одном линейном списке; ненайденные элементы
  сохраняют положение в конце отчёта. Размер media не режет Unity art bundle.
- Streaming build читает ручной порядок и состав из `NovelContentAsset`; если
  GUID-разметка пуста, сохраняется прежний экспериментальный fallback.
- Media одного логического чанка получают общий delivery group и готовятся
  runtime параллельно.
- `NovelContentAsset` получил необязательный список дефолтной внешности по
  имени персонажа: одежда, причёска, цвет волос и аксессуар.
- Глобальный hardcode `распущенные/блонд` удалён; resolver читает дефолты
  конкретной истории.
- В `tzm.asset` добавлены Салли и Алекса. Для Алексы базовая одежда оставлена
  пустой, поскольку однозначного authoring-значения нет.
- Неясная колонка `Первое` переименована в `Позиция в Ink`.
- `AudioMixer` удалён из story definition TZM/ZDM; общий mixer хранится
  в `EntryPoint` основного приложения и передаётся runtime явно.
- Ink Tools получил кнопку `Обновить эпизоды`: порядок берётся из
  `INCLUDE`, ID/title/source/story/end marker вычисляются, а version/silent IDs
  сохраняются из существующих записей.
- В таблице отчёта показываются разделители `Чанк N` с размером.
- Неизвестные ассеты показываются под отдельным разделителем
  `Не входят в чанки` и визуально не относятся к последнему чанку.
- В `NovelContentAsset` добавлены скрытые authoring-поля: корневой Ink, целевой
  размер и последовательность чанков. Состав хранится GUID-строками, поэтому
  ссылки не создают Unity bundle dependencies.
- Общий custom Inspector story asset получил генератор из Ink, проверку,
  ручное добавление/удаление/перестановку чанков и их art/media-состава.
- `tzm.asset` настроен на корневой `tzm.ink`; готовую разметку автор создаёт
  кнопкой `Рассчитать по Ink`, после чего может отредактировать её вручную.
- Старый `*.ink.chunks.json` больше не читается и не влияет на сборку.
- Старое `StoryAssetOrderWindow` и пункты `Novels/Content/Ink Tools` /
  `Assets/Novels/Open Ink Tools` удалены: отдельного authoring-окна больше нет.
- Весь workflow находится в Inspector `NovelContentAsset`: compile Ink и
  source-map, Episodes, линейный список, генерация/валидация и ручной состав
  чанков.
- Операции с файлами вынесены в editor-only `StoryInkAuthoring`, поэтому
  Inspector остаётся единственной UI-точкой, а логика не дублируется.
- Линейный список адаптирован к узкому Inspector: вертикальные строки,
  фильтр и пагинация по 40 записей.
- Ручной состав чанков свёрнут на верхнем уровне и больше не обходит все GUID
  при каждом `Repaint`; размер лениво кэшируется только для раскрытого чанка.
- Одновременно раскрывается один чанк, а его объекты показываются страницами
  по 30 строк, поэтому стоимость кадра Inspector ограничена даже для крупных
  ручных списков.

## Проверено

- `dotnet build Novels/Novels.Content.csproj` — успешно.
- `git diff --check` — успешно.
- Открытый TZM Editor пересобрал `Novels.Content` и
  `Novels.ContentSdk.Editor` без C#-ошибок.
- `dotnet build Novels/Novels.Content.csproj` — 0 warnings, 0 errors.
- Поиск `_audioMixer`/`content.AudioMixer` в story projects и Content SDK —
  ссылок нет; осталась только application-level ссылка.
- Поиск старых `DefaultHairStyle`, `DefaultHairColor`,
  `IsRuntimeDefaultAsset` — совпадений нет.
- Полная Unity-компиляция отложена: открытый TZM Editor не перечитал внешний
  package, запуск второго Unity запрещён координационным протоколом.
- Scoped `git diff --check` для `StoryAssetOrderWindow.cs` — успешно.
- Unity 6000.3.11f1 в открытом TZM Editor пересобрал
  `Novels.Content.dll` и `Novels.ContentSdk.Editor.dll` с новым Inspector без
  C#-ошибок.
- Дополнительная компиляция Editor assembly через актуальный Unity Bee rsp —
  успешно.
- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- Scoped `git diff --check` для нового authoring-блока — успешно.
- Финальный `Novels.ContentSdk.Editor` с единым Inspector скомпилирован Unity
  Roslyn по актуальному Bee rsp — без ошибок и предупреждений.
- Поиск старого класса и menu paths в Editor C# — совпадений нет.
- `Novels.ContentSdk.Editor` после оптимизации Inspector скомпилирован Unity
  6000.3.11f1 Roslyn по актуальному Bee rsp — без ошибок и предупреждений.
- Статическая проверка TZM: 44 чанка и 1027 GUID-строк больше не участвуют в
  полном AssetDatabase/filesystem-обходе при свёрнутом разделе.
- Проверка trailing whitespace изменённого Editor-файла — успешно. Общий
  `git diff --check` по worktree по-прежнему видит ранее существующие пробелы
  Unity YAML в `tzm.asset`; они не относятся к этому атомарному блоку.

## Требуется при интеграции

- Ручной Inspector/PlayMode smoke для дефолтной внешности Салли и Алексы.
- После обновления или перезапуска открытого Editor проверить компиляцию
  `Novels.Character`, `Novels.ContentSdk.Editor` и основного `Novels` runtime.
- Основной `Novels` Editor не запускался, пока открыт TZM Editor; нужен
  обычный compile/PlayMode smoke application-level mixer wiring.
- В Inspector `tzm.asset` нажать `Рассчитать по Ink`, просмотреть границы и
  сохранить полученную GUID-разметку перед следующей content build.
- После генерации выполнить `Проверить разметку`, затем собрать TZM и сверить,
  что manifest содержит отдельные bundles/media groups согласно Inspector.
- Открытый TZM Editor не выполнил автоматический package refresh после
  внешнего изменения; перед визуальной проверкой нужен обычный `Assets/Refresh`
  или повторное открытие проекта.
- После refresh проверить отзывчивость Inspector при закрытом разделе, затем
  раскрыть крупнейший чанк и перелистнуть его страницы.
