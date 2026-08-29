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
- `Choose` и `Wardrobe` — разные фичи, хотя переиспользуют option-list UI.

Подробности: [ProjectOverview.md](../architecture/ProjectOverview.md),
[MultiProjectSplitPlan.md](../architecture/MultiProjectSplitPlan.md) и
[ContentPipeline.md](../guides/ContentPipeline.md).
