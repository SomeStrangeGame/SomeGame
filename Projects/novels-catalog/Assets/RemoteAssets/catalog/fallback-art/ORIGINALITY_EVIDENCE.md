# Fallback catalog art evidence

The two raster assets in `sprites/` were generated specifically for the neutral
fallback catalog on 2026-09-07 with the built-in OpenAI image-generation tool.
No external image, franchise, logo, character or product artwork was used as an
input. The approved neutral fallback UI mockup was supplied only as a palette
and composition reference.

## Asset roles

- `scroll-background.png` — long, low-detail charcoal/slate background intended
  to move with the vertically scrolling catalog content.
- `episode-placeholder.png` — genre-neutral abstract placeholder shown when an
  episode does not provide its own authored image.
- `icons/state/*.svg` — clean geometric lock, completion, restart and warning
  masters for episode state.
- `icons/navigation/*.svg` — clean geometric menu and library masters kept
  separate from episode state.

The raster assets contain no baked UI, text, controls or borders. Those remain
live prefab elements so product variants can override them safely.

The icon geometry is authored directly in repository SVG source. Initial
generated raster drafts were rejected during visual inspection because their
transparent edges contained visible noise; those drafts are not retained.

On 2026-09-07 the SVG masters were deterministically rasterized with Sharp to
128px transparent PNG sprites and connected to the fallback prefab (lock,
completion, restart, warning and library). The menu icon remains reserved.
`rounded-panel.svg` and `card-shade.svg` are authored UI primitives, rasterized
without image-generation: a nine-sliced rounded rectangle and a readability
gradient. They do not replace the generated background/placeholder artwork.

## Prompt constraints

The generation prompts required original abstract geometric landscapes, a
charcoal/slate palette, crop-safe composition and no text, logos, people,
recognizable genre objects, copyrighted imagery or watermarks.
