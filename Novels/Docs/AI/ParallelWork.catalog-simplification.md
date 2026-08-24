# Parallel work: catalog-simplification

- Статус: ready-for-integration
- Ветка: `main`
- Базовый commit: `0477677f4c1196737dddf2594594e8329d4c563e`
- Ответственный поток: упрощение Catalog без изменений prefab
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/CatalogContracts/CatalogContracts.cs`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Novels/Assets/Novels/CatalogFlow.cs`
- `Projects/novels-catalog/Config/catalog.json`
- `Projects/novels-catalog/Packages/manifest.json`
- `Projects/novels-catalog/Packages/packages-lock.json`
- `Projects/novels-catalog/README.md`
- `Novels/Docs/AI/ParallelRefactoringCoordination.md` — только фиксация нового
  registry-контракта
- собственные coordination files

## Не изменять

- `Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab`
- проекты TZM и ZDM
- остальной Game runtime и Content SDK

## Атомарные блоки

1. Заменить registry entries на упорядоченный массив `storyId` и согласованно
   обновить codec, validation и единственного runtime-потребителя.
2. Удалять только доказанно лишние прямые зависимости Catalog с проверкой
   разрешения пакетов после каждого шага.
3. Переписать README как два коротких пользовательских сценария.

## Проверка

- `novels-content validate catalog`
- `novels-content build catalog editor`
- Unity-компиляция затронутых runtime/editor assemblies
- `git diff --check`

## Выполнено

- Registry schema 2 хранит только упорядоченный массив `storyId`.
- Runtime использует порядок массива; `order` и `enabled` удалены.
- Из manifest удалён неиспользуемый `com.unity.2d.sprite`; обязательность
  JSON-модуля подтверждена отдельной отрицательной проверкой.
- README разделён на сценарии изменения списка и внешнего вида.
- Prefab не изменялся.

## Проверено

- `novels-content validate catalog` — успешно.
- Unity batch compile проекта `Novels` — успешно, C#-ошибок нет.
- `novels-content build catalog editor` — успешно; bundle audit пройден.
- Собранный registry содержит schema 2 и `stories: ["tzm", "zdm"]`.
- `git diff --check` для файлов scope — успешно.

## Требуется при интеграции

- Публиковать schema-2 registry одновременно с клиентом, который содержит
  обновлённый `CatalogFlow`.
- Play Mode не запускался; prefab и визуальный layout не менялись.
