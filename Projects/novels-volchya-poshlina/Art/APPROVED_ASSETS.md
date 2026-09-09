# Approved asset manifest — `volchya-poshlina`

Status: `approved-for-import` → project-bound mapping complete; runtime proof deferred.

## Locations

| Scene | Runtime selector | Unity file |
|---|---|---|
| Broken bridge | `bridge` | `Assets/Locations/bridge.png` |
| Birch boundary | `boundary` | `Assets/Locations/boundary.png` |
| Charcoal clearing | `charcoal` | `Assets/Locations/charcoal.png` |
| Frozen ravine | `ravine` | `Assets/Locations/ravine.png` |
| Wolf ford | `ford` | `Assets/Locations/ford.png` |
| Watch hut | `watch-hut` | `Assets/Locations/watch-hut.png` |
| Oath oak | `oath-oak` | `Assets/Locations/oath-oak.png` |
| Berezhki dawn | `berezhki` | `Assets/Locations/berezhki.png` |

All eight backgrounds are required, 16:9 RGB PNG, and used by `s01e01.ink`.

## Whole-character packages

| Character / outfit | Required variants | Runtime root |
|---|---|---|
| Ярина / `travel` | `main`, `alarmed`, `frostworn` | `story/character/characters/ярина/view/whole/travel/` |
| Макар / `travel` | `main`, `confession` | `story/character/characters/макар/view/whole/travel/` |
| Анфиса / `travel` | `main`, `ashamed`, `resolute` | `story/character/characters/анфиса/view/whole/travel/` |
| Савва / `travel` | `main`, `confession` | `story/character/characters/савва/view/whole/travel/` |
| Седой / `natural` | `main`, `speaking` | `story/character/characters/седой/view/whole/natural/` |
| Митя / `village` | `main` | `story/character/characters/митя/view/whole/village/` |

All character assets are full-body RGBA PNGs. Edited variants derive from their
corresponding whole identity master. Generated checkerboard backgrounds in edited
drafts were rejected; accepted files use a deterministic colour-key alpha mask and
passed dark/light compositing review. No modular body parts are used.

## Presentation, cover and audio

- Cover: `Config/cover.png`, vertical 2:3, no embedded words.
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
