# Integration checkpoint — 2026-09-08

Branch: `codex/story-volchya-poshlina`.
Story corrections were preserved in checkpoint `ee5c6226b4f3` before merging
`origin/main` at `f234c9a758a6afd025cb17e32fb2a0c62009bdbb`.
Both original story commits remain ancestors; no history was rewritten.

## Resolution policy

The main snapshot contains the earlier six-character/eight-location story.
The story branch contains the expanded eight-character/twelve-location version.
After the checkpoint there were 78 add/add conflicts:

- 73 files differed only in whitespace. This was checked bytewise after
  whitespace normalization before accepting the main versions. No GUID or
  non-whitespace setting changed in these files.
- Five files retain the reviewed story checkpoint: `APPROVED_ASSETS.md`,
  `ORIGINALITY_EVIDENCE.md`, `s01e01.ink`, `volchya-poshlina.asset`, and `README.md`.
  This preserves the corrected prose, four endings, Veya/Luka, twelve locations,
  content version 2 and the real reading-length statement.
- All common SDK, catalog, tooling and documentation changes come from main
  unchanged. The resulting difference from that main SHA is story-local only.

## Validation and handoff

No unmerged paths; staged and unstaged whitespace checks pass. Source Ink retains
SHA-256 `57d02cb8f410949afeb400be4dc8c201d2645c27302b06cae16a37ccfb56653b`.
The standalone audit passes all 26,244 routes before and after reconciliation,
with zero compiler errors/warnings and unchanged ending counts.

No Unity, content build, Player or publication was run. These checks do not
establish runtime readiness or old-save compatibility. Episode-cover authoring
is a separate subsequent story-local change. The historical worktree registry
base remains unchanged; candidate generation against that pre-main base must
not be misreported as a valid story-only diff after the merge. For subsequent
integration, compare this branch against the main SHA above and reconcile the
candidate lifecycle in the integration owner phase.
