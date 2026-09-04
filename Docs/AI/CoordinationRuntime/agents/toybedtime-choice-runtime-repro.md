# Agent: `toybedtime-choice-runtime-repro`

- Status: ready-for-integration
- Task: воспроизвести в Unity и исправить невидимые подписи и чрезмерную ширину illustrated choice-кнопок toybedtime.
- Scope: `Projects/novels-toybedtime/Assets/Presentation/bubble/screen-variant.prefab`, story-local `Projects/novels-toybedtime/Assets/Presentation/bubble/liberationsans-regular.ttf` plus metadata, generated toybedtime editor release, runtime-only Unity evidence, own coordination records and handoff.
- Constraint: shared Bubble/runtime and other stories remain unchanged unless runtime evidence proves a shared regression and scope is explicitly expanded before editing.
- Validation: live Unity or Android Embedded runtime evidence at `s01e01.ink:43`, toybedtime validate/build, attached Novels compile, portrait screenshot, scoped verify/diff-check.
- Base: `1daa8129`.
- Requested UTC: `2026-09-03T16:20:47Z`.
- Acquired UTC: `2026-09-03T16:24:45Z`.
- Scope expanded UTC: `2026-09-03T16:26:00Z` after build evidence proved the prefab font GUID resolves only inside the Novels project and is null in the atomic story project.
- Validation route expanded UTC: `2026-09-03T16:28:30Z` to an ignored Android Embedded APK/emulator smoke after macOS accessibility denied programmatic Unity Play Mode input.
- Paused UTC: `2026-09-03T16:35:50Z`.
- Evidence: all Bubble Text components referenced a font GUID available only under `Novels/Assets`, so the atomic story build resolved it to null. A story-local Liberation Sans with GUID `0f9e6c86b9464fa685edec863513a62d` is now referenced by every Bubble Text; the Android bundle includes the 344.5 KB font and prefab. Button width is 260 px. Toybedtime editor/android builds and Novels compile passed; Embedded development APK built successfully at `Novels/Build/Players/toybedtime-choice-runtime/Novels.apk`.
- Runtime blocker: Pixel AVD has 2.7 GB free and cannot install the 2.15 GB streamed APK; LLM AVD has 4.3 GB free but never completes boot or exposes Android package service. Resetting/wiping a test AVD requires user approval because it deletes emulator-only apps and saves.
- Emulator reset approved UTC: `2026-09-03T16:36:30Z`.
- Resumed/acquired UTC: `2026-09-03T16:45:57Z`.
- Completed UTC: `2026-09-03T16:51:30Z`.
- Runtime evidence: approved reset of `Novels_Pixel_7_API_34` completed with a 16 GB data partition; the existing Embedded APK installed and reached `s01e01.ink:43`. Portrait evidence at `Novels/Build/Logs/toybedtime-choice-runtime-line43.png` shows the question, both labels, both icons, correct wrapping, and compact centered 260 px buttons. The app was force-stopped after validation; the AVD remains running and responsive.
- Result: visual runtime gate passed. No shared Bubble/runtime changes were required for this correction.
