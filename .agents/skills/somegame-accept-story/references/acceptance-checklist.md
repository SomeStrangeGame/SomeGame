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

## Originality evidence

- Narrative design has a `passed` originality result with sources, findings,
  iterations, revisions, limitations, and final risk/confidence.
- Every character package has a separate `passed` visual-originality result.
- The non-character art manifest has a `passed` visual-originality result.
- The complete source Ink, not only its outline, has a `passed` text-originality
  result before final compilation.
- Licensed, public-domain, adapted, and homage material has explicit provenance
  and required attribution/rights handling.
- Any material post-review change returned to its owning skill for a fresh
  review; acceptance did not recreate or waive missing evidence.

## Validation and handoff

- Run static checks before only the gates required by the changed-path plan.
- Validate the story, build required editor content, and validate/build the
  catalog when it changed.
- Build a fresh Android Embedded APK from final content and record its path,
  timestamp, cryptographic hash, package name, and story/release identity.
- Record the exact emulator/device model, Android API level, and ADB serial;
  install that APK and launch it through the real catalog-to-story flow.
- Use the smallest replay/checkpoint set that covers every episode, every
  semantically distinct branch, and every reachable ending; record the tested
  paths, choices, and endings.
- Confirm ordered smoke/runtime markers, no crash or ANR, no unexpected error or
  `fallback.used`, correct selectors/assets, transitions, and save/resume state.
- Capture relevant logs and screenshots or equivalent observations for key
  visual scenes. Editor Play Mode is not runtime acceptance evidence; Unity
  Editor is only a technical build/compile mechanism here.
- Treat emulator evidence as stale after any material Ink, compiled content,
  asset, selector, catalog, or APK change. Missing, incomplete, or stale
  evidence makes acceptance `blocked`, never `ready-with-limitations`.
- Return defects to the owning production skill; acceptance does not fix or
  waive them.
- Distinguish automated, build, emulator-runtime, platform, and manual visual
  evidence.
- Review the scoped diff and report assumptions, reconstructions, skipped
  gates, warnings, and unresolved risks.
- Do not merge, publish, or delete the story branch without explicit authority.
