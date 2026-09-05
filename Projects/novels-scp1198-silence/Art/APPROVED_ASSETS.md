# Approved art manifest

## Story identity

- Story: `scp1198-silence` — «Тише, Нина».
- Audience and tone: psychological containment horror for age 16+; oppressive clinical realism without gore.
- Shared visual grammar: cold cyan facility light, restrained emergency red, documentary composition, realistic painterly treatment, no text, logos, insignia or imitation of named artists and studios.

## Approved assets

| Runtime role | Project asset | Identity / composition lock | Technical acceptance |
| --- | --- | --- | --- |
| Main character | `Assets/Characters/maincharacter/view/whole/facility/main.png` | Nina Valeeva, 34, acoustic engineer; short dark hair, exhausted attentive expression, pale patient clothes under a dark facility jacket | 1024×1536 RGBA, transparent background, complete readable silhouette |
| Main emotions | `Assets/Characters/maincharacter/view/whole/facility/{alarmed,suspicious,overwhelmed,determined,exhausted}.png` | Identity-preserving whole-image variants derived from Nina's approved master; screen-right facing and fixed pose | Five 1024×1536 RGBA variants; scene-wired in Ink |
| Nina patient wardrobe | `Assets/Characters/maincharacter/view/whole/patient/{main,alarmed,exhausted}.png` | Pale gray-blue wrap gown, matching trousers, slip-on shoes and patient wristband; identity and screen-right facing preserved | Three scene-derived 1024×1536 RGBA whole variants used by the archived first-cycle recording and protocol ending |
| Supporting character | `Assets/Characters/кирилл/view/whole/orderly/main.png` | Kirill Savin, 42, night orderly; close-cropped hair, tired face, practical grey scrubs and utility jacket | 1024×1536 RGBA, transparent background, complete readable silhouette |
| Supporting emotions | `Assets/Characters/кирилл/view/whole/orderly/{concerned,wary,urgent,resolute}.png` | Identity-preserving whole-image variants derived from Kirill's approved master; screen-left facing and fixed pose | Four 1024×1536 RGBA variants; scene-wired in Ink |
| Kirill protective wardrobe | `Assets/Characters/кирилл/view/whole/protective/{main,concerned,resolute,urgent}.png` | Off-white sealed response coverall with charcoal reinforcements, gloves, belt and boots; hood down; identity and screen-left facing preserved | Four scene-derived 1024×1536 RGBA whole variants used only after Kirill crosses the exposed-sector barrier |
| Background | `Assets/Locations/acoustic-lab.png` | Empty acoustic laboratory, observation glass, waveform monitors and isolated chair | 1672×941 RGB, wide gameplay composition |
| Background | `Assets/Locations/medical-corridor.png` | Sterile night corridor with glass isolation partition and distant emergency light | 1672×941 RGB, wide gameplay composition |
| Background | `Assets/Locations/secure-archive.png` | Compact secure records room, racks, desk terminal and evidence staging space | 1672×941 RGB, wide gameplay composition |
| Background | `Assets/Locations/final-junction.png` | Facility junction between server access and anechoic chamber, red alarm state | 1672×941 RGB, wide gameplay composition |
| Cover | `Config/cover.png` | Nina isolated before a dark acoustic chamber, concentric waveform motif and red warning light | 1024×1536 RGB, title-free safe crop |
| Dialogue UI | `Assets/Presentation/bubble/` | Original matte containment-record interface with clipped corners, asymmetric status rails, desaturated sage keylines and amber registration marks; no comic tails, logos or copied SCP game/wiki chrome | Story-local prefab, four visible RGBA sprites with nine-slice-friendly geometry, hidden legacy pointer slots, and bundled Cyrillic-capable font |

## Provenance

All approved raster assets were generated specifically for this story on 2026-09-04 with OpenAI's built-in image generation tool. No external image was supplied as a reference. Prompts described original people, locations and compositions and excluded text, logos, branded characters and named-artist or named-studio imitation.

An attempted early distressed Nina variant and two later pilot edits were rejected before import because their checkerboard backgrounds were painted into opaque pixels. A determined pilot with changed global lighting was also rejected. None is part of the project or runtime manifest.

The approved 2026-09-05 emotion package was produced through identity-preserving edits of the two masters. Because the generator did not reliably return true transparency, accepted edits were generated on uniform `#00FF00`, then received a deterministic chroma alpha mask and green despill. No head, body, clothing or hair layers were composited from unrelated images. Contact and alpha-proof sheets are `Art/nina-emotions-dark.png`, `Art/nina-emotions-faces-light.png`, `Art/kirill-emotions-dark.png` and `Art/kirill-emotions-light.png`.

The 2026-09-05 wardrobe package used the same identity-preserving whole-image process. Each outfit received one neutral reference before its used emotional states were derived. Proof sheets are `Art/nina-patient-wardrobe-{dark,light}.png` and `Art/kirill-protective-wardrobe-{dark,light}.png`. One protective `wary` draft was rejected and removed because Kirill's gaze crossed toward screen-right; it is not a runtime asset.

The initial 2026-09-05 Bubble treatment was rejected after visual review because
its rounded panel, floating name capsule and speech pointers remained too close
to the existing TZM presentation. The replacement visible sprites were created
from blank prompts without using TZM art as an image reference. They use clipped
record-card silhouettes, integrated side rails and flat evidence tabs; all
speech-pointer renderers are suppressed. Output dimensions and alpha were
normalized deterministically for the existing sliced-sprite import contracts.
No external UI, SCP logo, wiki skin, game HUD, text or franchise-specific
insignia was used.

The files listed above are the approved identity and composition masters. Regeneration must retain these locks unless this manifest is deliberately revised and revalidated.
