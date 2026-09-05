---
name: somegame-create-story
description: Orchestrate a new atomic SomeGame visual-novel story from author brief through design, project creation, art, playable content, and acceptance. Use for end-to-end creation or resuming an unfinished new story; do not use for routine edits to an existing story.
---

# Create a SomeGame story

Use this skill together with `$somegame-workflow`. This skill owns stage order,
inputs, outputs and stopping conditions. Each invoked production skill owns its
domain; do not reproduce or override its procedure here.

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

Neither mode authorizes the final heavy validation slot. The canonical approval
and execution rules live only in `UnityConcurrency.md`.

## Prepare repository state

Create the registered story worktree through the exact lifecycle in
`ParallelWorkDetails.md`. That protocol exclusively owns worktree scope,
parallelism, candidate commits, resource locks and removal. A discovered shared
dependency stops at handoff instead of silently widening story ownership.

## Produce the story

1. Invoke `$somegame-design-story`; continue only with its approved narrative
   package and passed originality result.
2. For factual stories, maintain claim evidence and clearly separate verified
   fact, inference, reconstruction, and invention.
3. Invoke `$somegame-create-unity-project`; static scaffold readiness is the
   required output at this stage.
4. Invoke `$somegame-create-character` for every scene-required cast member;
   continue only with complete, originality-passed character handoffs.
5. Invoke `$somegame-produce-story-art` for the remaining manifest; continue
   only with its complete, originality-passed handoff.
6. Invoke `$somegame-author-story-content` with the approved narrative,
   character and remaining-art handoffs; require `ready-for-final-validation`.
7. When all production stages are ready, follow `UnityConcurrency.md` to obtain
   the separate current authorization for the final slot, then invoke
   `$somegame-accept-story`. Acceptance exclusively owns catalog registration,
   runtime/manual gates and the final readiness decision.

Do not generate speculative assets merely to fill a fixed matrix. Create only
backgrounds, outfits, expressions, poses, media, and branches that the current
story uses or that the author explicitly requested.

## Validate and hand off

Before acceptance, report `ready-for-final-validation`, never accepted or
runtime-validated. Candidate commit, integration and worktree removal follow
`ParallelWorkDetails.md` and `IntegrationProtocol.md`; this skill does not own
those mechanics.

Report the branch, project and catalog paths, authored assets, factual or
creative assumptions, validation evidence, manual visual checks, unresolved
risks, and whether the branch is ready for separately authorized integration.
