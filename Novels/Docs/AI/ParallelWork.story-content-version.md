# Parallel work: story content version

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: Story-level content version migration
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Novels/Assets/Novels/NovelRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.Content.cs`
- `Novels/Assets/Novels/NovelProgress.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/ParallelWork.story-content-version.md`
- собственные файлы в `Novels/Docs/AI/CoordinationRuntime/`

## Не изменять

- формат `SaveDataCodec`
- исходные и скомпилированные Ink/source-map артефакты
- разметку чанков и остальные content assets TZM/ZDM
- Catalog
- чужие status-файлы

## Изменённые контракты

- `ContentVersion` переносится из `EpisodeDefinition` на единый уровень
  `NovelDefinition` и `NovelContentAsset`.
- Сохранения эпизода продолжают хранить тот же строковый version envelope, но
  получают общую версию истории.
- Progress принимает прежний агрегированный version envelope при первом чтении
  и далее записывает общую версию истории.

## Атомарные блоки

1. Runtime Content schema.
2. Save/progress consumers с совместимостью существующего прогресса.
3. Authoring и Inspector.
4. Миграция TZM/ZDM.
5. Scoped compilation и handoff.

## Выполнено

- `_contentVersion` удалён из `EpisodeEntry` и `EpisodeDefinition`; единая
  версия добавлена в `NovelContentAsset` и `NovelDefinition`.
- Episode save envelope получает `_definition.ContentVersion`; идентификатор
  сохранения по-прежнему включает ID эпизода.
- `NovelProgress` записывает общую версию и принимает старую агрегированную
  строку `episodeId:version` для плавной миграции текущих сохранений.
- `StoryInkAuthoring` больше не генерирует и не сохраняет версии эпизодов;
  Inspector показывает одно поле `Версия истории`.
- TZM/ZDM мигрированы на одну корневую `_contentVersion: 1` без изменения
  Episodes, Ink, source-map или разметки чанков.

## Проверено

- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- Unity 6000.3.11f1 Roslyn: `Novels.Content`, TZM
  `Novels.ContentSdk.Editor` и основной `Novels` — успешно.
- В TZM найдено 7 Episodes, в ZDM 11; в каждом asset ровно одна корневая
  `_contentVersion`, вложенных — 0.
- В прежних TZM/ZDM definitions все episode versions были одинаковыми (`1`),
  поэтому legacy progress envelope воспроизводится без потери значения.
- Scoped `git diff --check` — успешно. Общий check сохраняет только прежние
  trailing spaces Unity YAML в TZM вне изменённых строк этого блока.

## Требуется при интеграции

- Package refresh и визуальный smoke двух story definitions.
- Пересобрать content bundles перед следующим запуском тестового runtime:
  сериализуемая схема definition изменилась.
