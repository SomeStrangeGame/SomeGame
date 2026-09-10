# Android acceptance V2 — 2026-09-09

Status: **blocked — Lada character visual defect; not accepted**.
User closed foreign Unity and explicitly resumed the final slot. Candidate was
not modified during acceptance. Only this story and catalog were built. No
publication, commits, primary product edits, save deletion or other-story builds.

## Fresh build identity

- Clone: `/private/tmp/tk-integration-X88NJZ`, HEAD
  `f01cd1d2584fa068af9b02c83a4487e808371847` plus documented candidate changes.
- Story Android content PASS,10.634s; log
  `Novels/Build/Logs/automation/content-gate-20260909T101728Z.log`.
- Catalog Android content PASS,5.404s; log
  `Novels/Build/Logs/automation/content-gate-20260909T101755Z.log`.
- Fresh Embedded Android Player PASS,46.589s,10:18:27–10:19:14 UTC; log
  `Novels/Build/Logs/automation/player-20260909T101827Z.log`.
- APK: `Novels/Build/Players/automation/nochelessie/Android/Embedded/tk-20260909-1018.apk`;
  86117171 bytes; SHA256
  `b27e0003edbd41ae747cb0da8aef79a4f8474a87a62b3e8d9424c0c6e9e65c8b`.
- Package `ru.nochelessie.novels`, label Ночелесье, version2026.09.09,
  code3518538, minSDK25,target/compileSDK36. Test signature v2 verified.
- Story release
  `b12a61191ac26d1305a13062607ee354381c147419186491acb11dacbac4ed79`.
- Catalog release
  `1f951997ea55f4a30c5f1ab1ecfbca52f3c2988ceeb09bd3c3318a5f1e44d52b`.
- APK registry and payload story-ID set contain only `trinadtsatyy-kolokol`.
  APK Android release equals current exported release;3 bundle and18 file
  size/SHA256 checks pass. Other story sources are preserved.

## Actual Android evidence

AVD `TK_Final_API34`, isolated root `/private/tmp/tk-android-validation-HRgLrU`,
serial `emulator-5560`, model `sdk_gphone64_arm64`, API34,1080×2400.
Installed using `adb install -r` without clearing saves. Launched ordinary
`com.unity3d.player.UnityPlayerGameActivity`, then catalog Continue button.
Earlier diagnostic save resumed at episode1 first Roman choice with NEW release.

Run `ebf1d2f1e4eb45a8beef5f2b0987fd94`:
seq1 app.started →2 catalog.loading →3 catalog.ready(storyCount1) →4/5
download_started/ready →6 story.selected →7 release.activated(new hash) →8
episode.ready(s01e01) →9 dialogue.ready(choiceCount2) →10 choice.selected(id1,
visible «Спрятать оригинал и отдать копию») →11/12 dialogue.ready(choiceCount0).
Observed subsequent Lada and Roman replies match this branch.
No fallback.used, Unity E line, FATAL EXCEPTION or app ANR in captured interval.
Expected cache-missing warnings preceded new bundle activation; their
TrySetException stack frames are not treated as independent runtime failures.

Screens and complete logs: `Build/AndroidAcceptanceV2/`:
`catalog.png`, `resumed-choice.png`, `keep-reel.png`, `lada-settled.png`,
`roman-after-choice.png`, `device-logcat-final.txt`.
Source UI286 strings/63 contexts/13 labels/five negative fixtures and86 narrative
routes pass, but these are not Android route completion evidence.

## Visual result and defect handoff

New graphite/bronze panels and both choice labels actually render. The second
button is clickable and resolves the intended branch. Roman's face remains
clear above the panels. This is a first-choice smoke, not full Bubble acceptance:
long narrator, bright background, three-choice endings, all episode transitions,
remaining branches/endings and full save/audio matrix were not completed.

**Blocking character defect**: Lada appears heavily pixelated/posterized on the
reply «Сначала я проверю монтаж». Repeated settled screenshot after waiting is
unchanged; the subsequent Roman sprite is substantially more detailed. This is
not attributed to the Bubble changes. All four Lada source PNGs are620×620,
whereas secondary characters are1024×1536. Mobile import uses shared ASTC8×8;
low source resolution plus magnification/compression is a plausible contributor,
not a proven sole cause. No loader/texture setting was changed to obtain a pass.

Additionally, source `main.png` and `focused.png` contain a fully opaque pale
horizontal strip: all620 pixels at y610 have alpha>240 and RGB channels>200.
Their alpha bounding boxes therefore span the whole width. This contradicts
the earlier clean-alpha handoff and is independently a source-art defect.

Return the four Lada whole-image assets to `somegame-create-character` for
identity-preserving high-resolution/alpha repair and updated visual evidence.
Preserve approved identity, selector addresses, GUIDs, pose/variant semantics;
review shared compression/load behavior only if high-quality source still fails.
Acceptance does not edit assets or waive the defect. After repaired candidate
completion obtain a fresh explicit final-slot approval and rerun Android gates.

Own package and emulator stopped10:24 UTC; AVD and test save retained. Primary
product diff and index hashes match pre-task baselines; own locks released.
