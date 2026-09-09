# Chyornaya Melnitsa source publication receipt

The canonical `Tools/somegame git-publish` completed on 2026-09-09 at 06:24 UTC.
Its verified local and origin/main SHA both equal
`a48a2bdaf30e08f9deb8b73936af45969866496e` (five commits published).
This receipt is a separate documentation-only follow-up to that verified push.

## Scope

- Remote base `fce268b19f4d00b064e16b700b7652dc78d1cc65` and its published
  episode-progress changes were preserved.
- Original story commits `8e7f2b6f`, `10281a7b`, `571aca29`, `f1721a63`
  were cherry-picked without conflicts, then current story-local source,
  character/presentation fixes, both corrected illustrations and evidence
  were committed as `a48a2bda`.
- The shared catalog retains zdm/tzm and registers chernaya-melnitsa.
  Single-story test configuration remains local, not a shared-catalog reset.
- No foreign runtime/channel-manifest/branding working changes were included.
- Generated compiled Ink JSON/source maps, APKs, content bundles and caches
  were excluded. Their recorded historical evidence is not a fresh gate.
- Publication used a user-authorized temporary clone with an independent
  index and refs. Primary checkout product files, branch and index were not changed.
- The old clean story worktree was removed on 2026-09-08 using the canonical
  runner after ancestry checks. Branch `codex/story-chernaya-melnitsa` and
  its commit `8e7f2b6f435439c5018ee6143b3af6f34ba9e3da` were retained.

## Fresh lightweight validation

- `Art/audit_story.py`: 72 routes, 3 endings, 5 choice groups, all 12 choices,
  13 locations, 8 audio selectors, 11 character variants, no unreachable dialogue.
- Acceptance-route planner: 2 tests passed; character edge/matte: 15 tests passed.
- Bubble audit: 338 strings and 360 route-choice contexts checked; scale 1.2,
  character viewport Y 150; conservative bottom clearance 66 logical pixels.
- Corrected illustration PNG/hash/format and unchanged meta hashes matched
  `Art/IllustrationRepair/source-check.json`; candidate JSON parsed successfully.
- Full published diff whitespace check and exact story/catalog scope check passed.

## Remaining limits

No Unity, content build, Player, APK or ADB command was run for this publication.
The user will build and test manually. Source publication is not final story
acceptance: the fresh illustration overlay/crop, R01 completion, R02–R05,
catalog completion/restart and full visual matrix remain unverified. Historical
v6 APK evidence predates both corrected illustrations; residual character-edge
limitations remain explicitly non-blocking rather than claimed fixed.
