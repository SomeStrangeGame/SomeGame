# Parallel work: tzm-precise-usage-order

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, точный порядок streaming chunks TZM
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/Novels.ContentSdk.Editor.asmdef`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- `Novels/Docs/AI/archive/parallel-work/ParallelWork.tzm-precise-usage-order.md`
- собственные coordination-файлы и новая запись в `Novels/Docs/AI/CoordinationRuntime/HANDOFF.md`

## Не изменять

- `Projects/novels-zdm/**`
- PNG, MP4, аудио и их `.meta`
- runtime воспроизведения видео
- чужие coordination-файлы

## Изменённые контракты

- Первое использование фона, video и audio определяется только разобранными
  командами через существующий `Novels.StoryCommands` parser.
- Video aliases разрешаются до сопоставления физического MP4.
- Character art сопоставляется с точным speaker/asset candidate и отдельными
  вариантами wardrobe; имя персонажа в обычном тексте больше не активирует всю
  его папку.

## Выполнено

- Подтверждена причина: chunks 0–2 заполнены 147 character assets из-за совпадения широкого токена имени персонажа; video scan также читает metadata/dialogue.
- `ExperimentalStreamingPlan.cs` переведён на command-aware usage index; для
  остальных PNG fallback ограничен самым конкретным токеном файла.
- `StoryInkAuthoring.cs` передаёт runtime definition с aliases; Editor asmdef
  получил зависимости `Novels.StoryCommands` и `Novels.StoryContracts`.
- `tzm.asset` детерминированно пересчитан: 24 → 18 чанков, 700 GUID назначены,
  все 51 MP4 сохранены, 56 poster GUID в `Не используется` не изменены.

## Проверено

- Unity Roslyn compile `Novels.ContentSdk.Editor`: успешно.
- YAML: 18 чанков, 700 назначений, duplicate/unused overlap 0; повторный
  пересчёт даёт тот же SHA-256.
- `номер в отеле`, `вид из окна`, `кафе` находятся в chunk 1; `атлантида`
  перенесена из ложного раннего совпадения в chunk 9; MP4 51/51.
- Scoped `git diff --check`: успешно.

## Требуется при интеграции

- Unity refresh, последовательные validate/build и Inspector smoke; второй
  Unity не запускался при существующем `Projects/novels-tzm/Temp/UnityLockfile`.
