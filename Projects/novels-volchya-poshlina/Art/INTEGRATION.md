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
was completed as a separate subsequent story-local change; its evidence and
remaining visual gate are in [EPISODE_COVER.md](EPISODE_COVER.md).
The historical worktree registry
base remains unchanged; candidate generation against that pre-main base must
not be misreported as a valid story-only diff after the merge. For subsequent
integration, compare this branch against the main SHA above and reconcile the
candidate lifecycle in the integration owner phase.

## Remote refresh and skill review — 2026-09-10

Scope: update this isolated story branch from fetched `origin/main` and inspect
the creation workflow, not rewrite the story, publish other tasks' changes or
run Unity. Incoming main: `b6896bb7b188dc2f6350077ef18b70c80fbcd165`;
previous story HEAD: `678034ec013ba6d018dc678c7026287e884642fd`.

The merge has no conflicts. Its only changes to pre-existing story files are
the upstream series/catalog label `Ночелесье` → `Кострома` in Ink and README.
The expanded cast, twelve locations, four endings and distinct episode cover
remain intact. Shared files match the fetched main; the candidate difference
from it remains wholly under this story prefix.

Fresh standalone audit: 26,244 routes, zero compiler errors/warnings, unchanged
ending counts and 2,846–3,693 displayed words per route. New source SHA-256:
`4d783fef34f38a53729865f100b9a1b14741b5c4a036d7c5905c58b7783dd488`.
Earlier source hashes in the historical evidence describe their own checkpoints;
this update changes only the series label, not story prose or transitions.
Episode-cover audit passes with the same approved image hash. The changed
story/skill paths pass whitespace checks relative to the preceding story HEAD.
The wider diff against main includes existing whitespace in old Unity `.meta`
files; no repository-wide whitespace-cleanliness claim is made.

Reviewed the full published creation skill and its website-preview reference.
Its new pre-release requirement is `Config/Preview/preview.json`, a faithful
linear excerpt from canonical Ink plus only referenced character images.
This story does not yet have that preview. It is distinct from the episode
cover and generated runtime `catalog-preview.json`.

Also inspected, but did not copy or commit, the canonical checkout's unpublished
creation/design skill edits and `references/story-archive.md`. Those add dated,
non-overwriting creation archives with verified provenance and explicit gaps;
reader orientation, motives and supernatural-rule explanations; branch-earned
notifications; and a default proposal of 15–20 minutes per episode, subordinate
to the author's agreed scope. They do not mandate a fixed episode count.
Skill-contract checks and local links in the creation skill/references and
story-design reference pass for both inspected versions. This is not a fresh
literary review of every story scene. Historical archive migration, new prose,
notifications and preview authoring are not performed by this inspection.

Coordination incident: another task acquired the integration resource between
the initial free-status check and acquisition. The merge preparation mistakenly
ran after the acquisition refusal. It affected only this isolated worktree;
no commit followed until the resource became free and was acquired by
`volchya-skill-refresh`. The shared checkout and other owner's files were not
modified. Subsequent write operations use the acquired resource.

Pending: preview authoring; separate review/adoption of the unpublished archive
and scenario guidance; the existing worktree-registration baseline issue; and
separately authorized runtime/acceptance gates. No Unity, website deployment,
source publication or acceptance is claimed. Suggested next step: agree which
local skill revision to adopt, then prepare the story-owned reading preview.

## Follow-up main refresh — 2026-09-10

The author requested another main refresh and enabled auto-approve. Fetched
`origin/main` is `3e449934bf86ab84ed08853d34845b30f7eacf25`; preceding story
HEAD is `85a5e2cabfc7ba35de57e1a3877dfb702ffd3501`. The previously local-only
archive and scenario skill rules are now published and arrive unchanged from
main. The unpublished-rule distinction in the earlier checkpoint is historical.

Waited for the shared integration resource, acquired it as
`volchya-main-refresh-auto`, then prepared the merge without conflicts. Existing
Ink, definition and episode-cover bytes exactly match the preceding story HEAD;
only upstream `Assets/Presentation/bubble.meta` is added to the story project.
Its GUID has one declaration in this story. The cover audit and scoped
story/skill whitespace checks pass; the difference against fetched main remains
story-local. No fresh route traversal is claimed: the exact Ink bytes retain
the prior successful 26,244-route evidence and source hash recorded above.

Auto-approve is recorded for reversible decisions within the authorized task;
this refresh does not implement the proposed literary expansion or select a new
episode count. Story rewriting, archive population and preview authoring remain
future work. The earlier registration-baseline issue also remains unresolved.
No Unity, acceptance or publication was requested or run in this update.
