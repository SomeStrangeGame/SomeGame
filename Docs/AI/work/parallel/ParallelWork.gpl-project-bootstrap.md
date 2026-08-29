# Parallel work: gpl-project-bootstrap

- Статус: integrated
- Ветка: main
- Базовый commit: 86c2002f
- Ответственный поток: создание проекта истории «Голос подо льдом»
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Projects/novels-gpl/**`
- `Docs/AI/work/parallel/ParallelWork.gpl-project-bootstrap.md`
- собственные записи в `Docs/AI/CoordinationRuntime/**`

## Не изменять

- существующие проекты `Projects/novels-*`
- общие `Packages/**` и `Tools/**`
- посторонние пользовательские изменения рабочего дерева

## Изменённые контракты

- Нет; новый проект использует действующий story-project contract.

## Выполнено

- Создан `Projects/novels-gpl` на чистых настройках и packages актуального TZM.
- Добавлены card/cover, definition, пустые authoring-каталоги и trim manifest.
- Добавлены исходный Ink эпизода 1, compiled Ink и source map.
- Проект автоматически обнаруживается общим CLI без story-specific кода.

## Проверено

- Общий pipeline обнаруживает новые истории по `Config/card.json`.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `Tools/novels-tools/novels-content validate gpl` — успешно.
- `git diff --check -- Projects/novels-gpl` — успешно.

## Требуется при интеграции

- Нет; дальнейшее наполнение выполняется отдельными asset-scoped задачами.
