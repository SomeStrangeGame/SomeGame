# Current cross-chat handoff

Этот файл содержит только актуальное незавершённое состояние. Полный журнал до
ротации 2026-08-29T11:28Z сохранён без потери текста в
[`CoordinationHandoffHistory-through-2026-08-29T1128Z.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-29T1128Z.md).
Более ранняя история находится в
[`CoordinationHandoffHistory-through-2026-08-28.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-28.md).

Завершённые docs/memory изменения 2026-08-29 сохранены в
[`CoordinationHandoffHistory-2026-08-29-docs-memory.md`](../archive/reports/CoordinationHandoffHistory-2026-08-29-docs-memory.md).

## 2026-08-29T13:08:00Z — project-root-chat-restart-protocol — completed

Task: закрепить `SomeGame` как project root и безопасно перезапускать длинные чаты.

Changed: `Fork` объявлен только контейнером; root/workspace/coordination cwd —
Git-корень `SomeGame`. Restart разрешён на safe checkpoint без процессов/lock;
новый чат читает компактный handoff, проверяет Git/FIFO и получает новый lock.

Validation: docs-check и planner 6/6 passed; links/anchors/diff clean.

Pending: Codex saved-project list пока не содержит `SomeGame`; папку
`/Users/iantonishin/Fork/SomeGame` нужно один раз добавить через UI Codex.

## 2026-08-29T12:57:00Z — editor-gate-recompile-status-fix — completed

Task: автоматизировать и live-проверить шесть повторяемых docs/Unity/build/ADB/licensing задач.

Changed:
- `Tools/somegame`: единый JSON entrypoint для `docs-check`, `content-gate`,
  `editor-gate`, `player-build`, `android-smoke`, `licensing-preflight`;
- все write workflows fail-closed проверяют точного repository lock owner;
- licensing recovery требует exact `--confirm-pid`, не удаляет sockets/caches;
- добавлены scoped `content-gate --target` и явный lifecycle-флаг `--close-hub`;
- исправлены фактическое имя helper socket и разбор Unity rich-text suffix в
  `[NOVELS_SMOKE]` JSON; planner, memory и automation guide обновлены.

Validation:
- runner tests 7/7, planner tests 6/6, MCP helper tests 24/24;
- `docs-check`, read-only licensing preflight и real Catalog content-gate: passed;
- Android Embedded player-build: passed за 76.945 s, APK 1,815,925,191 bytes;
- AVD `Novels_Pixel_7_API_34`: install/start/foreground и ordered
  `app.started -> catalog.loading -> catalog.ready` passed за 8.263 s;
- licensing recovery guard без exact PID корректно отказал до mutation.

Additional validation:
- причина прежнего 300 s timeout подтверждена: финальный trigger `up_to_date`
  ошибочно перепроверялся через `recompile_status=idle`, а cold start начинался
  до реального Pipeline readiness;
- helper ждёт ready state, сохраняет retryable trigger errors и принимает
  финальный trigger без лишнего polling;
- warm gate passed за 2.209 s; два независимых cold-start gate passed за
  12.032 s и 11.442 s; helper tests 26/26.

All-project cold-start `editor-gate --compile` passed: catalog 12.769 s, template
  19.398 s, GPL 21.195 s, TZM 11.010 s, ZDM 11.246 s;
- у всех ready/no compile/no reload, 0 new Console errors, clean scene,
  `unexpectedChanges=false`; Editors/helpers завершены. Pending: none.

## 2026-08-29T11:35:00Z — token-efficient-unity-workflow — ready-for-integration

Task: сократить токенный расход Unity-задач через changed-path gates,
агрегированный Editor check, компактные результаты и ротацию runtime state.

Changed:
- `Tools/novels-tools`: добавлены `plan`/`verify` и dependency-free planner.
- `Tools/unity-mcp-helper`: добавлен one-call `editor-check`.
- Build/MCP/coordination docs: закреплены минимальные gates и log budgets.
- Старый полный handoff перенесён в датированный архив.

Validation:
- Planner tests: 5/5 passed.
- MCP helper tests: 24/24 passed.
- `zsh -n`, CLI plan smoke и scoped `git diff --check`: успешно.
- `HANDOFF.md`: 104 строки; полный прежний текст сохранён в архиве (988 строк).

Pending / risks:
- Реальный Editor не запускался: tooling проверен unit/static уровнем; первый
  следующий Unity scope должен использовать `editor-check` как live smoke.

Suggested next step:
- Использовать `novels-content plan`, затем минимальный gate; не запускать full
  matrix без changed-path основания.

## 2026-08-29T13:26:26Z — docs-contract-reconciliation — completed

Task: согласовать whole-variant, legacy runtime и licensing документы.
Changed: новый art отделён от legacy layered compatibility; manual/Ink/
architecture синхронизированы; automatic recovery больше не удаляет sockets;
для FIFO, content commands и texture profile указаны канонические источники.
Validation: `Tools/somegame docs-check` passed; terminology review clean.
Pending / risks: one-address runtime adapter ещё не реализован; Unity не запускался.
Suggested next step: интегрировать adapter отдельным code scope либо none для docs.

## Ready for integration

## 2026-08-29T13:36:18Z — full-tree-integration — completed

Task: разбить всё dirty tree на атомарные commits и отправить в `origin/main`.
Changed: создано 8 scoped commits от docs/tooling/Ink до runtime/GPL/settings;
generated duplicate Ink JSON сохранены в ignored `Build/IntegrationDiscarded`.
Validation: docs/tooling tests, doctor и scoped diff-check passed; свежие Unity/
Android gates перечислены выше. Hub PID 40613 не закрывался без разрешения.
Pending / risks: manual visual gates из отдельных ready scopes остаются
platform-specific evidence; общий push выполнен без закрытия работающего Hub.
Suggested next step: none. GitHub push `86c2002f..21d1859e` completed.

### ink-domain-reload-fix

- Ink Unity Integration v1.2.2 закреплён локально в
  `Packages/InkUnityIntegration`; все шесть manifest/lock пар используют его.
- Persisted `Compiling` после domain reload безопасно возвращается в `Queued`.
- TZM и ZDM прошли по два чистых startup cycle без Ink timeout/Progress error.
- Интегрировать package и шесть manifest/lock пар атомарно.

### runtime-smoke-telemetry

- Добавлены компактные `[NOVELS_SMOKE]` JSON events для app/catalog/story/
  episode/dialogue/choice/completion/error и `fallback.used`.
- Unity compile и scoped static checks прошли; Android logcat flow ещё не
  проверен.
- Следующий Embedded APK smoke должен считать `fallback.used` блокирующим и
  собирать screenshot/full logcat/activity только при failure.

### android-embedded-emulator

- Embedded Android использует debug signing только для development smoke и
  читает APK StreamingAssets через `UnityWebRequest`.
- APK ранее собран, установлен и запущен; catalog/story runtime достигнут.
- Source changes не закоммичены; нужен scoped integration commit и новый smoke
  по `[NOVELS_SMOKE]` после текущих runtime-изменений.

### character-whole-variants

- Текущий production contract: один цельный полнофигурный PNG на сочетание
  позы, одежды и эмоции; лицо и причёска фиксированы.
- Нужен runtime adapter, выбирающий ровно один полный sprite address без
  наложения face/hair/clothes layers.

### wardrobe-interaction-fix

- Backdrop вынесен под character canvas; вкладки face/hair/clothes/accessory
  интерактивны, preview работает.
- Нужен ручной Play Mode gate на 1080×1920.

### catalog-mockup-parity

- Schema 2, genre, Safe Area, CTA и page indicator реализованы.
- Нужен один агрегированный Editor/Play Mode gate; SDK+Game+Catalog+cards
  интегрировать атомарно.

## Blocked / limitations

### gpl-clothes-only-story

- GPL Ink фиксирует внешность/волосы и предлагает выбор только одежды; runtime
  Ink/source map пересобраны.
- Unity validation ранее остановилась на Licensing Client; повторить только
  `verify` для GPL после появления трёх цельных clothing sprites.

### gpl-character-art

- Персонажные assets не импортированы. Прежний layered набор не соответствует
  принятому whole-variant contract.
- Требуются утверждённые цельные Lea/Mark/Vera варианты; старый API-key blocker
  относится только к отвергнутому mask workflow и не меняет новый контракт.

### WebGL prototype

- Branch `prototype/webgl-local-platform`, commit `cfb92896` отсутствует в
  `main`; compilation и browser smoke не выполнены.

## Runtime rules

- Перед работой проверить этот snapshot, полный локальный `git status`, FIFO и
  точного lock owner; утверждения перепроверять по текущим файлам.
- Архив читать только для конкретной истории/регрессии.
- Успешные build/Editor/ADB результаты передавать компактно; полные логи читать
  только при failure.
- `HANDOFF.md` снова ротируется до превышения 200 строк.
