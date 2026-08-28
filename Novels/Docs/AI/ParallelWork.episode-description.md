# Parallel work: episode description

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: описание эпизодов из Ink для каталога выбора
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Novels/Assets/Novels/CatalogFlow.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/ParallelWork.episode-description.md`

## Не изменять

- исходные и скомпилированные Ink/source-map файлы
- authoring-разметку чанков и остальные story metadata
- Catalog content project/prefab
- чужие status-файлы

## Изменённые контракты

- Добавлен `EpisodeDefinition.Description`, извлекаемый из `Описание:` с
  fallback на `Аннотация:`.

## Выполнено

- `EpisodeEntry` и `EpisodeDefinition` хранят описание.
- `StoryInkAuthoring` при обновлении Episodes ищет `Описание:`, затем
  `Аннотация:`; префикс и пробелы не попадают в значение.
- `CatalogFlow.SelectEpisode` передаёт description существующей карточке.
- Все 18 descriptions мигрированы в definitions TZM/ZDM; ID, title и прочие
  поля не менялись.

## Проверено

- Все 18 episode Ink содержат `Описание:` или fallback `Аннотация:`.
- Автоматическая сверка множества извлечённых Ink-строк с 18 asset values —
  полное совпадение.
- TZM содержит 7 `_description`, ZDM — 11.
- Статический поиск constructor/UI consumers и scoped `git diff --check` C# и
  ZDM asset — успешно.

## Требуется при интеграции

- Unity compile/refresh не запускался: открытый TZM Editor PID 97689 владеет
  Unity-ресурсом; второй тяжёлый процесс запрещён проектным протоколом.
- После refresh пересобрать content bundles, поскольку сериализуемая схема
  episode definition изменилась.
