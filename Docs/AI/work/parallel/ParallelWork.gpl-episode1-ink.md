# Parallel work: gpl-episode1-ink

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c7727
- Ответственный поток: перенос полного текста эпизода 1 в GPL Ink
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Projects/novels-gpl/Assets/Ink/**`
- собственные coordination-записи

## Не изменять

- `Projects/novels-gpl/Assets/Characters/**`
- `Projects/novels-gpl/Assets/Locations/**`
- существующие проекты и общий SDK

## Изменённые контракты

- Нет.

## Выполнено

- Сверены четыре сюжетных выбора и переносимые состояния.
- Полный текст эпизода перенесён в 569 строк канонического Ink.
- Голосовые сущности оставлены репликами рассказчика и не требуют спрайтов.
- Четыре выбора получили стабильные метки и строковые состояния.
- Пересобраны compiled Ink и source map.

## Проверено

- Ink compilation — успешно.
- `Tools/novels-tools/novels-content validate gpl` — успешно.
- `git diff --check -- Projects/novels-gpl/Assets/Ink` — успешно.

## Требуется при интеграции

- Создать отдельный commit после освобождения очереди соседней задачи.
