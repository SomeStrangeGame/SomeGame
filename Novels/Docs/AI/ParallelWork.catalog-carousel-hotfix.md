# Parallel work: catalog-carousel-hotfix

- Статус: ready-for-integration
- Ветка: `main`
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: исправление CanvasGroup карточки Catalog
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Catalog/View/Card.cs`
- собственные coordination files

## Не изменять

- Catalog prefab и registry
- проекты TZM и ZDM
- остальной Game runtime

## Изменённые контракты

- Нет.

## Выполнено

- Исправлена Unity-null проверка `CanvasGroup`: компонент сначала ищется,
  затем при фактическом отсутствии добавляется на карточку.

## Проверено

- Scoped `git diff --check` — успешно.
- Unity batch compile не запущен: проект уже открыт в Unity Editor.

## Требуется при интеграции

- Проверить открытие каталога в Play Mode.
