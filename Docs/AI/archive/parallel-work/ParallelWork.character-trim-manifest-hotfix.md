# Parallel work: character-trim-manifest-hotfix

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: `main`
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: исправление конкурентной загрузки trim-manifest
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterSpriteSetLoader.cs`
- собственные coordination files

## Не изменять

- контентные проекты и manifests
- Catalog и Game runtime
- остальные файлы Content SDK

## Изменённые контракты

- Нет.

## Выполнено

- Trim-manifest кешируется через многократно ожидаемый `AsyncLazy`.
- Manifest ожидается до запуска параллельной загрузки спрайтов.
- `GetSprite` больше не ожидает общий незавершённый `UniTask`.

## Проверено

- Scoped `git diff --check` — успешно.
- В loader больше нет `Preserve` и ожидания manifest внутри `GetSprite`.
- Отдельный Unity compile не запускался: проект открыт пользователем.

## Требуется при интеграции

- Повторно запустить TZM s01e01 в Play Mode.
