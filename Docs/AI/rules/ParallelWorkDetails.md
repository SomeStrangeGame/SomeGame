# Parallel work details

Читайте этот документ перед изменением файлов, расширением scope или работой с
пересекающимися потоками.

Исключение — корректный docs-only fast path из coordination core: он не создаёт
scope record, но обязан предварительно подтвердить отсутствие пересечения.

## Владение

- Каждый поток объявляет точные пути, ожидаемые изменения, базовый commit и
  проверки в собственной agent/status записи.
- Эти объявленные пути передаются в `Tools/somegame context --paths` и
  `Tools/somegame verify --paths`; чужой dirty tree остаётся collision evidence,
  но не основанием для расширения gates текущей задачи.
- Один файл не входит одновременно в scope двух активных владельцев. При
  пересечении приоритет у ранее объявленного владельца.
- Чужой status-файл не редактируется для получения владения. Нужны завершение,
  явная передача либо новый непересекающийся scope.
- При паузе на пользователе или внешнем ресурсе владелец удаляет собственные
  lock и request. Оставлять request разрешено только при активном автоматическом
  ожидании FIFO; иначе она превращается в orphaned head и блокирует очередь.
- Широкие области вроде `Packages/**` допустимы только с обоснованием; по
  умолчанию указываются точные файлы или минимальные каталоги.

Для длительного архитектурного потока используется
`Docs/AI/work/parallel/ParallelWork.<scope>.md` со статусом `active`,
`ready-for-integration`, `integrated` или `paused`. Ожидание общей проверки не
является причиной оставлять завершённую локальную работу в `active`.

## Расширение scope

До первой правки:

1. Прочитать пересекающиеся active/status records.
2. Проверить, что требуемые пути свободны или явно переданы.
3. Записать точные новые пути, контракт и проверки в собственный scope.
4. Получить обычный runtime write-lock.
5. Разделить межпроектную работу на атомарно проверяемые блоки.

Временный межпроектный scope не даёт право менять соседние каталоги сверх
перечисленного. Общий SDK не получает project-specific hardcode; истории не
меняют контракт SDK самостоятельно.

## Постоянные границы

- Shared pipeline: `Packages/NovelsContentSdk/**`, `Tools/novels-tools/**` и
  связанные общие контракты.
- Catalog: `Projects/novels-catalog/**`.
- Story: точный `Projects/novels-<storyId>/**`.
- Game: `Novels/Assets/Novels/**`, Player и runtime integration.

## Несколько новых историй одновременно

Если пользователь явно заказал несколько историй одновременно, один
оркестратор может выделить отдельный поток на каждый заранее согласованный
`storyId`. Каждый поток получает только точный
`Projects/novels-<storyId>/**` и story-local авторские/evidence-файлы; один путь
не передаётся двум потокам. Исследование, narrative design, asset manifest и
подготовка story-local изменений могут идти параллельно без write-lock, пока не
меняют checkout или runtime state.

Для каждой новой истории оркестратор создаёт отдельные
`codex/story-<storyId>` и Git worktree через `Tools/somegame story-worktree
create`. История владеет только `Projects/novels-<storyId>/**`; story-local
запись, статические проверки и commits не требуют repository-wide write-lock,
поскольку branch и index изолированы. Один worktree не переиспользуется для
другого `storyId`, а worker не переключает в нём ветку.

Общий runtime хранится вне checkout в Git common dir
`.git/somegame-runtime/` либо в явно заданном `SOMEGAME_SHARED_RUNTIME`. Там
находятся registry worktree, candidate manifests и locks ресурсов `unity`,
`catalog`, `shared-sdk`, `integration`. Локальная копия
`Docs/AI/CoordinationRuntime` не используется как меж-worktree mutex.

Завершённый worker обязан иметь clean worktree и передать commit SHA через
`Tools/somegame story-candidate`; команда fail-closed проверяет, что diff от
base затрагивает только его story prefix. `story-batch-plan` принимает только
такие кандидаты. Catalog/shared-contract изменения выполняются отдельными
ветками и интегрируются раньше зависимых историй.

Unity Editor, MCP write, import, генераторы, compile, tests, Player и emulator
остаются глобально последовательными под shared `unity` lock. Они запрещены до
единого финального слота и отдельного человеческого разрешения. Изменения
Catalog, template, shared SDK/tooling, общих документов и финальная Git-интеграция принадлежат отдельной
последовательной integration-фазе после готовности story-local scopes. Если
история требует нового общего контракта, её поток останавливается на handoff и
не расширяет ownership самостоятельно.

Worktree удаляется только командой `story-worktree remove --confirm`, когда он
clean и его HEAD уже содержится в указанном integration ref. Уникальные или
незакоммиченные изменения не удаляются автоматически.

Нельзя массово переименовывать общие пути, мигрировать все проекты одним
потоком, чистить чужие caches, выполнять общий reset/clean или коммитить весь
dirty tree без scoped review.

Если новый случай владения здесь не описан, нельзя брать scope по исторической
аналогии: сначала дополнить этот действующий протокол под обычным write-lock.
