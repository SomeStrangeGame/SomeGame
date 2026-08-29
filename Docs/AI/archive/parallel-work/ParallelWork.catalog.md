# Parallel work: catalog

- Статус: integrated
- Ветка: `grandChange`
- Базовый commit: `c6c7853b`
- Ответственный поток: упрощение и документирование `novels-catalog`
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Projects/novels-catalog/README.md`
- `Projects/novels-catalog/Assets/Editor/**`
- `Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab`
- `Docs/AI/archive/parallel-work/ParallelWork.catalog.md`

## Не изменять

- `Packages/**`;
- `Tools/**`;
- `Novels/Assets/**`;
- `Projects/novels-content-template/**`;
- `Projects/novels-tzm/**`;
- `Projects/novels-zdm/**`;
- `Projects/novels-catalog/Config/**` — принадлежит текущей миграции shared
  pipeline;
- `Projects/novels-catalog/Assets/RemoteAssets/catalog.meta` — bundle label
  удаляется shared pipeline.

## Изменённые контракты

- Нет.
- Путь `Assets/RemoteAssets/catalog/screen.prefab` сохранён.
- Компоненты и сериализованные имена полей не изменялись.
- Формат `Config/catalog.json` не изменялся потоком Catalog.

## Выполнено

- README переписан как краткая инструкция для новичка.
- Имена объектов prefab уточнены без изменения иерархии и layout:
  `CatalogScreen`, `CatalogContent`, `StoryList`, `TEMPLATE - Story Card`.
- Шаблон карточки выключен непосредственно в prefab; runtime продолжает
  создавать и активировать его копии через сериализованную ссылку.
- Поле `Card._cover` явно связано с уже существующим компонентом `Image`.
- GUID, file ID, путь prefab и runtime-поведение сохранены.
- Добавлен локальный Editor-аудит состава и размера собранного bundle.
- Установлены бюджеты: цель 50 КиБ, предупреждение 100 КиБ, ошибка 500 КиБ.

## Проверено

- `novels-content validate catalog` — успешно в последовательной проверке
  shared pipeline.
- Unity-компиляция проекта Catalog — успешно.
- `novels-content build catalog editor` — успешно в shared pipeline.
- `git diff --check -- Projects/novels-catalog/README.md
  Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab` — успешно.
- Сериализованная ссылка `_cover` указывает на существующий `Image` с file ID
  `8383563251787723355`.
- Текущий Mac bundle — 6598 байт; размер файла совпадает с `release.json` и
  укладывается в целевой бюджет 50 КиБ.
- Статическая проверка `CatalogBundleAudit.cs` и `git diff --check` — успешно.

## Требуется при интеграции

- Сохранить изменения shared pipeline в `Config/catalog.json`, удаление
  `Config/build.json` и удаление AssetBundle label из `catalog.meta`.
- Не приписывать изменения `README.md` и `screen.prefab` потоку shared
  pipeline: они принадлежат потоку Catalog.
- После сведения потоков повторить `validate catalog` и
  `build catalog editor`.
- После освобождения Unity Editor выполнить
  `Novels.Catalog.Editor.CatalogBundleAudit.AuditBuiltContent`; второй Unity
  Editor не запускался из-за активного соседнего проекта `Novels`.
- Проверить, что итоговый bundle по-прежнему содержит один
  `screen.prefab` и не получил зависимости контента конкретных историй.

## Контроль размера

- `CatalogBundleAudit` собирает размер bundle для всех уже собранных платформ
  и перечень зависимостей `screen.prefab`.
- Зависимость от project asset за пределами `Assets/RemoteAssets/catalog`
  считается ошибкой состава каталога.
- Не менять алгоритм сжатия AssetBundle из потока Catalog: общие настройки
  сборки принадлежат shared pipeline.
- Отсутствующая платформа не считается ошибкой: аудит проверяет результаты,
  которые уже существуют в `Build/LocalContent/Remote`.

## Запросы к shared pipeline

- Перенести универсальную проверку размера и состава bundle в
  `ContentPipeline`, после чего удалить локальный `Assets/Editor` из Catalog.
- Полностью объявить зависимости в `package.json` пакетов
  `NovelsContentSdk`, `Bundles`, `NovelInk` и `UITransitions`. Пока custom и
  Unity module dependencies объявлены не полностью, сокращать локальный
  `Packages/manifest.json` небезопасно.
- Если отключение и независимое числовое упорядочивание историй не требуются,
  рассмотреть простой контракт `stories: ["tzm", "zdm"]`. Поток Catalog не
  меняет этот общий JSON-контракт самостоятельно.

## Известные особенности рабочего дерева

- В рабочем дереве одновременно находятся незакоммиченные изменения shared
  pipeline и других потоков.
- Поток Catalog не удалял `Config/build.json`, не менял `catalog.json` и не
  удалял AssetBundle label.
- Коммит потоком Catalog не создавался, чтобы не смешивать параллельные
  изменения.

## Итоговая интеграция

- Общий pipeline повторно выполнил `validate catalog` и сборки Mac, Android,
  iOS.
- Финальный `CatalogBundleAudit` прошёл для всех трёх платформ.
- Размеры bundle: Android 6,4 КиБ, Mac 6,5 КиБ, iOS 6,4 КиБ.
