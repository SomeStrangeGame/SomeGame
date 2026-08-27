# Parallel work: tzm-unused-video-poster-label

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, Inspector-метка video posters
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- `Novels/Docs/AI/ParallelWork.tzm-unused-video-poster-label.md`
- собственные coordination-файлы и новая запись в
  `Novels/Docs/AI/CoordinationRuntime/HANDOFF.md`

## Не изменять

- serialized schema `NovelContentAsset`
- `Projects/novels-tzm/Assets/tzm.asset`
- chunks, PNG, MP4, audio и их `.meta`
- ZDM и runtime воспроизведения видео

## Изменённые контракты

- Добавлена только вычисляемая Editor-метка для unused location PNG, которым
  соответствует direct или alias-resolved MP4; serialized schema не менялась.

## Выполнено

- Определена точка расширения: `DrawUnusedAssets` / `DrawAsset` существующего
  custom Inspector.
- В строке такого PNG показывается бейдж `Постер видео` с tooltip; help box
  объясняет вычисляемое правило.
- Набор video IDs кэшируется на время жизни Inspector и инвалидируется при
  изменении проекта или aliases.

## Проверено

- Статическая сверка TZM: 56/56 unused GUID получают метку по 51 direct MP4 и
  5 aliases.
- Unity Roslyn compile `Novels.ContentSdk.Editor`: успешно.
- SHA-256 `tzm.asset` остался `796dceff…a78a96c`; serialized content не менялся.
- Scoped whitespace check: успешно.

## Требуется при интеграции

- Визуальный Inspector smoke после Unity refresh; bundle rebuild не требуется,
  потому что изменение только Editor UI.
