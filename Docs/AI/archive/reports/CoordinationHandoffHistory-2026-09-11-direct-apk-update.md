# Coordination handoff history — 2026-09-11 direct APK update

Completed records rotated verbatim from `Docs/AI/CoordinationRuntime/HANDOFF.md`
before the direct APK updater handoff was appended. Open or deferred records remain
in the current snapshot.

## 2026-09-10T11:58:33Z — remove-tzm-launch-branding — completed

Task: Removed the TZM artwork from the Android splash and default application icon, and deleted the obsolete shared Icon asset; branded player builds retain their own profile icon.
Changed: Novels/ProjectSettings/ProjectSettings.asset, Novels/Assets/Settings/Build Profiles/Android.asset, Novels/Assets/Icon.png, Novels/Assets/Icon.png.meta
Validation: finish-task passed (1 gates).
Pending / risks: editor-gate --compile, editor-gate --test-filter <affected-suite>, player-build --target <platform> --mode <Remote|Embedded>
Suggested next step: none

## 2026-09-10T15:24:00Z — chernaya-melnitsa-v3-server-main — completed

Task: Expanded Chyornaya Melnitsa to four episodes, built Android content without
emulator, published immutable v3 to Kostroma dev, and integrated scoped source.
Changed: `Projects/novels-chernaya-melnitsa` story source/archive; remote immutable
`stories/chernaya-melnitsa/3`; mutable `kostroma-dev.json`; Git main commit
`b6896bb7b188dc2f6350077ef18b70c80fbcd165`.
Validation: 72 literary routes/5 decisions/3 endings; Unity/Ink Android content
gate; 31-file local/remote checksum equality; exact public manifest/card/cover/
release/preview/episode-cover/bundle checks. No Player, APK build, ADB or emulator.
Publication: ordered map is `chernaya-melnitsa=3`, `trinadtsatyy-kolokol=1`,
`les-zabyvshiy-tropy=1`; manifest SHA `663b4c96c3e06eb0ca87895d6e2a2c6aa9447d5a9ff744821d0cdc6f06dde632`;
APK unchanged. Remote backup path is recorded in the agent handoff.
Late instruction: the user asked not to push after the push had already completed;
no destructive rollback or additional remote Git operation was performed.

## 2026-09-11T08:18:02Z — scoped-story-runtime — completed

Task: Implemented safe scoped resource keys and scoped heavy-workflow authorization without a global checkout lock; verified independent story owners, collision rejection, docs and all tooling suites.
Changed: Tools/somegame-tools/runner.py, Tools/somegame-tools/tests/test_runner.py, Docs/AI/guides/AutomationRunners.md, Docs/AI/rules/UnityConcurrency.md, Docs/AI/CoordinationRuntime/HANDOFF.md, Docs/AI/archive/reports/CoordinationHandoffHistory-2026-09-11-project-rules.md
Validation: finish-task passed (2 gates).
Pending / risks: none
Suggested next step: none

## 2026-09-11T10:02:00Z — nevesta integration / website preview publication — completed

Task: Integrated candidate `9a5b3d9b` as `56fa70a0`; published v1, then immutable `nevesta-izo-lda=2` with the approved website preview.
Changed: story source; remote immutable v1/v2 and `kostroma-dev.json`; APK/site shell unchanged.
Validation: Android content build passed earlier; v2 35-file checksum dry-run clean; card/cover/release/preview/characters HTTP 200; manifest SHA `fa26b51fd3bc1a8271e6a0527f5bf4486add94dedf80ced2c7021e1a3c4f92ac` preserves all five stories.
Pending / risks: emulator/device acceptance explicitly skipped by user; publication is live but full story acceptance remains incomplete.
Suggested next step: optional device smoke when desired.

## 2026-09-11T09:11:40Z — startup-coldload-fix — completed

Task: Show bootstrap before remote startup; parallelize bounded catalog metadata/art downloads; persist catalog cover cache; move notifications off critical startup path.
Changed: Novels/Assets/Novels/EntryPoint.cs, Novels/Assets/Novels/ApplicationRuntime.cs, Novels/Assets/Novels/CatalogFlow.cs
Validation: finish-task passed (1 gates).
Pending / risks: editor-gate --compile, editor-gate --test-filter <affected-suite>
Suggested next step: none

## 2026-09-11T09:47:14Z — znak-release — completed

Task: Content-only znak-na-dube v1 publication completed and publicly verified; APK unchanged. Formal story acceptance remains blocked because the required fresh Android emulator gate was explicitly skipped. Scoped repair files and generated evidence are preserved unchanged for a later acceptance run.
Changed: Docs/AI/CoordinationRuntime/agents/znak-release.md, Docs/AI/CoordinationRuntime/HANDOFF.md
Validation: finish-task passed (1 gates).
Pending / risks: none
Suggested next step: none

## 2026-09-11T10:54:37Z — znak-publish — completed

Task: Published corrected Znak na Dube v2 content-only to Kostroma dev with atomic manifest switch and rollback backup
Changed: Novels/Build/ChannelContent/stories/znak-na-dube/2, Novels/Build/ChannelContent/dev.json, Docs/AI/CoordinationRuntime/agents/znak-publish.md
Validation: finish-task passed (1 gates).
Pending / risks: none
Suggested next step: none
