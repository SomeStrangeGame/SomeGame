# Agent: fallback-bubble-android-layout

- Status: completed-with-limitations
- Task: исправить перекрытие текста и choice-кнопок fallback bubble на Android, пересобрать Embedded APK и пройти GPL s01e01 до конца либо документированного блокера.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/BubbleScreen.cs`, `Novels/Assets/Novels/Fallbacks/EpisodeUI/bubble/screen-variant.prefab`, focused tests, generated `Novels/Build/**`, emulator runtime/evidence, own coordination records and handoff.
- Constraints: не менять TZM/custom story bubble prefabs; shared runtime behavior remains opt-in through the fallback prefab flag.
- Base commit: `d0d986021b47`
- Started UTC: 2026-08-30T17:46:17Z
- Heartbeat UTC: 2026-08-30T18:10:06Z
- Acceptance: line 80 choices flow below dialogue on Android; APK builds/installs/launches; GPL story is exercised to its ending or a precisely recorded blocker; no new blocking runtime errors.
- Result: fallback bubble now measures each header/dialogue `Text.preferredHeight` before rebuilding its parent layout. The opt-in flag remains enabled only in the game fallback prefab; TZM/custom story prefabs were not changed. Fresh Novels compile passed. Android Embedded development APK (1,849,899,715 bytes) built and installed; line 80 visually passed with all three choices below the dialogue, and the single-category wardrobe at line 132 passed. At the user's stop request the run had reached line 339 of 601; the app was force-stopped and the AVD left running.
- Evidence: `Novels/Build/Logs/automation/gpl-android-line80-after-fix-v2.png`, `gpl-android-wardrobe-after-fix.png`, `gpl-android-stop-state.png`, `gpl-android-after-fix-logcat.txt`; APK `Novels/Build/Players/automation/Android/Embedded/Novels.apk`.
- Remaining: the run did not reach the episode ending. Logcat contains one non-blocking `fallback.used` event for character `Лея` (`required_character_assets_missing`) around the mid-episode run; no `INITIALIZATION_FAILED`, `CONTENT_PREPARATION_FAILED`, `NullReferenceException`, `MissingReferenceException`, or fatal crash marker was found.
