# Workflow memory

Все команды выполняются из корня `SomeGame` под требованиями общей очереди.
Базовый поддерживаемый license tier — Unity Personal: проект, MCP, validation и
build не используют функции, требующие Pro или иной платной entitlement.
Каноническое правило: [ContentPipeline.md](../guides/ContentPipeline.md#базовый-уровень-лицензии-unity).

## Единый automation runner

```bash
Tools/somegame docs-check
Tools/somegame context --task <inspect|docs|code|unity|content|art|integration> [--resume] [--paths <owned> ...]
Tools/somegame queue-status [--agent-id <agent>]
Tools/somegame queue-prune --request <exact-terminal-orphan-id>
Tools/somegame verify --explain --base-ref origin/main
Tools/somegame verify --agent-id <lock-owner> --base-ref origin/main
Tools/somegame commit-plan
Tools/somegame finish-check --agent-id <lock-owner>
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

`context` объединяет task-plan и chat-resume в одном read-only snapshot;
task-owned `--paths` не позволяют чужому dirty tree расширять plan, а
fingerprints позволяют не перечитывать неизменившиеся документы при resume.
`queue-status` одним read-only снимком объясняет FIFO/lease/process blocker;
долгие runner-команды владельца автоматически поддерживают heartbeat lock.
`queue-prune` удаляет только точную terminal orphan-заявку вне текущего lock.
`verify` исполняет changed-path plan и кэширует только полностью успешное
evidence; `--release` и `--no-cache` обходят кэш. `commit-plan` только предлагает
группы, а `finish-check` fail-closed проверяет handoff, lock и Editor-процессы.
`docs-check` включает dependency-free проверку Markdown и project-local skill
contracts; отдельный `PyYAML` для проверки frontmatter не требуется.

При создании новой истории все Unity/MCP live, import, content build, compile,
tests, Player/APK и emulator/ADB операции откладываются до одного финального
validation/acceptance-слота. Он начинается только после отдельного актуального
разрешения человека; `auto-approve` и исходный запрос на создание истории его
не заменяют. До разрешения используются только статические проверки, а готовый
пакет передаётся как `ready-for-final-validation`.

## Выбор минимальной проверки

```bash
Tools/novels-tools/novels-content plan [base-ref]
Tools/novels-tools/novels-content verify editor [base-ref]
```

`plan` определяет затронутые targets и ручные gates. `verify` выполняет только
детерминированную часть. Unity/Player/manual gates не запускаются скрыто.

При явно заказанном параллельном создании нескольких новых историй допускается
отдельный поток на каждый `storyId`. Параллельны только подготовка и
непересекающиеся story-local worktree, static checks и commits. Кандидат
передаётся clean commit SHA; Unity/import/build/test/emulator, Catalog, shared
contracts и финальная интеграция сериализованы общими resource locks.
Канонические границы описаны в
[ParallelWorkDetails.md](../rules/ParallelWorkDetails.md#несколько-новых-историй-одновременно).

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
