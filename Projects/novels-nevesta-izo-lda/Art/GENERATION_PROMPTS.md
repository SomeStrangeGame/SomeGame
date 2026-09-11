# Generation provenance

Built-in image generation was used. Prompts consistently specified original painterly cinematic 2D visual-novel art, Eastern European winter-folklore atmosphere, cobalt/silver palette with amber and rust-red accents, no text/logos/watermarks/franchise resemblance, character-safe backgrounds, and genuine transparency for sprites/icons. Character prompts recorded protected traits and requested only the three scene-used states. Background prompts correspond one-to-one to the approved scene matrix. Choice-icon sheet was re-extracted into five standalone transparent icons through identity-preserving edits.

Expansion iteration 2 added three one-to-one background prompts: a thread-suspended House of Letters with a cold stove; an inverted apple orchard visible beneath clear lake ice with amber memory fruit and a brass case; and a frozen timber weir with chain gate, seven copper speaking tubes and flood-measure posts. All prompts prohibited people, text, UI, logos and artist/franchise imitation; all three outputs are used by explicit Ink selectors.

## Episode catalog cover — 2026-09-08, candidate 1

- Mode: built-in image generation, one new composition with a local world/style reference; no CLI/API fallback and no raster post-processing.
- Reference: `Assets/Locations/frozen-orchard.png`; not an instruction to crop or duplicate the scene.
- Selected output: `Config/EpisodeCovers/s01e01.png`, 1024×1536 RGB PNG.
- Source generation file: `/Users/iantonishin/.codex/generated_images/01a07c22-5a8e-7da2-82f2-931aa4575624/exec-f878ee56-0e92-4c64-ad2a-a5204fcbd84b.png`.
- SHA-256: `591958954c28fdfcebb3bb3271fc27e2eb0502ac8af431ba45bae05af0a9540c`; copied byte-for-byte to the story.

Exact prompt:

```text
Use case: illustration-story. Create ONE new finished portrait catalog episode cover for an original Russian visual novel, 'Nevesta izo lda', episode s01e01. Image 1 is a style/world reference ONLY, not the image to crop or repeat. Match its painterly cinematic winter realism, cobalt and silver with quiet amber accents. New independent portrait composition, intended 1024x1536. Scene: Glass Mere at winter midnight, looking obliquely down THROUGH thick crystal-clear blue lake ice at an entire drowned apple tree rooted on the lake bed, branches reaching upward just below the ice. A few amber apples glow warmly ON those underwater branches as vessels of lost shared memories; small amber bubbles float around the upper branches under the sealed ice. This is a submerged orchard, NOT an ordinary orchard above water, not a reflection, and not a tree growing out through the surface. Restrained snow powder and fine silver frost around the edges establish the horizontal ice plane; distant dark bank is a small soft backdrop, not the focus. Compose a memorable branching silhouette concentrated in the central and upper middle of the portrait, readable in a small catalog card. Lower 25 percent is quieter dark blue water/ice with minimal detail for overlaid UI text. Intimate mystery and tenderness, not horror or an epic battle. No people or faces, no bride or princess, no castle, no boat, no chest, no letters/ribbons, no typography, no UI, no borders, no logos/watermarks, no named-artist or franchise imitation. Preserve this physically legible underwater perspective. Deliver a fully painted image, not a mockup or contact sheet.
```
