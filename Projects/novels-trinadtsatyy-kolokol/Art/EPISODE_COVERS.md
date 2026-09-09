# Episode catalog covers

Owner: current Trinadtsatyy Kolokol task, `tk-episode-covers-20260908`.
Status: ready-for-Inspector-assignment; six approved PNGs placed in Config.
Base HEAD: `1120f84256c5b2613392e2631c3a485fd01044ed`.
Scope: this file, `Config/EpisodeCovers/`, and `_catalogCover` fields in
`Assets/trinadtsatyy-kolokol.asset`. No Ink, character, shared SDK, catalog,
generated preview, Git integration or Unity changes belong to this scope.

## Frozen manifest

Requested: six distinct episode illustrations. All six are required by the
author's current request, even though the shared runtime field is optional.
Style: existing painterly neo-noir, blue-black wet surfaces, oxidized copper,
restrained cyan instruments and sparse amber light. Portrait 2:3, target
1024×1536 PNG, opaque. No baked-in titles, logos, people or ending spoilers.
Keep the subject in the central upper/middle area; preserve subdued lower
space for catalog text. These are catalog images, not in-scene backgrounds.

| Episode | Title | Scene-derived subject and purpose | Target / `_catalogCover` | Approval |
| --- | --- | --- | --- | --- |
| s01e01 | Лента номер ноль | Archive reel recorder and cyan trace: impossible recording starts the investigation | `Config/EpisodeCovers/s01e01.png` / `s01e01.png` | approved artwork |
| s01e02 | Кольцо последнего вагона | Empty rain-soaked tram loop: first disappearance's address | `Config/EpisodeCovers/s01e02.png` / `s01e02.png` | approved artwork |
| s01e03 | Город в прямом эфире | Radio studio microphone: public warning versus quiet test | `Config/EpisodeCovers/s01e03.png` / `s01e03.png` | approved artwork |
| s01e04 | Смена, которой не было | Twelve industrial sockets and physical key: access to the forgotten civic network | `Config/EpisodeCovers/s01e04.png` / `s01e04.png` | approved artwork |
| s01e05 | Адрес под дождём | Flooded service tunnel, cables and concealed amplifiers before the collapse: the human mechanism | `Config/EpisodeCovers/s01e05.png` / `s01e05.png` | approved artwork |
| s01e06 | Нулевая комната | Suspended copper membrane inside the circular dispatch room: unanswered mystery before the final choice | `Config/EpisodeCovers/s01e06.png` / `s01e06.png` | approved artwork |

Sources: existing `NARRATIVE_PACKAGE.md`, current source Ink and approved
story/location artwork. The scenes above are independent of the changed final
choice conditions. Narrative/Ink originality was pending when these covers
were produced; the later full review passed separately on 2026-09-08, as recorded
in `ORIGINALITY_EVIDENCE.md`. This art task did not reset or promote those gates.

## Production and checks

Use built-in imagegen, one asset per call, pilot first. Drafts remain at the
generator's external output paths. Only visually reviewed and originality-
screened deliverables enter Config. Bind by stable episode ID after approval.
Check file format/geometry, distinct hashes, six matching IDs and paths,
scoped diff, and unchanged source Ink regression results. Runtime/export and
portrait catalog crop remain deferred until an authorized final Unity slot.

## Visual review and originality — 2026-09-08

Scope: the complete new six-cover deliverable set, not a replacement narrative,
Ink, character or historical location-package pass. Non-character art iteration
3 (the earlier expanded location package completed iteration 2). All six covers
reviewed together after generation; no post-review redesign or hidden retries.

Visual result: six distinct focal subjects, coherent portrait geometry and
materials, no depicted people or branch outcomes. The switchboard retains two
rows of six sockets; the membrane retains its approved suspended concave shape,
not a new portal or a vertical disc. The tram and radio booth remain empty.
Small surface marks are pictorial texture, not readable story text. Lower-frame
detail is not actual UI evidence: final catalog crop/contrast is still pending.
Art approval is the production review under the package's existing auto-approve
mode, not a claim that the user has manually approved a rendered catalog.

Provenance: six separate built-in imagegen calls using only this story's
approved cover/location images and then the first generated cover as style
references. No third-party image was given to the generator or imported.

Descriptive/image search compared radio-studio horror and circular copper
machinery motifs. Direct source pages read:

- [Team17's Killer Frequency listing](https://store.steampowered.com/app/1903620/Killer_Frequency/):
  late-night radio and vintage audio equipment overlap as functional/genre
  elements. The new set instead uses a rain-facing fictional civic archive,
  tram loop, industrial sockets, service collector and suspended membrane;
  it does not copy the game's named radio branding or slasher iconography.
- [Cyan's Riven page](https://cyan.com/games/riven/): circular metal mechanisms
  and atmospheric environments are shared broad motifs. A giant golden dome
  is described there; our source-derived concave membrane inside a municipal
  service room is a different object and composition. This is a limited
  descriptive comparison, not a claim to have inspected every game screenshot.
- Image search surfaced official Team17 studio/wallpaper material and Riven
  mechanism images. Search descriptions informed candidate selection, not
  stand-alone proof. Some direct image/page fetches failed, including the
  Science Museum collection; those are not positive evidence.

Findings: no substantial distinctive-combination match established in this
bounded comparison. Functional microphones, reels, sockets, tunnels and
teal/amber lighting are not findings by themselves. Result: passed for the six
new covers; risk low, confidence medium. Limitations: no reverse-image upload
search, incomplete external image coverage, not a guarantee of uniqueness or
a legal assessment. Existing narrative/Ink owner-review status is unchanged.

## Assignment boundary

### Static validation receipt

All six PNGs are 1024×1536, 8-bit RGB, opaque and non-interlaced. PNG signatures,
every chunk CRC, complete zlib decompression and expected scanline byte counts
passed. All hashes are distinct; filenames match the six existing episode IDs.
Definition and source Ink were not modified by this task. `Art/check_story.py
--self-test` passed: 32 pre-final paths, 86 complete routes, all seven injected
regressions rejected. Scoped `git diff --check` passed. These are source/file
checks, not compiled Ink, export, runtime or release acceptance.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| s01e01.png | 2130270 | `15ac0bcc1a397fb4086bda93e42186d6c63fd0b2d8e12dcb71b595feace264f8` |
| s01e02.png | 2590034 | `4903c4f7510f3760e08c89d575f4229fc8dd1c190162690996e2d0f1b856225f` |
| s01e03.png | 2209088 | `575f875287fc0b62575d4bc363313de8ce5d585bceeb5073f7578f34663b6001` |
| s01e04.png | 2402152 | `94e9e093805f9314b54044a01f403add0d6fff770bf19321838a6447b8367352` |
| s01e05.png | 2294694 | `8cfe3a6c75fa3f17e41548072f3ad6748e5e0fd675863ff8714cb91f031b21ae` |
| s01e06.png | 2580136 | `3af2ea58cc68c5ec46e219f3564dcd0bbe930e2b54d0d9819fd38d5e6282ad49` |

### Remaining integration

The canonical `Docs/AI/guides/ContentAuthoringGuide.md` table requires the
definition `.asset` to be edited through Unity Inspector. No `.asset` fields
were changed in this non-Unity task. The six table entries are planned
assignments, NOT live runtime links. After updating this branch to the current
SDK under the integration protocol and obtaining a final-slot authorization,
assign each episode's Catalog Cover filename in Inspector by its stable ID.
Build the affected story and confirm generated preview/export and catalog
fallback/crop. Do not hand-edit generated `catalog-preview.json`.

These PNG/JPEG catalog files live outside Assets and are not Unity Sprites or
story-bundle dependencies. No `.meta` or compression overrides are needed here.

## Exact generation prompts and output provenance

Built-in mode; one call per episode; no rejected images. The initial manifest
was refined against existing location references before the corresponding
calls (twelve sockets and concave membrane); no generated image was edited.

### s01e01

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-1d53f3a6-68d5-4863-9698-a9912bc2d5a9.png`.
Reference images: `Config/cover.png` and `Assets/Locations/bg01-sound-archive-night.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536, not a collage. Episode 1 'Tape number zero' of an original fictional urban mystery. Image 1 is only the approved visual style/palette reference; image 2 is the approved sound archive environment reference. Scene: municipal sound archive at night, a worn reel-to-reel recorder on a wood desk, one large copper-edged tape reel and cyan oscilloscope trace form the strong focal cluster, shelves of archived reels recede into blue-black shadows, rain on a tall arched window, a small warm desk lamp. Invent a distinct close editorial composition consistent with the reference environment, not a crop or copy. Painterly neo-noir, realistic material textures, oxidized copper, dark teal and blue-black, restrained amber practical light. Readable focal silhouette at thumbnail scale, focal objects within central upper/middle two-thirds; lower quarter subdued for UI overlay. Quiet investigative dread, no monster, no magic symbols. No people, no hands, no lettering, digits, logos, watermark, borders, grid or extra panels. Opaque full bleed. This cover previews the first clue, never a solution or ending.
```

### s01e02

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-3e3308c6-c8da-4477-a4d0-ff1c36c1a137.png`.
Reference images: s01e01 generated master above and `Assets/Locations/bg02-tram-loop-rain.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536, not a collage. Episode 2 'The last carriage loop' of the same original fictional urban mystery. Image 1 is the approved pilot cover style reference; image 2 is the approved tram-loop environment reference. Scene: an empty older municipal tram standing at its rain-soaked terminal loop after midnight, curved wet rails leading toward its amber-lit windows, overhead contact wires disappearing into blue-black rain, deserted fictional city facades. The tram is the large clear mid/upper focal subject; reflection and curving rails form a subdued lower quarter beneath the UI. Painterly neo-noir, realistic weathered surfaces, dark teal, blue-black, oxidized copper and sparse warm amber light; match the pilot's polish and restrained cyan accents, not its objects/composition. Portrait composition designed anew, not an environment crop. No people, monsters, visible driver, ghost, actual landmarks, text, route numbers, logos, watermark, border or panels. Opaque full bleed, legible at thumbnail size. A mysterious first disappearance location, no plot resolution.
```

### s01e03

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-05858dbb-fec2-4a0f-b240-f9d07b4972e7.png`.
Reference images: s01e01 generated master above and `Assets/Locations/bg03-radio-studio.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536. Episode 3 'The city live on air' of an original fictional urban mystery. Image 1 is the approved pilot cover STYLE reference; image 2 is the approved radio studio ENVIRONMENT reference. A substantial vintage broadcast microphone in its mount is the central upper/middle focal subject, with mixing desk faders and headphones nearby, a rain-streaked studio window behind showing distant rooftop antennae, quiet cyan monitor glow and a small amber status lamp. Room is empty, before the choice to broadcast, equipment has no readable writing. Painterly neo-noir, rich blue-black shadows, muted teal and oxidized copper, amber light, realistic materials consistent with reference. New intimate editorial composition, not a crop, no repetition of archive tape recorder from pilot. Leave lower quarter subdued for catalog text. No people, hands, lettering, digits, logos, watermark, monster, magic symbols, border, collage or panels. Opaque full-bleed portrait, strong silhouette readable small, no ending spoiler.
```

### s01e04

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-1cbcdf7d-331b-422a-889f-a3af09c46b42.png`.
Reference images: s01e01 generated master above and `Assets/Locations/bg05-switchboard-room.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536. Episode 4 'The shift that never happened' of an original fictional urban mystery. Image 1 is the approved pilot STYLE reference; image 2 is the approved switchboard room ENVIRONMENT reference. The wall-mounted alert-network switchboard from image 2 fills the central upper/middle scene: preserve its twelve large round industrial sockets in two rows of six, aged metal wall plate and single heavy plugged cable hanging down. Worn oxidized copper rims, peeling teal wall paint and sparse warm maintenance lighting; it is NOT a dense telephone operator console. Frame closer in a new portrait composition. In the nearer middle foreground a single worn brass mechanical key rests on the shallow wooden operator ledge, with a closed faded duty ledger nearby, no legible markings. A silent empty room and dark service doorway behind imply forgotten work rather than supernatural spectacle. Rich painterly neo-noir, blue-black and muted teal, oxidized copper, sparse amber practical light, realistic aged material. New portrait composition not a crop of reference; lower quarter subdued wood/shadow for UI. No people, hands, magic sigils, weapons, readable text, numbers, logos, watermark, borders, collage or panels. Opaque full bleed, strong thumbnail shapes. Neither branch outcome nor ending depicted.
```

### s01e05

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-2d81fb76-301e-472f-98eb-d56c25024d56.png`.
Reference images: s01e01 generated master above and `Assets/Locations/bg06-storm-drain.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536. Episode 5 'An address in the rain' of an original fictional urban mystery. Image 1 is the approved pilot STYLE reference; image 2 is the approved storm-drain ENVIRONMENT reference. Claustrophobic but navigable brick-and-concrete service collector, shallow flowing water, a narrow grated walkway and handrail, heavy old pipes and cables, one small modern amplifier cabinet discreetly mounted between the old pipes with subdued cyan indicator light. A distant amber maintenance light draws the eye along the tunnel, warm copper highlights reflected in blue-black water. Before the grate collapses: intact infrastructure, nobody injured or present, no floating evidence book. Low-angle portrait with convincing tunnel perspective and clear upper/middle focal cabinet/path; lower quarter dark water reflection for UI. Match painterly neo-noir and tactile weathering of the pilot, not its objects. New distinct composition, no crop, no people, hands, bodies, monster, skull, magical runes, text, numbers, logos, watermark, borders or panels. Opaque full bleed, legible small, no branch/ending spoiler.
```

### s01e06

Output master: `/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/exec-21e74895-fc08-4444-9bcb-b3d07f1eb96f.png`.
Reference images: s01e01 generated master above and `Assets/Locations/bg09-zero-room.png`.

```text
Use case: illustration-story. Create a NEW standalone vertical 2:3 episode catalog cover, target 1024x1536. Episode 6 'The zero room' of an original fictional urban mystery. Image 1 is the approved pilot STYLE reference; image 2 is the approved zero-room ENVIRONMENT reference. A circular underground municipal dispatch chamber viewed from its threshold before the final decision. The central focal object is a large round shallow concave canopy-like aged copper membrane suspended from an overhead assembly and held at its rim by tension cables, preserving the distinctive curved umbrella-like form of the approved environment, surrounded by concentric floor service rings and conduits curving with the walls. Subtle cyan instrument light traces the circle; one restrained amber light illuminates the copper surface. The membrane is whole and untouched, equipment neither smashed nor visibly switched off; room is empty. Painterly neo-noir, blue-black and muted teal, oxidized copper and tactile concrete/metal, human-scale engineering, not sci-fi portal or occult altar. Atmospheric depth and clean silhouette in central upper/middle frame, lower quarter subdued floor reflection for UI. New portrait composition, not a crop; no people, ghosts, written names, numbers, logos, magical glyphs, clock face, watermark, borders, panels or collage. Opaque full bleed. Tease the location, do not show or imply which of three endings happened.
```
