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

### Явно разрешённая параллельная правка при проверке неизменного APK

Если пользователь явно отменил ожидание source-only задачи на время уже
запущенной проверки готового APK, владелец может передать checkout write-lock
первой следующей заявке, не прерывая этот device-прогон, только если:

- APK, release, package и ADB serial зафиксированы; новый APK не собирается,
  не устанавливается и не подменяется, исходники для работы Player не читаются;
- прежний владелец сохраняет shared `unity` resource и точный scope только
  своего устройства/сохранений и ignored каталога evidence/helpers;
- до передачи он фиксирует разрешение, границы и статус в своей agent-записи
  и handoff, затем удаляет только свой checkout request/write-lock;
- новый checkout-владелец не запускает Unity, build, Unity-backed tests, ADB,
  эмулятор, Git/branch/index mutations и не меняет story/art/APK/cache/evidence
  проверяемого кандидата; допустимы его source-only правки и дешёвые static checks;
- прежний владелец откладывает любые tracked/coordination правки до нового
  обычного checkout-lock. Его shared `unity` lock остаётся process barrier;
- device evidence относится исключительно к записанному APK, а не к новым
  исходникам. После изменения runtime/catalog code новый checkout не получает
  автоматический acceptance pass; его финальная проверка требует нового APK
  и отдельного разрешённого слота.

Это адресное исключение не разрешает параллельные тяжёлые процессы или
произвольные source-only изменения без человеческого разрешения.

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

Narrative design, production manifests и утверждение персонажей, прочего арта,
presentation sprites и audio могут завершаться до создания Unity-проекта.
Pre-production drafts хранятся вне Git; handoff фиксирует approved deliverables,
logical addresses, formats и import requirements. Story worktree создаётся,
когда narrative, manifest и необходимые материалы стабильны и требуется
project-bound scaffold/import. Это не отменяет отдельную runtime-проверку после
импорта.

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

Каждая история может использовать один task-owned Editor своего точного
story-worktree и один task-owned emulator с уникальным AVD/serial. Процессы
разных историй могут сосуществовать; совпадающий project path, serial, outputs,
Catalog, shared SDK и integration остаются последовательными под применимыми
resource locks. Тяжёлые gates запрещены до единого финального слота истории и
отдельного человеческого разрешения. Изменения
Catalog, template, shared SDK/tooling, общих документов и финальная Git-интеграция принадлежат отдельной
последовательной integration-фазе после готовности story-local scopes. Если
история требует нового общего контракта, её поток останавливается на handoff и
не расширяет ownership самостоятельно.

Story-local финальная проверка не ждёт глобальный repository FIFO. До первого
тяжёлого шага задача получает `story:<storyId>`, затем locks точных mutable
ресурсов `unity-project:<canonical-project-id>`, `emulator:<serial>` и
`build-output:<canonical-output-id>`. Другая история с непересекающимися
идентификаторами выполняется параллельно. Общий `unity` lock для разных atomic
project paths не используется; он остаётся только для licensing recovery или
другой доказанно общей Unity-инфраструктуры.

Результат каждой проверки — immutable candidate SHA и связанный с ним evidence:
source/release/APK SHA, Unity version, project path, AVD/serial, точные маршруты
и runtime markers. Изменение кандидата инвалидирует evidence только этой
истории. После готовности нескольких кандидатов один интегратор получает
`integration` и при необходимости `catalog`, проверяет SHA/scope через
`story-batch-plan`, переносит commits в `main`, разрешает только общие
catalog-конфликты и выполняет один общий compose gate.

### Обновление базы preparing-worktree без собственных коммитов

По явному запросу обновить ветку разрешён ограниченный ручной fast-forward,
пока runner не предоставляет отдельную команду refresh. Операция требует
обычного checkout write-lock и shared `integration` lock. Перед ней:

1. Проверить точные registry/path/branch, статус `preparing`, отсутствие
   candidate manifest и активных владельцев/Unity-процессов целевого worktree.
2. Получить origin/main и зафиксировать полный SHA. Требовать, чтобы HEAD
   совпадал с прежним registry `baseSha` и был предком целевого SHA; staged и
   tracked diff должны быть пусты. Другие случаи требуют отдельного плана.
3. Проверить весь untracked scope: только зарегистрированный story prefix,
   которого нет в целевом Git tree. Проверить также коллизии с ignored files
   вне prefix. Сохранить перечень и SHA-256 всех файлов истории до операции.
4. Выполнить `git merge --ff-only <verified-sha>` в точном worktree. Не применять
   reset/clean, автоматический stash, переключение ветки или копирование dirty
   файлов в другой checkout. Основной checkout не обновляется этой командой.
5. Проверить HEAD, неизменность файлов истории и отсутствие нового tracked
   diff. Только после успеха изменить `baseSha` в точной shared registry на
   проверенный SHA, сохранив branch/path/allowedPrefix и статус `preparing`.
   В записи сохранить прежний baseSha и фактическое UTC-время обновления.
6. Повторно проверить registry, ancestry и diff от новой базы; входящие общие
   изменения не должны считаться вкладом истории. Записать evidence в handoff.

При ошибке до fast-forward registry не менять. При ошибке после него сохранить
обе версии SHA в handoff и остановить candidate handoff до согласования registry;
не выполнять обратный reset. Это техническое обновление не утверждает контент,
не создаёт готовый candidate и не разрешает Unity, commit или publication.

Worktree удаляется только командой `story-worktree remove --confirm`, когда он
clean и его HEAD уже содержится в указанном integration ref. Уникальные или
незакоммиченные изменения не удаляются автоматически.

Нельзя массово переименовывать общие пути, мигрировать все проекты одним
потоком, чистить чужие caches, выполнять общий reset/clean или коммитить весь
dirty tree без scoped review.

Если новый случай владения здесь не описан, нельзя брать scope по исторической
аналогии: сначала дополнить этот действующий протокол под обычным write-lock.
