# Parallel work: tzm-video-posters-unused

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, постеры видеолокаций TZM
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Content/NovelContentAsset.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Runtime/Features/Location/BackgroundPresentationController.cs`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- `Novels/Docs/AI/ParallelWork.tzm-video-posters-unused.md`
- собственные coordination-файлы и новая запись в `Novels/Docs/AI/CoordinationRuntime/HANDOFF.md`

## Не изменять

- `Projects/novels-zdm/**`
- PNG и `.meta` постеров
- чужие coordination-файлы

## Изменённые контракты

- На уровне истории добавлена скрытая authoring-группа GUID «Не используется»;
  она не входит в runtime definition и сохраняется при пересчёте чанков.
- Ассет не может одновременно находиться в чанке и в «Не используется».
- Runtime сначала разрешает видео: при его отсутствии загружает статический
  PNG, при наличии готовит первый кадр за однотонным переходным экраном.

## Выполнено

- Зафиксировано правило: PNG остаётся в чанках, если для локации нет видео.
- В Inspector добавлена редактируемая группа `Не используется` и отдельная
  группа в линейном отчёте.
- 56 TZM-постеров с реальными видео или video aliases исключены из чанков;
  девять статических PNG сохранены.
- Разметка TZM сокращена с 44 до 39 валидных чанков без потери media entries.
- PNG и `.meta` постеров не изменялись и не удалялись; ZDM не изменялся.

## Проверено

- Unity Roslyn `Novels.Content`, `Novels.Location`,
  `Novels.ContentSdk.Editor` — успешно.
- Статическая проверка TZM: 51 MP4, 5 aliases, 56 unused-постеров,
  9 статических локаций; overlap/duplicates/media-only chunks отсутствуют.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` для tracked diff — успешно; отдельная проверка нового
  Editor-файла не нашла whitespace-ошибок.
- В `tzm.asset` остаются четыре старых Unity YAML trailing spaces вне
  изменённой authoring-разметки.

## Требуется при интеграции

- Unity refresh и последовательная content validation/build после освобождения открытого TZM Editor.
- В Inspector `Assets/tzm.asset` проверить группу `Не используется (56)`.
- Выполнить runtime smoke одной видеолокации и одной статической локации без
  MP4; для видео не должен запрашиваться PNG-постер.
