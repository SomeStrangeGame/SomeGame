---
name: somegame-create-scp-story-bubbles
description: Create or substantially redesign story-local Bubble presentation for a SomeGame story set in the SCP universe, including clinical dialogue states, anomaly-safe horror treatment, and text-led decisions. Use when an SCP story needs its own Bubble prefab and visual choice treatment; do not use for ordinary copy changes, shared fallback redesign, or unrelated horror UI.
---

# Create SCP-story Bubble presentation

Use this skill together with `$somegame-workflow`; use `$imagegen` when new
raster artwork is needed. Treat the target story, its license evidence, current
Bubble runtime contract, approved art direction, audience rating and horror
boundaries as authoritative. This skill changes presentation, not story facts,
choice topology, attribution, or shared runtime behavior.

Read `Docs/AI/guides/StoryBubblePresentation.md`; it owns the shared Bubble
implementation boundaries, state matrix and handoff evidence. This skill adds
only the SCP licensing and containment-horror profile below.

Build an original interface for the particular story. Do not copy an SCP Wiki
page, another SCP adaptation, or the UI of an existing game. Do not introduce
the SCP Foundation logo, article screenshots, classification seals, or other
licensed visual material unless the story package deliberately requires them
and records their source, attribution and ShareAlike treatment. Generic
containment signage, document structure and clinical equipment are not a reason
to imitate a specific published design.

## Establish the local contract

Inspect the current story prefab, the effective fallback, one known-good
story-specific Bubble, and the runtime component fields before designing.
Identify separately:

- narrator and no-character dialogue;
- ordinary named-character dialogue;
- intercom, recording, terminal or document text that has a real story role;
- anomalous speech, only when the source is distinguishable in the narrative;
- choice prompt, choice container and the nested button prefab;
- optional icons, labels and the runtime rules that populate them;
- the exact story-local addresses and Unity metadata involved.

Create or modify presentation only inside the target SCP story project unless
the user explicitly requests a shared contract change. Keep fallback Bubble
assets and every other story unchanged. Preserve required filenames, hierarchy
references, serialized fields and component types.

## Design restrained containment horror

Prefer a low, compact panel that preserves character faces, evidence objects
and threatening negative space. Use a dark neutral surface, strong light-text
contrast, restrained cold cyan or institutional green for normal states, and a
limited warning amber or red accent only when the current scene earns it.
Borders may suggest laminated glass, a terminal frame, an evidence sleeve or a
case-file edge, but decoration must remain subordinate to text.

Give presentation states semantic meaning rather than arbitrary variety:

- narrator: quiet, neutral observational treatment;
- named character: stable readable treatment consistent with the story palette;
- intercom, recording, terminal or document: a distinct state only if the
  delivery medium matters to comprehension;
- anomalous voice: controlled irregularity at the edge, never damage to the
  letters themselves;
- alert state: reserved for actual escalation, not every frightening line.

Never use rapid flashing, aggressive shake, continuous chromatic separation,
low-contrast text, dense scanlines over letters, or animation that prevents a
reader from holding their place. Horror should come from pacing, implication,
sound, framing and small controlled deviations. Respect reduced-motion and
photosensitivity constraints available in the target runtime.

Create UI sprites without embedded words. Keep a quiet central content area,
genuine transparent exterior pixels and clean connected silhouettes. Verify
alpha rather than trusting a checkerboard preview. Fill hidden RGB near
transparent edges with a compatible dark matte to prevent bright compression
halos. Use nine-slicing only when border geometry has been proved at the actual
runtime size; keep target-platform texture overrides narrow and evidence-based.

## Build text-led decisions

Adult and teen SCP choices are text-led. Keep every label visible, legible and
fully tappable; an icon may reinforce evidence, risk or action but must not be
required to decode the choice. Use consistent button geometry and enough
vertical room for long investigative wording. When choices express different
risk classes, distinguish them through restrained accent, icon and spacing
rather than hiding consequences behind decorative symbols.

Do not make every choice resemble a clearance badge, warning placard or file
tab. Choose one visual metaphor supported by the current scene. Preserve label
fallbacks when optional artwork is missing, and never allow a missing icon to
produce a blank or collapsed button.

## Validate actual horror states

Check prefab serialization and exact asset resolution first, then observe at
least:

1. long narrator text over the brightest and darkest used backgrounds;
2. long named-character dialogue with the protagonist and a supporting
   character in their authored screen positions;
3. every special medium state actually used by the story;
4. the strongest anomalous or alert state, including motion duration;
5. the largest real choice group with its longest labels;
6. missing optional icon/art fallback when the contract permits it;
7. the narrowest supported portrait viewport and safe area.

Validate in a freshly rebuilt target Player when the changed-path plan requires
it. Confirm contrast, line wrapping, reading order, text stability, character
and evidence visibility, tap areas, safe-area placement, correct state changes,
clean alpha edges and absence of pink or fallback sprites. Inspect fresh runtime
logs for missing assets and blocking markers. A prefab preview, source PNG or
successful compilation does not satisfy the visual gate.

Iterate on the smallest responsible layer: artwork for surface treatment,
import/render settings for edge or slicing defects, prefab layout for geometry,
and runtime binding only for a demonstrated binding defect. Do not compensate
for a story-local visual defect by changing shared Bubble behavior.

## Hand off

Report the exact story-local prefab and sprites, semantic dialogue states,
choice behavior, motion and accessibility treatment, license/provenance notes,
material import settings, validation commands and fresh visual evidence paths.
Call out every state not observed in a real Player. Do not commit, publish,
alter Ink, or register the story in the catalog; new-story registration belongs
to `$somegame-accept-story`.
