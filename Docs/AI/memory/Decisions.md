# Active decisions

Здесь перечисляются только действующие решения. Детали и нормативные формулировки
принадлежат связанным первичным документам.

## ADR-001 — Atomic content projects

- Status: accepted.
- Decision: Catalog и каждая история являются отдельными Unity-проектами;
  общий SDK не зависит от Game или конкретного контента.
- Consequence: межпроектные изменения интегрируются атомарно через общий scope.
- Source: [MultiProjectSplitPlan.md](../architecture/MultiProjectSplitPlan.md).

## ADR-002 — One bundle per atomic project

- Status: accepted.
- Decision: один проект выпускает один bundle на платформу; episode/shared
  bundles и ручные AssetBundle labels не используются.
- Source: [ContentPipeline.md](../guides/ContentPipeline.md).

## ADR-003 — Published content is the runtime boundary

- Status: accepted.
- Decision: Editor и Player читают одинаковый release-контракт через
  `IContentSource`; runtime не читает authoring assets напрямую.
- Source: [MultiProjectSplitPlan.md](../architecture/MultiProjectSplitPlan.md).

## ADR-004 — Whole-character variants

- Status: accepted; replaces layered face/hair/clothes composition.
- Decision: production-вариант персонажа — один цельный полнофигурный sprite;
  wardrobe/emotion выбирают один канонический address.
- Migration: существующие layered TZM/ZDM остаются legacy-compatible до
  интеграции one-address adapter; legacy-пути не являются разрешением создавать
  новые модульные ассеты.
- Source: [CharacterLayeringRules.md](../rules/CharacterLayeringRules.md).

## ADR-005 — One shared Unity resource

- Status: accepted.
- Decision: Editor, build, import и write-capable MCP выполняются
  последовательно под общей FIFO/write-lock очередью.
- Source: [ParallelRefactoringCoordination.md](../rules/ParallelRefactoringCoordination.md).

## ADR-006 — Lock-free minor documentation edits

- Status: accepted.
- Decision: мелкая непересекающаяся правка существующего Markdown в guides,
  architecture или plans может выполняться без runtime records/lock/handoff
  только по всем критериям docs-only fast path.
- Consequence: любые изменения контрактов, memory/rules/index, новые файлы,
  перемещения и массовые правки остаются в обычной FIFO.
- Source: [ParallelRefactoringCoordination.md](../rules/ParallelRefactoringCoordination.md#docs-only-fast-path).

## ADR-007 — One bounded automation entrypoint

- Status: accepted.
- Decision: повторяемые docs, content, Editor, Player, Android smoke и
  licensing workflows запускаются через `Tools/somegame` и возвращают один
  compact JSON; полный лог остаётся на диске.
- Consequence: модель не выполняет polling/log triage по шагам; write-workflows
  fail-closed требуют точного repository lock owner.
- Source: [AutomationRunners.md](../guides/AutomationRunners.md).

## ADR-008 — SomeGame root and checkpointed chat restart

- Status: accepted.
- Decision: saved workspace, cwd и coordination root указывают на Git-корень
  `SomeGame`, а не на родительский `Fork`; длинный чат передаёт работу новому
  чату только через safe checkpoint и компактный handoff без live lock.
- Consequence: новый чат заново проверяет Git/FIFO и получает новый lock;
  полная история диалога не переносится.
- Source: [IntegrationProtocol.md](../rules/IntegrationProtocol.md#checkpoint-и-перезапуск-чата).
