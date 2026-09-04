---
name: somegame-create-story
description: Orchestrate a new atomic SomeGame visual-novel story from author brief through design, project creation, art, playable content, catalog integration, and acceptance. Use for end-to-end creation or resuming an unfinished new story; do not use for routine edits to an existing story.
---

# Create a SomeGame story

Use this skill together with `$somegame-workflow`. Treat the current SomeGame
documentation and tooling as authoritative; this skill adds story-production
decisions and does not replace repository coordination, project creation, or
content contracts.

## Establish the brief

Before creating content, obtain these author decisions:

- working title and stable `storyId`;
- genre, written freely by the author;
- factual basis: fictional, inspired by reality, based on real events, about
  real people, documentary, or another author-defined relationship to reality;
- audience, boundaries, approximate scope, and approval mode.

Genre is mandatory author input. Do not choose, normalize, or silently change
it, including in auto-approve mode. Factual basis is a separate axis, not a
genre. When real people, places, organizations, or events materially affect the
story, read [historical integrity](references/historical-integrity.md); this
conditional responsibility remains here and is not a separate skill.

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

1. Invoke `$somegame-design-story` to turn the author brief into an approved
   narrative package, scene matrix, choice/state graph, endings, continuity
   constraints, downstream art/media requirements, and a passed narrative
   originality gate. Stop the workflow if that gate is blocked.
2. For factual stories, maintain claim evidence and clearly separate verified
   fact, inference, reconstruction, and invention.
3. Invoke `$somegame-create-unity-project` to create
   `Projects/novels-<storyId>` from the current canonical template. Do not begin
   project-bound authoring until that skill has configured a unique optional
   Unity MCP server and proven a live connection to the exact new project path.
   If its live MCP gate is blocked, hand off the scaffold as not ready instead
   of silently continuing without MCP.
4. Invoke `$somegame-create-character` for each required cast member, using the
   approved scene-derived character requirements. Its identity masters, used
   appearance variants, runtime selectors, provenance, contact sheets, and
   visual evidence form the character handoff. Require a passed visual-
   originality result for every character before continuing.
5. Invoke `$somegame-produce-story-art` with the approved scene package and
   character handoff. It produces the remaining backgrounds, choice and
   presentation art, and media without recreating character assets. Require its
   visual-originality gate to pass before content authoring.
6. Invoke `$somegame-author-story-content` with the approved narrative package,
   character handoff, and remaining art handoff. Require coherent source Ink,
   metadata, selectors, compiled story, source map, and target validation before
   continuing. Its full-text originality gate must pass before final compilation
   and acceptance. The episode must enter its first real scene directly rather
   than presenting a one-option `Начать`, `Начать историю`, or `Играть` choice.
7. Invoke `$somegame-accept-story` with every stage handoff. It owns catalog
   registration, end-to-end audits, changed-path validation, runtime/manual
   gates, originality-evidence audit, and the final readiness status. Preserve
   the author's intended catalog position. It must fail closed rather than waive
   a missing, incomplete, stale, or blocked originality result.

Do not generate speculative assets merely to fill a fixed matrix. Create only
backgrounds, outfits, expressions, poses, media, and branches that the current
story uses or that the author explicitly requested.

## Validate and hand off

Do not declare the story ready before `$somegame-accept-story` returns its
evidence-backed status. Use current SomeGame planning and validation commands
to determine exact gates; do not invent a parallel build pipeline.

Report the branch, project and catalog paths, authored assets, factual or
creative assumptions, validation evidence, manual visual checks, unresolved
risks, and whether the branch is ready for separately authorized integration.
