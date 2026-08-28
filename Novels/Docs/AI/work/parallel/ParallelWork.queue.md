# Parallel integration queue

- Владелец: integration coordinator
- Последнее обновление: 2026-08-28
- Режим: последовательный
- Unity concurrency: 1

Этот файл отражает только незавершённую архитектурную интеграцию. Фактическое
право записи выдаёт FIFO/write-lock из `CoordinationRuntime/`.

## Queue

### WebGL local prototype

- Состояние: `blocked`
- Статус: [ParallelWork.webgl-local-prototype.md](ParallelWork.webgl-local-prototype.md)
- Ветка: `prototype/webgl-local-platform`
- Commit: `cfb92896`
- Причина: commit отсутствует в `main`; Unity compilation и browser smoke test
  не выполнены из-за проблемы Unity Licensing Client.
- Следующий шаг: восстановить Unity license, выполнить compile и
  `Novels/Prototype/Build & Preview WebGL`, затем принять или отклонить
  интеграцию в `main`.

## Archived

Старые элементы очереди, реализация которых присутствует в `main`, перенесены
в `../../archive/parallel-work/` аудитом 2026-08-28.
