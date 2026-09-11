# Static validation handoff

## Current revision — 2026-09-10

Current source SHA-256: `d33f3b7eded22668ece53a564e854425544ca6638d81f486b4734f170a60ab0f`.
`check_story.py` passes all 372 routes, 192 pre-final combinations, 18 options,
13 knots, three distinct final-state contracts and seven authored notifications.
There are no unreachable executable lines. All 14 locations, 5 audio files,
6 choice icons, character selectors, episode cover and authored GUIDs resolve.
The 17-block/two-character website preview exactly matches canonical opening
prose, order and speakers; PNGs equal approved source states byte-for-byte.
The readable complete scenario includes every prose alternative in source order.
Seven regression tests pass (baseline plus six in-memory negative controls).

Displayed word count: 4,800–4,939 including selected labels and notifications.
Estimated duration: 34.3–44.9 minutes at 110–140 effective words/minute; unmeasured.
Added final-state variables require fresh saves. Full revision, both originality
gates and archive limitations: [REVISION_HANDOFF.md](REVISION_HANDOFF.md).
Status remains **ready-for-final-validation**, not compiled or runtime-accepted.

## Historical baseline and earlier checks (not current source metrics)

Status: **ready-for-final-validation** (not accepted, not runtime-validated).

Prepared on 2026-09-06; refreshed on 2026-09-07 after the continuity/runtime-address audit, without launching Unity, content build, Player, ADB or emulator:

- exact story/card ID and required atomic scaffold paths checked;
- Ink structural audit: 13 knots, 15 inter-knot diverts plus terminal `END`, balanced conditional braces, 18 authored choice options across 8 meaningful decision points, 3 ending knots and one shared cliffhanger;
- current source: 5,530 whitespace-delimited words including Ink control text. Exhaustive traversal of the supported source subset counts 3,753–3,896 displayed words per complete route, including the selected choice labels and excluding command/metadata text. Estimated 27–36 minutes at 110–140 effective words/minute; actual play time is unmeasured;
- `python3 Projects/novels-znak-na-dube/Art/check_story.py` passes: 192 pre-final combinations, 372 complete routes (148 keeper / 32 iron / 192 nameless), all 18 choice labels and every executable source line reached. This is a restricted source interpreter, not official Ink compilation or runtime proof;
- route checks cover exclusive nail/clapper ownership, retained map/tablet, wound chronology, NPC location continuity, and silent Miron. In-memory regression injections (trust treated as nail ownership, early wound, silent Miron speaking) are all rejected;
- all 14 referenced location IDs resolve to production PNGs; the three extension backgrounds are 724×724 and were visually checked for composition, embedded text, logos and watermarks;
- every used character/outfit/variant selector resolves to a whole-image PNG, including the protagonist's mandatory `maincharacter` directory (the earlier name-only audit incorrectly accepted `яр`);
- voice-only reflection, memory, root and cliffhanger lines use narrator presentation rather than unresolved synthetic character IDs;
- all 6 authored choice-icon tags resolve to distinct production PNGs; the unused combined draft was removed;
- all 5 referenced audio IDs resolve to 44.1 kHz stereo non-empty WAV files;
- character alpha audit confirms transparent pixels and opaque subject pixels in every runtime sprite;
- all authored GUIDs are exactly 32 hex characters, unique in the story/package asset universe, and all prefab/definition references resolve. The earlier audit missed a 33-character button GUID; it and its prefab reference are now corrected;
- four unreferenced byte-identical Bubble sprite copies were removed; the prefab keeps one canonical PNG per visual role;
- card JSON parses; `storyId`, definition ID and project path agree;
- dependency-free `novels-content doctor` and scoped `git diff --check` passed.

Deferred by `UnityConcurrency.md` until a separate current human authorization for the single final slot:

- Unity import and generated `.meta` files for new assets;
- Ink compilation, compiled story JSON and source map;
- Official Unity MCP live/restart proof for unique server `unity_novels_znak_na_dube` (the Pipeline package is inherited from the maintained template; adding the user-level Codex server entry is a shared/out-of-story scope);
- `novels-content build znak-na-dube editor`, selector/bundle validation and catalog registration;
- story-local Bubble nine-slice, safe-area, contrast and long-copy manual review;
- full light/dark alpha, scale, gaze and composition contact-sheet review in runtime;
- fresh Android Embedded APK, catalog-to-story emulator replay of all semantically distinct paths and all three endings.

Catalog is intentionally unchanged. Acceptance/integration is therefore not claimed.

## Episode cover addition — 2026-09-08

`s01e01` now explicitly maps `_catalogCover: s01e01.png` to `Config/EpisodeCovers/s01e01.png`. After user feedback that the oak version looked nearly identical to the story cover, it was replaced by a 1024×1536 RGB close composition of Tikhon's bronze bell and timber belfry. Full generation/edit provenance, visual/originality iteration 2 and checksum are in [EPISODE_COVER_HANDOFF.md](EPISODE_COVER_HANDOFF.md). The static checker validates this mapping and image header; all 372 modeled routes passed again after replacement. Source Ink and its recorded hash are unchanged. Preview export, early catalog loading, actual card crop and fallback remain deferred; no Unity/build/publish was run for this addition.

## Continuity fixes and save compatibility

- Trust scores no longer manufacture inventory. New booleans `has_iron`, `has_bronze`, `kept_path_token` track actual acquisition; existing stats, knots and named choices are preserved.
- `saved_lada=false` means delayed rescue, not death or abandonment. She accompanies Яр on both routes, stays wounded, and reacts differently according to the rescue priority.
- Тихон explicitly stays at the shrine; Весна stays at the archive exit. Their remote voices do not display physical character sprites at the oak.
- Bronze is sold to fund iron nails; no unexplained metal conversion. The iron ending uses a found stone, not the unchosen bronze clapper. Keys remain iron under bark, and the eighth oak remains standing.
- Archive investigation uses the acquired map or witnessed clues, never the placeholder phrase “map or memory of it”. The retained tablet receives a later conditional response. Мирон no longer speaks immediately before being described as silent, and his released appearance matches the available art.
- The candidate is unpublished. Added inventory state requires a fresh story run; compatibility with saves made from an earlier candidate is not claimed. Do not resume an old candidate save for final acceptance.
- Source SHA-256: `be0d7846f69904333fd744189716b2e1994cf02a49ac2143c2e83a70bc5c7c63`.
- Planned generated outputs: `Assets/Ink/znak-na-dube.ink.json` and `Assets/Ink/znak-na-dube.ink.json.source-map.json`; neither was handwritten.

## Second continuity pass — 2026-09-07

- Reconciled Мирон leaving first with Яр's later memory of shouting after him. Мирон now explicitly promises only to close the charcoal furnace; Весна wraps his own axe later rather than supplying the axe he already sharpened at home.
- Removed the eighth shrine name from the archive's seven restored names. Весна remembers the name read earlier; the unresolved eighth debt concerns Мирон carrying the path, not characters forgetting a known label. The static checker now verifies seven unique names excluding the eighth and its recognition in the next line.
- Clarified that debt-bearing boundary iron suppresses the mark at close contact, unlike ordinary knives/keys. Moving the nail away restores the pulse; sealing one mark does not silence the entire network. This reconciles subsequent voices, bronze responses and the underground bells.
- Added the physical tablet pickup before the restore/retain choice; kept the little bell on Лада's jug in the archive; removed walking on stones immediately after the stones end. Future-price images are possibilities rather than guaranteed prophecy.
- No choice IDs, gates, persistent variables, assets or endings changed in this pass. Existing fresh-run advice still applies to the earlier inventory revision.
- Manual full-source narrative review complements the restricted route model; passing that model does not establish that every possible literary inconsistency is absent. Unity-backed gates remain deferred.
- Five in-memory negative controls passed: duplicate eighth name, forgotten known name, trust manufacturing a nail, premature wound, and silent Мирон speaking were each rejected for the expected reason. No production source was modified by these tests.

## Third continuity pass — 2026-09-07

Reread the entire current source. Corrected three editorial inconsistencies: the `read_mirons_trace` label now asks to follow Мирон's path using the eighth tablet, not rediscover its already known name; the water shows another memory, not necessarily an evening (the departure occurred in the morning); Мирон's silence is further evidence of resistance, not the first evidence after his earlier independent smile. No additional branch-logic issue was identified in this pass.

Only three source lines changed; choice IDs, commands, conditions, variables, causal structure and endings are unchanged. The route checker still passes 372 routes with all executable lines reached. The editorial corrections introduce no new distinctive originality fingerprints; iteration 4's bounded assessment is retained with an explicit current-hash note in `ORIGINALITY_EVIDENCE.md`, not presented as a fresh external search. Runtime/visual gates remain unperformed.
