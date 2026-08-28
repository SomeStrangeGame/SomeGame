# Parallel work: debug HUD initial chunk progress

- Статус: ready-with-limitations
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `debug-hud-initial-chunk-progress`
- Последнее обновление: 2026-08-28

## Цель

Вернуть delivery-данные debug HUD во время блокирующей стартовой загрузки
`chunk-0`, потерянные при отказе от отдельного preview-бандла.

## Изменение

- `ContentDeliveryFlow.PrepareGroup` одновременно обновляет bootstrap loading
  text и `StreamingExperimentDiagnostics`.
- Перед стартовой загрузкой HUD получает quality `Preparing` и queue
  `chunk-<index>`; для монолитного fallback используется delivery group.
- После создания `StoryStreamingController` существующая диагностика чанков и
  media продолжает владеть quality/queue без изменения контракта.

## Проверки

- Unity Editor 6000.3.11f1: initial refresh и compile завершены; C# errors нет.
- Editor переоткрыт с `NOVELS_SIMULATED_MBITS=5`, latency/jitter 0 и streaming
  flag 1.
- `novels-content doctor` и `git diff --check`: успешно.
- В focused source diff изменён только `ContentDeliveryFlow.cs`; Ink, bundles,
  ProjectSettings и authoring assets не менялись.

## Ограничения

- Финальная визуальная проверка значений выполняется вручную через
  `Play -> TZM -> Cold App`; автоматического управления Game View нет.
- Внешний `dotnet build --no-restore` не запустился из-за отсутствующего
  Unity-generated `Temp/obj/Novels/project.assets.json`; штатная Unity compile
  прошла успешно и является основной проверкой.
