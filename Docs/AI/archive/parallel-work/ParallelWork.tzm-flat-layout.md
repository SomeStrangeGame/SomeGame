# Parallel work: TZM flat authoring layout

- Статус: ready-for-integration
- Ветка: experiment/story-preview-streaming
- Базовый commit: bfee19aac01ce2ec39f113265b79dfd255f12260
- Ответственный поток: текущий чат, упрощение физических папок TZM
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs`
- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentAssetEditor.cs`
- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Packages/NovelsContentSdk/Editor/CharacterSpriteAlphaTrim.cs`
- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`
- `Projects/novels-tzm/Assets/**`
- `Projects/novels-tzm/README.md`
- `Docs/AI/ContentPipeline.md`
- `Docs/AI/ContentAuthoringGuide.md`
- собственные coordination-файлы и запись в `CoordinationRuntime/HANDOFF.md`

## Не изменять

- `Projects/novels-zdm/**`
- `Projects/novels-catalog/**`
- `Novels/Assets/Novels/**`
- остальные shared packages и tools
- чужие coordination status-файлы

## Изменённые контракты

- План: физические authoring-пути TZM становятся короткими (`Assets/Ink`,
  `Assets/Characters`, `Assets/Locations`, `Assets/Choices`,
  `Assets/Presentation`, `Assets/Video`), а прежние runtime-адреса сохраняются
  через build-time mapping.
- Legacy layout ZDM и Catalog продолжает поддерживаться без миграции.

## Выполнено

- TZM перенесён в короткую физическую структуру: `Assets/tzm.asset`, `Ink`,
  `Characters`, `Locations`, `Choices`, `Presentation`, `Video`.
- Все файлы и каталоги перенесены вместе с существующими `.meta`; старые
  `RemoteAssets`, `StreamingAssets`, `content/tzm` и `story` удалены.
- SDK отображает короткие физические пути в прежние Unity bundle addresses и
  namespaced file paths; release/runtime контракт не изменён.
- Inspector, authoring, validation, streaming plan, texture postprocessor и
  alpha trim поддерживают новый layout; legacy ZDM/Catalog сохраняют прежний.
- Удалён неиспользуемый untracked `tzm.ink.chunks.json` с meta.
- README и общая authoring/pipeline документация обновлены.

## Проверено

- Unity Roslyn по актуальному TZM `Novels.ContentSdk.Editor.rsp` — успешно,
  без ошибок и предупреждений.
- `novels-content doctor` — успешно.
- Полное отображение bundle keys: 1270 старых адресов = 1270 новых mapping,
  missing/extra 0/0.
- Полное отображение file payload keys: 61 = 61, missing/extra 0/0.
- Все 1027 уникальных GUID ручной chunk-разметки имеют существующий `.meta`;
  дубликатов GUID и файлов/каталогов без meta не найдено.
- Scoped `git diff --check` — успешно.

## Требуется при интеграции

- Unity refresh открытого TZM Editor и последовательная пересборка TZM bundle.
- `validate tzm` и bundle build не запускались: открытый Editor PID 97689
  владеет `Projects/novels-tzm/Temp/UnityLockfile`; второй Unity запрещён.
