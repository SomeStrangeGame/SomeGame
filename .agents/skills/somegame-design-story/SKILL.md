---
name: somegame-design-story
description: Turn an author's brief into an originality-screened narrative package for a new SomeGame visual-novel story, including structure, cast, scenes, choices, consequences, endings, state, and downstream production requirements. Use before project, art, and Ink production; do not use for implementation or routine edits to an existing story.
---

# Design a SomeGame story

Use this skill together with `$somegame-workflow`. Read
[story design](references/story-design.md). The author's wording and decisions
are authoritative; this skill produces a reviewable narrative contract rather
than project files, art, or Ink.

## Establish the brief

Require a working title, stable `storyId`, genre in the author's own words,
factual basis, audience, boundaries, approximate scope, and approval mode.
Genre and factual basis are separate axes. Never infer, normalize, or silently
replace the genre, including in auto-approve mode. Ask only for a missing
decision that would materially change the story.

Respect the orchestrator's approval mode:

- `guided`: stop for approval of the concept, narrative package, and resulting
  scene/production requirements;
- `auto-approve`: make reversible creative decisions inside the accepted brief
  while still stopping for missing genre, scope conflicts, sensitive factual
  ambiguity, destructive actions, or publication.

## Produce the narrative package

Define the player promise, premise, synopsis, central conflict, setting rules,
cast goals and relationships, emotional arcs, scene order, meaningful choices,
consequences, reconvergence, endings, and persistent state. Every scene must
identify time/location, dramatic purpose, participants, appearance state,
player agency, and required media without prematurely inventing extra assets.

Choices should express perspective, risk, relationship, knowledge, or
consequence. Do not add cosmetic branches to inflate choice count. Make every
ending and state transition reachable and explain which state must survive
reconvergence or episode boundaries.

When the orchestrator supplies factual constraints or an evidence ledger,
preserve their classifications and uncertainty. Do not perform a separate
historical-research workflow or turn reconstruction into documented fact.

## Iterate through the originality gate

Apply `Docs/AI/rules/OriginalityReviewProtocol.md` to the complete narrative
package using the narrative criteria in [story design](references/story-design.md).
Review distinctive expression, character configuration, scene sequence, choice
structure and endings. Preserve the approved genre, premise, factual basis,
boundaries and player promise; a material change to them returns to the author.
A `blocked` result stops all downstream production.

## Hand off

Return the approved brief, narrative package, scene matrix, choice/state graph,
ending conditions, continuity constraints, factual/reconstruction boundaries,
scene-derived character requirements, and the remaining art/media requirements.
Character requirements identify narrative function, protected identity facts,
used outfits/conditions, emotions, poses, transformations, and composition
constraints without inventing unused visual variants. Mark unresolved author
decisions explicitly. Downstream production must not silently change this
package; material revisions return here for approval. Include the originality
iteration log, sources, final risk assessment, limitations, and explicit
`passed` or `blocked` gate result.
