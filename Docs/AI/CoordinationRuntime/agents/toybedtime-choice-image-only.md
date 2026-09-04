# Agent: `toybedtime-choice-image-only`

- Status: ready-for-integration
- Task: two large horizontal image-only choices in toybedtime.
- Scope: `Projects/novels-toybedtime/Assets/Presentation/bubble/screen-variant.prefab`, opt-in layout fields in `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs`, generated toybedtime release, runtime-only evidence, own coordination records and handoff.
- Constraint: ordinary Bubble and fallback retain their serialized defaults and remain visually unchanged.
- Requested UTC: `2026-09-03T16:52:58Z`.
- Acquired UTC: `2026-09-03T16:52:58Z`.
- Scope expanded UTC: `2026-09-03T16:55:00Z`; runtime evidence shows `PlaceButtons` always forces a vertical stack, so the prefab-only target requires an opt-in horizontal layout flag. Defaults preserve every existing Bubble.
- Completed UTC: `2026-09-03T17:03:52Z`.
- Implementation: `BubbleScreen` has opt-in horizontal placement and visual text hiding, both disabled by default. Toybedtime enables both; its authored choice button is `184x160`, while `ChoiceIcon` stretches inside with 12 px padding on every side.
- Validation: toybedtime validate and Android content build passed; Embedded development APK rebuilt successfully at `Novels/Build/Players/toybedtime-choice-image-only/Novels.apk` (2,157,051,554 bytes). Android runtime reached `s01e01.ink:43`; `Novels/Build/Logs/toybedtime-choice-image-only-line43.png` confirms two large image-only cards in one horizontal row. Unity error-only log is empty, app stopped, AVD remains running. The separate MCP Editor startup gate timed out waiting for its Pipeline port, but the subsequent full player build compiled successfully.
