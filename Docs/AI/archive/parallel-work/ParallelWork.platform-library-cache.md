# Parallel work: platform Library cache

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: main
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: основной чат Novels
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Tools/novels-tools/novels-content`
- `Tools/novels-tools/README.md`
- `Docs/AI/guides/ContentPipeline.md`
- собственные coordination-файлы

## Не изменять

- Content SDK, runtime, контентные assets и import settings.

## Изменённые контракты

- `novels-content build` сохраняет независимый Unity `Library` для каждой
  платформы внутри игнорируемого `Build/UnityLibraryCache` проекта.

## Выполнено

- Добавлено переключение постоянных `Library`-кэшей по платформе.
- Открытый Unity-проект блокирует перемещение кэша с понятной ошибкой.
- Исходники и import settings не копируются и не меняются.

## Проверено

- `zsh -n Tools/novels-tools/novels-content` — успешно.
- `novels-content doctor` — успешно.
- Catalog Android → iOS → Android — три сборки успешны.
- Повторный Android активировал готовый кэш и не запускал TextureImporter.
- `git diff --check` — успешно.

## Требуется при интеграции

- Первая сборка каждой платформы остаётся холодной; последующие используют
  собственный прогретый кэш.
