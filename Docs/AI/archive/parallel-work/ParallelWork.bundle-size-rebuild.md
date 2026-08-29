# Parallel work: current editor bundle size rebuild

- Статус: ready-with-limitations
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `bundle-size-rebuild`
- Последнее обновление: 2026-08-28

## Цель

Последовательно пересобрать Mac/Editor bundles TZM и ZDM после текущей серии
оптимизаций и измерить точную delta к существующим эквивалентным артефактам.

## Baseline до пересборки

- TZM: 188 315 864 B, version `e28c94ce1cd82ecdc99ea2ed84836e51`,
  SHA-256 `9ee70bc454b10028783431c48779ba5f51fa78fbfb94b061fd4236cce7c273a3`.
- ZDM: 149 133 366 B, version `69f4945a9496798cdaa5af663241d80f`,
  SHA-256 `69b3d7ddf237c8dd2ad0f4b48d4713ecfe26854273ca5ea1f410fbb80e22c3f8`.

## Разрешённая область

- игнорируемые build/log/Library artifacts TZM и ZDM, создаваемые штатным CLI;
- `Novels/Build/LocalContent`, обновляемый штатным compose;
- `Docs/AI/ContentSizeBaseline.md`;
- собственные coordination status/runtime/handoff-файлы

## Не изменять

- Ink и authoring assets;
- SDK/runtime и import settings;
- Android/iOS artifacts: текущий запрос измеряет эквивалентный Editor/Mac
  baseline;
- чужие незавершённые изменения

## Команды

1. `Tools/novels-tools/novels-content build tzm editor`
2. `Tools/novels-tools/novels-content build zdm editor`

## Критерии

- обе сборки и встроенный bundle audit успешны;
- новые bundle/release существуют, size/hash согласованы;
- delta рассчитана в bytes, MiB и процентах;
- Ink hashes и source diff не изменились от запуска Unity.

## Результат

- TZM: 188 315 864 B → 130 621 560 B; −57 694 304 B
  (−55,022 MiB; −30,637%). Version
  `9e55f2e8a566d2780b3bfb7e925f7f61`, SHA-256
  `16fdbcf0d541bcae07a2e76dffba69a2a2a1b7997e1506ffd559eb64f6004e08`.
- ZDM: 149 133 366 B → 132 260 907 B; −16 872 459 B
  (−16,091 MiB; −11,314%). Version
  `4a10bd8a52ef9556c9352bcd4a468235`, SHA-256
  `fb699c2d1e305508668fd5002da4a2f9b0477a22bcab526137ab62f27c236d62`.
- Итого bundles: 337 449 230 B → 262 882 467 B; −74 566 763 B
  (−71,112 MiB; −22,097%).
- Обе Unity batchmode-команды завершились с exit code 0; bundle audit прошёл
  для 454 TZM и 452 ZDM root assets.
- Project output, story release и composed release согласованы по size/hash.
- Ink SHA-256 до и после сборки совпали.
- Unity переписала пробелы в 51 video `.meta`; этот форматирующий шум возвращён
  к исходному тексту, после чего `git diff --check` прошёл.

## Ограничения

- Пересобран только эквивалентный Editor/Mac target; Android/iOS требуют
  отдельной последовательной пересборки перед мобильным release.
- Runtime smoke test не выполнялся; вывод подтверждает сборочную целостность и
  фактический размер артефактов.
