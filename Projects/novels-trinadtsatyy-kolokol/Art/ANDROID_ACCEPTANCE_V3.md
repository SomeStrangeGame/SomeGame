# Android acceptance V3 —2026-09-09

Status: **blocked by newly observed substantial Tim rendering defect in R2 epilogue; all four route/choice/ending telemetry gates passed. Minor edge limitation accepted; broader defect not silently waived. Audio listening remains unverified.**
Fresh explicit user approval followed the completed Lada source repair.
No story/art/runtime code was edited to pass this acceptance. Primary product,
index, foreign projects and saves were preserved. Only TK and catalog were built.

## Build and artifact

- Retained clone `/private/tmp/tk-integration-X88NJZ`, HEAD
  `f01cd1d2584fa068af9b02c83a4487e808371847` plus recorded candidate changes.
- Story Android content PASS13.682s, log
  `Novels/Build/Logs/automation/content-gate-20260909T113108Z.log`.
- Catalog Android PASS5.207s, log
  `Novels/Build/Logs/automation/content-gate-20260909T113138Z.log`.
- Player Embedded Android PASS48.303s,11:32:03–11:32:51UTC, log
  `Novels/Build/Logs/automation/player-20260909T113203Z.log`.
- APK `Novels/Build/Players/automation/nochelessie/Android/Embedded/tk-20260909-1132.apk`,
  86288163bytes, SHA256
  `fd689229dfa2dc929ba475d03e3c3e32285a8c046453b4c5e7da291bfc9bbab0`.
- Package `ru.nochelessie.novels`, label Ночелесье, version2026.09.09,
  code3518612, minSDK25/targetSDK36; test APK signature v2 verifies.
- New story release
  `834935a0ec81038b97267abf6f4c3563381021c828d26c46e14ce8622a74ee37`.
- Catalog release remains
  `1f951997ea55f4a30c5f1ab1ecfbca52f3c2988ceeb09bd3c3318a5f1e44d52b`.
- ZIP registry and story payload IDs contain only TK. Exported Android release
  byte-identical to APK;3bundle/18file size+SHA256 checks PASS.
- Post-build Lada source/copy/meta audit PASS: all4RGBA1254×1254 unchanged from
  `Art/lada-repair-v1/package-before-import.json`; no import policy overrides.

## Real catalog and Android observations

Own saved AVD TK_Final_API34, root `/private/tmp/tk-android-validation-HRgLrU`,
serial emulator-5560, model sdk_gphone64_arm64, API34,1080×2400. Started with
no-snapshot/no-window and SwiftShader; no data wipe. Installed APK using `-r`.
Ordinary UnityPlayerGameActivity launches catalog; visible Continue resumes the
existing episode1 save at Roman's «Тогда я проверю вас» after earlier keep_reel.
This run did NOT reselect keep_reel and must not be counted as new choice proof.

Run `53c8819d4c41439d842019a39f61bd91`, seq1–11:
app.started →catalog.loading →catalog.ready(storyCount1) →download_started/ready
→story.selected →release.activated(new hash) →episode.ready(s01e01)
→dialogue.ready(Roman,0choices) →dialogue.ready(narrator,0)
→dialogue.ready(Lada focused,0).

Evidence in `Build/AndroidAcceptanceV3/`: catalog.png, resumed.png,
archive-narrator.png, lada-focused.png, lada-focused-settled.png,
device-logcat-final.txt. No fallback.used/INITIALIZATION_FAILED/Unity E/F,
FATAL EXCEPTION or package ANR in captured interval. Cache-missing warnings for
the3new bundles precede successful activation; TrySetException stack frames
belong to those known recovery warnings, not a separate crash.

## Historical visual finding / original production handback

Lada focused is substantially clearer than the old620px source shown in V2;
face and clothing detail now resolve. But thin hair wisps to image-left of the
forehead/cheek have conspicuous green/olive fringe, with similarly tinted areas
around the ponytail boundary. It remains on the later settled screenshot.
This is not a transient loading frame. The source contact-sheet review accepted
the edge too readily: numeric alpha/geometry/core-preservation checks did not
prove fringe-free rendering at the actual magnified Android size.

Return Lada edge extraction to `somegame-create-character`/imagegen production.
Inspect measured-key unmix and isolated fine-hair opacity; residual-G clamping
can leave opaque olive pixels and is a plausible cause, not a proven full
diagnosis. Preserve legitimate copper rim, eye/teal core colours and character
identity. Do not globally remove green or alter shared compression to hide it.
Review all4variants at real display magnification on dark/light backgrounds.
Do not assemble independent facial/body parts. No fix or rebuild in acceptance.

Four-state rendering, same-line main comparison, all6episodes/13choice branches/
3endings, bright/long/3choice UI, full save/reload/audio matrix remain **not run**.
Only retained-save resume with new release was observed. Missing full coverage
and the visible defect block acceptance. Obtain a fresh final-slot approval
only after the repaired source package is ready.

Own package and emulator stopped11:36UTC; AVD/save/APK retained. No foreign
process stopped, no commit/push/publication, no caches or user saves deleted.

## Author decision and unchanged-APK continuation — 12:15 UTC

User explicitly instructed: «Давай не будем исправлять подобные дефеткы в этой
истории. Продолжай». The fine-hair fringe and similar minor image-edge defects
are accepted cosmetic limitations for this story. This supersedes the above
production handback/new-repair request, which is preserved as historical evidence.
No image edits or rebuild are being performed. Missing assets, fallback, broken
choices/transitions/saves, unreadable UI, crashes and ANRs remain blocking.
Primary FIFO acquired by tk-route-final; shared unity acquired after a clean
actual Unity/emulator process probe. APK SHA, source Ink hashes, four imported
Lada PNGs and their metas match the recorded V3 candidate. Required full route,
ending and save/UI/audio evidence is still pending; no acceptance pass claimed.

## Route1 completed — 12:52 UTC

Unchanged installed base.apk SHA256 exactly matches the V3 APK above. No
reinstall, build, source/art changes or data wipe. Own AVD was resumed after FIFO
and shared unity acquisition. Ordinary catalog restart was confirmed only after
copying the two original test save files (57bytes) to `save-before-routes`.

- Actual path: keep_reel → go_live → trust_roman_map → take_ledger → hold_tim
  → publish_archive → **ending_city_hears** with Tim still missing.
- Runs `9f84a3854b974ef3a20eebfce0e57f0c` seq1–181 and, after deliberate app
  restart, `0b4c95e29da945c2ae05ece4a255ad36` seq1–108. All289 ordered markers
  recovered without gaps/conflicting duplicates; all6 episode.completed events,
  six expected choice selections, unchanged release and storyCount1 verified by
  `Build/AndroidAcceptanceV3/audit_r1.py` → `r1-telemetry-audit.json` PASS.
- Real catalog entry/open/return was used separately for every episode. All six
  distinct episode-cover cards were observed. Final catalog shows6/6 completed;
  completed episode card shows100%. Evidence: `r1-e2-card.png` through
  `r1-e6-card.png`, `routes-catalog.png`, `r1-completed-catalog.png`.
- Save/resume: after e5 narrator «Самые прочные тайны печатают мелким шрифтом»
  pressed Home, backed up15files/3065bytes, force-stopped own package, relaunched
  and used real catalog Continue. E5 card retained≈53%; exactly the same text
  and scene returned. See `r1-e5-start-9f84a385-0181.png`,
  `r1-e5-resume-card.png`, `r1-e5-resumed.png`; `save-mid-r1-e5` retained.
- Inspected settled two-choice screens in e1/e3/e4/e5 and the long e6 prompt
  with all3 readable, fully on-screen buttons (`r1-e6-choice-0b4c95e2-0099.png`).
  Dark and bright backgrounds/long narrator text remain readable; no content
  clipping or blocked choice observed. Accepted edge/magnification limitations
  remain unchanged. Some early automatic screenshots caught entrance animation;
  those are not treated as settled visual evidence.
- Finale visibly preserves consequences: Tim's phone and missing brother in
  `r1-ending-city-0b4c95e2-0104.png`; archive route reached, followed by series
  completion and catalog return. Final save copy:20files/4121bytes in
  `save-r1-completed`; before-ending copy retained separately.
- No fallback.used, Unity E/F, INITIALIZATION_FAILED, save read/write failure,
  FATAL EXCEPTION or package ANR found in captured route logs/final device log.
  Evidence-only helper advanced zero-choice dialogue and stopped at choices,
  boundaries and timeouts; no runtime hooks or manufactured save inputs.
- Audio technical check: active Android track56 belongs to app PID7122,
  stereo24kHz with48kHz mixer, zero underruns in sampled AudioFlinger dump.
  This proves an active audio path, NOT subjective listening quality or a full
  per-cue/loop/volume matrix. `r1-e6-audio-flinger.txt` retained.

Own app/emulator stopped after completion; data/APK and all evidence retained.
Shared unity released; actual probe shows no remaining own emulator/Unity.
Checkout will be yielded to the already queued rebrand task. Its future renamed
app/package is NOT covered by this immutable old-package APK evidence.

Remaining: R2/R3/R4 from `agents/tk-route-final.md`, seven unselected options,
other conditional consequences and two other endings; full listening/audio and
remaining key visual coverage. No new blocking functional defect found in R1,
but incomplete mandatory Android branch coverage is not acceptance success.
Efficient next checkpoints: in-app restart only e6 for R4 (same R1 prefix), then
restart e3 for R3 (same R1 e1/e2 prefix), then full e1 restart for R2. Confirm
the real reset prompt and retained prior decisions each time; no raw save edits.

## R4 completed — 13:17 UTC

Same V3 APK, installed base.apk hash reverified13:05; no rebuild or product/art
edits. Real catalog restart of e6 explicitly confirmed after observing its
single-episode reset prompt. The completed R1 e1–e5 save files are byte-identical
before reset, immediately after reset and after R4 completion. Thus R4 retains
keep_reel/go_live/trust_roman_map/take_ledger/hold_tim, changing only the final
decision to **take_the_watch → ending_zero_watch**.

Run `c4c49f7fd9054c7489bf758a950d6a18`:83ordered events without gaps or conflicting
duplicates; choiceId2 seq74, episode.completed82, catalog.returned83; unchanged
release identity verified. `Build/AndroidAcceptanceV3/audit_r4.py` and generated
`r4-telemetry-audit.json` PASS. Long three-choice prompt and all buttons inspected
in `r4-e6-choice-c4c49f7f-0073.png`. Tim returns in
`r4-ending-a-c4c49f7f-0078.png`; final watch text in
`r4-ending-b-c4c49f7f-0080.png`; catalog6/6 in
`r4-ending-c-c4c49f7f-0083.png`. Intermediate screenshots are sampling evidence,
not a claim of reviewing every dialogue frame. No fallback, Unity E/F, save or
initialization errors, crash or package ANR in `device-logcat-r4-final.txt`.

Completed save backup:20files/4122bytes in `save-r4-completed`; prefix comparison
uses `save-r1-completed` and `save-r4-after-e6-reset`. Own app/emulator stopped
13:17UTC with data retained; shared unity released. Primary product diff hash
remains `1cb0c58c9569c69dfbf0619a9f0f5d004b9983adb6e487237794a33706f76c74`.
Yielding at this completed-route boundary to queued kostroma-dev-release.
Remaining: R3 from actual e3 restart, then R2 from e1 restart; six previously
unselected options, quiet_shift ending and other conditional consequences.
Full audio listening/per-cue matrix remains unverified. No new functional
blocker found, but incomplete mandatory route evidence still blocks acceptance.

## R3 completed — 14:18 UTC

Actual e3 catalog restart explicitly reset e3–e6 only. R1/R4 e1–e2 prefix
remained byte-identical before reset, after reset and after R3 completion.
Path: keep_reel → quiet_test → trust_zoya_key → save_inga → hold_tim →
destroy_circuit → ending_quiet_shift. Tim and Nina remain missing.

Run 7244f4321b6644329773f12bee242ae3 seq1–205; five expected choices
at14/56/115/121/198, e3–e6 completion and catalog.returned205. audit_r3.py PASS.
Final choice197 has two buttons: publication is correctly unavailable with
insufficient evidence. Visually inspected quiet-test Asya text, Zoya key,
Inga/Tim choices, two-option finale, destroyed membrane/Nina/archive consequence,
and completed6/6 catalog. Evidence: r3-e3-b-7244f432-0031.png,
r3-e4-choice-7244f432-0055.png, r3-e6-choice-7244f432-0197.png,
r3-ending-a-7244f432-0202.png, r3-completed-7244f432-0205.png.
save-r3-completed preserves20files/4011bytes. No fallback, Unity E/F,
initialization/save failure, crash or package ANR in device-logcat-r3-final.txt.

## Coordination continuity

Foreign emulator was not terminated: ten-minute heartbeat waited until it ended
naturally. On13:56UTC preflight was clear; own saved emulator resumed, unchanged
installed APK hash verified. R3/R2 use only immutable V3 APK. Explicit approved
source-only checkout concurrency allowed continued device/ignored evidence work
while retaining shared unity. Tracked reports waited for primary FIFO reacquisition.
The transient empty foreign checkout-lock directory was not deleted by this task;
by14:36 it had been replaced by a valid foreign owned lock. No foreign source,
process, save, lock record, Git index, branch, publication or release was modified.

## Limitations and bounded next action

Accepted fine-hair fringe and similar minor image defects remain unchanged.
Manual screenshot sampling is not inspection of every rendered frame. No claim
of listening to every cue/transition/loop or validating volume balance: only the
recorded Android active-audio/no-underrun technical sample is proven. Separate
manual listening remains. New Kostroma package, later catalog edits, Remote
delivery, other devices and store signing/publication are outside this immutable
Embedded APK acceptance. Candidate-changing integration invalidates these APK
results and requires fresh bounded validation, not reuse of old evidence.

## R2 and combined route coverage completed — 14:51 UTC

Actual full e1 restart, after R3 save backup, followed give_reel → quiet_test →
trust_roman_map → save_inga → trust_tim → publish_archive → ending_city_hears.
Same run7244f4321b6644329773f12bee242ae3 seq206–482,277 ordered markers;
choice IDs0/1/0/0/0/0 at215/289/331/391/397/473. Six episode completions and
catalog.returned482; all six release activations match the immutable V3 hash.
audit_r2.py PASS. save-r2-completed contains20files/4110bytes. Final catalog6/6
visually confirmed in r2-completed-7244f432-0481.png (capture after return settled).
No fallback, Unity E/F, initialization/save failure, crash or package ANR found
in device-logcat-r2-final.txt. A log without errors does not prove visual quality.

Roman gives Nina's token/map correction at r2-e4-roman-7244f432-0346.png, showing
the stronger-trust consequence. Tim choice and response inspected in
r2-e5-tim-choice-7244f432-0396.png and r2-e5-tim-saved-7244f432-0398.png.
Final prompt r2-e6-final-choice-7244f432-0472.png correctly says Tim is with the
group, Nina is still missing, and presents all three readable ending choices.
The city epilogue reaches Tim's «Слышишь? Ничего» instead of the missing-Tim
phone text observed in R1. Functional consequence confirmed; visual failure below.

All four individual audits rerun PASS; routes-coverage-audit.json records
854 ordered runtime markers, all6episodes,13 distinct options and3endings.
This is four actual replay/checkpoint routes, not all86 static combinations.
R3/R4 inherited prefixes are explicitly save-byte verified; no save injection.

## New blocking visual finding — Tim in bright city epilogue

At seq477, Tim main has broad white/transparent-looking patterned areas across
hair, hoodie, jacket and trousers, plus a dark/coloured surrounding halo on the
bright square. This is substantially larger than the accepted fine-hair fringe.
r2-ending-tim-7244f432-0477.png and a separate later screenshot
r2-ending-tim-settled.png show the same persistent result, not entrance animation.
Source Assets/Characters/тим/view/whole/hoodie/main.png was visually inspected;
it does not show these white patterned body regions. Cause (alpha/material,
import/compression or emulator rendering) has NOT been established. No fix,
recompression, asset change, rebuild or new emulator run was attempted.

Per author's no-cosmetic-repair instruction, preserve the candidate and report
the finding. Do not treat the earlier small-edge acceptance as automatic
approval of this much broader defect. Next bounded action: obtain author's
explicit decision to retain this exact appearance or authorize a separate
production/rendering investigation. Only an actual candidate change requires
a newly authorized build/replay slot. Mandatory route coverage itself is complete.

Cleanup: installed base.apk SHA reverified unchanged after R2. Own package and
emulator stopped, all test saves/evidence retained; shared unity released and
actual process probe found no Unity/emulator remaining. Primary report-only
checkout is released after scoped diff review. Automation remains paused because
external-resource waiting is over; outstanding appearance decision is not a
reason to rerun the already completed routes on a timer.
