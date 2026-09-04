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

After the first complete narrative draft, run the originality-review loop from
[story design](references/story-design.md). Perform at most five review
iterations. Each iteration reviews the current complete draft against available
external sources, records evidence and risk, and either passes or identifies
specific elements for the next targeted revision.

Exit the loop immediately when no material originality risk remains and allow
the story workflow to continue. When a risk is found before the fifth review,
revise the implicated expression, character configuration, scene sequence,
choice structure, or ending for the next iteration while preserving the
author's approved genre, premise, factual basis, boundaries, and intended player
promise. A change that would materially alter those author decisions still
requires approval rather than being hidden as an originality edit.

If material risk remains after the fifth review, stop the entire downstream
story workflow and report the matching works and sources, implicated elements,
risk and confidence, and the revisions attempted in all iterations. Do not
create the Unity project, characters, other art, or Ink until the author resolves
the block.

The review is an evidence-backed risk screen, not a legal opinion, exhaustive
copyright clearance, or proof of absolute uniqueness. Do not treat shared genre
tropes, stock situations, historical facts, or broad themes alone as plagiarism.
If search or source access is insufficient to perform a meaningful review,
report the limitation and do not silently mark the gate passed.

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
