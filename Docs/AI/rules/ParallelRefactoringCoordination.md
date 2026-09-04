# Coordination core

Это обязательный компактный протокол всех чатов в репозитории `SomeGame`.
Подробности выбираются адресно. Core и четыре адресных протокола являются
единственным действующим coordination contract.

## Перед работой с репозиторием

Чисто разговорный/мета-вопрос без чтения файлов проекта, runtime probe или
изменения state не инициализирует project workflow. При первом обращении к
репозиторию применяется порядок ниже.

1. Подтвердить, что workspace и cwd указывают на Git-корень `SomeGame`, а не
   на его родительскую папку `Fork`.
2. Полностью прочитать `Docs/AI/README.md`, выполнить
   `Tools/somegame context --task <type> [--paths <owned> ...]` и прочитать
   перечисленные документы.
   Если runner недоступен, использовать ручной fallback из индекса.
3. Проверить показанный снимок полным `git status --short`; незнакомые изменения
   сохранить.
4. Выбрать минимальный точный scope и проверить активные agent/status records.
5. Для read-only анализа не создавать lock и не запускать тяжёлые процессы.
6. Перед изменением файлов создать собственные agent/request records, стать
   первой FIFO-заявкой и атомарно получить `active/write-lock`, кроме правки,
   полностью удовлетворяющей docs-only fast path ниже.

В том же непрерывном чате с неизменными task type и scope использовать
`context --resume --paths <owned>`. Документ с тем же fingerprint повторно не
читается; Git/FIFO/lock и актуальные task-owned paths проверяются всегда.

## Право записи

Единственное право записи — собственный `write-lock/owner.md`, где `Agent` и
`Request` совпадают с текущей первой заявкой. Markdown-status сам по себе права
не даёт. Нельзя менять чужие runtime-файлы, scope или незнакомый dirty tree.

Перед расширением scope проверить отсутствие активного владельца затрагиваемых
файлов и обновить собственную запись до первой правки. Подробности:
[ParallelWorkDetails.md](ParallelWorkDetails.md).

## Docs-only fast path

Мелкую непересекающуюся правку документации разрешено выполнить без FIFO,
agent/request/write-lock и записи в handoff, только если одновременно верно:

- меняются не более трёх уже существующих `.md` файлов и не более 80 строк
  diff суммарно;
- файлы находятся в `Docs/AI/guides/`, `Docs/AI/architecture/` или
  `Docs/AI/plans/`;
- правка ограничена опечатками, локальными формулировками, Markdown-разметкой,
  исправлением ссылки или пояснением уже действующего поведения;
- не меняются обязательность, архитектурное решение, публичная команда,
  workflow, формат данных, ownership, validation/build semantics или версия;
- нет перемещений, удалений, новых файлов, массовой замены, форматтера,
  генератора или иной команды записи;
- ни один изменяемый файл не входит в active scope другого потока; полный
  `git status` проверен, существующий чужой diff в этих файлах не затрагивается;
- не затрагиваются `AGENTS.md`, `Docs/AI/README.md`, `Docs/AI/memory/`,
  `Docs/AI/rules/`, `Docs/AI/CoordinationRuntime/`, `Docs/AI/work/` или archive.

Перед fast-path правкой объявить её в commentary, после — выполнить scoped
`git diff --check` и проверить локальные ссылки затронутых файлов. Если любой
критерий не выполнен или появился спор о смысле, используется обычный
scope/FIFO/write-lock/handoff. Наличие чужого непересекающегося lock само по
себе fast path не запрещает; этот режим не даёт права запускать процессы или
трогать runtime state.

## FIFO и тяжёлые процессы

Unity Editor/batch, compile, tests, import, build, генераторы и массовая
обработка выполняются по одному на весь репозиторий и только под lock. До старта
проверяются очередь и реальные процессы ОС. Read-only probe уже открытого
Editor допустим, если не меняет его state.

Lock не удерживается во время ожидания пользователя или внешнего ресурса.
Heartbeat обновляется не реже пяти минут; чужой stale lock не удаляется
автоматически. Долгие bounded-команды runner обновляют heartbeat владельца;
`Tools/somegame queue-status` одним снимком показывает FIFO, lease,
согласованность записей и process barrier. Полный порядок:
[UnityConcurrency.md](UnityConcurrency.md).

## Проверки и токены

- Когда scope известен, `context` и `verify` получают точные task-owned
  `--paths`; полный dirty tree используется только для обнаружения конфликтов,
  но не расширяет validation plan. Без точного scope сначала выполняется
  `Tools/novels-tools/novels-content plan [base-ref]`.
- Для живого Editor использовать один persistent MCP `editor-check`, а не
  повторные status/Console/hierarchy циклы.
- Успех передавать компактно; полные логи читать только при failure.
- Unity не запускается для docs/tooling scope, если plan этого не требует.
- Memory bank используется по
  [MemoryBankProtocol.md](MemoryBankProtocol.md); runtime state туда не пишется.
- Субагенты, фоновые jobs и параллельные worker-пулы в репозитории запрещены.

## Завершение

Перед release lock:

1. Выполнить соразмерную проверку и scoped diff-review.
2. Добавить компактную запись в `CoordinationRuntime/HANDOFF.md`.
3. Пометить собственную agent-запись итоговым статусом.
4. Удалить только собственные request и lock.

Этот раздел не применяется к корректному docs-only fast path: у него нет lock
и runtime-записей, результат сообщается пользователю после scoped проверки.

`HANDOFF.md` содержит только незавершённое состояние и остаётся короче 120
строк; завершённая история ротируется в `archive/`. Интеграция,
изменение общего контракта и handoff описаны в
[IntegrationProtocol.md](IntegrationProtocol.md).

## Маршрутизация

| Ситуация | Дополнительно прочитать |
| --- | --- |
| Мелкая правка существующего guide/architecture/plan Markdown | Проверить все критерии docs-only fast path выше |
| Изменение файлов, расширение scope, пересечение владельцев | [ParallelWorkDetails.md](ParallelWorkDetails.md) |
| Unity, MCP write, import, test, build, ожидание FIFO | [UnityConcurrency.md](UnityConcurrency.md) |
| Общий контракт, ready-for-integration, commit/handoff | [IntegrationProtocol.md](IntegrationProtocol.md) |
| Обновление долговременной памяти | [MemoryBankProtocol.md](MemoryBankProtocol.md) |
| Не покрытый текущими правилами случай | Дополнить подходящий действующий протокол под обычным scope/lock |
