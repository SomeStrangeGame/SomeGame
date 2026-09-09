---
name: somegame-create-story
description: Orchestrate a new atomic SomeGame visual-novel story from brief through validated pre-production, project creation, playable content, web preview, acceptance, and release handoff. Use for end-to-end creation or resuming an unfinished new story; do not use for routine edits to an existing story.
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

Capture the author credit or pen name when supplied, including episode-specific
credits if needed. Keep it separate from the story/episode title. Store the
story credit as optional `author` in `Config/card.json`, and episode overrides
as `_author` in the definition's episode entries. Never invent an author or
insert a placeholder: unspecified credits stay empty and are not displayed.
An episode without its own credit inherits the story credit. See the author
contract in `Docs/AI/guides/ContentPipeline.md` for export and display rules.

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

## Produce the story

1. Invoke `$somegame-design-story`. Treat narrative design and its originality
   review as one gate; continue only with the approved package and `passed`
   result. `OriginalityReviewProtocol.md` exclusively owns the bounded
   full-candidate review loop and its stopping conditions.
2. For factual stories, maintain claim evidence and clearly separate verified
   fact, inference, reconstruction, and invention.
3. Freeze a scene-derived production manifest before creating a Unity project.
   Record only used characters, appearance states, backgrounds, choices,
   presentation, audio/media, logical selectors/addresses, target formats,
   geometry, alpha and platform constraints. The canonical template contract,
   not a speculative project layout, defines these constraints.
   Include catalog artwork decisions: one story cover and, only where useful
   or requested, an optional cover for each episode. Map these to stable episode
   IDs; omitted episode art intentionally falls back to the story cover. Do not
   generate duplicates just to fill every episode. Read the episode-cover
   contract in `Docs/AI/guides/ContentPipeline.md` before handing off this art.
4. Produce and approve pre-production deliverables outside the repository:

   - invoke `$somegame-create-character` for every required cast member;
   - invoke `$somegame-produce-story-art` for remaining visual media;
   - invoke the applicable Bubble skill for presentation design and sprites;
   - invoke an audio skill only for manifest-required sound or music.

   Each stage must pass its originality and manual review. Preserve drafts and
   rejected variants outside Git; hand off approved masters, logical addresses,
   provenance and import requirements. Do not claim runtime resolution yet.
5. Only after narrative, manifest and required production deliverables are
   stable, create the registered story worktree through `ParallelWorkDetails.md`
   and invoke `$somegame-create-unity-project`. Static scaffold readiness is the
   required output; Unity remains closed.
6. Reinvoke each asset owner for project-bound import/integration. Map every
   approved manifest row to its exact Unity file, `.meta`, import settings,
   selector and runtime address. Build story-local Bubble prefabs at this stage.
   Reject unmapped or unapproved files; do not regenerate accepted art merely
   because the project was created later.
   For catalog episode art, hand off approved PNG/JPEG files in
   `Config/EpisodeCovers/` and the corresponding `_catalogCover` file name in
   each definition episode; no Sprite/bundle reference is needed. Require
   acceptance to check both an episode-specific image and the story fallback
   when both states are present. Covers must remain available in the initial
   lightweight catalog phase, before story payload downloads complete.
7. Invoke `$somegame-author-story-content` with the approved narrative,
   character and remaining-art handoffs; require `ready-for-final-validation`.
8. Create the story-owned website reading preview described in
   [web preview and publication](references/web-preview-publication.md). Every
   new story carries `Config/Preview/preview.json` and only the character images
   it actually uses. The excerpt must come from canonical Ink and must be ready
   before release handoff; generated runtime `catalog-preview.json` is a
   different artifact and does not satisfy this requirement.
9. Commit the clean story-local candidate and record it through the worktree
   lifecycle. When all stages are ready, follow `UnityConcurrency.md` to obtain
   the separate current authorization for the final slot, then invoke
   `$somegame-accept-story`. Acceptance exclusively owns catalog registration,
   runtime/manual gates and the final readiness decision.
10. If the request includes deployment, proceed only after acceptance and
    integration through the release handoff in
    [web preview and publication](references/web-preview-publication.md), then
    invoke `$somegame-release-app`. Story creation does not itself grant server,
    APK, source-publication or production-channel authority.

Do not generate speculative assets merely to fill a fixed matrix. Create only
backgrounds, outfits, expressions, poses, media, and branches that the current
story uses or that the author explicitly requested.

## Validate and hand off

Before acceptance, report `ready-for-final-validation`, never accepted or
runtime-validated. Candidate commit, integration and worktree removal follow
`ParallelWorkDetails.md` and `IntegrationProtocol.md`; this skill does not own
those mechanics.

The handoff must say whether the story-owned website preview is complete and
whether deployment was requested. A missing preview blocks release readiness,
but does not imply that acceptance or publication happened. When deployment is
requested, identify the intended app profile, channel and exact ordered story
set; leave build, remote mutation and public verification to
`$somegame-release-app`.

Report the branch, project and catalog paths, authored assets, factual or
creative assumptions, validation evidence, manual visual checks, unresolved
risks, and whether the branch is ready for separately authorized integration.
