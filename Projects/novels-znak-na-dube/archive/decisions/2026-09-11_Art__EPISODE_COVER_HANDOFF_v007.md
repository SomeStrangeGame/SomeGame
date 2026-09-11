# Episode cover handoff — s01e01

Date: 2026-09-08. Status: **ready-for-final-validation**, not runtime-validated.
User explicitly requested a separate episode image, then rejected the oak composition as too similar to the story cover. The bell redesign below is selected under the existing auto-approve brief; explicit user acceptance of this new version is not claimed.

## Deliverable and mapping

- Scene: `bell_tower`, the silent bronze bell in Tikhon's freestanding wooden belfry, without ending spoilers or character redesign.
- Final: `Config/EpisodeCovers/s01e01.png`, 1024×1536 (2:3), 8-bit opaque RGB PNG, 2,677,764 bytes.
- SHA-256: `2655ffe2417be2182d49ef3cbd53bce40330a959eeb650bb6016ed14e913e78f`.
- `Assets/znak-na-dube.asset`: episode `_id: s01e01`, `_catalogCover: s01e01.png`.
- This is a plain catalog image, not a Unity Sprite. No `.meta`, bundle labels, manual preview JSON or release output was created. The builder owns export to platform `episode-covers/`.
- Existing `Config/cover.png`, Ink, character assets and in-story backgrounds are unchanged by this task. The story cover remains the fallback.

## Production and visual review

Generated with the built-in image-generation tool; no CLI/API-key fallback. Draft 1 had a vertical eye hollow. A targeted second pass used the existing `Assets/Locations/oak-mark-close.png` only as a symbol reference and restored the horizontal empty-eye / forked-root geometry. Both oak versions are now superseded: the user correctly identified insufficient distinction from the general cover. The earlier claim that removing characters made the composition sufficiently different is withdrawn.

Production generation 3 is a fresh low-angle composition using only the project-owned `Assets/Locations/bell-tower.png` as environment/identity reference. Reviewed the complete output: broad patinated bronze bell dominates the upper-middle frame; blackened posts and diagonal braces replace the oak silhouette, cold forest occupies the gaps, wet steps and roots remain below. No oak-eye icon, central moon, sunset, characters, church, visible lettering or logo. Cut rope ends are visible, but this cropped illustration is not evidence of the exact narrative count of seven. No crop, repaint or format conversion was performed after generation. Runtime thumbnail/crop acceptance is still pending.

Original generated files remain outside Git:

- Rejected draft: `/Users/iantonishin/.codex/generated_images/01a07c22-8e29-7343-8148-1241fc75279e/exec-b46513cf-d5cf-4d1d-86b2-ce6d8302ce3a.png`.
- Superseded oak master (recoverable): `/Users/iantonishin/.codex/generated_images/01a07c22-8e29-7343-8148-1241fc75279e/exec-d5ab7a77-2dab-40e0-99be-2b124b6f85fd.png`.
- Selected bell master: `/Users/iantonishin/.codex/generated_images/01a07c22-8e29-7343-8148-1241fc75279e/exec-7fa7edc7-a67c-4e70-9f84-3ac103672ae6.png`.

## Bounded visual-originality review — episode cover, iteration 1

Historical review of the superseded oak candidate only; it does not cover the bell redesign.

Scope: the entire finished episode-cover image and its relationship to the previously approved story art; not a new certification of unchanged character/background assets or story text.

Search: `fantasy tree bark hollow eye roots book cover art`. Directly read [The Hollow Tree publisher page](https://www.penguinrandomhouse.com/books/562269/the-hollow-tree-by-james-brogden/) and [Taken Root designer listing](https://thebookcoverdesigner.com/premade-book-covers/taken-root/). They establish generic hollow-tree and magical-root/tree-cover territory, not exclusive ownership of these motifs. The retained combination is the story's already authored empty-eye root sign, close bark framing, distant amber timber village and cool pine edge. No third-party image was supplied to generation; the only editing reference was project-owned generated art.

Result: **passed for this bounded descriptive/visual review**, risk **low**, confidence **medium**. No substantial distinctive collision was identified in the available comparison. Limitations: reverse-image search was unavailable; the publisher's cover image request failed, and no full external-image visual comparison is claimed. This result rests on direct inspection of the generated image, project-art continuity, documented generation inputs and limited descriptive source comparison, not proof of absolute uniqueness.

## Bounded visual-originality review — episode cover, iteration 2

Scope: the whole new bell cover, including composition, props, environment and palette. Reviewed against the project-owned bell background and general oak/character cover. The new cover retains the story's rustic belfry identity while replacing the repeated oak composition with a close bronze bell and angular structural frame.

Searches: `dark fantasy old wooden belfry bronze bell forest artwork` and image search `wooden belfry bell dark fantasy artwork`. Directly read Børge Bredenbekk's [Charcoal Drawn Children Book](https://www.behance.net/gallery/87783779/Charcoal-Drawn-Children-Book) and visually inspected its [full belfry illustration](https://mir-s3-cdn-cf.behance.net/project_modules/1400_webp/dd9e4887783779.5dc2afc40cea0.jpg) in the browser. Shared elements: low viewpoint, bell architecture and bare branches. Material differences: that work uses a tall enclosed plank tower, slender pointed spire, small tilted bell, bright monochrome charcoal sky and line-drawn vegetation; our image uses an open heavy timber frame, close broad stationary bell, patinated color, cropped canopy and dark rooted steps. These are functional/genre motifs, with no substantial distinctive collision found in this comparison. The external image was viewed only after generation and was not supplied as a generation input.

Other search leads at Deep Dream Generator and Chatbotsplace failed direct retrieval; ArtStation returned only its login shell. Those snippets were not treated as evidence. The web image fetch also failed for the Behance image, but browser display succeeded and supplied the actual visual comparison. Reverse-image search was unavailable, and the external comparison corpus is narrow.

Result: **passed**, risk **low**, confidence **medium** for this bounded full-candidate review. This is not a guarantee of absolute uniqueness. No further originality iteration was needed after the comparison; the historical iteration 1 result remains superseded.

## Validation and pending gates

- `sips` identifies a 1024×1536 PNG with no alpha; final/master byte comparison and SHA-256 match.
- `Art/check_story.py` checks the exact episode ID/file mapping, allowed filename, existing PNG header/dimensions/RGB format, and existing story routes/resources.
- Unity import, official content build/preview export, actual catalog crop/legibility and fallback behavior remain deferred to the separately authorized final slot. Desktop full-image inspection is not a phone/runtime acceptance test.

The selected 1024×1536 bell master was inspected again in full on 2026-09-11:
the bell and angular belfry dominate, the lower steps remain quiet, and no oak
sigil, character, church mark, lettering, logo or watermark is visible. The
frayed ropes are atmosphere rather than proof of an exact narrative count.

## Bell redesign prompt — production generation 3, built-in mode

Use case: illustration-story. Create a completely new production episode cover for an original fictional dark-fantasy visual novel, portrait 1024x1536 opaque RGB PNG. Reference image is ONLY the project's existing bell-tower location: preserve its rustic four-post freestanding wooden belfry and aged bronze bell identity, but invent a fresh close composition. Main subject: a massive weathered bronze bell seen close from a dramatic low three-quarter angle beneath its blackened timber beam; bell occupies upper-middle half of frame, with a broad flared rim and dark hollow underside, subtle oxidized green patina, plain casting ridges, no writing. Several frayed cut rope ends hang beside the bronze crown. Strong diagonal timber braces and two prominent near posts frame the bell; other supports recede naturally. A section of simple shingled canopy can appear at top. Background is cold blue-gray mist and distant slender forest silhouettes, not an enormous central tree. Wet tangled roots and worn stone steps in shadow occupy the quiet lower quarter. Cinematic painterly realism, tactile old bronze and splintered wood, restrained charcoal and blue-gray palette with soft muted golden bronze reflected light on bell, cool moonlight from outside frame. Ominous stillness, readable silhouette even as a tiny phone thumbnail. This must look radically different from a poster of a massive centered oak: the rounded bronze bell and angular beams are the dominant shapes, no oak trunk, no glowing eye, no root sigil, no sunset, no central moon disk. No people, faces, hands, church, cross, inscriptions, runes, lettering, typography, borders, logos, watermarks, gore or modern objects. Standalone finished illustration, not collage, not mockup. Keep focal bell within central 65 percent width for catalog crop; lower region calm for external title UI.

## Original generation prompt — superseded

Use case: illustration-story. Asset: production catalog cover for episode s01e01, 'Знак на дубе', a wholly fictional dark-fantasy visual novel. Create one finished portrait 2:3 illustration, target 1024 by 1536 pixels, opaque background. Depict the opening encounter, NOT a later ending: at dusk a massive rough oak trunk stands at the edge of a dark encroaching pine forest near a small timber village. Main focal point is a single small pale bone-white root mark on the bark: two thin branching root lines fork upward around an EMPTY BLACK EYE-SHAPED hollow, no pupil, no realistic eyeball. It looks organically grown into the bark, not painted heraldry. Center this crisp luminous mark in the upper-middle of the frame, large enough to read in a phone thumbnail; bark texture and the base roots lead the viewer to it. Slender dark pines and faint mist frame the trunk; a few distant village windows glow restrained amber through a gap. Earth and moss, no flooded landscape. Painterly realistic dark-fantasy illustration with subtle oil brush texture, tangible craggy bark, charcoal black, deep pine green, weathered bronze browns, cool blue-gray shadows, restrained bone-white light. Moody but legible, nuanced shadows rather than crushed blacks. Composition distinct from a character poster: no people, no faces, no hands, no silhouettes. Keep essential features within the central 60 percent of width, and bottom quarter quiet and dark for externally rendered title UI; leave natural breathing room at top. No written text, typography, letters, title, numbers, border, watermark, logo, real runes, franchise symbols, weapon, blood or gore. No artist or franchise imitation. The result should be a compelling standalone scene-specific episode cover, not a collage, not a mockup.

## Targeted edit prompt — superseded

Edit image 1 (the portrait episode cover). Image 2 is ONLY a reference for the established story's oak mark. Keep image 1's exact 1024x1536 composition, oak, sunset, village, lighting, forest, roots and quiet lower space unchanged. Change ONLY the pale root symbol on the trunk: its central EMPTY dark eye-shaped hollow must be HORIZONTAL, like image 2, not the current tall vertical slit. Pale organic branching roots outline the horizontal almond-shaped empty eye, with two root strands crossing and forking upward as in image 2. No eyeball, iris, pupil, lettering or new markings. Preserve subtle integrated bone-white bark/root texture, no neon glow. No other changes.
