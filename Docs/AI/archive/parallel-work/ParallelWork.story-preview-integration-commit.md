# Parallel work: story-preview integration commits

- Статус: integrated
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, интеграция накопленного story-preview дерева
- Последнее обновление: 2026-08-27

## Разрешённая область

- Все уже изменённые и untracked файлы внутри `Novels/Assets/Novels/**`,
  `Novels/ProjectSettings/ProjectSettings.asset`, `Packages/Bundles/**`,
  `Packages/NovelsContentSdk/**`, `Projects/novels-tzm/**`, текущий
  `Projects/novels-zdm/.../zdm.asset` и `Docs/AI/**`.
- Git index и создание последовательных коммитов на текущей ветке.
- Собственные coordination-файлы и новая запись `HANDOFF.md`.

## Не изменять

- Остальные проекты и пользовательские файлы вне текущего `git status`.
- `Library`, `Temp`, `Logs`, `Build`, generated IDE-файлы.
- Не использовать Git LFS, не переписывать историю и не выполнять destructive
  reset/clean/checkout.

## Изменённые контракты

- Новых продуктовых контрактов в этом блоке нет; задача только интегрирует уже
  готовые изменения логическими коммитами.

## Выполнено

- Предварительный audit: 6097 status records; 2953 из 3040 untracked файлов
  переиспользуют существующие Git blobs на 1 506 031 045 байт.
- Реально новых untracked blobs: 87 файлов / 351 420 байт; максимальный файл
  рабочего дерева 18 354 220 байт, LFS не требуется.
- Созданы логические коммиты:

  - `3c1a2e7d` — runtime/Game contracts и ZDM definition;
  - `a81f79fb` — общий Editor authoring/streaming pipeline;
  - `05dc9494` — плоская TZM-структура с 2955 распознанными rename;
  - `62bcf9e3` — authoring-документация и завершённые coordination-записи.
- В Unity YAML/meta перед TZM-коммитом удалён только trailing whitespace;
  актуальный SHA-256 `tzm.asset` —
  `95032a7a122c6bf2c0159e76ae86f51ab96e76c0d992ee448297ff73ce3e4882`.

## Проверено

- Unity 6000.3.11f1 Roslyn `Novels.ContentSdk.Editor`: успешно.
- `novels-content doctor`: успешно.
- Детерминированный TZM layout: 12 чанков, 416 GUID, 51/51 video, 56 unused
  posters; duplicate/unused overlap/media-only chunks — 0.
- `git diff bfee19aa..HEAD --check`: успешно.
- В committed tree нет Git LFS pointers и generated `Library/Temp/Build`.

## Требуется при интеграции

- Полный `validate tzm` и `build tzm editor` остаются после refresh/закрытия
  пользовательского Unity: PID 97689 продолжает владеть UnityLockfile, второй
  Unity не запускался.
