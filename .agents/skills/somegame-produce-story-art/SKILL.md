---
name: somegame-produce-story-art
description: Plan, create, originality-screen, import, and visually validate non-character production art for a SomeGame story from an approved scene package and character handoff. Use for backgrounds, choices, presentation art, and media; use somegame-create-character for character identity, outfits, emotions, and poses.
---

# Produce SomeGame story art

Use this skill together with `$somegame-workflow`. The approved narrative
package, character handoff from `$somegame-create-character`, exact story-project
path, current repository art rules, and author decisions are inputs; do not
revise the story to justify speculative assets or recreate character assets.
Use `$imagegen` whenever AI raster generation or editing is required.

## Freeze the art manifest

Read the current manual-content rules. Derive every remaining asset from an
actual scene and record its narrative purpose, subject, visual state, runtime
selector/address, target file, and approval status. Distinguish required,
optional, and draft work. Treat the character package as an immutable input;
return missing or inconsistent character work to `$somegame-create-character`.
Do not turn incidental prose into extra backgrounds without approval.

For factual stories, preserve material visual evidence and reconstruction
labels supplied by the story package. Stop when an unresolved visual choice
would invent identity, responsibility, historical detail, or sensitive harm.

## Produce coherent non-character assets

Establish the shared style, palette, lighting, composition, period, and output
geometry with a small pilot before bulk production. Backgrounds, choice art,
UI/presentation art, audio, and video must map to manifest rows and follow the
current category-specific contracts. Check their composition against approved
character scale and screen placement without altering the character package.

## Pass the visual-originality gate

Review the current authored background, choice, and presentation-art package
against available published visual works before final import. Use available
visual/reverse-image search, direct source inspection, and descriptive searches.
Assess distinctive combinations of composition, silhouettes, environment
design, palette relationships, props, iconography, and scene-specific motifs;
do not treat generic locations, functional UI shapes, historical facts, or
genre conventions as material similarity by themselves.

Perform at most five reviews. In each review, record inspected sources, matched
elements, meaningful differences, provenance, confidence, and `low`, `medium`,
or `high` risk. Medium and high findings are material. Exit immediately with
`passed` when none remain. Otherwise, before review 5, make a targeted revision
to the distinctive combination responsible for the risk without changing the
approved narrative purpose or factual constraints. Recoloring, mirroring,
cropping, or adding noise alone does not resolve structural similarity.

If material risk remains after review 5, mark the art handoff and downstream
story workflow `blocked`; report direct sources, implicated assets and elements,
risk/confidence, and all attempted revisions. Licensed, public-domain, adapted,
or homage material requires explicit provenance and rights/attribution handling.
The gate is a risk screen, not legal clearance or proof of absolute uniqueness.
If meaningful source search is unavailable, disclose the limitation rather than
silently passing.

## Import and validate

Place only approved deliverables in the exact story project. Preserve Unity
`.meta` identity when replacing an asset; let the current Unity import pipeline
create new metadata and apply its canonical texture settings. Do not establish
per-file compression policy that competes with the shared postprocessor.

Validate the chain from manifest row to runtime address and its composition with
the handed-off characters. Use a bounded in-game visual check when the changed
path plan requires it. Automated dimension/alpha checks do not replace visual
review.

Hand off the approved non-character manifest, exact files and runtime addresses,
generation/edit provenance, rejected/draft work, visual evidence,
visual-originality sources, iteration log and final gate result,
import/validation results, and unresolved manual gates. Missing required art or
a blocked originality gate blocks downstream content completion; neither may be
hidden with an unrelated fallback.
