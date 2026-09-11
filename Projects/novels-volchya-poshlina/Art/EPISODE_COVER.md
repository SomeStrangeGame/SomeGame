# Episode cover — s01e01

Author request: a distinct episode catalog image, confirmed 2026-09-08.
Episode: `s01e01`, «Долги зимней дороги» (the only episode).
Status: approved, mapped and statically checked; ready for final validation.
Unity catalog crop/overlay validation remains pending and is not claimed here.

## Manifest

- Purpose: identify the medicine-delivery stakes and winter-road atmosphere
  before downloading the story; no ending spoilers.
- Subject: one strapped wooden medicine crate, a damaged wooden bridge over a
  dark winter river, the birch boundary and a weathered tally post beyond it.
- Narrative anchors: `broken_bridge` and `boundary_post`; an illustrative
  composition of their existing props, not an added scene or chosen outcome.
- Cast: none; approved character identities remain untouched.
- Style reference: project-owned `Config/cover.png`, inspected before generation.
  Preserve its textured painterly folk-horror rendering, slate blues, charcoal
  shadows, snowy whites and restrained amber accent. Do not duplicate its layout.
- Target: `Config/EpisodeCovers/s01e01.png`, portrait 2:3, opaque PNG,
  preferably 1024×1536; no embedded words, logo, title or watermark.
- Binding: `Assets/volchya-poshlina.asset`, episode ID `s01e01`,
  `_catalogCover: s01e01.png`. This is lightweight catalog art, not a Unity
  Sprite, scene background or manually compiled preview.
- Keep the primary subjects inside the central area with a quiet lower portion
  for the existing catalog text/gradient. Actual viewport crop remains unverified
  until the authorized Unity visual gate.

## Generation prompt

Use case: illustration-story. Create one finished portrait 2:3 episode-cover
illustration, ideally 1024×1536, opaque PNG. The attached project-owned story
cover is a STYLE AND PALETTE REFERENCE ONLY, not an edit target. Keep its textured
painterly winter folk-horror aesthetic, slate-blue snow, charcoal silhouettes
and a restrained amber accent; compose a new image, not a crop or copy.

Depict the opening winter journey: a small weathered wooden medicine crate
secured by leather straps on the snowy riverbank in the foreground, with straw
packing and a few amber glass vial necks sheltered inside. Behind it a damaged
wooden bridge angles across a narrow ink-black river toward a dense birch and
pine forest. Beyond the bridge stands one modest old wooden boundary post with
three simple tally notches; fine silvery frost threads catch the blue twilight
on its rough wood. Thin ice and deep snow make the route feel precarious, but
there are no drowned or broken medicine bottles and no predetermined ending.
Strong coherent spatial depth, believable bridge supports, painterly wood and
snow detail. Main objects legible at catalog-thumbnail size, middle of frame;
keep the bottom portion quiet for runtime UI overlays. Restrained suspense,
not gore. No people, animals, character redesigns, giant moon, skulls, runic
alphabet, lettering, typography, border, logo or watermark. Do not introduce
modern medical markings or glowing magical medicine. Generate with the built-in
image tool; no third-party image or artist/franchise reference.

## Production and delivery — 2026-09-08

Generated with the built-in image tool. The only generation reference was the
project-owned story cover above. External comparison images were discovered
after generation and were not supplied to the generator.

1. First draft: `exec-fa272d42-8c14-417d-a2b7-91e480b27811.png`.
   Rejected for delivery because the tally post had crossed markings rather
   than the specified three horizontal notches. Draft retained outside Git.
2. Bounded image edit of that draft: change only the post markings to exactly
   three horizontal notches, preserving the composition and all other objects
   (edit instruction summarized here). Final output:
   `exec-b3a60537-1a65-4239-9062-4e064de904d4.png`.

Both generated files are retained in
`/Users/iantonishin/.codex/generated_images/01a07c22-6c41-75a1-aed1-c9ba20292061/`.
The final approved bytes were copied to
`Config/EpisodeCovers/s01e01.png`, not regenerated or recompressed.

- Final SHA-256:
  `f52697591caa1bd05ab2445c8cce31bd3347c8ceb336d672a6489dd05120782f`.
- Format: PNG, 1024×1536, opaque 8-bit RGB, 2,719,763 bytes.
- Visual inspection: strapped wooden chest, three amber vial necks and straw;
  damaged diagonal bridge, dark river, snowy forest and three horizontal post
  notches. No people, animals, embedded text, branding or outcome-specific loss.
- Catalog mapping: the sole episode `s01e01` explicitly selects `s01e01.png`.
  No `.meta`, Sprite, bundle entry or hand-authored preview was added.
- `node Projects/novels-volchya-poshlina/Art/Checks/episode-cover-audit.cjs`
  passes: binding, safe filename, PNG chunk CRCs, complete deflate payload,
  geometry/opacity, difference from story cover, root Ink GUID, content version
  and retained expanded cast. Run from the story worktree root.

## Visual originality — new episode-cover candidate, iteration 1

Scope: the complete final episode-cover image at the hash above. This is one
new-asset originality assessment after two production drafts, not a new review
of the unchanged historical character/background package.

Descriptive image searches: `winter forest broken wooden bridge medicine chest
fantasy illustration` and `snow wooden bridge forest painting`. Search results
were used only for discovery. The following actual images were opened and
visually inspected in the browser; direct image fetches through the web reader
failed and are not represented as successful inspections by that reader.

- [SILENT ECHO Studio — Establishing Refined 1](https://www.silentechostudio.com/concept-art/kkbfcxkudna2pzoqrtjzfwjfmdassx):
  a wide icy chasm, massive fallen wooden/tree bridge high across the frame,
  foreground broken planks, luminous pink rune stone, blue crystals and purple
  mushrooms. Winter and damaged bridge are generic overlaps. The candidate's
  vertical foreground medicine chest, modest unlit tally post, river and
  diagonal footbridge do not repeat that distinctive arrangement or iconography.
- [Stephanie Cook — Laurelwood Bridge](https://stephaniecookartist.artspan.com/large-multi-view/Landscape_CityScape_Interiors/2209053-30-188845/Painting/laurelwood-bridge.html):
  a portrait pastoral winter creek, intact small horizontal bridge centrally
  placed, prominent bare tree on the left and a pale blue/white/green/brown
  palette. Snow, creek and rustic bridge overlap generically; its composition
  lacks the medicine chest, tally post, damaged bridge and dark-river layout.

Finding: no substantial visual match observed in the inspected comparisons.
The three-notch edit corrected narrative fidelity, not an originality finding;
the final candidate retains the otherwise reviewed composition.
Result: `passed`; risk `low`, confidence `medium`.
Limitations: descriptive image search, not a dedicated upload/hash reverse-image
search; no exhaustive coverage of unindexed or unpublished work and no claim of
legal exclusivity. Neither finding renews historical art evidence nor proves
catalog viewport fit. Actual crop, gradient/text overlap and thumbnail legibility
in the running catalog remain for the authorized final Unity validation slot.
