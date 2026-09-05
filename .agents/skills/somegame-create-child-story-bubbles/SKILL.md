---
name: somegame-create-child-story-bubbles
description: Create or substantially redesign story-local Bubble presentation for a SomeGame story aimed at pre-reading children, including warm illustrated dialogue panels and large image-led choice buttons. Use when a children’s story needs its own Bubble prefab and visual choice treatment; do not use for ordinary copy changes, shared fallback redesign, or unrelated story UI.
---

# Create child-story Bubble presentation

Use this skill together with `$somegame-workflow`; use `$imagegen` when new
raster artwork is needed. Treat the target story, current Bubble runtime
contract, approved art direction, and the child’s age as authoritative. This
skill changes presentation, not story meaning, choice topology, or shared
runtime behavior.

Read `Docs/AI/guides/StoryBubblePresentation.md`; it owns the shared Bubble
implementation boundaries, state matrix and handoff evidence. This skill adds
only the pre-reading-child profile below.

Assume a pre-reading child may use the story beside an adult. The adult must be
able to read dialogue comfortably, while a child should be able to distinguish
the available choices from their pictures alone. Do not silently remove text
that is required for accessibility or for a safe missing-image fallback.

## Establish the local contract

Inspect the current story prefab, the effective fallback, one known-good
story-specific Bubble, and the runtime component fields before designing.
Identify separately:

- ordinary dialogue, including speaker and no-character variants;
- choice prompt and choice container;
- the button prefab nested in the Bubble prefab;
- optional icon and label objects and the runtime rules that populate them;
- the exact story-local asset address and Unity metadata involved.

Create or modify presentation only inside the target story project unless the
user explicitly requests a shared contract change. Keep fallback Bubble assets
and every other story unchanged. A story-local prefab must keep the runtime’s
required filename, hierarchy references, serialized fields, and component
types; visual similarity is not enough if the runtime cannot resolve it.

## Design for the illustration, not over it

Prefer a low, compact dialogue panel that leaves the important background art
visible. Use a light, warm, calm surface with strong dark-text contrast and
large readable type. Decorative motifs should live near corners or edges and
must not compete with words. Do not add a detached top ornament by default; add
one only when it communicates a real state and survives the visual check.

Create UI sprites without embedded words. Preserve transparent exterior pixels
and a quiet central content area. Import them through the project’s normal
Unity pipeline, preserve existing `.meta` identity when replacing an asset, and
use nine-slicing only when the sprite was designed for the target resizing
behavior.

Do not treat a checkerboard-looking source or preview as proof of real
transparency. Inspect the alpha channel itself before import: the exterior must
be zero-alpha, the intended panel/card must form one clean connected shape, and
there must be no isolated or patterned alpha pixels around its edge. When an
edited or generated sprite inherits a noisy mask, replace the mask
deterministically instead of repeatedly eroding it until the preview looks
acceptable.

Transparent pixels still carry RGB that bilinear filtering and platform texture
compression can bleed into the visible edge. Fill hidden RGB with a suitable
matte derived from the adjacent panel edge, never a white or painted
checkerboard background. Keep antialiasing only when the target import format
preserves it cleanly. If a built target still produces a white rim, coloured
halo, or checkerboard speckles, test a clean thresholded mask and an
alpha-capable lossless/uncompressed platform override for this small UI sprite;
use the narrowest import override that fixes the demonstrated target rather than
changing all story textures.

## Build illustrated choices

Keep the choice buttons inside the story’s Bubble prefab. For two choices,
prefer two large horizontal image-led cards in one row when the target viewport
can display them without overlap. Make the whole card tappable and leave a
clear inset between its frame and illustration.

Use a dedicated choice-card sprite whose source aspect is close to the rendered
button aspect. Do not reuse a wide dialogue-panel sprite on a compact card: its
nine-slice borders become visually heavy or can sit inside the illustration.
When source and target aspect already agree, prefer proportional/simple image
rendering; use sliced rendering only when the border must stretch and its
border metadata has been visually proved at the actual card size.

An image-only presentation is valid only when every production choice has a
distinct, resolved, age-readable icon. Keep the label populated as a fallback
or accessibility source even when the authored visual state hides it. Missing
or ambiguous artwork must not produce a blank button.

## Validate the actual states

Check prefab serialization and asset resolution first, then validate at least:

1. a representative long ordinary dialogue line;
2. the real choice prompt with all authored buttons and icons;
3. a missing-icon or fallback state when the contract permits one;
4. the narrowest supported portrait viewport.

Inspect the result in the built runtime when the changed-path plan requires it,
not only in prefab mode. Confirm text visibility and contrast, illustration
occlusion, button orientation and tap area, icon containment, thin consistent
frames, safe-area placement, and absence of pink/missing sprites or unexpected
fallback UI. View transparent rounded corners against the real scene on every
changed target platform and explicitly reject white outlines, colour bleed,
painted checkerboards, alpha speckles, and compression-created halos. A source
PNG preview and a successful compile do not satisfy this visual gate; rebuild
the relevant atomic content and Player so the observation cannot come from a
stale bundled texture.

Iterate on the smallest responsible layer: sprite artwork for border character,
sprite import/render mode for deformation, prefab layout for geometry, and
runtime binding only for a demonstrated binding defect. Do not compensate for
one bad state by changing shared Bubble behavior.

## Hand off

Report the exact story-local prefab and sprites, intended dialogue and choice
states, image/label fallback behavior, import settings that materially affect
rendering, validation commands, and visual evidence paths. Call out any state
that was not observed in a real Player. Do not commit, publish, alter Ink, or
register the story in the catalog; new-story registration belongs to
`$somegame-accept-story`.
