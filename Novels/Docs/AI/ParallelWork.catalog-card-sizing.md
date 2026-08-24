# Parallel work: catalog-card-sizing

- Статус: ready-for-integration
- Ветка: `main`
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: адаптивный размер карточек Catalog
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Catalog/View/Card.cs`
- `Packages/NovelsContentSdk/Runtime/Catalog/View/CatalogCarousel.cs`
- собственные coordination files

## Не изменять

- Catalog prefab и registry
- проекты TZM и ZDM
- остальной Game runtime

## Изменённые контракты

- Нет.

## Выполнено

- Перед чтением размера viewport принудительно пересчитывается родительский
  layout.
- Карточки занимают до 80% актуального viewport с сохранением исходных
  пропорций.
- Изменение размера экрана автоматически вызывает повторный расчёт.

## Проверено

- Scoped `git diff --check` — успешно.
- Статическая проверка сериализованных ссылок: новые ссылки не добавлены.
- Unity compile не запускался отдельно, поскольку проект открыт пользователем.

## Требуется при интеграции

- Ручная проверка размера и свайпа Catalog в Play Mode.
