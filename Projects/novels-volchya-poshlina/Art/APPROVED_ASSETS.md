# Approved asset manifest — `volchya-poshlina`

Status: `approved-for-import` → project-bound mapping complete; runtime proof deferred.

## Locations

| Scene | Runtime selector | Unity file |
|---|---|---|
| Broken bridge | `bridge` | `Assets/Locations/bridge.png` |
| Birch boundary | `boundary` | `Assets/Locations/boundary.png` |
| Charcoal clearing | `charcoal` | `Assets/Locations/charcoal.png` |
| Snow chapel | `snow-chapel` | `Assets/Locations/snow-chapel.png` |
| Frozen ravine | `ravine` | `Assets/Locations/ravine.png` |
| Abandoned toll village | `toll-village` | `Assets/Locations/toll-village.png` |
| Wolf ford | `ford` | `Assets/Locations/ford.png` |
| Drowned belfry | `drowned-belfry` | `Assets/Locations/drowned-belfry.png` |
| Watch hut | `watch-hut` | `Assets/Locations/watch-hut.png` |
| Wolf den | `wolf-den` | `Assets/Locations/wolf-den.png` |
| Oath oak | `oath-oak` | `Assets/Locations/oath-oak.png` |
| Berezhki dawn | `berezhki` | `Assets/Locations/berezhki.png` |
| Berezhki witness dawn | `berezhki` | `Assets/Locations/berezhki.png` |

All twelve unique backgrounds are required, 16:9 RGB PNG, and used by
`s01e01.ink`; the witness ending intentionally reuses the same settlement image
for continuity in the fourth ending.

## Whole-character packages

| Character / outfit | Required variants | Runtime root |
|---|---|---|
| Ярина / `travel` | `main`, `alarmed`, `frostworn` | `story/character/characters/ярина/view/whole/travel/` |
| Макар / `travel` | `main`, `confession` | `story/character/characters/макар/view/whole/travel/` |
| Анфиса / `travel` | `main`, `ashamed`, `resolute` | `story/character/characters/анфиса/view/whole/travel/` |
| Савва / `travel` | `main`, `confession` | `story/character/characters/савва/view/whole/travel/` |
| Седой / `natural` | `main`, `speaking` | `story/character/characters/седой/view/whole/natural/` |
| Митя / `village` | `main` | `story/character/characters/митя/view/whole/village/` |
| Вея / `winter` | `main`, `warning` | `story/character/characters/вея/view/whole/winter/` |
| Лука / `bell` | `main`, `alarmed` | `story/character/characters/лука/view/whole/bell/` |

All character assets are full-body RGBA PNGs. Edited variants derive from their
corresponding whole identity master. Generated checkerboard backgrounds in edited
drafts were rejected; accepted files use a deterministic colour-key alpha mask and
passed dark/light compositing review. No modular body parts are used.
For the two new characters, studio-background neutral drafts were rejected
because alpha extraction damaged dark clothing. The accepted clean-alpha pose
is intentionally shared by each character's two scene selectors; identity,
anatomy and edge integrity take precedence over an unnecessary pose matrix.

## Presentation, cover and audio

- Cover: `Config/cover.png`, vertical 2:3, no embedded words.
- Episode cover: `Config/EpisodeCovers/s01e01.png`, 1024×1536 opaque PNG,
  bound to episode `s01e01` via `_catalogCover`. Separate approved catalog art,
  not a copy of the story cover or an extra scene background. See
  [EPISODE_COVER.md](EPISODE_COVER.md) for manifest, prompts and review.
- Bubble prefab: `Assets/Presentation/bubble/screen-variant.prefab`.
- Dialogue surface: `Assets/Presentation/bubble/sprites/dialogue-panel.png`.
- Choice surface: `Assets/Presentation/bubble/sprites/choice-card.png`.
- Ambient: `Assets/Audio/winter_forest_loop.wav`, 72 s seamless stereo 48 kHz.
- Motif/SFX: `wolf_motif`, `ice_crack`, `rope_strain`, `distant_howl`,
  `wooden_notch`; stereo 48 kHz WAV.

Bubble keeps the proven story-local hierarchy and accessible label fallback. Its
actual narrow-viewport, pressed-state and Player appearance remain deferred to the
final authorized validation slot.

## Generation and import provenance

Raster masters were produced with the built-in OpenAI image-generation tool from
story-specific prompts. Variants were identity-preserving edits of approved masters.
Audio was synthesized locally from original oscillators/noise envelopes; it contains
no recorded or licensed samples. Unity `.meta` files use unique project-local GUIDs
and the repository production texture profile; Bubble metadata was preserved with
its verified prefab dependency graph.
