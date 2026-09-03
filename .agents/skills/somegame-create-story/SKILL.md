---
name: somegame-create-story
description: Design, author, illustrate, integrate, and validate a new atomic visual-novel story in SomeGame. Use when creating a new story project or taking a new story from brief to catalog-ready content; do not use for routine edits to an existing story.
---

# Create a SomeGame story

Use this skill together with `$somegame-workflow`. Treat the current SomeGame
documentation and tooling as authoritative; this skill adds story-production
decisions and does not replace repository coordination or content contracts.

## Establish the brief

Before creating content, obtain these author decisions:

- working title and stable `storyId`;
- genre, written freely by the author;
- factual basis: fictional, inspired by reality, based on real events, about
  real people, documentary, or another author-defined relationship to reality;
- audience, boundaries, approximate scope, and approval mode.

Genre is mandatory author input. Do not choose, normalize, or silently change
it, including in auto-approve mode. Factual basis is a separate axis, not a
genre. Read [story design](references/story-design.md) for the narrative brief.
When real people, places, organizations, or events materially affect the story,
also read [historical integrity](references/historical-integrity.md).

Approval modes:

- `guided`: stop at the agreed concept, scenario, art-list, and acceptance
  checkpoints;
- `auto-approve`: make reversible creative and implementation decisions inside
  the accepted brief, but stop for missing mandatory author input, material
  factual ambiguity, scope conflicts, destructive actions, or publication.

## Prepare repository state

Work only in the canonical SomeGame checkout. Never create or use a separate
worktree for story creation. A new story is developed on its own branch from an
up-to-date `main`, normally `codex/story-<storyId>`.

Before switching or creating the branch, inspect the full dirty tree, active
owners, FIFO, current branch, worktree path, and relationship to `origin/main`.
If unrelated uncommitted work makes an in-place branch switch unsafe, stop and
report the conflict; do not move, stash, commit, reset, or absorb another
owner's changes. Branch integration and publication require separate explicit
authorization.

After repository state is safe, follow the normal SomeGame request/write-lock
workflow before any file creation, art generation, build, test, or validation.

## Produce the story

1. Create the premise, synopsis, dramatic structure, character arcs, scene
   list, choices, consequences, and ending structure from the author brief.
2. For factual stories, maintain claim evidence and clearly separate verified
   fact, inference, reconstruction, and invention.
3. Derive an art manifest from actual scenes before generating assets. Read
   [characters, costumes, and emotions](references/art-and-emotions.md).
4. Create `Projects/novels-<storyId>` from the current canonical template and
   keep it independent of other stories.
5. Write Ink, connect only existing assets, and make costume and emotional
   changes occur at their narrative moments.
6. Add the story to the catalog only when its card and minimum playable content
   exist. Preserve the author's intended catalog position unless asked to
   choose one.

Do not generate speculative assets merely to fill a fixed matrix. Create only
backgrounds, outfits, expressions, poses, media, and branches that the current
story uses or that the author explicitly requested.

## Validate and hand off

Before declaring the story ready, read and apply the
[acceptance checklist](references/acceptance-checklist.md). Use current
SomeGame planning and validation commands to determine exact gates; do not
invent a parallel build pipeline.

Report the branch, project and catalog paths, authored assets, factual or
creative assumptions, validation evidence, manual visual checks, unresolved
risks, and whether the branch is ready for separately authorized integration.
