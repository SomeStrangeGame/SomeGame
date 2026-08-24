# Parallel integration queue

- Владелец: shared pipeline / integration coordinator
- Последнее обновление: 2026-08-24
- Режим: последовательный
- Unity concurrency: 1

Этот файл отражает порядок интеграции. Соседние рабочие потоки обновляют только
свои `ParallelWork.<scope>.md`.

## Queue

### 1. Shared pipeline

- Состояние: `done`
- Зависимости: нет
- Область: `Packages/NovelsContentSdk`, `Tools/novels-tools`
- Результат: стабильные validation/build/CLI-контракты
- Unity: не требуется для реализации; требуется один Catalog compile/build для
  приёмки

### 2. Catalog handoff

- Состояние: `done`
- Зависимости: `1. Shared pipeline`
- Источник: `ParallelWork.catalog.md`
- Локальная работа фактически завершена; оставшиеся пункты относятся к общей
  интеграции, поэтому stale-значение `active` не блокирует очередь.
- Результат: общий bundle audit, Catalog validate/build/audit

### 3. Content authoring handoff

- Состояние: `done`
- Зависимости: `1. Shared pipeline`
- Источник: `ParallelWork.content-authoring.md`
- Статус источника: `ready-for-integration`
- Результат: требования к inspect/size/validation сопоставлены с общим SDK

### 4. Sequential content validation

- Состояние: `done`
- Зависимости: `2. Catalog handoff`, `3. Content authoring handoff`
- Unity: эксклюзивно
- Порядок: `catalog -> tzm -> zdm`
- Результат: все атомарные проекты валидируются общей версией SDK

### 5. Editor bundle validation

- Состояние: `done`
- Зависимости: `4. Sequential content validation`
- Unity: эксклюзивно
- Порядок: `catalog -> tzm -> zdm`
- Результат: bundle, release и size audit для Editor

### 6. Mobile builds

- Состояние: `done`
- Зависимости: `5. Editor bundle validation`
- Unity: эксклюзивно
- Порядок: `all android`, затем `all ios`
- Результат: финальное серверное дерево двух мобильных платформ

## Current resource state

- Очередь завершена; активных Unity-задач координатора нет.
- Catalog, TZM и ZDM содержат Mac, Android и iOS releases одновременно.
- Общая композиция создана в `Novels/Build/LocalContent`.
- Размер общей композиции — около 1,6 ГБ; она не помещается целиком в прежний
  серверный лимит 1 ГБ.

## Deadlock rule

Ни один элемент не может ждать статус `integrated` элемента, который сам от
него зависит. При обнаружении цикла координатор переводит локально завершённый
поток в handoff-этап очереди и выполняет общую проверку один раз в конце.
