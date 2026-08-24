# Parallel work: bundle audit

- Статус: ready-for-integration
- Ветка: `grandChange`
- Базовый commit: `c6c7853b`
- Ответственный поток: перенос контроля размера и состава в общий Content SDK
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs`
- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs.meta`
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Projects/novels-catalog/Assets/Editor/**`
- `Projects/novels-catalog/README.md`
- `Novels/Docs/AI/ParallelWork.bundle-audit.md`

## Не изменять

- остальные `Packages/**`;
- `Tools/**`;
- `Projects/novels-tzm/**`;
- `Projects/novels-zdm/**`;
- Game runtime;
- JSON-контракты и `release.json`.

## Ожидаемый контракт

- После каждой bundle-сборки общий pipeline проверяет фактический размер файла
  и состав явных root assets.
- Для Catalog действуют бюджеты: цель 50 КиБ, предупреждение 100 КиБ, ошибка
  500 КиБ.
- Catalog содержит только assets из `Assets/RemoteAssets/catalog`.
- Story-проекты не получают Catalog-specific hardcode и сохраняют текущую
  сборку без нового лимита.

## Атомарные блоки

1. Общий audit API.
2. Подключение после сборки bundle.
3. Удаление локального Catalog audit.
4. Catalog validation/build и проверка отчёта.

## Выполнено

- Scope объявлен, runtime write-lock получен.
- Добавлен общий `ContentBundleAudit`.
- Audit вызывается после создания bundle и до записи `release.json`.
- Для Catalog перенесены прежние бюджеты и проверка внешних зависимостей.
- Локальный `Projects/novels-catalog/Assets/Editor` удалён.
- README больше не требует отдельного Unity-меню.

## Проверено

- `novels-content doctor` — успешно.
- `novels-content validate catalog` — успешно, Unity-компиляция прошла.
- `novels-content build catalog editor` — успешно.
- Общий audit вызван автоматически и завершился сообщением
  `Content bundle audit passed`.
- Catalog bundle — 6606 байт (6,5 КиБ), целевой бюджет 50 КиБ соблюдён.
- В `Projects/novels-catalog/Assets` больше нет C#-файлов.
- `git diff --check` — успешно для всех изменённых tracked-файлов; новый
  `ContentBundleAudit.cs` проверен через `git diff --no-index --check`.

## Требуется при интеграции

- Повторить `validate catalog` и `build catalog editor`.
- Убедиться, что Catalog не содержит локального C#.
- При интеграции не отделять удаление локального audit от подключения общего:
  эти два изменения образуют один миграционный блок.
