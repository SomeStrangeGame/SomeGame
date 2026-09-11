# Approved production manifest

Every row maps to a used scene in `Design/NARRATIVE_PACKAGE.md`.

## Environment additions — expansion iteration 2

| Selector | Runtime file | Narrative use | Provenance / gate |
|---|---|---|---|
| `letter-house` | `Assets/Locations/letter-house.png` | Leda writes her first self-owned memories; Mira recovers her childhood warning | original image generation for this story; passed visual originality review |
| `frozen-orchard` | `Assets/Locations/frozen-orchard.png` | Drowned memory orchard and hidden hydrological evidence | original image generation for this story; passed visual originality review |
| `old-weir` | `Assets/Locations/old-weir.png` | Physical consent experiment at the frozen sluice | original image generation for this story; passed visual originality review |

All three are explicit `Локация:` selectors in `s01e01.ink`; no unused expansion art was retained.

## Character packages

| Selector | Outfit | Used whole variants | Source master / provenance | Gate |
|---|---|---|---|---|
| `Мира` | `winter` | `main`, `tender`, `determined` | built-in image generation; identity master plus identity-preserving derivations | passed |
| `Леда` | `frost` | `main`, `hopeful`, `wrathful` | one coherent three-state identity sheet | passed |
| `Осип` | `winter` | `main`, `protective`, `remorseful` | one coherent three-state identity sheet | passed |
| `Аграфена` | `keeper` | `main`, `commanding`, `compassionate` | one coherent three-state identity sheet | passed |
| `Савва` | `courier` | `main`, `afraid`, `relieved` | one coherent three-state identity sheet | passed |
| `Хранитель` | `reeds` | `main`, `wrathful`, `merciful` | one coherent three-state identity sheet | passed |

Runtime form: `Characters/<selector>/view/whole/<outfit>/<variant>.png`. Contact sheets are the approved generation masters; separated files are deterministic exports of those sheets. No modular body parts.

Protagonist address correction (2026-09-07): the SDK uses `maincharacter`, not the display name, at runtime. The story definition aliases its three `winter` whole-variant addresses to the existing `мира` addresses. Production PNGs and their GUIDs are unchanged. The table's passed gates concern originality; they do not certify cutout quality or complete visual acceptance.

## Locations and story illustrations

`glass-mere-dusk`, `village-square`, `ferry-house`, `ribbon-grove`, `bell-tower`, `under-ice-hall`, `covenant-storm`, `thaw-sunrise`, `sealed-dawn`. All are 16:9 painterly environments with character-safe composition and a scene-specific dramatic job.

## Choice and presentation art

### Episode catalog cover — requested 2026-09-08

| Episode / purpose | Subject and composition | Address / target | Status |
|---|---|---|---|
| `s01e01`; independent catalog illustration introducing the memory mystery without an ending spoiler | Portrait view through clear cobalt lake ice into the drowned apple orchard; amber memory fruit, restrained silver frost, dark lower area reserved for UI; no people, letters, logos or new story props | `Assets/nevesta-izo-lda.asset`: episode `s01e01` → `_catalogCover: s01e01.png` → `Config/EpisodeCovers/s01e01.png` | source-art review passed; file saved and source binding written; catalog/Player validation deferred |

The source scene is `frozen_orchard`. This is catalog artwork, not an extra
story background or a copy of `Config/cover.png`. The current-main contract
allows PNG/JPEG outside `Assets`, without Sprite metadata or bundle labels.
Full catalog preview export and runtime display remain deferred until the
registered worktree can receive the current SDK and final validation is authorized.

PNG: 1024×1536, RGB, SHA-256
`591958954c28fdfcebb3bb3271fc27e2eb0502ac8af431ba45bae05af0a9540c`.
Built-in image generation used the existing `frozen-orchard.png` solely as a
world/style reference; no external comparison image was used in generation.
The full candidate was visually inspected: submerged upright branches match
the episode text, no faces/type/logos are present, and the bottom is subdued.
Actual card cropping and text contrast remain unverified. New-cover originality
iteration 1 is recorded in `ORIGINALITY_EVIDENCE.md`; it does not renew the
historical gates for unchanged assets.

- Choice icons: `rescue-hand`, `untie-ribbon`, `bell-shard`, `diverging-paths`, `shared-rowan`.
- Cover: unique portrait illustration, no embedded type.
- Story-local Bubble: inherited stable prefab hierarchy, recolored/generated frost-and-rowan panel surfaces, accessible text retained. Runtime Player observation is deferred.

## Audio

- `winter-breath.wav`: seamless restrained lake-wind bed.
- `covenant-bed.wav`: unresolved subglacial low-mid ambience.
- `hearth-under-ice.wav`: restrained warm/cold motif for the ferry house and House of Letters.
- `thaw-theme.wav`: restrained warm harmonic resolution.
- `ice-whisper.wav`, `ice-crack.wav`, `ribbon-snap.wav`, `distant-bell.wav`, `covenant-bell.wav`, `empty-bell.wav`: short original synthesized/processed SFX.

### Audio delivery update — 2026-09-11

The under-ranged 2026-09-07 masters are preserved byte-for-byte under `archive/assets/audio-drafts-2026-09-11/`. All ten production files were re-rendered through floating-point filtering with shaped noise/dither rather than gain-only normalization, then exported as stereo PCM 16-bit/48 kHz. Measured peaks now range from -19.4 to -9.5 dBFS for beds/music and from -17.6 to -12.0 dBFS for short effects. Unity import, scene-relative balance and device listening remain final-validation work rather than a pre-Unity source blocker.

## Visual originality gate

- Iteration 1 reviewed complete current package for environment composition, silhouettes, palette, props, costume structures, iconography and character configurations.
- Generic winter-folklore materials were excluded as non-findings. The distinctive system is cobalt lake memory-bubbles + hydrology instruments + asymmetric covenant ribbons + cracked bell accounting + frost-rowan accents.
- No franchise-specific costume, named artist imitation, logo, or copied composition was requested or observed. Leda was explicitly constrained away from a familiar crowned/veiled ice-princess silhouette.
- Built-in generation outputs are newly generated for this story. Descriptive/reverse-image search was unavailable, recorded as a limitation rather than absolute uniqueness proof.
- Iteration 2 additionally reviewed the suspended-letter archive, inverted drowned orchard with amber fruit, and copper speaking-tube sluice against the existing cast and location set. No specific copied composition, protected character, logo, or text was identified.
- Final risk `low`, confidence `medium`; provenance original AI-assisted production; result `passed`.

## Character evidence update — 2026-09-11

`Art/ContactSheets/mira.png` supplies the missing three-state Mira sheet. `Art/AlphaProofs/` supplies white/black composites for the principal variant of all six characters. The proofs were generated from the exact production PNG alpha channels and visually inspected; silhouettes remain separated on both backgrounds without rectangular matte leakage. Runtime compression, scale and framing remain Unity-stage gates.

## Deferred visual gates

Source alpha evidence is now complete. Runtime selector resolution, narrow portrait Bubble states, platform compression edges, Player screenshots and in-game fallback absence remain part of the separately authorized final slot.
