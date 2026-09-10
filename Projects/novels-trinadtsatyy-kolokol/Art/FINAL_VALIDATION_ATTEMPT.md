# Final validation attempt — 2026-09-09

Current status: **blocked — fresh V2 Android character defect; not accepted**.
The old child-panel failure below is historical. Fresh graphite/bronze Bubble
APK builds and first-choice smoke pass; Lada quality/alpha blocks further
acceptance. See `Art/ANDROID_ACCEPTANCE_V2.md` for exact current evidence.
The initial failed attempt is preserved below, followed by the successful
user-requested retry. Android and catalog gates remain pending. No publication
was requested or performed.

## Exact candidate and environment

- Integration clone: `/private/tmp/tk-integration-X88NJZ`.
- Branch: `codex/story-batch-trinadtsatyy-kolokol`; baseline merge
  `a6ce3bfbeabafeea9dd513868297d70681175a6a`, candidate `d735a11f`.
- Unity 6000.3.11f1, configured official Pipeline 0.5.0-exp.1.
- Primary FIFO owner `tk-final-validation`, shared Unity and catalog locks.
- Primary runner `editor-gate --project <clone>/Projects/novels-trinadtsatyy-kolokol
  --start-editor --compile --no-stop-editor --human-approved`, startup limit 120s,
  operation limit 180s. Editor PID 49028 started at 07:34:56 UTC.
- Log: `/Users/iantonishin/Fork/SomeGame/Novels/Build/Logs/automation/editor-gate-20260909T073456Z-editor.log`.

## Observed failures and bounded cleanup

The gate returned `startup_timeout` after 120.833s: Pipeline port was not created.
The fresh licensing log at 07:34:57 shows child PID 49029 failed to acquire
`Unity-LicenseClient-iantonishin` and reported another running client. Editor
reported licensing initialization failure after 74.83s. This is an observed
local IPC conflict, not evidence of an invalid account or license.

Read-only `licensing-preflight` confirmed pre-existing bundled licensing clients
PID 36870 (standard named pipe, started 06:54:51 UTC) and PID 91796
(version-specific named pipe, started 2026-09-07 10:58:33 UTC), with no Hub.
Only this attempt's Editor PID 49028 received TERM; subsequent process and queue
checks confirmed no remaining Editor. Licensing clients were not stopped.
No socket, license, cache, user save or unrelated project was deleted or changed.
No recovery retry was run: exact-PID recovery permission remains to be resolved
under the repository's Unity concurrency contract.

## Story-local correction and static verification

Unity also reported a parser failure in `ProjectSettings/TagManager.asset` at
line 41. Empty layer scalars were bare `-` lines. They are now explicit empty
strings, preserving all 32 slot indices, named layers, sorting and rendering
layers. YAML parsing and normalized before/after equality passed. The Unity
parser fix remains **unconfirmed in Editor** until a successful restart.

`Art/check_story.py --self-test` passed: 86 complete routes, three endings,
all 13 options and seven rejected negative fixtures. Source Ink hashes are
unchanged. Scoped `git diff --check` passed. Changes remain local and uncommitted
in this clone; the original registered story candidate is unchanged.

## Separate Player dependency

Published base `3f6d110e` has no `Projects/apps` and its `player-build --help`
does not support `--app`. Current primary `ContentPipeline.md` requires this
profile. Corresponding branding/tooling changes exist only as foreign dirty
primary work and were not copied into this clone. A reviewed, separately scoped
integration of those prerequisites is required before a conforming APK build.

## Pending gates

Successful Editor import/MCP live proof and compilation; six episode-cover
bindings through supported Editor authoring; Ink JSON/source map; content and
catalog validation/build; local catalog registration preserving existing entries;
profile-aware fresh APK; exact-artifact Android route/save/UI/audio/visual checks.
No build, emulator, runtime, cover-binding or acceptance pass is claimed.

## Successful bounded retry — 07:44–07:48 UTC

After the user explicitly requested «Попробуй еще раз», agent `tk-final-retry`
acquired a new primary FIFO and shared Unity lock. One documented recovery loop
sent TERM to confirmed licensing PIDs 36870 and 91796 with no Editor/Hub running.
The runner confirmed both processes exited; no sockets, licenses or caches were
removed. Editor PID 51603 then started for the same exact clone project.

- Fresh log: primary `Novels/Build/Logs/automation/editor-gate-20260909T074407Z-editor.log`.
- Bundled client PID 51604 completed licensing handshake and entitlement lookup.
  A transient token-update warning subsequently resolved. TLS certificate-name
  warnings appeared in the Editor log; network/remote delivery is not validated.
- The corrected TagManager imported successfully, with no recurring parse error.
- `editor-gate --compile`: PASS, return code 0, 32.959s, zero C# compiler errors.
- Persistent helper: `Novels/Content/Validate`, then `Novels/Content/Build/Editor`.
  Post-operation `editor-check` PASS: ready, not compiling, scene not dirty,
  no relevant Console errors (cursor 11).
- Canonical `.ink.json` and source map generated by Unity. All JSON parses with
  UTF-8 BOM support; source map has 1262 entries across exactly six episode files.
  Unity also produced the automatic Ink integration's sibling `.json`; retained.
- 105 asset metadata files have real targets and unique valid GUIDs.
- Three content bundle audits PASS (18, 10 and 13 root assets). Output retained
  under story `Build/LocalContent`, platform `Remote/Mac` for Editor.
- Release ID: `642877f75b3ea58b302fa75b8dd630c254b585ea122f43947933214395a1e6d1`.
  Release JSON SHA-256: `b6b81cbebe4521202a196ab97b0d70f6dc72ae4b458926d7df69114a27c796aa`.
  All three bundle hashes/sizes and all 18 file-payload hashes/sizes verified.
- Only proven whitespace-only Unity churn in ProjectSettings.asset was reverted
  using a scoped patch; content settings are unchanged. Scoped diff check PASS.
- Helper stopped, owned Editor PID 51603 closed with TERM and confirmed absent.
  Shared Unity and primary FIFO locks released; no user saves touched.

Remaining: supported Editor binding of six approved episode covers, local catalog
registration/build, separately scoped app-profile prerequisite integration, fresh
Android APK and route/save/UI/audio/visual acceptance. This Editor build does not
establish those results or remote/TLS health. No publication or commit performed;
source changes and actual generated assets remain in the retained clone.

## Android final slot — 08:01–08:52 UTC

App-profile prerequisite commit f01cd1d2584fa068af9b02c83a4487e808371847.
Six approved episode covers were bound through the story-local Editor menu.
Unity compile/Validate PASS. Only this story and catalog were built for Android;
clone catalog is explicitly limited to trinadtsatyy-kolokol per user request.
Other source stories and primary product files were not changed or built.

- Story release: cee7d6090f83ba48c494e3ec9db446602bc00ac9c7ca9bfea4324c2fbdb4b44a.
- Catalog release: 1f951997ea55f4a30c5f1ab1ecfbca52f3c2988ceeb09bd3c3318a5f1e44d52b.
- APK: /private/tmp/tk-integration-X88NJZ/Novels/Build/Players/automation/nochelessie/Android/Embedded/tk-20260909-0810.apk.
- Build completed approximately 08:11:20 UTC; 84207759 bytes; SHA256
  593de7eb9ab0b7e8066b7ecc8faa9799af85e14b73f544151c7ef9325d862dba.
- Package ru.nochelessie.novels, test-signed, ARM64; API34 sdk_gphone64_arm64,
  serial emulator-5560, isolated AVD /private/tmp/tk-android-validation-HRgLrU.
- Real launcher activity opened catalog; catalog.ready storyCount1, story.selected,
  release.activated, episode.ready s01e01 and six dialogue.ready events observed.
  Run d88588a9a531431494e297153e3d8106, seq14 first choiceCount2.
- android-smoke runner failed its monkey launcher (no physical SYS_KEYS); actual
  launcher activity was then started with am start. Do not mark runner PASS.
- Build logs: Novels/Build/Logs/automation/content-gate-20260909T080657Z.log,
  content-gate-20260909T080758Z.log and player-20260909T080857Z.log.
- Device log/screens: story Build/AndroidAcceptance/device-logcat.txt,
  first-choice-settled.png, roman-choice.png, story-entry.png, catalog-launch.png.

User rejected the children's blue crystal Bubble styling. Confirmed source
cause: story-local dialogue-panel.png and choice-card.png retain that artwork,
not merely an absent override. Prefab also sets _hideChoiceText:1 and
_placeChoicesHorizontally:1; first two real choices render as empty cards.
Acceptance stops here and returns presentation to somegame-produce-story-art.
No choice or ending was completed; save/resume/audio/full visual gates untested.
Own package and emulator stopped at08:52; AVD/save data and APK retained.
Any new presentation makes this APK stale and requires a newly approved final slot.
