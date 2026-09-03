---
name: somegame-produce-story-art
description: Plan, create, import, and visually validate production art for a SomeGame story from an approved scene package. Use for story backgrounds, characters, outfits, emotions, poses, choices, presentation art, and media; do not use for narrative design or Ink authoring.
---

# Produce SomeGame story art

Use this skill together with `$somegame-workflow`. The approved narrative
package, exact story-project path, current repository art rules, and author
decisions are inputs; do not revise the story to justify speculative assets.
Use `$imagegen` whenever AI raster generation or editing is required.

## Freeze the art manifest

Read [characters, costumes, and emotions](references/art-and-emotions.md) and
the current character-layering and manual-content rules. Derive every asset
from an actual scene and record its narrative purpose, subject, visual state,
runtime selector/address, target file, and approval status. Distinguish
required, optional, and draft work. Do not generate a fixed emotion/outfit
matrix or turn incidental prose into extra backgrounds without approval.

For factual stories, preserve material visual evidence and reconstruction
labels supplied by the story package. Stop when an unresolved visual choice
would invent identity, responsibility, historical detail, or sensitive harm.

## Produce coherent assets

Establish the shared style, palette, lighting, composition, period, and output
geometry with a small pilot before bulk production. For each character,
approve one neutral full-body identity master before variants. Derive only the
used outfits, conditions, emotions, and poses through identity-preserving edits
of that master. Never repair incompatible generations by combining unrelated
heads, bodies, hair, clothes, or facial parts.

Prefer whole production variants. Create technical runtime layers only when
the story contract requires independent customization and only by
deterministically separating a coherent approved image on one registered
canvas. Backgrounds, choice art, UI art, audio, and video must likewise map to
manifest rows and follow the current category-specific contracts.

## Import and validate

Place only approved deliverables in the exact story project. Preserve Unity
`.meta` identity when replacing an asset; let the current Unity import pipeline
create new metadata and apply its canonical texture settings. Do not establish
per-file compression policy that competes with the shared postprocessor.

Validate the chain from manifest row to runtime address. For characters,
produce full-body and face contact sheets plus light/dark-background alpha
proofs; inspect identity, anatomy, hair, outfit continuity, gaze, scale, foot
position, edges, and seams. Use a bounded in-game visual check when the changed
path plan requires it. Automated dimension/alpha checks do not replace visual
review.

Hand off the approved manifest, exact files and runtime addresses, master and
variant provenance, rejected/draft work, visual evidence, import/validation
results, and unresolved manual gates. Missing required art blocks downstream
content completion; it must not be hidden with an unrelated fallback.
