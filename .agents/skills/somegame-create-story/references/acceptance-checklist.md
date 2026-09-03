# Story acceptance checklist

Read this reference before implementation planning and again before handoff.
Use current repository documents and `Tools/somegame` output for exact commands.

## Repository and scope

- Work is in the canonical SomeGame checkout, not a separate worktree.
- The story branch was created safely from an up-to-date `main`.
- Only the story project, its intended catalog entry, and declared supporting
  files are in scope.
- Foreign dirty changes and active ownership were not absorbed or modified.

## Narrative and factual integrity

- The author supplied the genre; the implementation preserves it.
- Factual basis is recorded separately.
- Choices, branches, endings, and state transitions are reachable and coherent.
- Real-world claims and material reconstructions have the required evidence.
- Text does not present speculation as documented fact.

## Content

- The atomic project follows the current template and content contract.
- Card, episode metadata, Ink, selectors, and all referenced assets exist.
- Each character has a consistent identity master.
- Every used outfit and outfit condition is present.
- Every narrative emotion or special pose is switched in Ink and resolves.
- Backgrounds, character gaze, composition, alpha, scale, and continuity pass
  a manual visual review.
- No unused bulk emotion/outfit matrix was generated accidentally.

## Validation and handoff

- Run the cheapest static checks first, then only gates required by the current
  changed-path plan.
- Validate the story, compile Ink, build the required editor content, and
  validate the catalog when it changed.
- Review the scoped diff and record manual gates separately from automated
  success.
- Report assumptions, reconstructions, skipped gates, and unresolved risks.
- Do not merge, publish, or delete the story branch without explicit authority.
