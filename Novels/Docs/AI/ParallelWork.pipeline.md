# Parallel work: shared pipeline

- Статус: integrated
- Ветка: `grandChange`
- Базовый commit: `c6c7853b`
- Ответственный поток: упрощение общего Content SDK, validation/build pipeline и CLI
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/**`
- `Tools/novels-tools/**`
- `Novels/Docs/AI/ContentPipeline.md`
- минимальные изменения общих конфигурационных контрактов в
  `Projects/novels-*/Config/**`

## Не изменять

- внутренний authoring-контент `Projects/novels-tzm/**`;
- внутренний authoring-контент `Projects/novels-zdm/**`;
- внешний вид и контент Catalog;
- Game runtime вне явно согласованных интеграционных изменений.

## Изменённые контракты

- `minimumClientVersion` находится в `card.json` или `catalog.json`.
- `Config/build.json` больше не используется.
- AssetBundle labels больше не используются.
- Сборка принимает ровно одну цель: `editor`, `android` или `ios`.
- Compose выполняется автоматически после `build`.

## Выполнено

- `AtomicContentBuild` сокращён до Unity/CLI entry point.
- Добавлены отдельные `ContentPipeline`, `ContentValidator` и
  `ValidationReport`.
- Bundle создаётся явным `AssetBundleBuild` из `Assets/RemoteAssets`.
- CLI сокращён до `doctor`, `validate`, `build`, `publish`.
- Обновлена документация pipeline и шаблон атомарного проекта.
- Валидация разделена на инспектор атомарного проекта и небольшие правила:
  структура, конфигурация, story, catalog и bundle.
- Выбор платформы выделен из pipeline; значение `all` внутри одного Unity-
  запуска удалено, каждая сборка создаёт ровно одну платформу.

## Проверено

- `novels-content doctor` — успешно.
- `novels-content validate catalog` — успешно.
- `novels-content validate tzm` — успешно.
- `novels-content validate zdm` — успешно.
- `novels-content build catalog editor` — успешно.
- `git diff --check` — успешно.
- После выделения validation rules: `zsh -n`, `doctor` и `git diff --check` —
  успешно.

## Требуется при интеграции

- Сопоставить изменения соседних статус-файлов с общими контрактами.
- Не восстанавливать удалённые `Config/build.json` и AssetBundle labels.
- Повторно выполнить последовательную валидацию после завершения соседних
  рефакторингов.
- Полные сборки TZM/ZDM и мобильных платформ запускать только после сведения
  всех потоков и строго последовательно.
- Повторить Unity-компиляцию и `build catalog editor` после закрытия уже
  открытого Editor основного Game-проекта. Второй Unity намеренно не запускался.

## Итоговая интеграция

- Общий SDK с validation rules скомпилирован во всех атомарных проектах.
- `validate catalog`, `validate tzm`, `validate zdm` — успешно.
- `build all editor`, `build all android`, `build all ios` — успешно и строго
  последовательно.
- Исправлена очистка output: новая платформа пересоздаёт только свой каталог и
  сохраняет releases остальных платформ.
- В каждом проекте и общей композиции одновременно подтверждены Mac, Android и
  iOS release.
- Catalog bundle audit прошёл на всех трёх платформах: 6,4–6,5 КиБ.
- Итоговая композиция занимает около 1,6 ГБ и превышает серверный лимит 1 ГБ.

## Известные особенности рабочего дерева

- `.DS_Store` изменён посторонним процессом и не относится к pipeline.
- Unity при проверке каталога пересериализовал его prefab и обновил README;
  перед интеграцией эти изменения необходимо сопоставить со статусом потока
  Catalog, а не автоматически откатывать или приписывать pipeline.
