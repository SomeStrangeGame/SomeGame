# AI documentation index

Это каноническая точка входа для разработчиков и автоматизированных агентов в
репозитории `SomeGame`. Документы не нужно читать все подряд: сначала прочитайте
обязательное ядро, затем полностью прочитайте набор для своего типа задачи.

Если задача затрагивает несколько областей, требования объединяются.
Исторические отчёты не являются текущими инструкциями, если актуальный контракт
явно на них не ссылается.

## Обязательное ядро

Перед любой инспекцией или изменением проекта:

1. [ParallelRefactoringCoordination.md](rules/ParallelRefactoringCoordination.md) —
   владение файлами, FIFO/write-lock, Unity concurrency и handoff.
2. [CoordinationRuntime/HANDOFF.md](CoordinationRuntime/HANDOFF.md) — актуальные
   результаты и незавершённые риски других потоков.
3. `git status --short` — текущее рабочее дерево; незнакомые изменения
   сохраняются и не откатываются.

Перед записью также создаются собственные agent/request-записи и получается
runtime `write-lock` по правилам координации.

## Как выбрать документы

| Тип задачи | Прочитать полностью |
| --- | --- |
| Первое знакомство, архитектура Game/runtime | [UnityProjectContext.md](architecture/UnityProjectContext.md), [ProjectOverview.md](architecture/ProjectOverview.md) |
| Создание или изменение истории и её ассетов | [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md), [InkSyntax.md](guides/InkSyntax.md) |
| Ink, команды сценария, адреса персонажей и ресурсов | [InkSyntax.md](guides/InkSyntax.md), релевантный раздел [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md) |
| Персонажи, тела, волосы, одежда, эмоции, аксессуары | [CharacterLayeringRules.md](rules/CharacterLayeringRules.md), [ManualContentChecklist.md](guides/ManualContentChecklist.md), релевантные разделы [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md) |
| Импорт PNG, настройки текстур, размер контента | [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md), [ContentSizeOptimization.md](plans/ContentSizeOptimization.md), [ContentSizeBaseline.md](plans/ContentSizeBaseline.md) как датированный baseline |
| Валидация, AssetBundle, release, publish | [ContentPipeline.md](guides/ContentPipeline.md), [ManualContentChecklist.md](guides/ManualContentChecklist.md) для визуальной приёмки |
| Player build или платформенная сборка | [ContentPipeline.md](guides/ContentPipeline.md), затем актуальный build-скрипт и его локальный README |
| Unity не запускается, Licensing Client, `Connection Lost`, protocol/mutex/IPC | [UnityLicensingTroubleshooting.md](guides/UnityLicensingTroubleshooting.md), затем свежие Editor/licensing-логи |
| Изменение общего SDK или межпроектного контракта | [ParallelRefactoringCoordination.md](rules/ParallelRefactoringCoordination.md), [ContentPipeline.md](guides/ContentPipeline.md), [MultiProjectSplitPlan.md](architecture/MultiProjectSplitPlan.md) |
| Текущий рефакторинг архитектуры | [UnityRefactoringPlan.md](plans/UnityRefactoringPlan.md), [UnityProjectContext.md](architecture/UnityProjectContext.md) |
| Ручной Play Mode/content smoke | [ManualContentChecklist.md](guides/ManualContentChecklist.md) |
| Расследование прежнего решения или регрессии | соответствующий `work/parallel/ParallelWork.<scope>.md` или `archive/parallel-work/ParallelWork.<scope>.md`, [RefactoringHistory.md](archive/reports/RefactoringHistory.md), релевантная запись `CoordinationRuntime/HANDOFF.md` |

## Действующие контракты и руководства

Эти документы описывают текущее ожидаемое поведение:

- [CharacterLayeringRules.md](rules/CharacterLayeringRules.md) — модульная графика
  персонажей и обязательная обратная сборка.
- [ContentAuthoringGuide.md](guides/ContentAuthoringGuide.md) — устройство атомарной
  истории и authoring-процесс.
- [ContentPipeline.md](guides/ContentPipeline.md) — validation/build/publish pipeline.
- [InkSyntax.md](guides/InkSyntax.md) — поддерживаемый сценарный синтаксис.
- [ManualContentChecklist.md](guides/ManualContentChecklist.md) — ручная визуальная и
  смысловая приёмка после автоматической проверки.
- [UnityLicensingTroubleshooting.md](guides/UnityLicensingTroubleshooting.md) —
  evidence-first диагностика и безопасное восстановление Unity Licensing на macOS.
- [ProjectOverview.md](architecture/ProjectOverview.md) — runtime flow и владение.
- [UnityProjectContext.md](architecture/UnityProjectContext.md) — краткий технический вход.
- [MultiProjectSplitPlan.md](architecture/MultiProjectSplitPlan.md) — границы Game, SDK,
  инструментов и контентных проектов.

При расхождении документа с текущим кодом нельзя молча выбирать удобную версию.
Зафиксируйте расхождение в собственной coordination-записи и проверьте текущий
контракт по коду, конфигурации и сборке.

## Планы и измерения

Эти документы полезны только для соответствующей задачи и не являются
универсальными инструкциями:

- [UnityRefactoringPlan.md](plans/UnityRefactoringPlan.md) — текущий план упрощения.
- [ContentSizeOptimization.md](plans/ContentSizeOptimization.md) — стратегия
  оптимизации размера.
- [ContentSizeBaseline.md](plans/ContentSizeBaseline.md) — снимок размеров на дату и
  commit, указанные внутри документа; не считать его текущими метриками.

Перед исполнением пункта плана необходимо проверить, не был ли он уже выполнен
или заменён более новым контрактом.

## Текущая параллельная работа

- `work/parallel/ParallelWork.<scope>.md` — только действительно незавершённые
  статусы `active`, `blocked` и `ready-for-integration`.
- `work/parallel/ParallelWork.queue.md` — архитектурная очередь интеграции.
- `CoordinationRuntime/` — фактическая FIFO/write-lock очередь и handoff.

Рабочие статусы читаются только при пересечении с их scope или при интеграции.

## История и отчёты

По умолчанию не читать и не использовать как действующие требования:

- `archive/parallel-work/ParallelWork.<scope>.md` — завершённые и
  интегрированные рабочие потоки, включая статусы, подтверждённые аудитом
  текущего `main`;
- [RefactoringHistory.md](archive/reports/RefactoringHistory.md) — архив архитектурных волн;
- [TZMImportReport.md](archive/reports/TZMImportReport.md) — датированный отчёт импорта TZM;
- [ZDMContentGapReport.md](archive/reports/ZDMContentGapReport.md) — датированный gap-report ZDM;
- `CoordinationRuntime/agents/*.md` — runtime-записи отдельных задач.

Они читаются только при расследовании истории конкретного файла, решения,
регрессии или незавершённой интеграции. Статус `active` в актуальной runtime
очереди имеет приоритет над историческими заявлениями владения.

## Что не является документацией

- `CoordinationRuntime/requests/` и `active/write-lock/` — операционное
  состояние очереди; его нельзя реорганизовывать как справочные файлы.
- Generated `Build`, `Library`, release и логи не становятся контрактом только
  потому, что присутствуют на диске.
- Комментарии в старом handoff не заменяют проверку текущего кода и конфигов.

## Поддержание структуры

При добавлении нового общего руководства:

1. Определите его категорию: контракт, тематическое руководство, план или
   исторический отчёт.
2. Добавьте его в этот индекс и в таблицу маршрутизации, если документ должен
   быть обязательным для типа задач.
3. Не создавайте вторую инструкцию с тем же назначением; обновите канонический
   документ или явно объявите замену.
4. Датированные результаты и одноразовые исследования оформляйте как отчёты,
   а не как общие правила.
5. Не перемещайте документ без обновления всех ссылок и проверки через `rg`.
