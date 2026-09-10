# Agent: `kolokol-worktree-cleanup`

- Status: paused
- Task: Remove whitespace-only Kolokol noise, then safely retire integrated story-batch worktree and branch
- Scope: Projects/novels-trinadtsatyy-kolokol/**;worktree:/private/tmp/somegame-story-batch;branch:codex/story-batch
- Base commit: `08548a0bf2172ff0651837f08d34195c38642256`.
- Requested UTC: `2026-09-10T15:06:15Z`.
- Paused UTC: `2026-09-10T15:40:23Z`.
- Completed scope: restored all 37 whitespace-only changes under `Projects/novels-trinadtsatyy-kolokol` to `HEAD`; scoped status is clean and commit `56129279` remains contained in `main`.
- Blocker: `/private/tmp/somegame-story-batch` is fully integrated into `main`, but remains dirty only because it contains three untracked coordination records owned by `codex-kolodets-integration`. Project protocol forbids deleting another owner's records, and `story-worktree remove` requires a clean worktree.
- Next: the owning flow must explicitly retire its three stale records; then re-register a short cleanup request and remove the clean integrated worktree with `Tools/somegame story-worktree remove --confirm --integrated-ref main`, followed by safe deletion of `codex/story-batch` if the runner does not remove the branch.
