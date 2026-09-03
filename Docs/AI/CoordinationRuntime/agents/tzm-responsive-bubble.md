# Agent: `tzm-responsive-bubble`

- Status: ready-for-integration
- Task: заменить специальные runtime-поправки финального бабла адаптивной геометрией базового bubble prefab и сохранить визуальный TZM variant.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs`, `Packages/NovelsContentSdk/BaseUI/Base/bubble/screen.prefab`, `Projects/novels-tzm/Assets/Presentation/bubble/screen-variant.prefab`, узкие bubble/UI tests при необходимости, собственные coordination records и handoff.
- Contract: header, body и любое число choice-кнопок должны раскладываться по содержимому без проверки конкретного текста; короткие диалоги и бабл над кат-сценой сохраняют штатный вид; TZM variant хранит только визуальные overrides; GUID/fileID сохраняются.
- Boundaries: не менять Ink, location/cut-scene runtime, generated content вручную и unrelated dirty changes.
- Base commit: `0dc0ef2000bd10149c3736fa572253f5d70d81ba`.
- Requested UTC: `2026-09-03T07:16:13Z`.
- Planned validation: scoped prefab/serialization audit, `git diff --check`, targeted tests if available, TZM content validation/build, fresh Unity compile and portrait visual replay of lines 68, 94, 171 and episode end.
- Validation: scoped prefab/serialization audit and `git diff --check` passed; `novels-content validate tzm` and `build tzm editor` passed; Unity 6000.3.11f1 fresh compile passed without compiler errors; matching EditMode filter returned no tests.
- Pending: portrait visual replay of lines 68, 94, 171 and episode end.
