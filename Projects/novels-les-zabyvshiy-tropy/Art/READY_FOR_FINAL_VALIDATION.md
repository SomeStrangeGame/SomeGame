# Ready for final validation

Status: source candidate for final validation (not accepted, not catalog-registered; visual format and duration remain unverified).

## Completed static scope

- Atomic scaffold copied from `Projects/novels-content-template`; Unity version and repository-relative packages preserved.
- `card.json`, `NovelContentAsset`, six source episodes and root include authored.
- Five characters, 20 distinct whole-image variants; all 20 PNGs report an alpha channel.
- Sixteen distinct locations, twelve used choice icons, cover, story-local Bubble prefab/sprites.
- Six distinct episode catalog covers in `Config/EpisodeCovers`, assigned by `_catalogCover` for s01e01–s01e06. Full source-art review and generation evidence: `EPISODE_COVERS.md` and `EPISODE_COVER_PROMPTS.md`.
- Story-owned schema-1 website preview in `Config/Preview/preview.json`: 47 ordered blocks copied from the canonical opening of `s01e01.ink`, stopping before its first choice, with only the three referenced approved character images.
- Two music/ambient WAV files and six SFX, synthesized without external samples.
- Five meaningful choice groups (five decisions per route, twelve authored options) and three statically reachable ending knots.
- All referenced location, choice-icon and audio IDs resolve to exact files in a dependency-free audit.
- Narrative/text review iteration 4 is recorded in `ORIGINALITY_EVIDENCE.md`; unchanged character and non-character art retain their prior limited reviews. This is not visual acceptance.

## Static evidence (refreshed 2026-09-07)

- `Tools/novels-tools/novels-content doctor` → configuration valid.
- `python3 -m json.tool Config/card.json` → valid JSON.
- Scoped `git diff --check` → clean.
- Selector audit → no missing referenced location, choice or audio file.
- `python3 Projects/novels-les-zabyvshiy-tropy/Art/check_source.py` → 72 source routes; sequential `LZT_s01e01`…`LZT_s01e06`; 24 shared, 9 white-map and 39 nameless endings, each reaches `END`. Checks five episode markers and one final marker per route, resource/speaker resolution, choice/global naming collisions, callback consistency and failed-bind explanation. This bounded source interpreter is not an Ink compiler.
- The same audit validates preview schema/story/episode/source IDs, safe relative image paths, exact referenced-file inventory, full character-marker coverage, speaker attribution and source-text order.
- Inventory: 20 character variants, 16 locations, 12 used choice icons, 8 WAV files.
- Selector audit: explicit selectors and neutral defaults resolve to 20 produced PNGs, including protagonist runtime ID `maincharacter`. Previous name-only auditing missed this mapping and indented speakers.
- Prior media header audit: all 51 PNG signatures valid; character PNGs RGBA; all 8 WAV files mono 16-bit PCM at 44.1 kHz. Header checks do not establish full decoding, alpha-edge quality or seamless audio loops.
- Length: 5,271 source word units; 3,542–3,650 displayed word units per route including offered choice text. At an assumed 145–165 words/minute this is roughly 21–25 minutes of reading, plus decisions/transitions. The target 25–45 minutes needs actual timing; the prior 27–34 estimate is withdrawn.
- Source SHA-256 (ordered root and episode names/bytes): `5705dd152565c3cb66cc498a87ee249468c6ffcd5887459ee6388bb215e6ed3a`. The audit prints a new digest when inputs change.
- Re-review checks: melody introduced before its callback on every route; warning about Lada's possible exclusion; closed jar stored for the crossing; all five characters covered by another person's memory in the shared resolution.

See `STORY_REVIEW.md` for fixes, save compatibility and residual evidence limitations.

### Episode-cover and website-preview update — 2026-09-09

All six RGB PNGs fully decoded and passed Pillow verification at 1024×1536; source bytes preserved from generated masters. Updated dependency-free source audit checks six exact bindings, PNG headers/dimensions, distinct hashes and no duplication of the story cover. Three in-memory invalid-binding probes were rejected. Doctor and 72 routes still pass; Ink digest is unchanged. The branch now contains the current cover-aware SDK; preview export and all six real catalog cards still require the final Unity-backed validation. PNG payload totals 18,452,350 bytes; startup delivery cost and UI crop still require final validation. The prior 51-PNG audit above is historical and excludes these six new files.

The branch now includes the current SDK and the newly required story-owned reading preview. Its excerpt is copied verbatim from the first episode and stops before the first unresolved choice; no bridging prose was invented. Portrait and landscape website layout/interaction remain deferred because this request does not include site deployment and no browser-generated runtime preview exists before acceptance outputs.

## Deferred by mandatory authorization boundary

No Unity Editor, Ink compilation, `.ink.json`/source-map generation, asset import, content validation/build, Catalog registration, Android Embedded APK, emulator replay, or Player visual check has run. New `.meta` files for newly authored raster/audio/source assets are expected from the canonical Unity import in that one final slot; existing Bubble GUIDs were preserved while its referenced surfaces were replaced.

The separately authorized acceptance slot must: import the project; confirm MCP live/restart proof; compile Ink; audit every selector in generated content; register the card in the requested Catalog position; build story and Catalog; build a fresh Android Embedded APK; replay all five choice groups and all three endings through the real Catalog flow; verify save/resume and absence of fallback markers; inspect character alpha edges/scale, all backgrounds, long dialogue, maximum choice group, safe area and Bubble pressed/fallback states.

Known visual risk to judge in Player: generated character sheets required deterministic dark-matte keying. Alpha exists, but subtle residual halo or loss in dark clothing edges is possible and is blocking if observed. Sixteen 627×627 backgrounds differ from the original 16:9 manifest target; `LocationLayout.SetVisualSize` fits source dimensions to the available height, not a fixed 16:9 crop. Every composition still needs review against the actual viewport, character and Bubble placement. The two tall choice illustrations also need UI crop inspection. Source-format compliance is not established by changing the manifest wording. No publication or integration is authorized.
