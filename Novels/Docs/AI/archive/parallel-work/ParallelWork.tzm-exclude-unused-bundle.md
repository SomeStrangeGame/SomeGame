# Parallel work: TZM exclude unused assets from bundle

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `tzm-exclude-unused-bundle`
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- `Novels/Docs/AI/archive/parallel-work/ParallelWork.tzm-exclude-unused-bundle.md`
- собственные coordination-файлы и новая append-only запись в
  `Novels/Docs/AI/CoordinationRuntime/HANDOFF.md`
- игнорируемые generated build outputs/logs TZM

## Не изменять

- `Projects/novels-tzm/Assets/**`, включая PNG, `.meta` и `tzm.asset`
- `Projects/novels-zdm/**`
- import settings и алгоритмы сжатия
- чужие coordination-файлы

## Изменяемый контракт

- Story Unity-ассеты из authoring-группы `Не используется` остаются в проекте,
  но не становятся root assets обычного story bundle.
- Catalog и story-проекты без unused-группы сохраняют прежний состав.

## План проверки

- статически подтвердить 56 исключённых TZM GUID и отсутствие unresolved paths;
- скомпилировать Editor assembly;
- выполнить `validate tzm` и `build tzm editor` последовательно;
- сравнить root asset count и bundle size с baseline 555 / 188 315 864 B;
- выполнить `doctor` и scoped `git diff --check`.

## Выполнено

- Обычная и streaming story-сборка получают прежний полный список доступных
  Unity-ассетов, но перед созданием bundle roots исключают пути из
  `_authoringUnusedAssetGuids`.
- Несовпадение числа найденных roots и unused GUID останавливает сборку вместо
  молчаливого частичного исключения.
- Authoring-поиск, Inspector и проверка доступности продолжают видеть полный
  source tree; `tzm.asset`, PNG, `.meta` и import settings не менялись.
- Документация уточняет, что `Не используется` не публикуется среди roots
  обычного story bundle.

## Проверено

- Статический TZM audit: 555 исходных roots, 56/56 unused GUID разрешены,
  результат фильтра — 499 roots, overlap/missing 0/0.
- Поиск serialized dependencies вне `tzm.asset`: 0 ссылок на 56 исключаемых
  GUID.
- Unity Bee/Roslyn успел успешно скомпилировать
  `Novels.ContentSdk.Editor.dll`; C# errors отсутствуют.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` — успешно.

## Требуется при интеграции

- Штатные `validate tzm` и `build tzm editor` не завершены: Unity Licensing
  Client зациклился на `Unsupported protocol version '1.18.0'`. Зависший
  процесс остановлен, оставленный им не удерживаемый `Temp/UnityLockfile`
  удалён.
- После исправления/перезапуска Licensing Client повторить validation/build и
  подтвердить audit на 499 roots и фактическую delta bundle относительно
  188 315 864 B.
