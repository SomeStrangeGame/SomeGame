# Workflow memory

Все команды выполняются из корня `SomeGame` под требованиями общей очереди.

## Единый automation runner

```bash
Tools/somegame docs-check
Tools/somegame git-publish --agent-id <lock-owner> [--ssh-key <local-key-path>]
Tools/somegame licensing-preflight
Tools/somegame content-gate --agent-id <lock-owner> --platform editor
Tools/somegame editor-gate --agent-id <lock-owner> --project Novels --compile
Tools/somegame player-build --agent-id <lock-owner> --target Android --mode Embedded
Tools/somegame android-smoke --agent-id <lock-owner> --apk <path> --package-id <id>
```

Runner выполняет bounded polling локально, сохраняет полный лог в
`Novels/Build/Logs/automation` и возвращает один компактный JSON. Все
write/Unity/ADB/Git workflows fail-closed проверяют точного lock owner; без lock
разрешены только `docs-check` и read-only licensing preflight.

`git-publish` — каноническая публикация готовых атомарных commits в
`origin/main`: только clean tree кроме собственных untracked coordination
records, fetch, отказ при remote-ahead, обычный push и финальная сверка SHA.
Ключ задаётся только локальным `--ssh-key`; pull/rebase/merge/force-push команда
не выполняет.

## Выбор минимальной проверки

```bash
Tools/novels-tools/novels-content plan [base-ref]
Tools/novels-tools/novels-content verify editor [base-ref]
```

`plan` определяет затронутые targets и ручные gates. `verify` выполняет только
детерминированную часть. Unity/Player/manual gates не запускаются скрыто.

## Контент

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate <catalog|story-id|all>
Tools/novels-tools/novels-content build <catalog|story-id|all> <editor|android|ios>
Tools/novels-tools/novels-content publish <destination-directory>
```

Проекты обрабатываются последовательно: Catalog, затем истории. Полную матрицу
не запускать без основания changed-path plan.

## Живой Unity Editor

Для состояния, compile и узких EditMode tests использовать один persistent
`Tools/unity-mcp-helper` и агрегированный `editor-check`. Write-capable workflow
запускается с точным `--agent-id` владельца lock и repository root:

```bash
python3 Tools/unity-mcp-helper/unity_mcp_helper.py \
  --project Novels --coordination-root . --agent-id <agent-id> \
  editor-check --compile
```

Полные логи читать только при non-success. UI/content art дополнительно требует
ограниченный ручной visual smoke.

Подробности: [ContentPipeline.md](../guides/ContentPipeline.md),
[UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md) и
[ManualContentChecklist.md](../guides/ManualContentChecklist.md). Право запуска,
FIFO и process barrier определяет [UnityConcurrency.md](../rules/UnityConcurrency.md).
