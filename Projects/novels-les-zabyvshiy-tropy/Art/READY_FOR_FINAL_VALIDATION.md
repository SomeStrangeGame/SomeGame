# Ready for final validation

Status: `ready-for-final-validation` (not accepted, not catalog-registered).

## Completed static scope

- Atomic scaffold copied from `Projects/novels-content-template`; Unity version and repository-relative packages preserved.
- `card.json`, `NovelContentAsset`, six source episodes and root include authored.
- Five characters, 20 distinct whole-image variants; all 20 PNGs report an alpha channel.
- Twelve distinct locations, ten used choice icons, cover, story-local Bubble prefab/sprites.
- Two music/ambient WAV files and six SFX, synthesized without external samples.
- Five meaningful choice groups and three statically reachable ending knots.
- All referenced location, choice-icon and audio IDs resolve to exact files in a dependency-free audit.
- Narrative, characters, non-character art and complete source Ink each have current originality evidence with the search limitation stated.

## Static evidence (2026-09-06)

- `Tools/novels-tools/novels-content doctor` → configuration valid.
- `python3 -m json.tool Config/card.json` → valid JSON.
- Scoped `git diff --check` → clean.
- Selector audit → no missing referenced location, choice or audio file.
- Reachability audit → sequential `LZT_s01e01`…`LZT_s01e06`; endings `ending_shared`, `ending_white_map`, `ending_nameless`, each terminates at `END`.
- Inventory: 20 character variants, 12 locations, 10 used choice icons, 8 WAV files; project size about 37 MiB before Unity import.

## Deferred by mandatory authorization boundary

No Unity Editor, Ink compilation, `.ink.json`/source-map generation, asset import, content validation/build, Catalog registration, Android Embedded APK, emulator replay, or Player visual check has run. New `.meta` files for newly authored raster/audio/source assets are expected from the canonical Unity import in that one final slot; existing Bubble GUIDs were preserved while its referenced surfaces were replaced.

The separately authorized acceptance slot must: import the project; confirm MCP live/restart proof; compile Ink; audit every selector in generated content; register the card in the requested Catalog position; build story and Catalog; build a fresh Android Embedded APK; replay all five choice groups and all three endings through the real Catalog flow; verify save/resume and absence of fallback markers; inspect character alpha edges/scale, all backgrounds, long dialogue, maximum choice group, safe area and Bubble pressed/fallback states.

Known visual risk to judge in Player: generated character sheets required deterministic dark-matte keying. Alpha exists, but subtle residual halo or loss in dark clothing edges is possible and is blocking if observed. UI crops must likewise be checked on real scene backgrounds. No publication or integration is authorized.
