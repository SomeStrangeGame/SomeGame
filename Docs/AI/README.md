# AI documentation index

Канонический project root — Git-корень `SomeGame`; `Fork` является только
локальным контейнером. Это единственная обязательная стартовая страница.

## Старт задачи

После полного чтения этой страницы выполните read-only снимок:

```bash
Tools/somegame context --task <inspect|docs|code|unity|content|art|integration> \
  [--paths <task-owned-path> ...]
```

`inspect` предназначен для узкой read-only инспекции без тематического workflow.
Снимок показывает Git/FIFO, незавершённые риски, changed-path plan и минимальный
список документов. Полностью прочитайте перечисленные `documents`, затем перед
записью получите write-lock. Передавайте точные `--paths`, когда scope уже
известен: тогда plan не расширяется чужим dirty tree. Для продолжения того же
scope используйте `--resume`; документы с неизменившимися fingerprint повторно
читать не требуется.

Если runner не запускается, прочитайте [coordination core](rules/ParallelRefactoringCoordination.md),
[Project memory](memory/Project.md), [Architecture memory](memory/Architecture.md),
актуальный [handoff](CoordinationRuntime/HANDOFF.md) и `git status --short`.

## Маршрутизация

| Задача | Канонический источник |
| --- | --- |
| Изменение файлов/scope | [ParallelWorkDetails.md](rules/ParallelWorkDetails.md) |
| Unity, tests, build, FIFO | [UnityConcurrency.md](rules/UnityConcurrency.md), [AutomationRunners.md](guides/AutomationRunners.md) |
| Commit, handoff, restart | [IntegrationProtocol.md](rules/IntegrationProtocol.md) |
| Memory bank | [MemoryBankProtocol.md](rules/MemoryBankProtocol.md) |
| Архитектура/runtime | [UnityProjectContext.md](architecture/UnityProjectContext.md), [ProjectOverview.md](architecture/ProjectOverview.md) |
| Content authoring/Ink | [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md), [InkSyntax.md](guides/InkSyntax.md) |
| Character/art | [CharacterLayeringRules.md](rules/CharacterLayeringRules.md), [ManualContentChecklist.md](guides/ManualContentChecklist.md) |
| Originality review | [OriginalityReviewProtocol.md](rules/OriginalityReviewProtocol.md) |
| Story-local Bubble UI | [StoryBubblePresentation.md](guides/StoryBubblePresentation.md) |
| Validation/release | [ContentPipeline.md](guides/ContentPipeline.md) |
| Unity MCP | [UnityMcpWorkflow.md](guides/UnityMcpWorkflow.md) |
| Licensing | [UnityLicensingTroubleshooting.md](guides/UnityLicensingTroubleshooting.md) |
| Размер контента | [ContentSizeOptimization.md](plans/ContentSizeOptimization.md) |
| Известная проблема | [KnownIssues.md](memory/KnownIssues.md) |

Для нескольких типов списки объединяются. При расхождении документа с кодом
зафиксируйте конфликт и проверьте текущий код, конфигурацию и evidence.

## Состояние и история

- `CoordinationRuntime/` — фактические request/write-lock/agent/handoff.
- `work/parallel/` — только незавершённые долгие scopes.
- `archive/` — история, читаемая адресно для конкретной регрессии.
- `memory/` — устойчивые факты, не runtime progress.
- `Library`, `Build`, `Temp` и логи — generated evidence, не контракт.

## Проверка и завершение

```bash
Tools/somegame verify --explain --base-ref origin/main
Tools/somegame verify --agent-id <lock-owner> --base-ref origin/main
Tools/somegame commit-plan
Tools/somegame finish-check --agent-id <lock-owner>
Tools/somegame git-publish --agent-id <lock-owner> [--ssh-key <path>]
```

`verify` исполняет только changed-path gates; manual/Player параметры не
угадываются. `commit-plan` и `finish-check` read-only относительно tracked
sources. Успех возвращается компактным JSON, полный лог читается при failure.

Новый общий документ создавайте только если существующий источник не подходит.
Не дублируйте нормативный текст: обновите источник и ссылки.
