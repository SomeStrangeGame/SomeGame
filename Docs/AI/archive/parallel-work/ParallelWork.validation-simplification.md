# Parallel work: validation simplification

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: main
- Базовый commit: `42e57290434e151ea8add0b330eb10394fad3d10`
- Ответственный поток: основной чат Novels
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ContentValidation.cs`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs`
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Docs/AI/guides/ManualContentChecklist.md`
- связанные ссылки в документации и собственные coordination-файлы

## Не изменять

- `Projects/novels-catalog/**`
- runtime игры и контентные assets

## Изменённые контракты

- Планируется заменить внутренний `ContentProject` компактным
  `ContentBuildPlan`; публичные release/content форматы не меняются.

## Выполнено

- Удалён неиспользуемый warning-слой `ValidationReport`.
- `ContentProject` заменён компактным `ContentBuildPlan`.
- Тип проекта определяется по `Config/catalog.json` или `Config/card.json`.
- Успешный bundle audit сведён к одной строке; подробные списки остаются для
  ошибки.
- Добавлен ручной чек-лист смысловой и визуальной приёмки.

## Проверено

- Unity batch compile — успешно.
- `novels-content validate all` — успешно для Catalog, TZM и ZDM.
- `novels-content build catalog editor` — успешно; audit выдал одну строку.
- `git diff --check` — успешно после нормализации Unity-generated `.slnx`.

## Требуется при интеграции

- Ручная проверка контента по новому чек-листу остаётся за человеком.
