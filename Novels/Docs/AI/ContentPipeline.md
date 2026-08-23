# Content Pipeline

## Единая команда

Из корня репозитория:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content build-local all
```

Сборка выполняется строго последовательно: Catalog UI, затем каждый найденный
story-project. Новая история обнаруживается автоматически по файлу
`Projects/novels-<storyId>/Config/card.json`.

Можно пересобрать только одну часть:

```bash
Tools/novels-tools/novels-content build-local catalog
Tools/novels-tools/novels-content build-local tzm
Tools/novels-tools/novels-content build-local zdm
```

После каждой сборки CLI автоматически обновляет композицию для Game в
`Novels/Build/LocalContent`. Если bundles уже готовы, композицию можно обновить
без запуска Unity:

```bash
Tools/novels-tools/novels-content compose-local
```

## Что принадлежит проектам

- `novels-catalog` собирает только визуальный Catalog UI bundle и хранит
  центральный `Config/catalog.json` (`storyId`, порядок, enabled).
- Каждый `novels-<storyId>` хранит весь контент одной истории, Ink и
  `Config/card.json` с собственной `Config/cover.*`.
- Каждый контентный проект содержит `Config/build.json`. Поле
  `minimumClientVersion` явно задаёт минимальную совместимую версию Game и не
  связано с `PlayerSettings.bundleVersion` контентного Unity-проекта.
- Каждая история собирается в один большой bundle на платформу. Эпизодных и
  shared bundles нет.
- `novels-game` не содержит authoring-контент историй и не кладёт bundles в
  `StreamingAssets`.

## Локальный и серверный root

```text
LocalContent/
  catalog/
    registry/catalog.json
    ui/Remote/<platform>/release.json
    ui/Remote/<platform>/<bundle>
  stories/<storyId>/
    card.json
    cover.<extension>
    Files/<sha256>.bin
    Remote/<platform>/release.json
    Remote/<platform>/<bundle>
```

Editor читает `Novels/Build/LocalContent` через `FileSystemContentSource`.
Android/iOS читают идентичное дерево через `HttpContentSource`; отдельных
Preview, Embedded и StreamingAssets-режимов нет.

## Локальная публикация

```bash
Tools/novels-tools/novels-content publish-local /absolute/server/root
```

Команда зеркально копирует готовую композицию в указанную папку. Для реального
HTTP-хостинга эта папка является корнем удалённого контента.

## Runtime-порядок

```text
catalog/registry/catalog.json
  -> stories/<id>/card.json
  -> Catalog UI release
  -> выбор истории
  -> stories/<id>/release
  -> проверка SHA-256 и кеш
  -> запуск истории
```

Catalog UI и каждая история имеют независимые release и namespace кеша. Поэтому
историю можно пересобрать и опубликовать атомарно, не пересобирая приложение,
каталог или другие истории.
