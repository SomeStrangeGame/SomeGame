# Parallel work: story-global-content

- Статус: ready-for-integration
- Ветка: grandChange
- Базовый commit: c6c7853b
- Ответственный поток: story-global-content
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/ContentAddressing/**`
- `Packages/NovelsContentSdk/Runtime/Features/Character/**`
- точные runtime loaders в `Novels/Assets/Novels/**`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/**`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/**`
- importer/validation-файлы Content SDK, необходимые для нового корня
- этот status-файл и собственные runtime coordination files

## Не изменять

- `Projects/novels-catalog/**`
- Ink, video и audio payloads
- остальные Game и SDK файлы

## Изменённые контракты

- На историю остаётся один AssetBundle `novels_content_<story>`.
- Unity assets используют один корень `content/<story>/story/**`.
- Episode/shared адресация и fallback удаляются; Game fallback остаётся только
  для отсутствующего presentation asset.
- Ink, video и audio остаются отдельными content-addressed payloads.

## Атомарные блоки

1. Runtime address API и loaders без перемещения assets.
2. Миграция и проверка ZDM.
3. Миграция и проверка TZM.
4. Texture compression и сравнение размеров.

## Выполнено

- Обновлён протокол ограниченного ожидания FIFO.
- Подтверждено отсутствие одноимённых локаций с разным содержимым.
- Runtime address API и loaders переведены на `story/**`; episode/shared
  fallback удалён.
- ZDM: `shared` переименован в `story`, уникальные локации объединены в
  `story/location/locations`, 12 файлов-дубликатов вынесены из проекта.
- TZM: выполнена та же миграция; вынесено 29 точных location-дубликатов.
- Android/iOS importer использует ASTC 6×6, Max Size 4096, quality 100.

## Проверено

- `git diff --check` для coordination block — успешно.
- `Tools/novels-tools/novels-content validate zdm` — успешно.
- `Tools/novels-tools/novels-content build zdm editor` — успешно.
- Исключённые ZDM-дубликаты: 64 409 963 байта исходных PNG; обратимая копия
  находится в `/tmp/novels-zdm-story-global-20260824T1115Z`.
- `Tools/novels-tools/novels-content validate tzm` — успешно.
- `Tools/novels-tools/novels-content build tzm editor` — успешно.
- `Tools/novels-tools/novels-content build zdm android` — успешно;
  111 465 312 B против baseline 311 892 533 B (−64,3%).
- `Tools/novels-tools/novels-content build tzm android` — успешно;
  304 933 451 B против baseline 488 767 379 B (−37,6%).
- Исключённые TZM-дубликаты: 158 357 019 байт исходных PNG; обратимая копия
  находится в `/tmp/novels-tzm-story-global-20260824T1121Z`.
- `git diff --check` — успешно.

## Требуется при интеграции

- Ручной визуальный quality gate ASTC на Android/iOS.
- iOS build для фиксации фактической delta; platform override задан, но в этой
  итерации собирался только Android.
