# Project Overview

## Поток запуска

```text
EntryPoint
  -> ApplicationRuntime
      -> BootstrapController
      -> CatalogFlow -> CatalogController
      -> NovelRuntime
          -> подготовка story/episode content
          -> создание экранов эпизода
          -> StoryProcessor + StoryCommands
          -> StoryQueueBuilder -> StoryOperationExecutor
```

`EntryPoint` получает сериализованные ссылки сцены и создаёт неизменяемое окружение. `ApplicationRuntime` загружает каталог, предлагает выбрать историю и владеет только одним активным `NovelRuntime`. `NovelRuntime` выбирает эпизод, создаёт `EpisodeScope` и передаёт каждой фиче episode cancellation token.

## Порядок чтения кода

Для знакомства с runtime достаточно идти по этому списку:

1. `EntryPoint.cs` — Unity lifecycle и composition root.
2. `ApplicationRuntime.cs` — каталог и lifetime выбранной истории.
3. `CatalogFlow.cs` — загрузка и выбор истории/эпизода.
4. `NovelRuntime.cs` и его partial-файлы — подготовка и фабрика эпизода.
5. `EpisodeRuntime.cs` — cancellation и завершение эпизода.
6. `StoryQueue/StoryQueueBuilder.cs` — перевод шагов Ink в операции.
7. `StoryExecution/StoryOperationExecutor.cs` — последовательное выполнение.

`ReplayValidator.cs` проверяет совместимость сохранения отдельно от запуска
эпизода. Детали отдельных UI-фичей нужны только при изменении конкретной фичи.

## Основные каталоги

| Путь | Ответственность |
|---|---|
| `Assets/Novels` | runtime и композиция приложения |
| `Assets/Novels/<Feature>` | отдельная фича и её UI |
| `Assets/Editor` | импорт, анализ Ink, валидация и сборка контента |
| `Assets/RemoteAssets/Content/<id>` | authoring assets истории |
| `Assets/StreamingAssets` | локальные Ink/media и собранный embedded content |
| `Packages/NovelInk` | контракты, парсер команд, Ink runtime и source map |
| `Packages/Bundles` | загрузка release, bundles, файлов и cache |

## Владение

- `ApplicationRuntime`: каталог, общая bundle-сессия, выбор истории.
- `NovelRuntime`: выбранная definition, прогресс, save system и композиция эпизода.
- `EpisodeRuntime`/`EpisodeScope`: экраны, медиа, операции и cancellation одного эпизода.
- `EpisodePresentation`: все UI- и media-контроллеры текущего эпизода.
- Feature controller: presentation state конкретной фичи.
- Feature screen: только Unity/uGUI-представление.
- `Bundles`: получение, проверка целостности, кеширование и lifetime bundle.

## Исполнение истории

1. `StoryProcessor` читает следующий Ink-фрагмент и варианты ответа.
2. `StoryCommands` преобразует авторский синтаксис в типизированный `StoryStep`.
3. `StoryQueueBuilder` накапливает команды до готовой реплики.
4. Команды становятся `IStoryOperation`: фон, персонаж, звук, ожидание, bubble или notification.
5. `StoryOperationExecutor` выполняет операции последовательно.
6. Решение игрока записывается `SaveSystem`; replay использует те же операции с `PresentationMode.Immediate`.

Композиция `NovelRuntime` сгруппирована по трём областям: `Content` загружает definition и сохранение, `Presentation` создаёт единый `EpisodePresentation`, `StoryQueue` связывает процессор истории с операциями напрямую, без промежуточных delegate-port структур.

## Сборки

Один asmdef соответствует самостоятельной фиче или реальной границе зависимостей. View-код и простые DTO-контракты входят в assembly своей фичи. `Choose` и `Wardrobe` сохраняют отдельные assemblies и контроллеры, поскольку будут развиваться независимо, но используют общие `OptionListController`, `OptionListScreen` и системный prefab его статической разметки. Очередь и исполнение истории входят в основную `Novels` assembly; простые действия представлены `DelegateStoryOperation`, отдельные классы остаются только у операций с собственным состоянием.

Content SDK validation выполняет один очевидный маршрут: инспекция проекта,
проверка структуры, Story или Catalog, затем bundle. Отдельных одноразовых
rule-классов и скрытого списка правил нет.
