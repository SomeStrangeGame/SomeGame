# Parallel work: catalog-carousel

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: `main`
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: горизонтальная карусель Catalog
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Catalog/View/**`
- `Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab`
- `Projects/novels-catalog/README.md`
- собственные coordination files

## Не изменять

- Catalog registry и pipeline
- проекты TZM и ZDM
- остальной Game runtime

## Атомарные блоки

1. Горизонтальный layout и адаптивные боковые отступы.
2. Snap ближайшей карточки к центру.
3. Масштаб и прозрачность относительно центра.
4. Первый click соседней карточки центрирует её, click центральной открывает.
5. Compile, prefab validation, Catalog build и ручной smoke checklist.

## Выполнено

- `StoryList` переведён на горизонтальный layout; первая и последняя карточки
  центрируются адаптивными боковыми отступами.
- Добавлен один `CatalogCarousel`: drag, snap, масштаб и прозрачность карточек.
- Нажатие соседней карточки центрирует её; нажатие центральной открывает
  доступную историю.
- README описывает поведение, настройки и ручную проверку устройств/размеров.

## Проверено

- Unity import/validation Catalog — успешно, missing scripts/references нет.
- `novels-content build catalog editor` — успешно; bundle audit 6,7 КиБ.
- Unity batch compile `Novels` — успешно, C#-ошибок нет.
- GUID нового компонента и все prefab file ID проверены статически.
- Scoped `git diff --check` — успешно.

## Требуется при интеграции

- Вручную пройти карусель мышью и touch на узком телефоне и широком планшете.
- Автоматическая проверка не подтверждает визуальную плавность и удобство
  реального свайпа.
