---
name: somegame-create-genre-catalog
description: Design, author, integrate, and visually validate a genre-specific SomeGame story catalog as a direct Unity Prefab Variant of the neutral catalog fallback. Use when a genre application needs its own catalog background, card, dots, buttons, restart popup, and selectable Player build flavor; do not use for story-local Bubble UI or ordinary catalog copy changes.
---

# Create a genre catalog

Use this skill together with `$somegame-workflow`; use `$imagegen` when new
raster artwork is needed and Unity feature/build-validation workflows for
implementation and final evidence.

Treat `Projects/novels-catalog/Assets/RemoteAssets/catalog/fallback.prefab` as
the single neutral working catalog and inheritance root. A genre catalog lives
at `catalog/<genre>/screen.prefab` and must be a direct serialized Prefab
Variant of that fallback. Never insert an intermediate neutral variant, copy
the full hierarchy, or create the prefab/UI at runtime or through a temporary
prefab-builder script.

## Establish the genre contract

Inspect the target stories before designing: their `card.json`, covers,
locations, characters, presentation assets, audience, license evidence and
actual narrative motifs. Derive a reusable genre language rather than skinning
the catalog around one protagonist or episode. Preserve the established
portrait interaction contract: no catalog heading, no back button, one focused
card with controlled neighbor peeks, circular page dots, a separate tall
primary action button, and the authored restart-confirmation popup.

Define the palette and roles before producing assets:

- background atmosphere and safe negative space behind the card;
- card surface/frame with a quiet readable center;
- active and inactive dot colors;
- primary, secondary and destructive/warning accents;
- restart popup surface and text contrast.

Avoid embedded words, logos and story-specific copyrighted marks in reusable
UI sprites. Record attribution or ShareAlike requirements when referenced
source material requires them.

## Author assets and the variant

Keep all genre assets in the catalog project under `catalog/<genre>/sprites/`.
Create final raster assets rather than baking the whole screen into one image.
Background, card panel and action-button sprites must match their runtime aspect
and resizing behavior. Preserve genuine transparency outside UI silhouettes;
inspect alpha and hidden-edge RGB before import. Use sliced rendering only when
the sprite border and Unity border metadata are designed and verified for it.

The variant may override authored sprites, colors, text contrast, button
heights and small optical offsets. It must preserve the fallback hierarchy,
component types, serialized runtime references, Safe Area, carousel behavior,
template-card identity and restart interactions. Genre visuals belong in the
variant, not in shared runtime code.

If the genre is a distinct application flavor, extend the existing catalog
variant selection end-to-end with one stable identifier: runner option, build
script argument, Player build define and `CatalogAddresses` branch. Keep the
default fallback and existing flavors unchanged. Filtering which stories ship
is a separate release-composition concern; a visual flavor must not silently
rewrite the canonical catalog registry.

## Validate the real result

Validate progressively:

1. inspect new `.meta` identities, prefab source GUID and override targets;
2. run the skill validator when this skill itself changed;
3. build the catalog for the target content platform and audit bundle contents;
4. compile `Novels` with every changed flavor path;
5. build a fresh portrait Player for the genre flavor;
6. inspect the focused card, both neighbor peeks, dots, long title/description,
   tall centered action label, continue/restart states and confirmation popup;
7. check narrow-phone Safe Area, missing optional art fallbacks, Console and
   runtime logs for missing assets or fallback markers.

A generated mockup, prefab YAML inspection, source PNG preview or successful
compile alone is not a visual acceptance gate. Capture fresh Player evidence
and state explicitly which interactions or viewports remain unverified.

## Hand off

Report the genre contract, exact variant and sprite paths, direct fallback
inheritance evidence, flavor identifier, licensing notes, build commands,
artifact/screenshot paths and remaining visual limitations. Do not commit or
publish unless the user asks separately.
