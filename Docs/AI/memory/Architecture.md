# Architecture memory

## Runtime flow

```text
EntryPoint
  -> ApplicationRuntime
      -> CatalogFlow
      -> NovelRuntime
          -> EpisodeRuntime / EpisodeScope
          -> StoryProcessor + StoryCommands
          -> StoryQueueBuilder
          -> StoryOperationExecutor
```

`ApplicationRuntime` владеет каталогом и одной активной историей.
`NovelRuntime` загружает definition/save и композирует эпизод.
`EpisodeScope` ограничивает lifetime UI, media и story operations.

## Границы

- Game зависит от Content SDK, но SDK не зависит от Game или конкретной истории.
- Истории не зависят друг от друга; Catalog не знает их внутреннее устройство.
- `IContentSource` — инфраструктурная граница доставки: filesystem для Editor,
  HTTP(S) для Player; разбор release, SHA-256 и cache остаются общими.
- Runtime читает опубликованные releases/bundles, а не authoring assets через
  `AssetDatabase`.
- Один атомарный project производит один bundle на выбранную платформу.

## Контент

- Catalog: `Config/catalog.json`; порядок задаёт массив `stories`.
- Story: `Config/card.json`; marker одновременно определяет тип проекта.
- Поддерживаемые content platforms: `editor`, `android`, `ios`.
- Editor использует `Novels/Build/LocalContent`; Player использует тот же
  release-контракт через целевой content source.
- `Choose` и `Wardrobe` — разные фичи с независимыми fallback prefab и
  story-local prefab variants; они переиспользуют только runtime option-list
  логику, но не общую authored hierarchy.
- Генерация character-арта всегда начинается с цельного согласованного
  персонажа; runtime может отрисовать образ одним PNG или комплектом игровых
  слоёв. ТЗМ штатно использует слоёную runtime-композицию.
- Аргумент Ink `переодеть <одежда>` обновляет состояние одежды персонажа до
  разрешения emotion/pose selector и одинаково работает для whole-вариантов и
  legacy layered assets; выбор игрока для сюжетной смены костюма не требуется.
- Цельный runtime-вариант адресуется как
  `Characters/<name>/view/whole/<outfit>/<variant>.png`; первый Ink-кандидат
  может выбрать authored outfit, следующий — emotion/pose, а missing exact
  возвращает neutral текущего outfit. Legacy layered resolution остаётся
  совместимым.

Подробности: [ProjectOverview.md](../architecture/ProjectOverview.md),
[MultiProjectSplitPlan.md](../architecture/MultiProjectSplitPlan.md) и
[ContentPipeline.md](../guides/ContentPipeline.md).
