# «Первый снег» — frozen production manifest

Status: `editorial-revision-v5`; approved source art retained, runtime addressing corrected.

## Characters

| Scenes | Selector | Outfit | Variant | Logical address | Approved source |
| --- | --- | --- | --- | --- | --- |
| 1–7,12 | `лёша` | `school` | `main` | `Characters/лёша/view/whole/school/main.png` | `characters/lesha/identity-master.png` |
| 8–10 | `лёша` | `festival` | `main` | `Characters/лёша/view/whole/festival/main.png` | `characters/lesha/festival-alpha.png` |
| 11–12 | `лёша` | `winter` | `main` | `Characters/лёша/view/whole/winter/main.png` | `characters/lesha/winter-alpha.png` |
| 2–7,12 | `мия` | `school` | `main` | `Characters/мия/view/whole/school/main.png` | `characters/miya/identity-master.png` |
| 8–9 | `мия` | `festival` | `main` | `Characters/мия/view/whole/festival/main.png` | `characters/miya/festival-alpha.png` |
| 10–12 | `мия` | `winter` | `main` | `Characters/мия/view/whole/winter/main.png` | `characters/miya/winter-alpha.png` |
| 1,4,5,6 | `соня` | `school` | `main` | `Characters/соня/view/whole/school/main.png` | `characters/sonya/identity-master-v4.png` |
| 8,12 | `соня` | `festival` | `main` | `Characters/соня/view/whole/festival/main.png` | `characters/sonya/festival-alpha.png` |

Whole-image representation is intentional. Dialogue and staging carry fine
emotion changes; the runtime must not synthesize an unused Cartesian emotion
matrix. Each authored outfit has a neutral `main` fallback.

All three speakers now have `school` initial clothes in the definition, required
by the current whole-variant loader before it processes explicit selectors.
Because Лёша is the declared protagonist, his requested runtime addresses use
`story/character/characters/maincharacter/view/whole/<outfit>/main.png`.
Three definition aliases map them to the corresponding physical Лёша PNGs in
the table. Miya and Sonya use their own names. The checker exercises defaults,
aliases and inherited outfits on all 704 routes; Unity import is deferred.

## Backgrounds

| Scenes | Selector/address | Source | State |
| --- | --- | --- | --- |
| 1,6,10,12 | `Locations/darkroom.png` | `art/backgrounds/darkroom.png` | approved |
| 2,8 | `Locations/assembly-hall-prep.png` | `art/backgrounds/assembly-hall-prep.png` | approved; lighting overlay may change in runtime |
| 3 | `Locations/bus-stop-rain.png` | `art/backgrounds/bus-stop-rain.png` | approved |
| 11 | `Locations/bus-stop-snow-night.png` | `art/backgrounds/bus-stop-snow-night.png` | approved edit from rain master |
| 12 | `Locations/bus-stop-winter-morning.png` | `art/backgrounds/bus-stop-winter-morning.png` | approved edit from rain master |
| 4 | `Locations/school-rooftop.png` | `art/backgrounds/school-rooftop.png` | approved |
| 4,5 | `Locations/radio-booth.png` | `art/backgrounds/radio-booth.png` | approved |
| 7,12 | `Locations/river-screen.png` | `art/backgrounds/river-screen.png` | approved |
| 9 | `Locations/school-courtyard-snow.png` | `art/backgrounds/school-courtyard-snow.png` | approved |

All backgrounds are 1672×941 PNG, landscape, with lower UI clearance. The
three stop images share geometry by edit provenance.

## Inserts, presentation and cover

| Scenes | Logical address | Requirement | Status |
| --- | --- | --- | --- |
| 1,9 | `Presentation/inserts/camera-counter.png` | small camera counter close-up; no readable branding | approved |
| 4 | `Presentation/inserts/soft-portrait.png` | deliberately soft portrait, jointly acknowledged later | approved |
| 6 | `Presentation/inserts/contact-sheet.png` | abstract reflections; no unauthorized identifiable close-up | approved |
| 12 | `Presentation/inserts/final-print.png` | reserved illustration; no print appears in scene 10 | source retained; revised-scene visual fit pending |
| 11,12 | `Presentation/inserts/paper-star.png` | close insert, lettering rendered as runtime text | approved |
| all | shared Bubble fallback | standard readable presentation for 12+ story | approved for v1 |
| choices | shared choice fallback | standard readable presentation for 12+ story | approved for v1 |
| catalog | `Config/cover.png` | first snow, paper stars, two figures implied without copying sprites | approved |
| episode s01e01 catalog | `Config/EpisodeCovers/s01e01.png` | scene 9: paper star under the walkway, first snow and warm school windows; no branch outcome | author-approved; source binding prepared for current main SDK |

Episode cover: PNG 1024×1536, opaque portrait 2:3, no typography. Definition
`s01e01._catalogCover` points to `s01e01.png`; the original story cover is retained.
See `episode-cover-handoff.md` for provenance, originality review and checks.
The story branch now contains the current main episode-cover contract. The
source binding and PNG pass static checks; catalog export, crop and display stay
deferred to the separately authorized Unity acceptance slot.

The five files under `Presentation/inserts` are retained source artwork. The
current Ink/prefab set does not display them; their table scene numbers express
intended use, not an implemented runtime feature. In particular, the final-print
image has not passed fit review for the revised photograph. Shared Bubble and
choice fallbacks remain the implemented v1 presentation.

## Audio

No story-local audio is required for v1. Rain, festival activity and the power
cut are conveyed by narration and available runtime ambience only when already
provided by the shared application. An original three-note motif and dedicated
environment loops are explicitly optional post-v1 enhancements; their absence
must not cause missing-asset errors or block the story.

## Story-owned website preview

`Config/Preview/preview.json` uses schema 1 and a faithful linear excerpt from
the canonical opening before its first meaningful choice. It references only
`characters/lesha.png` and `characters/sonya.png`, copied byte-for-byte from
their approved school masters. Static source-order, speaker, path, PNG and
unreferenced-file checks are required. Browser layout and deployment are not in
scope before Unity acceptance and a separate release request.

## Geometry and import

- Characters: 1024×1536 PNG with true alpha; whole-image runtime variants.
- Backgrounds: current generated 1672×941 sources; preserve aspect and use the
  template’s canonical import pipeline rather than per-file overrides.
- UI sprites: true transparent exterior, hidden RGB matte matching the edge,
  no painted checkerboard; nine-slice only after actual-size proof.
- No manual AssetBundle labels. All existing project addresses are retained.
- V3 changes text, definition defaults/aliases and checks; no PNGs, prefabs or import settings were
  edited. Existing stills do not prove every narrated prop action or outfit
  transition. Those remain part of the final visual review.
