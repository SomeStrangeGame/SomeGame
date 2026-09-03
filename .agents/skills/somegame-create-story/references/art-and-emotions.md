# Characters, costumes, and emotions

Read this reference before preparing character art or its Ink selectors. Also
read the current repository character-layering and manual-content rules.

## Scene-derived manifest

Derive art from the approved scene list. Each character appearance records:

- scene and narrative purpose;
- character identity;
- outfit and outfit condition;
- emotion and pose;
- screen side and gaze direction;
- expected selector and target file.

Direction is a scene composition decision. Preserve an author-specified rule
such as all secondary characters looking left, but do not universalize it to
unrelated stories.

## Identity-first character creation

Create and approve one neutral, standing, full-body master on transparency
before variants. It must be usable in the visual-novel composition: no chair,
background, unintended props, back-facing default, or cropped anatomy unless
the scene requires them.

Record stable identity traits: face, apparent age, body proportions, hair,
distinctive features, scale, palette, and posture. Produce later images by
editing or referencing the approved master, not by regenerating independently
from text. A variant may change expression, hands, pose, damage, or story state,
but must not accidentally change identity, art style, or scale.

## Costumes

Build an outfit inventory from the story. For each outfit, define period,
function, garments, footwear, accessories, palette, wear or damage, and used
scenes. Create a neutral whole reference for that outfit from the identity
master, then derive its used emotion/pose variants.

Treat a new outfit, an outfit condition, a pose, and an emotion as different
dimensions. Generate only combinations used by the story. Ordinary scripted
costume changes should use coherent whole variants; use layered body/clothing
assets only when interactive wardrobe behavior or the current project contract
requires them.

## Emotional variants

Map each scene to the character's emotional arc. Neutral is the baseline;
additional expressions and poses come from narrative use rather than a fixed
list. Integrate each change into Ink at the moment the emotion or pose changes.

Validate the chain `Ink scene -> selector -> outfit -> emotion/pose -> file`,
including the current runtime's fallback behavior. Create a contact sheet per
character and inspect identity, outfit continuity, gaze, scale, foot position,
alpha edges, anatomy, and emotional readability across all variants.
