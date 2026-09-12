# Architecture memory

## Runtime flow

```text
EntryPoint
  -> ApplicationRuntime
      -> CatalogFlow
      -> NovelRuntime
          -> Packages/NovelsStoryRuntime
              -> EpisodeRuntime / EpisodeScope
              -> NovelProcess
              -> StoryOperationExecutor
          -> StoryProcessor + StoryCommands
          -> StoryQueueBuilder
```

`ApplicationRuntime` владеет каталогом и одной активной историей.
`NovelRuntime` загружает definition/save и композирует эпизод.
`EpisodeScope` ограничивает lifetime UI, media и story operations.
Core assembly `Novels.StoryRuntime` не зависит от каталога, гардероба, audio/video,
content delivery, persistence backend или конкретной UI-презентации; эти
возможности композирует host player.
Отдельная `Novels.StoryPresentation` в том же пакете содержит общие очереди,
операции Bubble/выбора и ReplayValidator; она зависит от экранов SDK.
Audio передаётся callback хоста, web FlushCheckpoint ждёт commit IndexedDB.

В пакете также находится отдельная `Novels.Save`: общий decision-save codec и
replay state, без зависимости от Content SDK. Native host сохраняет файловый
backend; WebDecisionSaveSession использует IndexedDB и асинхронный durability
barrier. Формат v3 и чтение v2 сохранены, включая legacy wardrobe data.
Отдельная `Novels.Progress` переиспользует `NovelProgress` для unlock и Ink
entry state. Native NPR1/Cache сохранены; веб использует строгий callback backend
и атомарный IndexedDB envelope для completion markers и следующего entry state.
Content SDK имеет opt-in профили `NOVELS_MEDIA_FREE` и
`NOVELS_STORY_PLAYER_ONLY`: веб использует общие экраны без audio/video execution,
каталога и content-authoring Editor assembly; native/story authoring не меняются.
Подробности: [UnityProjectContext](../architecture/UnityProjectContext.md).

## Границы

- Game зависит от Content SDK, но SDK не зависит от Game или конкретной истории.
- Истории не зависят друг от друга. Catalog сначала читает обложки, `card.json`
  и generated `catalog-preview.json` из NovelDefinition, затем последовательно
  подготавливает story delivery groups в фоне. До готовности запуск закрыт;
  очередь и pinned release живут при переходе в чтение. Общие ресурсы истории
  пока готовят вместе для всех её эпизодов. См. ContentPipeline и Catalog README.
- Эпизод может иметь `_catalogCover` из `Config/EpisodeCovers`; build включает
  его в лёгкий platform preview. Нет своей картинки — используется обложка
  истории. Контракт и импорт: ContentPipeline, раздел «Обложки эпизодов каталога».
- Optional `_catalogVideo` истории/эпизода экспортируется в preview из
  `Config/CatalogVideos`. Один каталоговый плеер играет только основную видимую
  карточку с audio fade-in; при подготовке/ошибке остаётся картинка.
  Приоритет: видео эпизода → картинка эпизода → видео истории → картинка истории.
  Контракт: ContentPipeline, раздел «Видео на карточках каталога».
- `IContentSource` — инфраструктурная граница доставки: filesystem для Editor,
  HTTP(S) для Player; разбор release, SHA-256 и cache остаются общими.
- Runtime читает опубликованные releases/bundles, а не authoring assets через
  `AssetDatabase`.
- Один атомарный project производит один bundle на выбранную платформу.
- Попап настроек — authored-часть Catalog prefab с нейтральным fallback-стилем.
  Game владеет общей громкостью/сохранением и передаёт `ICatalogSettings`;
  Catalog владеет только UI и конфигурируемыми ссылками. См. Catalog README.
- Полоса эпизода — необязательная оценка одного пути по сохранённым решениям,
  не основание для unlock/completion. Version/hash-bound sidecar не меняет saves.
  Расчёт, неизвестные значения и сброс: [Catalog README](../../../Projects/novels-catalog/README.md#прогресс-чтения-эпизода).

## Контент

- Catalog: `Config/catalog.json`; порядок задаёт массив `stories`.
- Story: `Config/card.json`; marker одновременно определяет тип проекта.
- Автор: optional `card.json.author`, override эпизода `_author`; обе подписи
  пустые — byline скрыт, название не меняется. См. ContentPipeline.
- Content pipeline принимает `editor`, `windows`, `android`, `ios`, `webgl`.
  WebGL использует LZ4, `Remote/WebGL` и исключает внешние audio/video payloads;
  фактическая сборка требует WebGL Build Support. См. ContentPipeline.
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
