# Parallel work: TZM streaming throttle launch

- Статус: ready-with-limitations
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `tzm-streaming-throttle-launch`
- Последнее обновление: 2026-08-28

## Цель

Пересобрать актуальный TZM streaming release и открыть основной проект Novels
с воспроизводимой Editor-симуляцией канала 20 Мбит/с.

## Результат

- `NOVELS_STREAMING_EXPERIMENT=1 novels-content build tzm editor`: exit 0.
- Release `491eeb83168e25be15252020f666421b2690c9d9f7054abeccbfdcd7370eb072`:
  12 art bundles/chunks (`chunk-0..11`), 51 media entries в 12 media-группах.
- `chunk-0` является стартовым чанком; отдельный `preview` в актуальном
  streaming contract отсутствует.
- Project release и composed release совпадают; 12 bundles занимают
  96 582 258 B, весь delivery с media/Ink — 361 516 350 B.
- Unity Editor 6000.3.11f1 открыт на проекте `Novels` и сцене
  `Assets/Novels/Novels.unity` с `NOVELS_SIMULATED_MBITS=20`, latency/jitter 0.
- Unity import/compile завершён; C# compilation errors не обнаружены.
- Ink snapshot до/после сборки совпал; Unity-only whitespace в 51 video
  `.meta` возвращён к исходному тексту; `git diff --check` успешен.

## Ограничения

- Ручной Play Mode cold/warm smoke оставлен пользователю в открытом Editor.
- В Editor log есть неблокирующие Unity Services `Curl error 60`/403; import и
  compilation завершились успешно.
