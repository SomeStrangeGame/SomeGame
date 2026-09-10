# Handoff history and episode-progress static evidence — 2026-09-08

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

## Episode reading progress — source-only validation

- Isolated workspace: `/private/tmp/somegame-episode-progress-Ts4FYa`.
- `python3 .../compile.py`: current first-party sources compiled with Roslyn and existing Unity reference assemblies into temp outputs only. Novels.StoryProcessor, Novels.Save, Novels.Catalog and Novels passed. No Unity executable, import, Player build or project cache modification.
- `python3 .../probe.py`: 28 managed fixture assertions passed: 0/partial/99 ceiling; actual short/long saved choices; unknown invalid/excess decisions; episode boundary and entry state; bounded silent loop; sidecar round trip, version/hash mismatch, malformed data, save preservation and reset; non-mutating v2/v3 decode.
- Prefab static checks passed: unique fileIDs, all local references, Card bindings, horizontal gold fill, raycast-disabled decorations and nonoverlapping text/bar/button geometry.
- Initial static compiler harness used stale Bee references (duplicate former contract assemblies); adjusted only the temp harness to current asmdefs/references. Initial managed probe missed Unity JSONSerialize reference; corrected temp resolver and reran successfully. These are not Unity runtime passes.
- Scope diff check passed. Original source compatibility and save envelopes preserved. Old APK emulator evidence belongs to the other task and does not validate these changes.
- Still required: real Unity import/compile, rebuilt catalog, visual check at supported portrait sizes, live exit/pause/reload/restart behavior and updated Player if needed. No new application/story content was built or installed.
