# Handoff history — 2026-09-10 pre all-main integration

The following snapshot was moved verbatim from the current handoff before the
all-main integration. Open risks remain summarized in the current snapshot.

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

Completed/superseded fallback entries preserved in [`CoordinationHandoffHistory-2026-09-07-fallback-integration.md`](CoordinationHandoffHistory-2026-09-07-fallback-integration.md).

Earlier fallback and completed skill details preserved in [`catalog snap history`](CoordinationHandoffHistory-2026-09-08-catalog-snap.md).

Resume/reset and locked-card text evidence preserved in [`2026-09-08 catalog controls history`](CoordinationHandoffHistory-2026-09-08-catalog-controls.md).

Settings implementation and trim details preserved in [`background delivery history`](CoordinationHandoffHistory-2026-09-08-background-delivery.md).
Privacy/terms/support URLs remain empty pending real application links; UI and volume checks passed.

Scroll-snap details preserved in [publication history](CoordinationHandoffHistory-2026-09-08-publication.md); APK/tablet validation remains pending.

Background-delivery implementation evidence preserved in [episode progress history](CoordinationHandoffHistory-2026-09-08-episode-progress.md).
Pending: story-level download gating remains conservative; remote/mobile timing unverified.

Catalog header/story spacing evidence is preserved in [`spacing history`](CoordinationHandoffHistory-2026-09-08-catalog-spacing.md).

Episode-cover evidence: [history](CoordinationHandoffHistory-2026-09-08-episode-covers.md).
Pending: no new episode art assigned (11/7 use story covers); no Android/iOS Player or remote timing gate.

Author support evidence: [history](CoordinationHandoffHistory-2026-09-08-authors.md).
Set an author only when supplied; existing stories remain unsigned. No device verification.

## 2026-09-08T09:59Z — catalog-media-priority — ready-for-review
Corrected precedence: episode video → loaded own episode image → story video → story image. Runtime resolver, Inspector hint and canonical guide/memory updated; fresh compile, 8-case priority matrix, covers and live catalog checks passed (agents/catalog-media-priority.md). Previous player/build evidence: [video history](CoordinationHandoffHistory-2026-09-08-video.md). No production clips assigned; device/remote/offline playback unverified. Editor playing at start; helper stopped, saves unchanged; no rebuild/commit/publish.

Publication receipt preserved in [candidate handoff history](CoordinationHandoffHistory-2026-09-08-chernaya-candidate.md).

2026-09-08 — nevesta-episode-cover: isolated registered story worktree now has s01e01 PNG + source binding; new-cover originality and static PNG/binding/6-test/38-route checks passed (agents/nevesta-episode-cover.md). Pending: scope-safe SDK refresh, audio/alpha evidence, authorized Unity/catalog/Player validation; no main integration, build or publication.

## 2026-09-09 — chernaya-melnitsa-publish — source-published, manual acceptance pending

Canonical push verified source a48a2bda, then receipt 3f6d110e600f7ebf48b79f26425b8d74f79ec32b on origin/main. User-authorized isolated clone preserved primary branch/index/product files and foreign edits. Old clean worktree removed; story branch/8e7f2b6f retained. Static 72 routes/3 endings, 17 tests, geometry/hash/JSON/diff PASS; no Unity/build/ADB. Fresh illustrations and remaining completion/routes/visual gates await user manual build/test; source publication is not acceptance. Remote receipt: Docs/AI/archive/reports/ChyornayaMelnitsaPublication-2026-09-09.md. Local details: agents/chernaya-melnitsa-publish.md; historical art/runtime evidence: [story validation history](CoordinationHandoffHistory-2026-09-08-chernaya-source.md). Heartbeat paused; own locks/requests released.

## 2026-09-08T14:49Z — catalog-episode-reading-progress — ready-for-final-validation

Task: Approved authored per-episode reading bar and percentage above primary button. Changed: Card/model/fallback prefab, CatalogFlow, save projection and NovelRuntime flush; optional version/hash sidecar; opt-in bounded speculative Ink reads; Catalog README and Architecture routing.
Validation: isolated Roslyn compilation of four current assemblies PASS; 28 managed probes PASS (linear/branch/boundary/entry-state/loop limit, v2/v3, sidecar mismatch/corruption/reset); prefab IDs/local references/bindings/fill/layout and scoped diff PASS. Evidence commands and limits: [report](CoordinationHandoffHistory-2026-09-08-episode-progress.md). Follow-up: catalog-progress-live-check below covers catalog build, fresh compile and visible unread bar. Approximate forecast follows first available future choices; legacy saves without estimate show — until next reading exit. Saved-progress/branch/reset runtime and Player checks remain pending; old APK unchanged.

## 2026-09-08T15:40Z — catalog-progress-live-check — ready-for-review

User requested launch. Catalog-only Mac build and fresh Novels compile PASS, catalog.ready/download_ready confirmed; portrait capture shows Прочитано/0%/bar above primary button. Unity6000.3.11f1 Novels PID98697 left OPEN in Play Mode for manual review; clean baseline scene Assets/Novels/Novels.unity. Helper stopped, heartbeat paused, own locks released. Existing chernaya-melnitsa Mac story bundle deliberately not rebuilt (latest illustration not included); registry and saves unchanged. First Play ended without runtime error, followed by capture-tool error; second launch/capture succeeded, final aggregate cancelled during helper cleanup. Exact logs, run/release IDs and limitations: agents/catalog-progress-live-check.md. No APK, full story acceptance or saved-progress interaction pass claimed. Do not close user Editor without approval.

Completed branding/release entries: [preserved history](CoordinationHandoffHistory-2026-09-09-tk-integration.md). Branding compile/test limitations remain recorded there.
## 2026-09-09 — tk-route-final — blocked: all4routes/6episodes/13options/3endings and854ordered markers PASS on unchanged V3APK fd689229; no runtime/save/fallback/crash/ANR failures. New substantial persistent Tim rendering defect in R2 city epilogue (broad white body/hair pattern and halo); minor-edge waiver not silently extended; no repair/rebuild. Audio listening remains unverified. Evidence/next author decision: agents/tk-route-final.md and retained clone Art/ANDROID_ACCEPTANCE_V3.md; screenshot Build/AndroidAcceptanceV3/r2-ending-tim-settled.png. Own emulator stopped with saves retained; releasing own resources/request after review, automationPAUSED. No foreign process/source touched; old APK does not validate Kostroma. Historical TK/branding/release and completed Kostroma site/dev/video records: [history](CoordinationHandoffHistory-2026-09-09-tk-integration.md).
Completed release skill, shared-root/app-manifest and publisher/site/carousel records: [integration history](CoordinationHandoffHistory-2026-09-09-tk-integration.md), [release skills](CoordinationHandoffHistory-2026-09-09-release-skills.md), [publisher history](CoordinationHandoffHistory-2026-09-09-publisher-site.md).

Completed web-publication and final-main-integration records are preserved in [10 September history](CoordinationHandoffHistory-2026-09-10-story-writing.md).

Completed story-writing and Nightwood alpha/runtime work: [preserved history](CoordinationHandoffHistory-2026-09-10-story-archive.md).
Nightwood: fresh Embedded APK and alpha runtime evidence passed; full route matrix remains pending (agents/les-zabyvshiy-tropy-runtime-recheck.md).
