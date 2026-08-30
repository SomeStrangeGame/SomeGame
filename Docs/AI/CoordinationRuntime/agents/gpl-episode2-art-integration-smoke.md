# Agent: gpl-episode2-art-integration-smoke

- Status: blocked
- Task: создать и интегрировать весь новый арт GPL episode 2, собрать Android Embedded APK и выполнить emulator smoke.
- Scope: new GPL locations `нижний пост`, `буровая камера`, `лаборатория керна`; new Pavel character sources and definition mapping; GPL generated content; Android player/log evidence; own coordination records and handoff.
- Asset list: exactly three 1664x936 landscape backgrounds matching episode-one source dimensions and one 1024x1536 transparent full-body Pavel master/neutral whole variant. Existing Lea/Mark/Vera art is reused; no extra variants.
- Base commit: `00c015e157240f312a2fc0464accdf2d372da21c`.
- Acceptance: GPL validate/editor+android build pass; no missing references for episode 2; Android Embedded APK installs, reaches catalog, opens GPL episode 2, activates release/episode/dialogue without character/background fallback.
- Validation: alpha/dimensions and visual inspection, content gates, APK artifact checks, structured Android smoke markers and cleanup.
- Started UTC: 2026-08-30T22:05:00Z
- Progress: generated and imported three backgrounds; generated Pavel master, rejected baked-checkerboard output, accepted background-extracted 1024x1536 RGBA output with transparent corners; definition default maps Pavel to `drilling` whole variant.
- Result: exact approved asset list is integrated with Unity `.meta`: three 1664x936 backgrounds and one transparent full-body Pavel whole variant. Existing Lea/Mark/Vera variants cover all other episode selectors.
- Image generation: built-in imagegen used with episode-one GPL background and Mark/Vera character references; final workspace paths are `Assets/Locations/{нижний пост,буровая камера,лаборатория керна}.png` and `Assets/Characters/павел/view/whole/drilling/main.png`.
- Validation passed: dimensions/alpha/corner checks; GPL validate; GPL editor build; compiled episode-two source map; Unity import created stable meta files and no content reference error was reported.
- Blocker: trim report and Android GPL content build both reproduced the same Unity Licensing IPC failure after one permitted recovery (`ObjectDisposedException: IServiceProvider`, missing `LicenseClient-iantonishin` channel). Batch processes were stopped and empty UnityLockfile removed. Android bundle, APK and emulator smoke could not be produced.
- Required resume: restore Unity account/licensing IPC through the Hub, then run trim report/apply if needed, GPL Android build, production release-set Android build, Embedded development APK, and episode-two smoke through `dialogue.ready` with no fallback markers.
