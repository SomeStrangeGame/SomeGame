# Current cross-chat handoff

Previous snapshot preserved in
[`CoordinationHandoffHistory-2026-09-05-pre-publish.md`](../archive/reports/CoordinationHandoffHistory-2026-09-05-pre-publish.md).

## Ready for integration or validation

- `catalog-resume-reset`: fixed saved-episode primary action and labelled restart;
  catalog build, fresh compile and live zdm/tzm checks passed (one Continue action).

- `fallback-art-integration`: applied the prepared fallback artwork and icons;
  final catalog Editor build, Novels compile and live portrait interaction/visual
  checks passed for zdm/tzm. Original approved mockup file was not available for
  pixel-diff comparison; Android/tablet verification was not run.

- `scp1198-bubbles-layout-v4`: current story-local Bubble sprites, prefab and evidence are ready for publication with further visual fitting intentionally deferred.
- `option-screen-prefab-split`: Choice and Wardrobe now use independent authored fallback prefabs; scoped checks, TZM content build and fresh Novels compile passed. Manual portrait smoke remains.
- `tzm-choice-reference-parity`: story-local white/cyan presentation and neighboring-card affordance are implemented; scoped checks, content builds and fresh compiles passed. Final aesthetic approval remains.
- `scp-genre-catalog`: reusable genre-catalog skill and authored SCP catalog variant passed catalog and tooling gates. Fresh Player visual acceptance remains pending.
- `parallel-story-orchestration`: parallel story-local preparation with serialized checkout/Unity/integration was documented and validated.
- `fast-validation-protocol`: fast, standard and release validation levels plus batched validation slots were documented and validated.

## Blocked or deferred gates

- `catalog-playmode-review`: paused until manual visual review is explicitly resumed.
- `gpl-catalog-registration`, `gpl-lea-layered-rework`, `gpl-mark-integration`, `gpl-vera-integration`: content/build checks passed; bounded in-game visual gates remain.
- `tzm-wardrobe-runtime`: implementation and content checks passed; portrait visual review remains.
- `busya-lake-blanket-story`: implementation, story validation, Android content and Embedded Player build passed; strict acceptance still requires one fresh second-route replay.
- `tzm-episode1-android-smoke`: episode completed, with Sally fallback markers and final-screen overlap retained as limitations.
- `gpl-episode3-full-smoke`: paused at episode 3 line 257 after episodes 1-2 completed.
- `android-memory-full-smoke`: paused because the APK content was stale and must be rebuilt before resumption.
- The WebGL prototype remains only on `prototype/webgl-local-platform`; compilation and browser smoke were not run.

## 2026-09-07T11:03:00Z — nochelessie-catalog-drafts — ready-for-final-validation

Task: Finished a Nochelessie catalog candidate informed by «Колодец, который
зовёт» and «Волчья пошлина»: original background, direct fallback variant
override, provenance evidence and updated MVP status.
Changed: Projects/novels-catalog/Assets/RemoteAssets/catalog/nochelessie,
Projects/novels-catalog/README.md, Docs/AI/plans/SlavicMysticismMvp.md
Validation: scoped diff, GUID, prefab-inheritance, image-format and config checks
passed. Catalog build stopped because the catalog project is open in Unity.
Pending / risks: final catalog build and fresh Player visual acceptance require
an explicit heavy-slot approval; no APK was launched.
Suggested next step: close the catalog Editor, then run the single approved
final validation slot.

## 2026-09-07T15:16:20Z — catalog-custom-reset — ready-with-limitations

Task: Removed all custom Catalog prefab variants and their selection/build
wiring so fallback is again the only active catalog contract.
Changed: catalog variant assets, CatalogAddresses, Player build automation and
script, runner/parser tests, Catalog README and Nochelessie MVP status.
Validation: scoped diff check, shell syntax and 37 runner tests passed. Catalog
content build stopped because Projects/novels-catalog is open in Unity.
Pending / risks: design and approve fallback before introducing the four new
product variants; Unity validation belongs to that later design task.
Suggested next step: define fallback hierarchy, states and override points.

Completed/superseded fallback entries preserved in
[`CoordinationHandoffHistory-2026-09-07-fallback-integration.md`](../archive/reports/CoordinationHandoffHistory-2026-09-07-fallback-integration.md).

Earlier fallback and completed skill details preserved in
[`catalog snap history`](../archive/reports/CoordinationHandoffHistory-2026-09-08-catalog-snap.md).

Resume/reset and locked-card text evidence preserved in
[`2026-09-08 catalog controls history`](../archive/reports/CoordinationHandoffHistory-2026-09-08-catalog-controls.md).

Settings implementation and trim details preserved in
[`background delivery history`](../archive/reports/CoordinationHandoffHistory-2026-09-08-background-delivery.md).
Privacy/terms/support URLs remain empty pending real application links; UI and volume checks passed.

Scroll-snap details preserved in [publication history](../archive/reports/CoordinationHandoffHistory-2026-09-08-publication.md); APK/tablet validation remains pending.

## 2026-09-08T08:09:00Z — catalog-background-download — ready-for-review

Task: Metadata-first catalog with ordered background delivery and per-card progress/retry.
Changed: CatalogFlow/CatalogDownloads/ApplicationRuntime/NovelProgress/ContentDeliveryFlow;
SDK catalog model/controller/card, preview contract/convention/exporter; existing live validation,
Editor references, Catalog README, ContentPipeline guide and Architecture memory.
Build exports platform catalog-preview.json from the unique authored definition (both layouts),
pinned to releaseId; new client requires rebuilt story content. Card.json remains schema 2.
Queue uses verified delivery groups, continues across reading, retains leases/releases and
blocks open/reset until ready. Preview reads never delete incompatible progress.
Validation: zdm content-gate 075209 and tzm 075730 passed; editor-gate 075901 and final compile
completed without compiler errors. Download fixture passed serial order, failure/retry,
partial lockout, mismatch, cancellation and actual card controls in isolated temporary cache.
Live zdm/tzm, settings and snapping passed. Snap probe needed continuous Editor frames and
post-LateUpdate observation; earlier failed probes remain in log, corrected checks passed.
Local startup evidence: app.started 08:00:00.628, catalog.ready 08:00:02.304, zdm ready 07.372,
tzm ready 13.471. Preview sizes 2661/1731 bytes (11/7 episodes), excluding covers.
Pending: no per-episode dependency manifest: all shared story groups conservatively gate every
episode together. Remote/mobile timing and Android/iOS Player not tested; rebuild target outputs.
Runtime: Editor playing at catalog start; inspected ready-card Game View in
Novels/Assets/Build/Logs/catalog-background-final-20260908.png (TZM ready state).
No user cache/save reset, scene save, commit or publish. Isolated fixture cache removed.
Final cleanup compile and delivery suite passed again; validation restores temporary background-frame mode.

Catalog header/story spacing evidence is preserved in
[`spacing history`](../archive/reports/CoordinationHandoffHistory-2026-09-08-catalog-spacing.md).

Episode-cover evidence: [history](../archive/reports/CoordinationHandoffHistory-2026-09-08-episode-covers.md).
Pending: no new episode art assigned (11/7 use story covers); no Android/iOS Player or remote timing gate.

Author support evidence: [history](../archive/reports/CoordinationHandoffHistory-2026-09-08-authors.md).
Set an author only when supplied; existing stories remain unsigned. No device verification.

## 2026-09-08T09:59Z — catalog-media-priority — ready-for-review
Corrected precedence: episode video → loaded own episode image → story video → story image. Runtime resolver, Inspector hint and canonical guide/memory updated; fresh compile, 8-case priority matrix, covers and live catalog checks passed (agents/catalog-media-priority.md). Previous player/build evidence: [video history](../archive/reports/CoordinationHandoffHistory-2026-09-08-video.md). No production clips assigned; device/remote/offline playback unverified. Editor playing at start; helper stopped, saves unchanged; no rebuild/commit/publish.

## 2026-09-08T10:12Z — publish-main-snapshot — publishing

Task: Commit and publish the current primary checkout only; user explicitly excluded all worktrees.
Changed: fallback catalog/runtime/SDK and tools, primary Volchya Poshlina source snapshot, skills and existing documentation/evidence. Generated screenshots remain local and ignored; Kolodets whitespace-only churn normalized without content changes.
Validation: docs-check and all three tooling suites passed; zsh syntax and staged whitespace checks passed. Prior catalog Editor/build evidence above retained; no new Unity/device or story acceptance claimed.
Pending / risks: remote publication and SHA verification next; all existing device/remote/manual gates remain deferred. No worktree branches or files integrated; catalog still contains zdm/tzm only.
