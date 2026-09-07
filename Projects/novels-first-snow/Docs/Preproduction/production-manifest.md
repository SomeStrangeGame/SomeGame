# «Первый снег» — frozen production manifest

Status: `frozen-preproduction-v1`

## Characters

| Scenes | Selector | Outfit | Variant | Logical address | Approved source |
| --- | --- | --- | --- | --- | --- |
| 1–7 | `лёша` | `school` | `main` | `Characters/лёша/view/whole/school/main.png` | `characters/lesha/identity-master.png` |
| 8–10 | `лёша` | `festival` | `main` | `Characters/лёша/view/whole/festival/main.png` | `characters/lesha/festival-alpha.png` |
| 11–12 | `лёша` | `winter` | `main` | `Characters/лёша/view/whole/winter/main.png` | `characters/lesha/winter-alpha.png` |
| 2–7 | `мия` | `school` | `main` | `Characters/мия/view/whole/school/main.png` | `characters/miya/identity-master.png` |
| 8–9 | `мия` | `festival` | `main` | `Characters/мия/view/whole/festival/main.png` | `characters/miya/festival-alpha.png` |
| 10–12 | `мия` | `winter` | `main` | `Characters/мия/view/whole/winter/main.png` | `characters/miya/winter-alpha.png` |
| 1,5,8 | `соня` | `school` | `main` | `Characters/соня/view/whole/school/main.png` | `characters/sonya/identity-master-v4.png` |
| 8,12 | `соня` | `festival` | `main` | `Characters/соня/view/whole/festival/main.png` | `characters/sonya/festival-alpha.png` |

Whole-image representation is intentional. Dialogue and staging carry fine
emotion changes; the runtime must not synthesize an unused Cartesian emotion
matrix. Each authored outfit has a neutral `main` fallback.

## Backgrounds

| Scenes | Selector/address | Source | State |
| --- | --- | --- | --- |
| 1,6,10 | `Locations/darkroom.png` | `art/backgrounds/darkroom.png` | approved |
| 2,8 | `Locations/assembly-hall-prep.png` | `art/backgrounds/assembly-hall-prep.png` | approved; lighting overlay may change in runtime |
| 3 | `Locations/bus-stop-rain.png` | `art/backgrounds/bus-stop-rain.png` | approved |
| 11 | `Locations/bus-stop-snow-night.png` | `art/backgrounds/bus-stop-snow-night.png` | approved edit from rain master |
| 12 | `Locations/bus-stop-winter-morning.png` | `art/backgrounds/bus-stop-winter-morning.png` | approved edit from rain master |
| 4 | `Locations/school-rooftop.png` | `art/backgrounds/school-rooftop.png` | approved |
| 5 | `Locations/radio-booth.png` | `art/backgrounds/radio-booth.png` | approved |
| 7 | `Locations/river-screen.png` | `art/backgrounds/river-screen.png` | approved |
| 9 | `Locations/school-courtyard-snow.png` | `art/backgrounds/school-courtyard-snow.png` | approved |

All backgrounds are 1672×941 PNG, landscape, with lower UI clearance. The
three stop images share geometry by edit provenance.

## Inserts, presentation and cover

| Scenes | Logical address | Requirement | Status |
| --- | --- | --- | --- |
| 1,9 | `Presentation/inserts/camera-counter.png` | small camera counter close-up; no readable branding | approved |
| 4 | `Presentation/inserts/soft-portrait.png` | deliberately soft portrait, jointly acknowledged later | approved |
| 6 | `Presentation/inserts/contact-sheet.png` | abstract reflections; no unauthorized identifiable close-up | approved |
| 10 | `Presentation/inserts/final-print.png` | one branch-specific imperfect print | approved |
| 11 | `Presentation/inserts/paper-star.png` | close insert, lettering rendered as runtime text | approved |
| all | shared Bubble fallback | standard readable presentation for 12+ story | approved for v1 |
| choices | shared choice fallback | standard readable presentation for 12+ story | approved for v1 |
| catalog | `Config/cover.png` | first snow, paper stars, two figures implied without copying sprites | approved |

## Audio

No story-local audio is required for v1. Rain, festival activity and the power
cut are conveyed by narration and available runtime ambience only when already
provided by the shared application. An original three-note motif and dedicated
environment loops are explicitly optional post-v1 enhancements; their absence
must not cause missing-asset errors or block the story.

## Geometry and import

- Characters: 1024×1536 PNG with true alpha; whole-image runtime variants.
- Backgrounds: current generated 1672×941 sources; preserve aspect and use the
  template’s canonical import pipeline rather than per-file overrides.
- UI sprites: true transparent exterior, hidden RGB matte matching the edge,
  no painted checkerboard; nine-slice only after actual-size proof.
- No manual AssetBundle labels. Exact Unity target files are assigned only after
  the registered story project exists.
