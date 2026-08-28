# Parallel work: story episode defaults

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: перенос общих episode settings на уровень истории
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Novels/Assets/Novels/NovelRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.NovelPreparation.cs`
- `Novels/Assets/Novels/NovelRuntime.EpisodeComposition.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/archive/parallel-work/ParallelWork.story-episode-defaults.md`

## Не изменять

- исходные и скомпилированные Ink/source-map файлы
- authoring-разметку чанков и остальные story metadata
- Catalog
- чужие status-файлы

## Изменённые контракты

- `EndMarker` и `SilentAudioIds` перенесены из каждого `EpisodeDefinition` на
  единый уровень `NovelDefinition`.
- `EpisodeMediaDefinition` удалён как одно поле-обёртка без самостоятельного
  episode-level контракта.

## Выполнено

- `NovelContentAsset` хранит `_endMarker` и `_silentAudioIds` рядом с общей
  версией истории; `EpisodeEntry` оставляет только ID и title.
- Inspector показывает обе настройки один раз в разделе данных истории.
- Обновление Episodes больше не вычисляет marker и не копирует silent IDs.
- Runtime media scope и проверка конца эпизода читают общие настройки истории.
- Playable definition сохраняет общие настройки при фильтрации Episodes.
- Definitions TZM/ZDM мигрированы без изменения ID/title и разметки чанков.

## Проверено

- Все runtime/editor потребители найдены и переведены на `NovelDefinition`.
- Ссылок на `EpisodeMediaDefinition`, `_episode.Media`, `_episode.EndMarker` и
  episode-level SerializedProperty больше нет.
- TZM содержит 7 Episodes, ZDM — 11; в каждом asset ровно один story-level
  marker и один список silent IDs, вложенных копий нет.
- Scoped `git diff --check` для C# и ZDM asset — успешно.

## Требуется при интеграции

- Unity compile/refresh не запускался: открытый TZM Editor PID 97689 владеет
  Unity-ресурсом; второй тяжёлый процесс запрещён проектным протоколом.
- После refresh пересобрать content bundles, поскольку сериализуемая схема
  story definition изменилась.
