# Parallel work: Novels Editor throttle 5 relaunch

- Статус: ready-with-limitations
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `novels-throttle-5-relaunch`
- Последнее обновление: 2026-08-28

## Результат

- Предыдущий Unity process проекта `Novels` завершён после явного запроса
  пользователя на перезапуск.
- Новый Unity Editor 6000.3.11f1 открыт на том же проекте и сцене
  `Assets/Novels/Novels.unity`.
- Process environment подтверждает `NOVELS_SIMULATED_MBITS=5`,
  `NOVELS_SIMULATED_LATENCY_MS=0`, `NOVELS_SIMULATED_JITTER_MS=0` и
  `NOVELS_STREAMING_EXPERIMENT=1`.
- Initial refresh/compile завершён; C# compilation errors не обнаружены.
- Content release не пересобирался, production source не менялся,
  `git diff --check` успешен.

## Ограничения

- Ручной Play Mode smoke выполняет пользователь в оставленном открытым Editor.
- Неблокирующие Unity Services TLS/403 warnings сохранились от предыдущего
  запуска.
