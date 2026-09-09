# Remaining integration acceptance — 2026-09-08

Status: both illustration sources corrected; `ready-for-final-validation`.
Source follow-up: ill01 workshop and ill02 palm/flour corrected, source/meta/
originality gates passed; fresh runtime composition gates pending. See
IllustrationRepair/README.md. The existing v6 APK and
ACCEPTANCE_CANDIDATE.json are historical, not a current repaired-art candidate.
Historical runtime update: user «Давай проверим теперь» authorized the v6 run. R01 used
all5 planned choices and reached road title Ink473; cold Continue after C2/C3/C4
passed at137/293/333. Stopped before episode.completed: ill01 shows the old
crossing instead of the later Mitya/workshop scene; ill02 source has no hand/
close flour insert. R02–R05, completion/catalog reset and full visual matrix
remain untested. See ACCEPTANCE_EVIDENCE.md and ignored
Novels/Build/Logs/automation/chernaya-routes-20260908/REPORT.md/runtime-report.json.
After the now-completed source fix, obtain a fresh build/device slot; do not
continue acceptance of the old candidate as if those required images passed.

Historical preparation authorization:
User: «Это не страшно, продолжаем интеграцию» accepts residual v6 contours as
non-blocking, NOT fixed. Subsequent «Пока только подготовка без эмулятора» means
no Unity, APK rebuild, ADB, save mutation, commit or publication in this pass.

## Historical immutable v6 candidate (source art has since changed)

APK39049f824adf / releasefb7c51d4b322 is the existing catalog-tested candidate.
`ACCEPTANCE_CANDIDATE.json` pins full APK/source/compiled Ink/source-map hashes.
Static verification compares3bundle and10payload hashes with actual ZIP bytes,
checks11PNG/meta hashes and the one-story catalog. The four existing originality
results retain their scope/limitations; no new review or clearance was performed.

## Planned route coverage

`plan_acceptance_routes.py` selects5 redundancy-pruned runs from72 static routes:
12choices,35condition outcomes,365text/resource source lines,3endings. This is
branch/line coverage, NOT all72 runtime combinations or a proven minimum set.
Runtime fields in `ACCEPTANCE_ROUTES.json` remain `not-run`/empty by design: this
file is the reproducible static plan, not an execution report. Actual partial
run results are separate in runtime-report.json; expected states/lines are test
expectations, never observed runtime evidence.

| Run | C1 intention | C2 route | C3 sack | C4 burden | Ending |
| --- | --- | --- | --- | --- | --- |
| R01 | Find Mitya | Alone | Lada | Return all | Road |
| R02 | Sell house | With Yakov | Saveliy | Voluntary sharing | Keeper |
| R03 | Understand mill | Alone | Saveliy | Voluntary sharing | Silence |
| R04 | Sell house | With Yakov | Saveliy | Voluntary sharing | Road |
| R05 | Sell house | With Yakov | Saveliy | Return all | Road |

Choice blocks start at24,126,286,313,392. JSON contains exact stable IDs,
zero-based option indexes, pre-choice/final states and condition/line coverage;
do not infer button indexes from shortened table labels. R01 covers early
release, foreign re-exposure and required recovery; R04/R05 late-release variants.

## Replay checklist after production correction — requires fresh approval

1. Obtain explicit permission for emulator replay and test-story restart; take
   normal locks and confirm owned AVD/processes/current APK and release hashes.
   Corrected art and concurrent runtime/catalog source changes require a fresh
   APK; this old APK does not validate them. A rebuild requires separate approval.
   Build/catalog must remain only chernaya-melnitsa.
2. Preserve actual current save before replay. Never fabricate or patch saves
   to jump to a desired state; use real catalog restart for independent runs.
3. Traverse R01–R05 with real input; record runId, ordered smoke events, all5
   selected IDs, source-line observations and actual ending. Compare expected
   consequences/text; preserve evidence before stopping on crash/ANR/content
   error or any fallback.used. Static dictionaries are not actual game state.
4. In R01 pause/cold-relaunch after C2,C3,C4; Continue must restore dialogue,
   choice history and coercion/release/re-exposure consequence. Check episode
   completion, catalog progress and post-ending restart. All3 endings require
   actual completion markers and final-scene observations.
5. Across runs review all11character selectors,13scene/illustration selectors,
   transitions and8audio cues. Check long Bubble text, face separation, choice
   layout and pressed state. Accepted contours alone do not stop this check;
   report any other new functional/visual defect separately.
6. Save logs/targeted visual evidence, stop app, verify PID absent, leave AVD
   running. Preserve pre/post save copies; record only actual coverage.

Full acceptance stays blocked until runtime gates run. Physical ASTC quality/
performance, tablet matrix and measured reading duration remain unverified;
the contour exception does not waive them. Commit/merge/publication and restoring
a multi-story catalog are not authorized by this preparation request.

## Reproduce static preparation

From SomeGame with Pillow/numpy-capable Python:

```sh
python3 Projects/novels-chernaya-melnitsa/Art/plan_acceptance_routes.py
python3 Projects/novels-chernaya-melnitsa/Art/verify_acceptance_candidate.py
python3 -m unittest discover -s Projects/novels-chernaya-melnitsa/Art -p 'test_acceptance*.py'
python3 Projects/novels-chernaya-melnitsa/Art/audit_bubble_layout.py
Tools/novels-tools/novels-content doctor
```

Explicit `--output` regenerates only a test-plan/report JSON, never game state.
Two plan tests confirm deterministic coverage and no manufactured runtime pass.
