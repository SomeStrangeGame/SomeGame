# Parallel work: TZM exclude legacy Presentation art

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, очистка расчёта TZM chunks от legacy Presentation art
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Docs/AI/ContentAuthoringGuide.md`
- `Docs/AI/archive/parallel-work/ParallelWork.tzm-exclude-legacy-presentation-art.md`
- собственные coordination-файлы и новая запись в `Docs/AI/CoordinationRuntime/HANDOFF.md`

## Не изменять

- `Projects/novels-zdm/**`
- PNG, MP4, аудио и любые их `.meta`
- prefab и runtime presentation
- legacy-папки `Assets/Presentation/character/characters` и `Assets/Presentation/location/locations` физически не удалять
- чужие coordination-файлы

## Изменённые контракты

- Character art участвует в streaming layout только из поддерживаемых
  story roots `Assets/Characters/**` или legacy
  `*/story/character/characters/**`; похожие вложенные пути внутри
  `Assets/Presentation/**` считаются presentation dependencies, а не character
  roots.
- Старые `Presentation/character/characters/**` и
  `Presentation/location/locations/**` явно не проходят generic text fallback.

## Выполнено

- `ExperimentalStreamingPlan.cs` ограничивает распознавание character root и
  полностью игнорирует два legacy Presentation art-каталога при authored-use
  расчёте.
- Текущий `tzm.asset`, который открытый Editor уже успел пересчитать до 14
  чанков / 547 GUID, детерминированно пересчитан до 12 чанков / 416 GUID.
- Из чанков удалены ровно 131 legacy character PNG; сами 713 legacy PNG,
  их `.meta` и каталоги не изменялись и не удалялись.
- `ContentAuthoringGuide.md` закрепляет границу `Presentation` и настоящих
  story art roots.

## Проверено

- Unity 6000.3.11f1 Roslyn `Novels.ContentSdk.Editor`: успешно.
- Повторный расчёт: 12 чанков, 416 GUID, SHA-256
  `95032a7a122c6bf2c0159e76ae86f51ab96e76c0d992ee448297ff73ce3e4882`
  после whitespace-нормализации Unity YAML перед коммитом; состав разметки не
  изменился.
- 51/51 MP4 сохранены; 56 video posters остаются в `Не используется`, 9
  статических locations остаются в чанках.
- Duplicate GUID, unused overlap и media-only chunks: 0.
- Presentation audit: direct legacy roots 131 → 0; 10 prefab dependencies
  сохранены зависимостями prefab.
- `novels-content doctor` и scoped `git diff --check`: успешно.

## Требуется при интеграции

- Unity refresh и последовательная content validation/build после освобождения
  открытого TZM Editor.
