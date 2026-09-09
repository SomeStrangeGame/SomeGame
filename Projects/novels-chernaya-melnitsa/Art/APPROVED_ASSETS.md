# Approved production manifest

Status: production target, auto-approved
Style: painterly cinematic folk horror, carved-wood geometry, soot black,
moonlit indigo and muted rye-gold; no franchise iconography, gore or embedded text.

## Backgrounds and illustrations

| Runtime selector | File | Purpose |
|---|---|---|
| bg01-bus-stop-dusk | `Assets/Locations/bg01-bus-stop-dusk.png` | return to Chernotalye |
| bg02-house-kitchen-night | `Assets/Locations/bg02-house-kitchen-night.png` | family absence and first flour trace |
| bg03-forest-path-moon | `Assets/Locations/bg03-forest-path-moon.png` | trust branch |
| bg04-mill-yard | `Assets/Locations/bg04-mill-yard.png` | exterior reveal |
| bg05-grinding-room | `Assets/Locations/bg05-grinding-room.png` | promise mechanism |
| bg06-sack-loft | `Assets/Locations/bg06-sack-loft.png` | named sacks and consequences |
| bg07-wind-gallery | `Assets/Locations/bg07-wind-gallery.png` | structural climax |
| bg08-river-crossing-dawn | `Assets/Locations/bg08-river-crossing-dawn.png` | moral choice at dawn |
| bg09-village-archive-night | `Assets/Locations/bg09-village-archive-night.png` | documentary trail and trapped-village map |
| bg10-flooded-ford-night | `Assets/Locations/bg10-flooded-ford-night.png` | Mitya's river crossing evidence |
| bg11-mill-undercroft | `Assets/Locations/bg11-mill-undercroft.png` | hidden release letters and Saveliy's intervention |
| ill01-mitya-found | `Assets/Locations/ill01-mitya-found.png` | corrected later boat-motor workshop, two cups/window pinwheel in central portrait region; source checked, fresh runtime overlays pending |
| ill02-black-flour-hand | `Assets/Locations/ill02-black-flour-hand.png` | corrected upper-frame Lada palm with black flour, canon cuff, wet cloth/book; source checked, fresh runtime pending |
| cover | `Config/cover.png` | catalog cover |
| episode `s01e01` cover | `Config/EpisodeCovers/s01e01.png` | hidden correspondence beneath the mill; episode-specific mystery, distinct from the exterior story cover |

## Episode catalog artwork — 2026-09-08

The author requested a separate image for every episode. The current definition
contains one episode, `s01e01` (Кому дано слово). Its cover reuses the approved
`bg11-mill-undercroft.png` pixels without crop, repaint or regeneration: concealed
letters are central to this episode, and the image does not reveal an ending.
The existing auto-approval mode covers this reversible assignment. Source-image
review confirmed the indigo/rye-gold palette, legible chest/letters and absence
of people or embedded text. The catalog crop/readability check remains deferred.
The `_catalogCover` value is a plain file name, not a Sprite/bundle address.

## Characters

Whole-image runtime representation. Each listed variant is used by Ink.

- `maincharacter/coat` (Лада): main, wary, guilt, resolve.
- `яков/work`: main, guarded, alarm, honest.
- `настасья/wool`: main, requested explicitly.
- `савелий/raincoat`: main, requested explicitly.
- `митя/jacket`: main, requested explicitly in the present-day epilogue only.

Identity masters, scene-derived variants, contact sheets and alpha proofs are
stored under `Art/Characters/<selector>/`. Runtime PNGs are under
`Assets/Characters/<selector>/view/whole/<outfit>/`.
Lada's art evidence remains under `Art/Characters/лада`; the runtime directory
is `maincharacter`, as required by `CharacterSpriteResolver` for the protagonist.

## Audio

- `mill-pressure-loop.wav`: 72 s seamless restrained tonal bed.
- `forest-wind-loop.wav`: 58 s seamless exterior bed.
- `dawn-release-loop.wav`: 48 s restrained release bed.
- `sail-thump.wav`, `wood-catch.wav`, `paper-unfold.wav`, `flour-whisper.wav`,
  `stone-stop.wav`: sparse story events.

## Story-local presentation

- `Assets/Presentation/bubble/screen-variant.prefab`
- `Assets/Presentation/Fonts/liberationsans-regular.ttf`
- `Assets/Presentation/bubble/sprites/dialogue-panel.png`
- `Assets/Presentation/bubble/sprites/choice-card.png`
- `Assets/Presentation/bubble/sprites/promise-knot.png`

The panel uses a translucent soot surface with a quiet central area, rye-gold
edge marks and preserved text labels. Strongest state is the choice prompt in
the grinding room; no flashing motion is required.
