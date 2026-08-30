# Agent workflow optimization baseline — 2026-08-30

Датированный benchmark, не нормативный контракт.

## До изменения (`b910242f`)

| Метрика | Значение |
| --- | ---: |
| Обязательные README/core/project/architecture/handoff | 540 строк |
| `CoordinationRuntime/HANDOFF.md` | 190 строк |
| `work/parallel/ParallelWork.*.md`, включая queue | 21 файл |
| `Tools/somegame docs-check` | 0.595 с |
| Successful `docs-check` JSON | 634 байта |

## После локальной реализации

| Метрика | Значение |
| --- | ---: |
| README/core/project/architecture/handoff | 294 строки |
| `CoordinationRuntime/HANDOFF.md` | 39 строк |
| `work/parallel/ParallelWork.*.md`, включая queue | 12 файлов |
| `Tools/somegame docs-check` | 0.589 с |
| Первый scoped `verify` | 0.121 с |
| Повторный scoped `verify`, cache hit | 0.002 с |
| Один `context` при текущем большом dirty scope | около 4 КБ JSON |

Стартовый контракт больше не требует полного handoff: после 65-строчного
индекса `context` адресно возвращает документы и bounded runtime summaries.
Экономия строк полного bootstrap snapshot — 45.6%; фактическая экономия токенов
выше для чистого дерева, поскольку завершённые handoff/work details не читаются.

## Проверенные сценарии

- docs/tooling: static diff + automation tests, Unity не требуется;
- context types `docs`, `code`, `unity`, `content`, `art`, `integration`;
- cache miss и последующий hit с тем же fingerprint;
- `commit-plan` не меняет index и отделяет runtime handoff;
- `finish-check` обнаруживает отсутствующий итоговый handoff и не закрывает lock;
- `docs-check` контролирует лимит handoff 120 строк и stale/integrated work files.
