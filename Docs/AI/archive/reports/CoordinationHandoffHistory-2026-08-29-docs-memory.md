# Documentation and memory-bank handoff history

Завершённые documentation-only записи ротированы из текущего handoff при
оптимизации обязательного контекста.

## 2026-08-29T11:45:47Z — memory-bank-protocol — completed

Task: создать канонический компактный memory bank и правило его использования.

Changed:
- `Docs/AI/memory/`: добавлены Project, Architecture, Decisions, Workflows,
  KnownIssues, Glossary и собственный индекс;
- `AGENTS.md` и `Docs/AI/README.md`: добавлено обязательное ядро и адресная
  маршрутизация memory-файлов;
- `ParallelRefactoringCoordination.md`: закреплены границы, лимиты и порядок
  обновления memory bank.

Validation:
- 7 memory-файлов: 248 строк суммарно; обязательные Project+Architecture — 79;
- локальные Markdown-ссылки: 0 broken;
- planner documentation gate 5/5 и scoped `git diff --check`: успешно.

Pending / risks:
- Документы не закоммичены; Unity Editor не запускался и для docs-only scope не
  требуется.

## 2026-08-29T11:36:00Z — ai-docs-root-move — completed

Task: вынести общие AI-протоколы из Unity-проекта `Novels` на уровень
репозитория.

Changed:
- каноническое дерево перенесено из `Novels/Docs/AI` в `Docs/AI`;
- `AGENTS.md`, код, тесты и документация переведены на новый путь;
- общий Unity coordination root теперь задаётся как корень `SomeGame`
  (`--coordination-root .`), а не `Novels`.

Validation:
- старого дерева на диске нет, новое дерево существует;
- локальные Markdown-ссылки проверены после исправления одного архивного пути;
- planner tests 5/5, Unity MCP helper tests 24/24, `zsh -n` и scoped
  `git diff --check` прошли.

Pending / risks:
- перенос не закоммичен; Git показывает удаления старых путей и новое дерево.
- Unity Editor не запускался: изменение относится только к документации и
  локальному tooling contract.

## 2026-08-29T11:58:33Z — docs-only-fast-path — completed

Task: ускорить мелкие непересекающиеся документационные правки.

Changed:
- core разрешает без FIFO/runtime records/lock/handoff менять до 3 существующих
  Markdown-файлов и 80 строк в guides/architecture/plans при строгих критериях;
- contracts, memory, rules, index, runtime state, новые/перемещённые файлы и
  любые процессы остаются в обычной FIFO;
- решение закреплено в `memory/Decisions.md` как ADR-006.

Validation:
- core 104 строки; links/anchors 0 broken; docs tests 5/5; diff-check успешно.

Pending / risks:
- docs-only изменения не закоммичены; Unity не запускался.

## 2026-08-29T11:52:22Z — coordination-context-split — completed

Task: сократить обязательный coordination context без потери прежних правил.

Changed:
- обязательный core сокращён с 632 до 72 строк;
- scope, Unity/FIFO, integration и memory вынесены в четыре адресных протокола;
- полный прежний контракт сохранён как ненормативный исторический снимок в
  `archive/rules/CoordinationReference-2026-08-29.md`;
- завершённые docs/memory handoff-записи ротированы без потери текста.

Validation:
- обязательный стартовый набор: 446 строк до этой записи против 1030 ранее;
- local links, anchors, documentation tests и `git diff --check`: успешно.

Pending / risks:
- docs-only изменения не закоммичены; Unity не запускался.

## 2026-08-29T11:55:57Z — archive-coordination-reference — completed

Task: убрать прежний полный reference из активных правил.

Changed:
- reference перенесён в `archive/rules/CoordinationReference-2026-08-29.md` и
  явно помечен ненормативным;
- активные ссылки на него удалены; единственный контракт — core и четыре
  адресных протокола;
- непокрытый случай теперь требует дополнения действующего протокола.

Validation:
- active routing references: 0; broken local links: 0; `git diff --check` и
  documentation tests: успешно.

Pending / risks:
- docs-only изменения не закоммичены; Unity не запускался.
