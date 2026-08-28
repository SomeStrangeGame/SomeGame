# Parallel work: episode schema dedup

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: Episode runtime schema and Ink authoring
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Novels/Assets/Novels/NovelRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.NovelPreparation.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/ParallelWork.episode-schema-dedup.md`

## Не изменять

- исходные Ink и скомпилированные Ink/source-map артефакты
- остальные content assets TZM/ZDM
- Catalog
- чужие status-файлы

## Изменённые контракты

- `EpisodeDefinition.SourcePath` и сериализованное `_sourcePath` удаляются:
  физическое имя исходника уже хранится в source map и не используется runtime.
- Editor извлекает стабильный episode ID `sXXeXX` из имени INCLUDE-файла,
  включая префиксы вроде `ZDMs01e01.ink`.
- `StoryPath` переносится из каждого `EpisodeDefinition` на единый уровень
  `NovelDefinition` и `NovelContentAsset`.

## Атомарные блоки

1. Упрощение runtime Episode schema.
2. Поддержка prefixed episode filenames в authoring.
3. Точечная миграция definitions TZM/ZDM.
4. Scoped compilation и статическая проверка данных.
5. Перенос общего StoryPath из Episodes на уровень истории.

## Выполнено

- `_sourcePath` удалён из `EpisodeEntry` и `EpisodeDefinition`; runtime API и
  сериализованные definitions больше не дублируют имя исходного Ink-файла.
- `StoryInkAuthoring` извлекает нормализованный ID `sXXeXX` из любого места в
  имени INCLUDE-файла и отклоняет неоднозначные/дублирующиеся ID.
- Definitions TZM/ZDM точечно очищены от прежних `_sourcePath`; остальные
  episode metadata и авторская разметка чанков сохранены.
- Source map не менялась: физические имена 7 TZM и 11 ZDM Ink-файлов остаются
  доступными для диагностики и будущей аналитики.
- `_storyPath` удалён из `EpisodeEntry` и `EpisodeDefinition` и хранится один
  раз в `NovelContentAsset` / `NovelDefinition`; загрузчик и validator читают
  его с уровня истории.
- Definitions TZM/ZDM содержат ровно один story path: `tzm.ink.json` и
  `zdm.ink.json`; в элементах `_episodes` поле отсутствует.

## Проверено

- `dotnet build Novels/Novels.Content.csproj --no-restore` — 0 warnings,
  0 errors.
- Unity 6000.3.11f1 Roslyn по `Novels.Content.rsp` — успешно.
- Unity 6000.3.11f1 Roslyn по TZM `Novels.ContentSdk.Editor.rsp` — успешно.
- Unity 6000.3.11f1 Roslyn по `Novels.rsp` с актуальной reference assembly
  Content — успешно.
- Поиск `_sourcePath|SourcePath` в изменённом Content/Editor-контракте и двух
  definitions — совпадений нет.
- Поиск `_episode.StoryPath|episode.StoryPath` — совпадений нет; в каждом
  definition найден ровно один корневой `_storyPath`, вложенных — 0.
- ID definitions сохранены: TZM 7, ZDM 11; source-map содержит физические
  имена тех же 7/11 файлов.
- Scoped whitespace check C# и ZDM definition — успешно. Общий diff TZM
  сохраняет ранее существующие trailing spaces Unity YAML вне этого блока.

## Требуется при интеграции

- После package refresh нажать `Обновить эпизоды` в ZDM и подтвердить, что
  prefixed filenames формируют `s01e01`…`s02e01` без изменения version и
  silent-audio metadata.
- Выполнить визуальный Inspector smoke после package refresh; пересборка
  контентных бандлов для одной лишь проверки схемы не выполнялась.
