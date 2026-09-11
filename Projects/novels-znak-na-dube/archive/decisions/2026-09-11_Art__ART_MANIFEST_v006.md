# Production art and media manifest

Art direction: painterly dark-fantasy illustration, vertical visual-novel characters, charcoal-black forest, oxidized bronze, cold moonlight and restrained amber village light. Recurring motif: a pale forked root enclosing an empty dark eye. No text, logos, watermarks, recognizable franchise motifs, photoreal gore, or decorative asset duplication.

## Characters (whole-image runtime variants)

- Яр: `maincharacter/view/whole/travel/{main,alarmed,marked}.png`: opening, council, final mark; `maincharacter` is the mandatory runtime ID for the protagonist.
- `лада/forager/{main,urgent,wounded}.png`: guide, flooded path, climax.
- `весна/council/{main,stern,afraid}.png`: council, shrine and archive; her later remote voice uses narrator text.
- `тихон/bellkeeper/{main,listening}.png`: bell tower revelation.
- `мирон/memory/{main,sorrow,echo}.png`: memory and root imitation.

Each character has one identity master and only scene-used variants. Character canvas target 1024×1536 with alpha, full body, feet visible, coherent identity.

## Locations and illustrations

- `village-edge.png` — liminal village/forest establishing scene.
- `oak-mark-close.png` — narrative insert of the fresh sign.
- `headwoman-house.png` — timber council room with iron maps.
- `charcoal-boundary.png` — abandoned charcoal boundary yard; warm roots survive beneath cold ash.
- `bell-tower.png` — bronze bell and root-shadow geometry.
- `drowned-shrine.png` — half-submerged path shrine with broken name tablets and a bronze bell.
- `flooded-path.png` — black water and moving roots.
- `root-archive.png` — subterranean archive of hanging name tablets and eight central vacancies.
- `silent-bird-glade.png` — motionless pale birds, memory threshold.
- `miron-memory.png` — story illustration, brother between copied footsteps.
- `village-roots.png` — village foundations shown through Miron's memory.
- `heart-oak.png` — cathedral-scale ancient oak before the choice.
- `roots-awaken.png` — story illustration for the network awakening.
- `forest-eyes-dawn.png` — cliffhanger: distant hills and many marks.
- `Config/cover.png` — separate cover composition, Яр’s marked palm before the Heart Oak.

## Episode catalog cover — requested 2026-09-08

- Episode `s01e01`, `bell_tower`: close low-angle portrait of the silent bronze bell in Tikhon's freestanding timber belfry, with cut ropes, blackened supports, wet roots and cold forest mist. Bronze and angular timber replace the oak/eye silhouette; no characters, ending spoilers, church, lettering or logos. Same restrained charcoal/oxidized-bronze palette as approved art, with a large readable bell and quiet lower area for catalog copy.
- Target: `Config/EpisodeCovers/s01e01.png`, opaque RGB PNG, portrait 2:3, 1024×1536. Mapping: episode `_catalogCover: s01e01.png`; no Sprite reference, AssetBundle label or handwritten preview. Status: bell redesign selected under the auto-approve brief and mapped after the user rejected the oak version as too similar to the story cover. Revised scene/composition was frozen before generation; whole-candidate originality iteration 2 passed with bounded evidence in `EPISODE_COVER_HANDOFF.md`. Existing story cover remains unchanged. Explicit user acceptance and catalog runtime crop/export checks remain pending.

## Choice/presentation/audio inventory

- Choice icons: `touch-mark`, `study-mark`, `iron-nail`, `bronze-clapper`, `save-lada`, `save-map`; each is connected through a `choice_icon:` Ink tag at its corresponding decision.
- Bubble sprites: bark panel, root frame, amber choice, pale sigil indicator; prefab maps these through story-local presentation.
- Audio: `nochelestye-bed.wav` 48s seamless restrained ambient; `mark-pulse.wav`, `root-creak.wav`, `bronze-answer.wav`, `cliffhanger-sting.wav`.

## Visual originality gate

Descriptive comparisons focus on the combined empty-eye root sign, bronze acoustic motif, village iron cartography, black-water root trail, drowned path-name shrine and root-borne debt archive. No direct artist or franchise imitation is permitted. Generated outputs were visually inspected; evidence and limitations are recorded in `ORIGINALITY_EVIDENCE.md`.

## Website excerpt derivatives — 2026-09-10

`Config/Preview/characters/yar.png` is an unchanged copy of `Assets/Characters/maincharacter/view/whole/travel/alarmed.png`; `lada.png` copies `Assets/Characters/лада/view/whole/forager/urgent.png`. These are the two states shown by the canonical opening. Byte identity, exact JSON references and absence of unreferenced preview images are checked by `Art/check_story.py`. No new visual candidate or image manipulation; actual website layout remains untested.

## Desktop production-sheet review — 2026-09-11

Directly inspected the story cover, episode cover, all three location sheets,
the character sheets, the six-icon choice sheet and Bubble kit. The assets keep
one charcoal/bronze/cold-moonlight language and their scene identities remain
distinguishable. No visible title text, logo or watermark was found; the
episode bell reads independently from the oak-led story cover, and the choice
images remain distinguishable without relying on labels. This inspection does
not establish catalog crop, runtime alpha, import settings, safe-area or Player
contrast; those remain deferred.
