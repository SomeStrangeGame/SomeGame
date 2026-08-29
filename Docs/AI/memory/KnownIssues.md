# Known issues memory

Сюда попадают только воспроизводимые актуальные проблемы с подтверждённой
причиной или безопасным диагностическим маршрутом. Состояние конкретной задачи
остаётся в `../CoordinationRuntime/HANDOFF.md`.

## Unity Licensing / Connection Lost

- Symptom: Editor или batch mode не стартует, licensing IPC сообщает
  `Connection Lost`, protocol/mutex или Licensing Client error.
- Action: не удалять caches вслепую; сначала собрать свежие Editor/licensing
  logs и следовать evidence-first восстановлению.
- Source: [UnityLicensingTroubleshooting.md](../guides/UnityLicensingTroubleshooting.md).

## Official Unity MCP endpoint unavailable

- Symptom: package установлен, но namespace/endpoint отсутствует или descriptor
  устарел после restart/domain lifecycle.
- Action: подтвердить точный target Editor и transport; использовать checked-in
  fallback helper. Не запускать второй Editor.
- Source: [UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md).

## Console domain failures may have `log` level

- Symptom: Unity error-filter чист, но runtime не инициализировался.
- Cause: доменные markers вроде `INITIALIZATION_FAILED` могут быть записаны
  через `Debug.Log`.
- Action: `editor-check` должен анализировать свежую Console delta и failure
  markers, а не только entries уровня `error`.
- Source: [UnityMcpWorkflow.md](../guides/UnityMcpWorkflow.md#console-и-логирование-novels).

Завершённые или разовые проблемы удаляются отсюда; их история при необходимости
остаётся в `../archive/`.
