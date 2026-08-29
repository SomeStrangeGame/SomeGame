# Parallel work: story path convention

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: единый Inspector и вывод compiled Ink path из story ID
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Runtime/Content/NovelDefinition.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Novels/Assets/Novels/NovelRuntime.cs`
- `Novels/Assets/Novels/NovelRuntime.NovelPreparation.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Docs/AI/archive/parallel-work/ParallelWork.story-path-convention.md`

## Не изменять

- исходные и скомпилированные Ink/source-map файлы
- разметку чанков и остальные story metadata
- Catalog
- чужие status-файлы

## Изменённые контракты

- Сериализуемый `_storyPath` удалён: compiled Ink адресуется по единому
  соглашению `<story-id>.ink.json`.
- Публичный `NovelDefinition.StoryPath` сохранён как вычисляемое runtime-
  значение, чтобы потребителям не требовалось собирать адрес самостоятельно.

## Выполнено

- Поле `Скомпилированный Ink` удалено из единого Inspector.
- `NovelContentAsset` больше не сериализует и не передаёт story path.
- `NovelDefinition` выводит `StoryPath` из `Id`; оба конструктора упрощены.
- `StoryInkAuthoring` больше не записывает path при обновлении Episodes и
  проверяет соглашение `<story-id>.ink` перед compile/report/update.
- TZM/ZDM definitions очищены от `_storyPath`, остальные данные сохранены.
- Runtime выбора эпизода обновлён под упрощённый constructor.
- Сообщение validation уточнено: проверяется compiled Ink, а не source.

## Проверено

- Все C#-вызовы `NovelDefinition` и потребители `StoryPath` найдены и сверены.
- `_storyPath` и Inspector label `Скомпилированный Ink` в рабочей области
  отсутствуют.
- `tzm.ink`, `tzm.ink.json`, `zdm.ink`, `zdm.ink.json` существуют по новому
  соглашению; GUID корневого TZM Ink сохранён.
- Scoped `git diff --check` для C# и ZDM asset — успешно.
- TZM asset содержит прежние trailing spaces вне изменённой строки; они не
  исправлялись, чтобы не пересериализовывать большой asset.

## Требуется при интеграции

- Unity compile/refresh не запускался: TZM Editor занят PID 97689; второй
  тяжёлый процесс запрещён проектным протоколом.
- После refresh пересобрать content bundles, поскольку сериализуемая схема
  story definition изменилась.
