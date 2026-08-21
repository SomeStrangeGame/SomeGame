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
          -> StoryQueueBuilder
          -> StoryOperationExecutor
```

`EntryPoint` получает сериализованные ссылки сцены и создаёт неизменяемое окружение. `ApplicationRuntime` загружает каталог, предлагает выбрать историю и владеет только одним активным `NovelRuntime`. `NovelRuntime` выбирает эпизод, создаёт `EpisodeScope` и передаёт каждой фиче episode cancellation token.

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
- Feature controller: presentation state конкретной фичи.
- Feature screen: только Unity/uGUI-представление.
- `Bundles`: получение, проверка целостности, кеширование и lifetime bundle.

## Исполнение истории

1. `StoryProcessor` читает следующий Ink-фрагмент и варианты ответа.
2. `StoryCommands` преобразует авторский синтаксис в типизированный `StoryStep`.
3. `StoryQueueBuilder` накапливает команды до готовой реплики.
4. Команды становятся `IStoryOperation`: фон, персонаж, звук, ожидание, bubble или notification.
5. `StoryOperationExecutor` выполняет операции последовательно.
6. Решение игрока записывается `SaveSystem`; replay использует те же операции в immediate-режиме.

## Сборки

Один asmdef соответствует самостоятельной фиче или реальной границе зависимостей. Маленькие View-файлы Bootstrap, Catalog и Notification входят в сборку своей фичи. `Choose` и `Wardrobe` сохраняют отдельные runtime, View и contract assemblies, собственные экраны и собственные View-модели, поскольку будут развиваться независимо.
