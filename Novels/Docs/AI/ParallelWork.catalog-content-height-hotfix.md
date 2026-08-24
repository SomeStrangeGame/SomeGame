# Parallel work: catalog-content-height-hotfix

- Статус: ready-for-integration
- Ветка: `main`
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: исправление высоты content карусели Catalog
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Catalog/View/CatalogCarousel.cs`
- собственные coordination files

## Не изменять

- Catalog prefab и registry
- проекты TZM и ZDM
- остальной Game runtime

## Изменённые контракты

- Нет.

## Выполнено

- Content карусели получает высоту актуального viewport до layout rebuild,
  поэтому `HorizontalLayoutGroup` больше не сжимает карточки по вертикали.

## Проверено

- Scoped `git diff --check` — успешно.
- Отдельный Unity compile не запускался: проект открыт пользователем.

## Требуется при интеграции

- Ручная проверка Catalog в Play Mode.
