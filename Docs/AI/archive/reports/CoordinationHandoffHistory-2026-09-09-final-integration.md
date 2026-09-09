# Completed 2026-09-09 release and website records

## 2026-09-09T16:00:17Z — tk-catalog-restore — completed

Task: Restored ordered Kostroma dev channel with chernaya-melnitsa=1 and
trinadtsatyy-kolokol=1; public manifest and both story endpoints verified; APK
and story trees unchanged.
Changed: coordination agent/handoff and prior TK integration history.
Validation: finish-task passed (1 gate).
Pending / risks: none.

## Earlier web-preview and publication records

- `chernaya-melnitsa-web-preview`: added the canonical-Ink web preview; JSON,
  source and diff checks passed; Unity content build was deferred.
- `story-web-publication-skill`: required website reading previews for new
  stories and documented the safe Remote publication handoff; project docs and
  scoped diff checks passed.
- TK preview-layout and web-publication records were completed; the temporary
  single-story manifest was immediately corrected by `tk-catalog-restore`.

## Previous primary publication receipts

- Primary publication confirmed `origin/main` at
  `bb161ec48bdc2cafbaed310b469f13db2c23d0d7`, with worktrees explicitly
  excluded and existing device/manual gates deferred.
- `catalog-progress-publish` confirmed feature commit
  `dcb976a73ab946f8d413c836077cc1f8fd97a7c7` on `origin/main`; its detailed
  evidence remains in `CatalogEpisodeProgress-2026-09-08.md`.

## 2026-09-09T16:10:04Z — website-character-framing-v4 — completed

Task: Fixed top-safe framing for differently proportioned preview character
PNGs, staged offstage-right exit/entry, softened story crossfades and made the
final character marker reachable at scroll end. Production build, live release
20260909-14 HTTPS/JS checks and live Lada/Roman/Tim transitions passed.
Changed: Website/app/globals.css, Website/app/page.tsx.
Validation: finish-task passed (1 gate); Unity was not used.
Pending / risks: none.

## 2026-09-09T16:18:13Z — remote-story-release-automation — completed

Task: Incremental Remote story releases preserve existing channel stories,
require explicit replace mode and reuse compatible APKs by default.
Changed: release skill, novels-content channel tooling/tests and pipeline docs.
Validation: tooling tests and finish-task passed.
Pending / risks: none.

## 2026-09-09T16:23:29Z — website-reduced-motion-framing-v6 — completed

Task: Unified character positioning without story-specific CSS, retained the
right-side entrance and crossfade under prefers-reduced-motion, and published
release 20260909-17.
Changed: Website/app/globals.css.
Validation: finish-task passed (1 gate).
Pending / risks: none.
