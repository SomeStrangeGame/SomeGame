# Parallel work: player-build-matrix

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: main
- Базовый commit: 8f7082db
- Ответственный поток: автоматическая версия и матрица Player-сборок
- Последнее обновление: 2026-08-26

## Разрешённая область

- `Novels/Assets/Editor/PlayerBuildAutomation.cs`
- `Novels/Tools/build-player*.sh`
- `Novels/Tools/build-remote-player.sh`
- `Novels/Tools/build-embedded-test-player.sh`
- `Docs/AI/archive/parallel-work/ParallelWork.player-build-matrix.md`
- собственные записи `CoordinationRuntime`

## Не изменять

- `.gitignore`
- Catalog runtime и контентные ассеты
- чужие координационные файлы

## Изменённые контракты

- Единый автоматический build identity и одинаковая матрица Remote/Embedded для Android, iOS, Windows и macOS.

## Выполнено

- Добавлена общая команда `build-player.sh` для четырёх платформ и двух способов доставки.
- Добавлена полная команда `build-player-matrix.sh`.
- Версия имеет формат UTC `YYYY.MM.DD`, build number — число минут с 2020-01-01 UTC.
- Артефакты складываются в `Build/Players/<version>/<build>/<platform>/<mode>`.
- Старые Remote/Embedded команды сохранены как короткие совместимые оболочки.

## Проверено

- `zsh -n` для четырёх build-скриптов — успешно.
- Unity 6000.3.11f1 batch compile — `Tundra build success`, exit code 0.
- `git diff --check` — успешно.
- `.gitignore` — без изменений.

## Требуется при интеграции

- Для полной матрицы установить Android, iOS и Windows Build Support; локально найден только MacStandaloneSupport.
