# Story acceptance checklist

Read this reference before acceptance planning and again before handoff. Use
current repository documents and `Tools/somegame` output for exact commands.

## Repository and scope

- Work is in the canonical SomeGame checkout, not a separate worktree.
- The story branch and base are identified and safe.
- Only the exact story project, intended catalog entry, and declared supporting
  files are in scope.
- Foreign dirty changes and active ownership were not absorbed or modified.

## Narrative and factual integrity

- The author supplied the genre and the implementation preserves it.
- Factual basis is recorded separately.
- Choices, branches, endings, and state transitions are reachable and coherent.
- Real-world claims and material reconstructions have the required evidence.
- Text and visuals do not present speculation as documented fact.

## Project, content, and art

- The atomic project follows the current template, content, and MCP contracts.
- Card, episode metadata, Ink, compiled story, source map, selectors, and all
  referenced assets exist.
- Each character preserves an approved identity master and every used outfit,
  condition, emotion, and special pose resolves at the intended scene.
- Backgrounds, gaze, composition, alpha, scale, continuity, and media pass the
  required manual review.
- No accidental bulk emotion/outfit matrix or unexplained production asset was
  added.

## Validation and handoff

- Run static checks before only the gates required by the changed-path plan.
- Validate the story, build required editor content, and validate/build the
  catalog when it changed.
- Distinguish automated, runtime, platform, and manual visual evidence.
- Review the scoped diff and report assumptions, reconstructions, skipped
  gates, warnings, and unresolved risks.
- Do not merge, publish, or delete the story branch without explicit authority.
