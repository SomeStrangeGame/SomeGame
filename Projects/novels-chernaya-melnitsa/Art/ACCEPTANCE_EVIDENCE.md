# Acceptance evidence

Status: source art `ready-for-final-validation`; runtime acceptance incomplete.
Source repair 2026-09-08: both semantic mismatches corrected. ill01 is a centered
boat-motor workshop with two cups/window pinwheel; ill02 is Lada's upper-frame
palm with black flour, canon cuff, damp cloth/book. PNG/meta/source-selector
checks and full non-character originality iteration4 passed with limitations.
No fresh Unity import/Player visual pass. See `IllustrationRepair/README.md` and
source-check.json for exact hashes, composition boundaries and historical failures.
The old v6 APK/candidate pins and runtime findings below are historical; do not
mistake their unchanged artifact hashes for current source-art freshness.
The user accepted only the earlier residual character contours as non-blocking;
they were NOT fixed. Illustration fixes are source changes, not a waiver.
The full visual matrix and remaining semantic route/ending gates remain open.

## Runtime route check — 2026-09-08, 14:10–14:46 UTC

User «Давай проверим теперь» authorized the prepared immutable-v6 replay.
No new build/install: APK39049f824adf/releasefb7c51d4b322 and one-story catalog
verified before and after. Full candidate identity is in the v6 section below.
Actual test package/Activity, API34/ARM64 Pixel7/1080x2400, AVD
Novels_CHM_20260908/emulator-5554 are unchanged.

**Historical v6 blockers, distinct from the accepted character contours:**

- `Assets/Locations/ill01-mitya-found.png` contains the mill-side river crossing,
  repeating the composition of bg08 (not byte-identical), rather than the later
  Mitya/workshop settlement of narrative scene12/Ink459–473. Confirmed in the
  actual ending and source PNG. `R01-road.png`/`failure-final.png` are settled
  Ink473 evidence; `R01-road-line-466.png` is a partially faded dialogue frame,
  not a clean-opacity visual pass. Source SHA256
  `81f4bafc15b88343d6d6e0bcae7c9cbfb74c3d4db0f36881b727b4bcb38631c1`.
- `Assets/Locations/ill02-black-flour-hand.png` is a wide sack-room interior,
  with no hand or close flour insert required by APPROVED_ASSETS/Ink282–284.
  Confirmed by direct source-image review; no separate settled device capture
  of this short insert was preserved, so this is explicitly source evidence.
  SHA256 `6838d04132ccac0862639a19b684ae8cfb25d86936c91525ffe248c2aa032836`.

Return both assets to the production-art workflow for scene-matched correction,
applicable originality/provenance review, then a separately authorized fresh
single-story build/device gate. No images/Ink/runtime source were repaired here.

Actual R01 choices: find_mitya → go_alone → open_lada_sack → take_promises →
ending_road, numeric IDs `[1,1,0,1,0]`, confirmed by telemetry and real save.
Reached the road ending title at Ink473; stopped on art mismatch BEFORE tapping
the final title. No episode.completed, catalog-completion or post-ending restart
pass. R02–R05 were not run. The static72-route/365-line plan is not device coverage.

Cold Continue after C2/C3/C4 passed at Ink137/293/333: identical before/after OCR
text, new process each time, real decision history retained (94/208/220 saved
decisions). Read-only inspection follows SaveDataCodec v3; no save injection.
The observed text preserves release from Mitya's promise, foreign re-exposure
and delayed recovery at Ink437; runtime variables were not inspected directly.
Four app runIds/PIDs: b27f085e12c74c5f9952ee5eb036512d/5293,
a48758aa21874e95b9f81a7a4d010717/12579,
2edcf8d6a7134d819fa95f52a8e64342/19123,
701a74c4f730493887709385ffc9bb18/20785.
325 unique events,288 dialogue.ready,5choice.selected; no error/fallback.used,
FATAL/ANR/Unity error-level markers in saved PID logs. All four story activations
match the same release. Sampled long Bubble and C1/C2/C4/C5 text fit without face
overlap/clipping; full13scene/11character/8audio and platform matrices stay open.

Evidence: `Novels/Build/Logs/automation/chernaya-routes-20260908/REPORT.md`,
`runtime-report.json`, `runtime-journal.json`, observed JSONL, screenshots/PID logs,
full failure logcat and activity BEFORE stop. Early helper taps could precede
Bubble readiness: fixed only in the ignored helper. Transitional screenshots
and the initial still-open restart modal (`R01-C1`) are not Player failures or
visual passes. In-game restart used the real catalog hold2s confirmation.

Original checkpoint201bytes, final345bytes and three intermediate save copies
remain separate; actual final save contains284decisions/all5choices, no completion
marker. Original progress was not restored silently. App20785 force-stopped/PID
absent; AVD left device, shared unity released. Known ASTC software-emulator
limitations and the prior explicit contour exception remain unchanged.

User in «Проекты» explicitly allowed concurrent source-only catalog work.
Checkout lock was safely transferred under the new narrow ParallelWorkDetails
exception, retaining only immutable APK/device/ignored evidence ownership;
tracked final docs were deferred until a second normal checkout slot. New
catalog/runtime/Ink-engine source changes belong to that task and were NOT
validated by this APK. No commits, merge, publication or broad catalog build.

## Historical user decision and integration preparation — 2026-09-08

User: «Это не страшно, продолжаем интеграцию» after the v6 residual-contour report.
This is an explicit exception for the reported visual edges, not a retroactive
clean-alpha pass or waiver of content/runtime correctness. Subsequent answer
«Пока только подготовка без эмулятора» excludes Unity/ADB and new builds.
Existing v6 candidate unchanged. `ACCEPTANCE_PLAN.md`/`ACCEPTANCE_ROUTES.json`
define5 planned replays covering12choices/35condition outcomes/3endings and all
365text/resource lines. None executed in this preparation. Candidate JSON pins
source/compiled/map/APK hashes;3bundle/10payload ZIP hashes,11PNG/meta and
one-story catalog verified. Two plan tests,layout audit,doctor pass. No art/Ink/
catalog/SDK edit,save mutation,commit/publication. Next: explicit permission for
remaining actual route/endings/save-resume/visual acceptance.

## Fresh v6 APK validation — 2026-09-08, 13:48–13:58 UTC

- Fresh user «Да» authorized exact-story final slot. Process preflight found no
  Editor/Hub, existing LicensingClient91796 unchanged. Unity6000.3.11f1.
- Story Editor gate134915Z22.210s; story Android135005Z9.202s; one-entry catalog
  Android135118Z5.293s; Embedded ARM64 development Player135155Z37.940s: PASS.
  No build all. Actual ZIP contains only chernaya-melnitsa/Android and one-entry
  catalog. Previous composed cache recoverably preserved at
  `Novels/Build/LocalContent-before-chernaya-v6-only-20260908T1350`.
- APK `Novels/Build/Players/chernaya-melnitsa-v6-20260908/Novels.apk`,
  81,160,454bytes, SHA256
  `39049f824adf96c9f0b65a3b17671520f9853bb3fddaba52f83858dd5e456f01`,
  version2026.09.08/code3517311,min25/target36,package
  `com.UnityTechnologies.com.unity.template.urpblank`, UnityPlayerGameActivity.
- Exact new story release
  `fb7c51d4b322ccbee639cb642f7fa3c1cbb7019affe495ce2e355e38d2c3e267`
  matches APK and release.activated in real run
  `a10a52237baf4a66a29a36abaa1755ee`,PID4939.11events/3dialogue.ready;
  catalog download→story→episode→resume215→advance217 pass, no fallback/error
  events, Unity error-level/FATAL/ANR/content-root markers in saved PID log.
- Dedicated Novels_CHM_20260908,API34 ARM64 software GPU,emulator-5554. ADB
  transient offline after daemon restart resolved by bounded wait-for-device;
  install-r Success, cold Activity launch and foreground confirmed. Current
  inventory contains only5554; no foreign process/device changed.
- Visual evidence `Novels/Build/Logs/automation/chernaya-v6-check-20260908/`:
  `01-catalog.png` correct one-story cover/card; `02-resume.png` captured before
  first settled dialogue and is not a pass. `03-lada-settled.png` is exact
  Ink215 comparison with v3: pink fringe reduced, fine contour remains.
  `04-yakov-guarded.png` at Ink217 uses the same guarded variant as old206:
  large detached sleeve ribbon removed, but red/maroon strip remains along
  right trousers and greenish edge is visible at left sleeve. Visual FAIL.
  Do not equate improved source metric with complete edge acceptance. Green
  edge origin is unresolved; do not label it a proven compression regression.
- Each screenshot has PID-filtered log; `failure-full-logcat.log` preserves
  wider diagnostics, `post-stop-activity.txt` is explicitly AFTER cleanup.
  ASTC unsupported/decompress warnings persist: no physical ASTC quality,
  memory, performance or compatibility pass. No whole route/endings claims.
- Production11 PNG hashes and all11 metadata/GUIDs match v6 report. Unity-only
  trailing spaces normalized in11character metas,13location metas and one
  PlayerSettings file after byte/semantic comparison; no settings change.
  15regression tests,configuration doctor and scoped diff checks PASS.
- App force-stopped, PID absent; AVD remains device. Actual save copied before
  install(2files199bytes) and after(2files201bytes); never restored/reset/erased.
  Own locks released. No production fix, second build, commit or publication.
  Next: separate bounded correction of residual pants/sleeve edge, then fresh
  approval for another exact-story final slot. Preserve current failures.

## Source matte follow-up — 2026-09-08, 13:29–13:45 UTC

User approved deterministic repair without regeneration, not another heavy slot.
Actual APK ASTC textures compared offline: broad sleeve ribbon already exists
in source; compression adds staircase artefacts. Warm spill escaped v3 threshold.
All11 PNGs now use edge-matte-v6:17,300 chroma pixels,941 ribbon pixels and162
reviewed Yakov sleeve pixels corrected/removed. Six-pixel interior RGBA, canvas,
registration, all metadata/GUIDs, Ink, prefab and SDK policy preserved. Mitya's
right alpha bound contracts one pixel529→528, with no canvas movement.
All11 full-body dark/light proofs and two controlled offline ASTC comparisons
reviewed;15tests/72staticroutes/layout338dialogues/doctor pass. Exact hashes,
methods, proofs and backup locations: `EdgeCleanup/v6/README.md`.
No Unity/APK/ADB in this pass. Offline ASTC is not fresh Unity/device evidence;
fine compression edges and full matrix still require separately approved
one-story final slot. Catalog/build must contain only chernaya-melnitsa.

## Historical v3 APK validation — 2026-09-08, 13:15–13:26 UTC

- Fresh user «да» authorized this slot after the deterministic cleanup. Initial
  process probe: no Editor/Hub; existing LicensingClient91796 unchanged. Exact
  story Editor gate131522Z28.5s and Android131607Z10.652s passed; required
  one-entry catalog Android131640Z5.946s passed. No other story built.
- Player131734Z38.517s passed. APK
  `Novels/Build/Players/chernaya-melnitsa-alpha-20260908/Novels.apk`,
  81,157,466bytes, SHA256
  `a712f2ffeabc0f9c56da451b8dea3c5629eef8121ab4e5245790a5b7dc70ab6a`,
  package `com.UnityTechnologies.com.unity.template.urpblank`,
  version2026.09.08/code3517277,minAPI25/target36. Actual ZIP has only this story,
  Android, and one-entry catalog. No broad build all; Player skip-content-build
  used after exact hash-verified isolated staging. Previous composed cache
  preserved at `Novels/Build/LocalContent-before-chernaya-alpha-only-20260908T1317`.
- New release `6fe829364b698b2ec2b393b2caf0dc3f6beb39f9cb523056a9939e1781e04f21`
  appears in both APK and real release.activated event; old-cache identity is
  excluded. Changed bundles chunk0/chunk2, unchanged chunk1. All11 production
  PNG hashes and normalized meta hashes still match EdgeCleanup/report.json.
- Preserved Novels_CHM_20260908 API34 ARM64 Pixel7 1080x2400, software GPU,
  emulator-5554. Install-r Success and exact Activity COLD launch succeeded.
  Separate emulator-5580 was visible in inventory but is not task-owned and was
  never touched. Run `b0e18d85ddf24500962fb4e6a54992e5`, PID4151:18 events,
  10 dialogue.ready, no choice/endings or fallback.used/error events. Real
  catalog/download/release/episode chain passed; resumed at204, advanced to215.
- Evidence: `Novels/Build/Logs/automation/chernaya-alpha-check-20260908/`.
  `01-catalog.png` shows correct one-story card/cover and download22%; completion
  later confirmed by catalog.download_ready. `02-resume.png` is a transitional
  Ink169 background during restoration, NOT a visual pass. Filename labels in
  the next two files were assigned before the actual speaker was known:
  `03-yakov.png` actually shows Saveliy205; `04-lada.png` actually shows Yakov206.
  Yakov206 has visible coloured strips around right sleeve/trousers; FAIL.
  `05-lada-undercroft.png` and `06-final-failure.png` show Lada215 with residual
  coloured/jagged hair/coat contour; FAIL. Text/faces remain separated in these
  sampled scenes, but the whole layout/character matrix is not passed.
- Full PID logs accompany every screenshot. AVD reports ASTC8X8 unsupported,
  decompressing texture; this software-emulator run is not proof of physical
  ASTC-capable device behaviour, GPU memory or performance. Neither the exact
  compression contribution nor all remaining source-edge colours were isolated.
  The earlier min(R,B)-G metric/static proof is insufficient for visual acceptance.
- Seven regression fixtures and 338dialogue/12label/360choice-context layout
  checks pass. Unity-generated whitespace-only metadata/settings rewrites were
  normalized; no semantic importer, compression, prefab, Ink or art fix was made
  during validation. Original input/output hashes remain applicable.
- App force-stopped, PID absent; dedicated emulator remains device per standard
  smoke cleanup. Test save copied after stop to `checkpoint-chernaya-melnitsa/`,
  not altered/restored/deleted. No commit/publication. Locks released.
- Next: return to technical edge/mask correction, inspect remaining warm/red
  fringe and distinguish source from compressed output. Do not claim that the
  first cleanup fixed the Player defect. Any subsequent material art fix needs
  a separately approved fresh one-story final slot; full routes/endings and
  remaining state/safe-area visual matrix were not continued past this failure.

## Deterministic source-edge cleanup — 2026-09-08

- User explicitly approved technical mask/edge-colour correction without
  regeneration. Eleven runtime character PNGs corrected; no Ink, prefab,
  metadata, shared compression, catalog or device/save changes in this task.
- 14,046 edge pixels cleaned; isolated scraps removed from Saveliy229px and
  Yakov guarded244px. Full canvas/body registration and interior art preserved.
  Input/output/meta SHA256 manifest and source proof comparisons are in
  `Art/EdgeCleanup/`. Originals retained in ignored
  `Novels/Build/Logs/automation/chernaya-alpha-20260908/v3/before/`.
- Seven edge/mask regression tests, 11-file pixel/hash invariants, 72 narrative
  routes, 338 dialogue/12 label/360 choice-context layout checks and content
  doctor pass. Source proofs inspected on dark/light/blue backgrounds. This is
  source evidence, NOT ASTC simulation, Unity import or actual Player acceptance.
- APK098ff515a0e1 below now predates current production PNGs. No Unity, content
  build, APK or ADB run was started for this correction. Under
  UnityConcurrency, separately authorize the fresh single-story final slot;
  rebuild only chernaya-melnitsa and necessary one-entry catalog/Player. Never
  build all. Preserve existing saves/checkpoints and restage isolated payload.
- First recheck Lada172/204 and Yakov edges, both sides and light/dark scenes,
  then broad Saveliy/Mitya, choice/safe-area/state matrix and the five planned
  semantic paths/all endings/save-resume. Originality applicability retained
  as a technical cosmetic correction, not a new design or screening iteration.

## Composition APK validation — 2026-09-08, 12:41–12:56 UTC

- Explicit user «Да», then «Закрыто» authorized the current final slot.
  Initial process probe confirmed no Editor/Hub. Story Editor gate124135Z,
  story Android124209Z and catalog Android124217Z all passed; Player log
  `Novels/Build/Logs/automation/player-20260908T124243Z.log`, duration36.262s.
  Catalog Editor was unchanged and not rebuilt during this slot.
- Fresh APK: `Novels/Build/Players/chernaya-melnitsa-composition-20260908/Novels.apk`,
  81,158,886 bytes, SHA256
  `098ff515a0e17ac90f07fad6efb8ede99ed3369eee7d11918a0a12d10d49a504`.
  Runner initially resolved relative output under root `Build/Players`; the
  artifact was moved intact to the final path above. Package
  `com.UnityTechnologies.com.unity.template.urpblank`, version2026.09.08 /
  versionCode3517242, minAPI25, target36. Android story release
  `c4c8379cce3657d310c85294d57c7e1cc593b6f53af63ad48ba5af6d044e3669`.
  Actual ZIP confirms only chernaya-melnitsa and Android payloads; one-entry
  registry. No other stories built and no `build all`. Player used
  `--skip-content-build` after isolated staging; composed cache preserved at
  `Novels/Build/LocalContent-before-chernaya-composition-only-20260908T1243`.
- New character prefab imported with GUID `c33639195f26345c2a2f184e22156798`;
  its exact runtime address appears in the Android release. All original PNG
  bytes/Ink remain unchanged; generated texture metadata whitespace normalized.
- Dedicated preserved AVD Novels_CHM_20260908, Pixel7 1080x2400, API34 ARM64,
  emulator37.1.11, exact serial emulator-5554, software GPU. No resolution
  override. APK installed with `-r`, launched through verified Activity and real
  catalog. Run `956e46934594430ea381016efe8ee2cb`, PID3574, contains131 events:
  ordered app/catalog/download/story/release/episode markers,122 dialogue.ready,
  one choice.selected(1 = go_alone). No error/fallback.used markers occurred.
- Catalog cover/one-card/Continue passed. Previous test save resumed at Ink49,
  preserving prior sell_house selection. Continued through Ink204 with the
  go_alone branch. This is NOT a full current-APK route or all-branch save/resume
  proof. Real untouched save copies are under the evidence directory's
  `checkpoints/route15-c2/` and `checkpoints/paused-alpha-review/`; none restored
  or synthesized. Before C2 the SaveChoice envelope contains90 decisions and
  choices[0]. Final saved progress is retained on the stopped dedicated AVD.
- Evidence folder: `Novels/Build/Logs/automation/chernaya-composition-20260908/`.
  `01-start.png`: correct real catalog. `02-nastasiya-long.png` (Ink50) and
  `03-nastasiya-max.png` (Ink77): full named text fits and face stays above panel.
  `replay-155117.png` (Ink124): both C2 choices fit and are readable.
  `replay-155242.png` (Ink172) and `replay-155348.png` (Ink204): Lada's raised,
  enlarged portrait and text are separated, but visible jagged brown/magenta
  fringe around hair/clothing FAILS the alpha-edge visual gate. Original
  `wary.png` and prior Lada dark alpha proof also show residual coloured edges;
  Android compression may amplify them, but its contribution was not isolated.
  The canonical ASTC8/alphaIsTransparency settings were not changed.
- `runtime-final.log` preserves full PID logcat. `04-paused.png` is the final
  stopped frame. Some earlier replay screenshots captured transitions because
  dialogue.ready precedes presentation; the helper now waits3s before capture.
  Blank transitional screenshots are not missing-asset findings or visual passes.
- Returned to character/art production for edge cleanup; no PNG or shared
  pipeline fix performed during acceptance. UI geometry passes only the named
  cases above. First three-choice group, C5 variants, broad Saveliy, Mitya,
  pressed/fallback/safe-area matrix, full semantic routes and all3 endings remain.
  Static audit/doctor/diff checks pass, but do not waive these missing gates.
- Revised minimal replay plan indices15,32,67,30,33 of audit_story routes covers
  every source line/choice option and3 endings. Paths30/33 can use a real C4
  checkpoint from path32; no such checkpoint exists yet. Runtime evidence is
  incomplete, not a five-route pass. Test app and own AVD stopped, saves retained;
  locks released. No commit/publication. Any material art fix needs fresh build
  evidence and separate final-slot authorization under UnityConcurrency.

## UI rerun and portrait occlusion correction — 2026-09-08

- User's new explicit «Да» authorized this rerun after the first layout fix.
  Built only chernaya-melnitsa (Editor + Android), required Android catalog,
  and an Embedded development Player with `--skip-content-build`. Logs under
  `Novels/Build/Logs/automation/`: story `content-gate-20260908T120313Z.log`,
  story Android `120335Z`, catalog Android `120343Z`, Player `player-20260908T120417Z.log`.
  These passed; catalog Editor was not rebuilt during this rerun.
- APK: `Novels/Build/Players/chernaya-melnitsa-ui-20260908/Novels.apk`,
  81,157,530 bytes, versionCode 3517204, SHA256
  `f5cc9f0cc78383b2b48188bb59ba2976cab7ca79dec34581bdf61f6b27fb6a7c`.
  Android story release `74bc984e720eded0396a667390bf96dd9778d75f75fc9fb5513f84d591610d54`.
  Catalog release remains `cd07557fbab3fc3f79aef6dd3c138c4ed968180cb3ab8c2ef211043842bf37c4`.
  Actual APK ZIP/registry inspection confirmed only this story and Android payloads.
  Pre-isolation composed output retained recoverably at
  `Novels/Build/LocalContent-before-chernaya-ui-only-20260908T1204`.
- Same dedicated API34 Pixel7 AVD, 1080x2400, `-gpu software`. Installed with
  `install -r`, preserving the previous test save. Real catalog showed one card
  with its assigned episode cover and Continue, then resumed at the first choice.
  `runId=e35a1e5cd5444f62bdcc23640f4a994b`: 24 events through Ink line49,
  including catalog.ready(storyCount1), matching release.activated, episode.ready,
  15 dialogue.ready events and choice.selected(0 = sell_house). No fallback.used
  or error markers occurred. Endings were not reached; resume proof is limited
  to preservation across APK replacement, not full branch-state save/resume.
- Screenshots and full PID logcat in
  `Novels/Build/Logs/automation/chernaya-ui-recheck-20260908/`:
  `01-catalog.png`, `02-first-choice.png` (all three labels/targets inside screen),
  `03-choice-result.png` (Lada); `replay-151807.png` proves the new failure at
  `s01e01.ink:48`: the four-line named panel completely hides Nastasiya's face.
  `portrait-overlap-logcat.txt` records the complete run; the final screenshot
  `portrait-overlap-final.png` is the next narrator line49, not the failure frame.
- Returned to `somegame-produce-story-art`. Initial unbuilt idea of raising
  named panels to y=170 was discarded when the user requested raising characters,
  lowering Bubble, and increasing character scale. Current story-local character
  variant inherits the shared character screen, changing only viewport y=-220
  to150 and uniform XY scale1 to1.2 (plus root display name). PNGs, per-sprite
  registration, horizontal position logic and all runtime bindings are unchanged.
  New prefab/folder metadata is deliberately left to the next Unity import.
- Current Bubble named/thought roots: y=-115 to-180 (65 units lower); narrator
  y=35 to-55 (90 units lower). Previous body22/label20/button96/slicing fixes stay.
  All200 named lines fit the conservative lower panel budget (maxheight240,
  panelbottom794 at logicalheight1024). All360 actual pre-choice contexts across
  72 routes are measured, including C5 after Lada/Nastasiya: worstbottom958,
  bottomclearance66. Eleven source alpha bounds at three portrait heights pass
  a rough140-unit head reserve above named headers. This is not face detection
  or proof of horizontal cropping/visual quality. Pixel art/Ink/shared runtime
  remain unchanged; no raster generation or new originality iteration.
- Static layout/72-route audits and content doctor pass. New character address
  follows `story/presentation/character/screen-variant.prefab` convention;
  packaging/import and all new geometry still need real Unity validation.
  The APK above predates these corrections and is stale for final acceptance.
- Our test app and dedicated emulator stopped; data retained. A read-only process
  check at12:24:52Z found a separate user Editor for `/Users/iantonishin/Kids/skazbuka`
  and its Hub/import workers. They were not touched; check real processes before
  any future heavy slot and never auto-close this unrelated project.
- Next: separate repeat-slot approval under UnityConcurrency; rebuild/replay only
  this story and required catalog/Player. First verify long named lines77/465,
  enlarged portraits (especially broad Saveliy and both sides), first three-choice
  group and C5 named/narrator variants, then the remaining matrix and replay paths,
  all three endings and save/resume. No new commit, publication or other-story build.

## Authorized single-story rerun — 2026-09-08

- Baseline: `codex/story-batch-chernaya-melnitsa` at `f1721a63e046`.
  The user explicitly approved the repeated final slot, then required building
  only the tested story and keeping only it in the catalog. Current catalog
  is `["chernaya-melnitsa"]`; zdm/tzm source projects were not changed or built.
- Four content gates passed: story Editor `content-gate-20260908T112739Z.log`,
  catalog Editor `112818Z`, story Android `112825Z`, catalog Android `112900Z`.
  Logs are under `Novels/Build/Logs/automation/`. Real Ink compilation produced
  JSON and a source map with 1209 entries. All three Android chunk audits passed.
- Android story release: `cc9c49f2418449034e0a6529206230b5017c736ac5aa217d1eafe8560e11b9c9`.
  Catalog release: `cd07557fbab3fc3f79aef6dd3c138c4ed968180cb3ab8c2ef211043842bf37c4`.
  Episode preview exports `s01e01.png`, and the emulator shows the assigned
  undercroft image, episode title and description in the single-story catalog.
- Fresh Embedded development APK: `Novels/Build/Players/chernaya-melnitsa-20260908/Novels.apk`,
  built 2026-09-08 around 11:31 UTC; 81,157,658 bytes, SHA256
  `b4d4fd9ad9f8a784f8b6c573fc11deb840a9484b11ae442cbe21cba52744a536`.
  Package `com.UnityTechnologies.com.unity.template.urpblank`, version
  `2026.09.08` / `3517169`, ARM64, minimum API25, target API36.
  ZIP inspection confirms ONLY chernaya-melnitsa and Android payloads, with the
  one-entry catalog. Player build used `--skip-content-build` after the exact
  content gates; it did not call the broad `build all` path.
- Existing composed output was moved, not deleted, to
  `Novels/Build/LocalContent-before-chernaya-only-20260908T1130`.
  A later content-gate's compose can reintroduce other cached stories: before
  the next APK, preserve that output separately and restage only this story
  plus the catalog. Inspect the actual APK ZIP again.
- Existing Pixel AVD refused installation due to a signing-certificate conflict.
  No uninstall/save reset occurred. Dedicated AVD `Novels_CHM_20260908` uses
  Pixel 7 geometry 1080x2400, API34 / `sdk_gphone64_arm64`, serial `emulator-5554`,
  emulator 37.1.11. Its data is under `/private/tmp/somegame-chernaya-emulator.EJ3awb/avd`.
  `monkey` refused its physical-key configuration; verified launch Activity
  `com.unity3d.player.UnityPlayerGameActivity` was started with `am start -W`.
  This entered the real catalog, not a direct episode/scene.
- Host GPU hung in `vkCreateDescriptorPool`; original log retained. Restarting
  the same dedicated AVD with `-gpu software` loaded the unchanged APK correctly.
  First-run missing-cache warnings caused normal extraction from APK; they are
  not missing authored assets. Bundle `deliveryMode: Remote` is the release
  descriptor's mode, not proof that the Embedded APK accessed a remote server.
- Software-rendered run `20eb3df3e48b421a8f2599bf695f118a` recorded ordered
  app.started → catalog.loading → catalog.ready (storyCount 1) → download_ready
  → story.selected → release.activated (matching Android release) → episode.ready
  → dialogue.ready. No episode.selected event was emitted on the direct card
  flow; the runner's default event list was not passed and is not claimed green.
- Failure: source `s01e01.ink:22`, first three-option choice, third label crosses
  the lower edge; narrator text overlaps the stretched decorative frame.
  Evidence: `Novels/Build/Logs/automation/chernaya-emulator-20260908/`
  `replay-144241.png`, `first-choice-failure-logcat.txt`, `first-choice-activity.txt`.
  `07-ready.png` verifies catalog artwork. No `fallback.used` occurred before
  the visual failure. Later scenes, characters, five planned replay routes,
  three endings and save/resume were NOT validated.
- Returned to `somegame-produce-story-art`: story-local prefab now uses body22,
  label20, 96-unit buttons and raised viewport; panel nine-slicing preserves
  decoration instead of scaling it through the text. PNG bytes/GUIDs, Ink,
  runtime binding identities and shared UI code remain unchanged.
  `Art/audit_bubble_layout.py` checks 338 dialogue strings and all 12 labels
  with the actual font, conservative leading and worst-case geometry. It passes,
  as do the unchanged 72-route audit and scoped diff check. These are STATIC
  estimates, not visual acceptance; all old APK evidence is stale for this UI fix.
- Test app and our emulator were stopped. No publication or other-story builds.
  Next: separately authorize a repeat final slot under UnityConcurrency, rebuild
  only this story and the required one-entry catalog/APK, then repeat the visual
  matrix and five-route coverage. Do not reuse this failed APK as final evidence.

## First final-slot attempt and syntax correction — 2026-09-08

- Canonical integration branch: `codex/story-batch-chernaya-melnitsa`; story
  commit `8e7f2b6f`, catalog entry commit `10281a7b`. Existing zdm/tzm preserved.
- The earlier screenshot belongs to the closing Novels Editor, not this story:
  log stack `WindowLayout.SaveWindowLayout` failed to use a missing Temp file.
  The log records Force Quit; no tracked Novels source changed. The reason for
  the missing temporary file is unresolved; no caches/saves were cleared.
- A fresh batch Unity started successfully; Hub PID 79793 was closed by the
  approved runner. First cold import created expected Unity asset metadata.
- `content-gate-20260908T110028Z.log` failed in real Ink compilation:
  old s01e01.ink line 229 mixed inline-start condition and another conditional
  branch. The source now nests that branch inside `else`, with unchanged text.
- Current source hash is
  `dddcedbb0c94cf1513a5327058ff4d881d7bfc505998733c8e2e65c721fb4253`.
  All 72 routes preserve identical displayed text, choices and intermediate/final
  states. The static parser now rejects the original malformed syntax; its
  regression fixture checks that specific rejection, not merely any exception.
- No completed content/catalog build, compiled story/source map, APK, emulator
  run or visual acceptance is claimed. Repeat final-slot approval is requested
  separately under UnityConcurrency; generic auto-approval is recorded for
  ordinary scoped decisions, not substituted for this gate.

Earlier preparation records below are historical; old hashes and statements
about an unregistered catalog or unopened Unity are superseded by this section.

## Candidate identity

- Branch: `codex/story-chernaya-melnitsa`
- Base: `c5a431e30ff857a00e70fd50ed14ae0d559995ad`
- Scope: `Projects/novels-chernaya-melnitsa/**`
- Unity: `6000.3.11f1`; Official Unity Pipeline `0.5.0-exp.1` inherited from the
  maintained atomic template. Unique live server registration/restart proof is
  deferred with the final MCP slot; no external Codex config was mutated.

## Main refresh and episode cover — 2026-09-08

The registered branch was fast-forwarded from the original base above to
`f234c9a758a6afd025cb17e32fb2a0c62009bdbb` under the shared integration lock.
Immediately after refresh, HEAD matched fetched origin/main (ahead 0, behind 0).
All 115 original untracked files were verified byte-for-byte unchanged using
the same sorted path/SHA-256 manifest before and after. The lock was released.
No story commit was created or published, and the primary checkout was untouched.

The user requested an image for every episode. The sole episode `s01e01` now
assigns `s01e01.png` from `Config/EpisodeCovers/`, copied without pixel changes
from approved `bg11-mill-undercroft.png`. The original art evidence remains
applicable; the source and story cover are untouched. This is a source-image
review and static assignment, not a catalog visual pass.

Remaining gates include preview export/packaging, the assigned episode image
and story fallback, initial lightweight catalog delivery and actual card crop.
The worktree registry baseline was subsequently reconciled under the dedicated
FIFO/integration locks to the exact upstream SHA above. The original creation
base remains recorded under Candidate identity; no story commits were excluded:
HEAD equalled upstream before the first story-local checkpoint. Candidate
registration now compares only the story-local diff against this refreshed base.

Static verification passed: all 72 Ink routes, the sole episode's cover binding,
byte identity with approved source art, unchanged Ink hash, and content doctor.
The cover audit rejects missing, unsafe and duplicate assignments. No real Ink
compilation, catalog build or Unity/device check was executed.

## Final-slot preflight — 2026-09-08

The author authorized preparing the Git candidate and proceeding with final
Unity validation. Static audit still passes all 72 routes and the episode cover;
doctor passes and the reviewed Ink hash is unchanged. The checkpoint includes
only this story prefix, never generated build output or shared source changes.

The heavy slot has not started: an existing Novels Editor (observed PID 79728)
and Unity Hub (79793) require permission and an unsaved-state check before
closure. Separately, `codex-kolodets-integration` still declares active ownership
of `Projects/novels-catalog/Config/catalog.json`; registration must wait for an
explicit handoff/release, not a stale-age takeover. No existing process was
stopped and no catalog entry was changed. Acceptance is blocked until these
preconditions and the mandatory fresh APK/emulator/visual gates are satisfied.

## Earlier editorial corrections — 2026-09-07

- Fixed the disappearance chronology, prior unsuccessful searches, planted coat,
  intercepted later correspondence and the consent-based route to adult Mitya.
  His seventeen-year-old recollections no longer display his adult portrait.
- Restricted each character's knowledge to evidence already seen. The postal
  inventory proves interception but does not disclose the unread letter.
  The letter has identical wording wherever opened.
- Made travel, map/bottle/letter transfers, the exposed cart, delayed arrival on
  the solo route and the approaching ventilation deadline explicit.
- Distinguished genuine promises, private admissions and the elder's written
  intentions. Water contains dust but does not release promises. An absent
  addressee cannot be impersonated; factual testimony is not proxy consent.
- Moved public disclosure into the road ending. Silence now withholds the
  findings; the keeper perpetuates control by delaying genuine releases, not
  through an unexplained new power of the book.
- Earlier choices affect cooperation and physical consequences without hiding
  any of the three ending options behind undocumented stat thresholds.
- Corrected the runtime protagonist directory to `Assets/Characters/maincharacter`
  according to `CharacterSpriteResolver`/`CharacterAssetProfile`. Supporting
  speakers explicitly request existing `main` sprites, not absent variants.
  Identity masters remain under `Art/Characters/лада`.
- Updated narrative, asset and character handoffs to the same contract.

## Second continuity pass — 2026-09-07

- Found a branch-level contradiction the earlier terminal-state checks missed:
  reading Mitya's release did not stop his promise from coercing Lada later.
  C3 now clears that effect. C4 can expose her again, but if the letter was read,
  the loft/stairs/bridge explicitly show a different, foreign promise; the
  cancelled promise does not return. Dry-dust inhalation is now shown on C4.
- Fixed the relative dates: missed visit first, flood that night, letter next
  morning and disappearance by evening; the drawing is found the morning after.
- Accounted for the returned plaster cast, its continued presence in the satchel,
  the note/medallion transfer, possession of the book and map, the bag at the
  hopper, and the absence of any opportunity to make document copies at dawn.
- Aligned the teacher's and sawmill worker's promises across scenes. Removed
  the attribution of Yakov's broken road promise to Saveliy, Nastasya's
  contradictory assumption about burning letters, and Lada's mistaken claim
  that bringing Yakov would add a required witness to her old promise.
- Removed the unsupported guarantee that the damaged brake/wood would last
  until spring. The silence ending remains a risky temporary containment.

Genre, cast, choice IDs, three endings and approved media are unchanged.

## Third continuity pass — 2026-09-07

Re-read the complete episode and checked all branches. Four local wording
corrections align descriptions with facts already established elsewhere:

- map pins identify promises exploited by Saveliy, not promises all made to him;
  the teacher's addressees remain the parents and the carpenter's his neighbour;
- the note and attached postal medallion are both inside the sealed bottle;
- the miller's conversation with Mitya is placed before the boy's departure,
  without the ambiguous reference to the earlier flood night;
- the chest retains the old postal lock; it is not the replacement archive
  door lock that Nastasya's key cannot open.

No assignments, conditions, diverts, choice IDs, selectors or scene order were
changed. The existing originality result is retained for these non-material
copy edits with the explicit justification in `ORIGINALITY_EVIDENCE.md`.

## Current causal revision and recheck — 2026-09-07

The user explicitly requested repair of the seven-year contact gap and the
insufficiently motivated global mill shutdown. The revised source establishes:

- Lada originally called Mitya through the village office. Saveliy held her
  callback number, refused to give it to Mitya after departure, and falsely
  described her wishes. This obstructed initial calls and letters, not every
  conceivable contact method for seven years. Adult Mitya acknowledges later
  choosing avoidance out of anger, fear of rejection and accumulated shame.
- Personal release affects that promise, not every bag. The archive explains
  this through Marina's established case. Yakov describes earlier mechanical
  stops and the uninformed residents helping Saveliy restart the mill.
- Public disclosure does not magically stop the shaft. In road, Yakov closes
  feed, waits for residual material to be ground, then brakes the empty drive;
  residents subsequently dismantle feed and prevent unilateral restart.
- Silence uses the same safe stopping sequence but leaves intact feed, bags
  and keys with Saveliy. Old dust and possible restart remain the danger.
  Keeper continues operating the mechanism and withholding replies.
- A release letter clears its own coercion. Foreign C4 coercion instead needs
  time away from dust: the affected road paths include recovery at the river
  until the next morning, not an instant cure caused by public confession.

The full source was re-read for knowledge, time, prop custody, choice effects
and all three endings. No further serious contradiction was identified in this
pass; this is an editorial assessment, not a guarantee. Choice IDs, cast,
scene selectors and ending identities remain unchanged. The new causal detail
is material: prior originality clearance was not extended as another copy edit.
The author subsequently authorized one additional full-current-candidate
review; narrative iteration 5 and complete-Ink iteration 6 passed with the
limitations in `ORIGINALITY_EVIDENCE.md`. The source hash below is unchanged.

## Historical pre-Unity static checks

The hash and pending-build statements below describe the earlier snapshot.
Use current records at the top and `ACCEPTANCE_CANDIDATE.json`, not this section,
for new work; do not rebuild based on the historical list.

Reviewed Ink SHA-256:
`23c5a5fc2ce1d22b0c2d8dac06c76b26da4a75addc6f071312577293ddb7021e`.

Run from the registered story worktree:

```sh
python3 Projects/novels-chernaya-melnitsa/Art/audit_story.py
Tools/novels-tools/novels-content doctor
```

`audit_story.py` is a dependency-free, read-only interpreter for the restricted
Ink syntax used here, not an Ink compiler or a Player test. Unsupported syntax
fails the audit. It traverses choice/conditional/divert paths and checks:

- 72 complete routes, five choice groups, 12 options, 24 routes per ending;
- five choices per route, no missing divert targets or unreachable dialogue;
- `public_truth` only on road, appropriate `letter_read` state in other endings,
  and the cleared flour effect in road;
- intermediate states before C3/C4/C5: C2 exposure, C3 release, C4 re-exposure,
  sharing state and absence of public disclosure before the final choice;
- the recovery scene occurs exactly on road paths with C3 early release and
  subsequent C4 foreign exposure;
- all 13 location/illustration selectors, eight audio selectors and 11 exact
  character sprite paths exist; no obsolete Lada runtime folder.

The final `novels-content doctor` returned `configuration is valid`. Scoped
text-input whitespace/conflict-marker checks also passed, including untracked
files that ordinary `git diff --check` would not inspect. Worktree status
contained only the authorized story prefix; no shared source files were edited.
Four in-memory negative cases were rejected by `check_route`: reactivating
the cancelled promise, dropping C4 exposure, moving public disclosure before
C5, and removing required recovery. They changed copies of route records only,
not the source or runtime state.

Conservative displayed-word estimates (dialogue plus selected option labels,
excluding resource commands, unchosen labels and unvisited branches):

| Ending | Minimum | Maximum |
| --- | ---: | ---: |
| road | 4,314 | 4,516 |
| silence | 3,875 | 4,034 |
| keeper | 3,842 | 4,001 |

The 25–45 minute duration is a design target, not a measured result. Player
timing, layout, pacing and Ink compilation remain pending. Previous word counts
and the claim that text volume proved duration are superseded by this audit.

Current similarity screening passed narrative iteration 5 and complete-source
Ink iteration 6. The sixth text review was explicitly authorized by the author
for this revision only; no general protocol limit was changed. See
`ORIGINALITY_EVIDENCE.md` for sources and limitations, including the existing
*Die schwarze Mühle*/Krabat and *Czarny Młyn* title associations.
No claim of unique title or exhaustive clearance is made.

## Preserved earlier source-asset evidence

These assets were not regenerated in the editorial review:

- Five character packages and the final non-character art/audio package have
  scoped originality evidence with recorded reverse-search limitations.
- `ill02-black-flour-hand` is linked from Ink, followed by an explicit return to
  `bg05-grinding-room`.
- Liberation Sans resolves the Bubble prefab font GUID and was previously
  byte-compared with maintained SCP/TZM copies.
- Earlier file checks found 49 valid PNG headers, 11 runtime character sprites
  at `768x1280` RGBA, and eight non-empty stereo 16-bit PCM WAVs at 44.1 kHz.
- Earlier source-image review covered the cover, key backgrounds, Lada's dark
  alpha proof and Bubble surface; it is not Player visual acceptance.
- The audio generator derives its output root from its story-local path.

The earlier claim that all protagonist selectors already resolved was wrong:
the `лада`/`maincharacter` mismatch was found and corrected in this review.
The earlier follow-up also added dialogue; its claim of unchanged source is
withdrawn. Historical passes do not replace the separately documented current
review of the material rewrite.

Unity imports, real Ink compilation/source map, content validation/build and
Player/manual evidence remain deferred to the authorized final slot below.

## Historical pre-Unity final-slot plan

Superseded by completed build records above and current `ACCEPTANCE_PLAN.md`.

The originality hold is resolved for the unchanged candidate hash above.
Obtain new explicit human authorization under `UnityConcurrency.md` and shared
Unity/resource locks. Run atomic import/Ink compilation and source-map,
story content validate/build, catalog registration/build, Official MCP live and
restart proof, fresh Android Embedded APK, catalog-to-story emulator replay of
all five semantic choices and three endings, save/resume, smoke markers and
fallback audit, plus the Bubble/character/manual visual state matrix.

Catalog path planned by acceptance: `Projects/novels-catalog/Config/catalog.json`.
No catalog, main branch, publication, commit or external MCP config mutation was
performed in this preparation stage.
