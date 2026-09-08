# Episode reading progress — publication candidate

## Scope and isolation

The user approved the episode-card reading bar, checked the launched catalog,
then explicitly authorized publishing only this change through a separate clean
copy, without Git worktrees or changes to the primary checkout. The publication
copy starts at main `f234c9a758a6afd025cb17e32fb2a0c62009bdbb`.

Twelve feature files are byte-identical to the tested primary sources:
EpisodeReadingProgress.cs and its meta; CatalogFlow; NovelRuntime and its
EpisodeComposition partial; SaveSystem; StoryProcessor.Entity; CatalogItem;
Catalog.View.Card; fallback.prefab; Catalog README; Architecture memory.
None depends on the four unpublished story commits: the tracked feature-file
bases are identical between primary HEAD and the main base.

The source checkout, its index/branch, registry, story assets, saves and Editor
are not changed by publication. The clean copy uses the primary shared
integration resource lock and the normal git-publish workflow. No Unity,
content build, Player or device operation runs in this copy. No story commits,
worktree contents, generated evidence or unrelated metadata churn are included.

## Behavior

The authored episode card shows Прочитано, percentage and a gold fill above its
primary action. Locked episodes hide the block; unread is 0%, completed is 100%.
Incomplete progress is explicitly approximate, based on saved decisions plus
one bounded hypothetical continuation, capped at 99%. Optional metadata is
bound to the content version and the SHA-256 of the actual persisted save.
Legacy or unavailable estimates show an em dash without deleting the save.
Main save envelopes, episode completion, unlocking and reset dependencies remain
unchanged. See the [catalog contract](../../../../Projects/novels-catalog/README.md#прогресс-чтения-эпизода).

## Evidence from the primary checkout

- Isolated Roslyn compilation passed for Novels.StoryProcessor, Novels.Save,
  Novels.Catalog and Novels using current first-party sources/Unity references.
- 28 managed fixture assertions passed for linear/branch decisions, boundaries,
  entry state, bounded loops, unknown estimates, sidecar version/hash/corruption,
  reset and non-mutating v2/v3 save compatibility. Temporary harness:
  `/private/tmp/somegame-episode-progress-Ts4FYa` (compile.py and probe.py).
- Prefab fileIDs/local references/bindings/fill/layout checks passed.
- Catalog-only Mac content-gate passed on 2026-09-08 at 15:35 UTC (18.5s).
- Fresh Novels Editor compilation passed (18.2s), no compiler/Console errors.
- Final Play Mode run `83b9bc0d672347048ada876096fd97bc` reached catalog.ready
  and catalog.download_ready. Catalog release:
  `4d59ce50785f55b221dfc2f29dca59a8090cc7637c4b1f635cba59acdf8efef2`.
- Actual portrait Game View capture showed Прочитано / 0% / track above Открыть
  without overlap. Unity 6000.3.11f1 was left open for the requested manual check.

Local evidence under the primary checkout: content-gate-20260908T153551Z.log,
editor-gate-20260908T153628Z.log and editor-gate-20260908T153621Z-editor.log in
Novels/Build/Logs/automation; portrait capture in
Novels/Assets/Build/Logs/catalog-reading-progress-portrait-20260908.png.

## Limits

The live launch used the other task's existing one-story Mac bundle, not a
rebuilt story or the main registry's zdm/tzm. Only shared source/prefab evidence
is carried forward; no full story acceptance is claimed. First Play Mode exited
without a runtime error; capture outside Play Mode then emitted a tooling error.
Second launch/capture succeeded; the final aggregate check was interrupted by
helper cleanup. Its success is not claimed. Final runtime log window had no
failure markers. Live saved-progress, pause/reload/reset and multi-episode
interaction remain unverified; Android/iOS builds are not part of this publish.

## Publication verification

Pre-commit check: all twelve transferred files match the tested primary bytes;
scoped whitespace check passed. Primary sources and registry are preserved.
Remote SHA confirmation is recorded after the canonical push.
