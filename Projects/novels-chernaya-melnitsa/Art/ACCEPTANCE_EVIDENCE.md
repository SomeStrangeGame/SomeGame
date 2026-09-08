# Acceptance preparation

Status: `ready-for-final-validation` after the author-authorized additional
originality review. Static/story-logic checks are complete; this is not an
accepted or catalog-registered candidate. Unity still requires separate approval.

## Candidate identity

- Branch: `codex/story-chernaya-melnitsa`
- Base: `c5a431e30ff857a00e70fd50ed14ae0d559995ad`
- Scope: `Projects/novels-chernaya-melnitsa/**`
- Unity: `6000.3.11f1`; Official Unity Pipeline `0.5.0-exp.1` inherited from the
  maintained atomic template. Unique live server registration/restart proof is
  deferred with the final MCP slot; no external Codex config was mutated.

## Main refresh and episode cover — 2026-09-08

The registered branch was fast-forwarded from the original base above to
`f234c9a758a6afd025cb17e32fb2a0c62009bdbb` under the shared integration lock.
Immediately after refresh, HEAD matched fetched origin/main (ahead 0, behind 0).
All 115 original untracked files were verified byte-for-byte unchanged using
the same sorted path/SHA-256 manifest before and after. The lock was released.
No story commit was created or published, and the primary checkout was untouched.

The user requested an image for every episode. The sole episode `s01e01` now
assigns `s01e01.png` from `Config/EpisodeCovers/`, copied without pixel changes
from approved `bg11-mill-undercroft.png`. The original art evidence remains
applicable; the source and story cover are untouched. This is a source-image
review and static assignment, not a catalog visual pass.

Remaining gates include preview export/packaging, the assigned episode image
and story fallback, initial lightweight catalog delivery and actual card crop.
The worktree registry baseline was subsequently reconciled under the dedicated
FIFO/integration locks to the exact upstream SHA above. The original creation
base remains recorded under Candidate identity; no story commits were excluded:
HEAD equalled upstream before the first story-local checkpoint. Candidate
registration now compares only the story-local diff against this refreshed base.

Static verification passed: all 72 Ink routes, the sole episode's cover binding,
byte identity with approved source art, unchanged Ink hash, and content doctor.
The cover audit rejects missing, unsafe and duplicate assignments. No real Ink
compilation, catalog build or Unity/device check was executed.

## Final-slot preflight — 2026-09-08

The author authorized preparing the Git candidate and proceeding with final
Unity validation. Static audit still passes all 72 routes and the episode cover;
doctor passes and the reviewed Ink hash is unchanged. The checkpoint includes
only this story prefix, never generated build output or shared source changes.

The heavy slot has not started: an existing Novels Editor (observed PID 79728)
and Unity Hub (79793) require permission and an unsaved-state check before
closure. Separately, `codex-kolodets-integration` still declares active ownership
of `Projects/novels-catalog/Config/catalog.json`; registration must wait for an
explicit handoff/release, not a stale-age takeover. No existing process was
stopped and no catalog entry was changed. Acceptance is blocked until these
preconditions and the mandatory fresh APK/emulator/visual gates are satisfied.

## Earlier editorial corrections — 2026-09-07

- Fixed the disappearance chronology, prior unsuccessful searches, planted coat,
  intercepted later correspondence and the consent-based route to adult Mitya.
  His seventeen-year-old recollections no longer display his adult portrait.
- Restricted each character's knowledge to evidence already seen. The postal
  inventory proves interception but does not disclose the unread letter.
  The letter has identical wording wherever opened.
- Made travel, map/bottle/letter transfers, the exposed cart, delayed arrival on
  the solo route and the approaching ventilation deadline explicit.
- Distinguished genuine promises, private admissions and the elder's written
  intentions. Water contains dust but does not release promises. An absent
  addressee cannot be impersonated; factual testimony is not proxy consent.
- Moved public disclosure into the road ending. Silence now withholds the
  findings; the keeper perpetuates control by delaying genuine releases, not
  through an unexplained new power of the book.
- Earlier choices affect cooperation and physical consequences without hiding
  any of the three ending options behind undocumented stat thresholds.
- Corrected the runtime protagonist directory to `Assets/Characters/maincharacter`
  according to `CharacterSpriteResolver`/`CharacterAssetProfile`. Supporting
  speakers explicitly request existing `main` sprites, not absent variants.
  Identity masters remain under `Art/Characters/лада`.
- Updated narrative, asset and character handoffs to the same contract.

## Second continuity pass — 2026-09-07

- Found a branch-level contradiction the earlier terminal-state checks missed:
  reading Mitya's release did not stop his promise from coercing Lada later.
  C3 now clears that effect. C4 can expose her again, but if the letter was read,
  the loft/stairs/bridge explicitly show a different, foreign promise; the
  cancelled promise does not return. Dry-dust inhalation is now shown on C4.
- Fixed the relative dates: missed visit first, flood that night, letter next
  morning and disappearance by evening; the drawing is found the morning after.
- Accounted for the returned plaster cast, its continued presence in the satchel,
  the note/medallion transfer, possession of the book and map, the bag at the
  hopper, and the absence of any opportunity to make document copies at dawn.
- Aligned the teacher's and sawmill worker's promises across scenes. Removed
  the attribution of Yakov's broken road promise to Saveliy, Nastasya's
  contradictory assumption about burning letters, and Lada's mistaken claim
  that bringing Yakov would add a required witness to her old promise.
- Removed the unsupported guarantee that the damaged brake/wood would last
  until spring. The silence ending remains a risky temporary containment.

Genre, cast, choice IDs, three endings and approved media are unchanged.

## Third continuity pass — 2026-09-07

Re-read the complete episode and checked all branches. Four local wording
corrections align descriptions with facts already established elsewhere:

- map pins identify promises exploited by Saveliy, not promises all made to him;
  the teacher's addressees remain the parents and the carpenter's his neighbour;
- the note and attached postal medallion are both inside the sealed bottle;
- the miller's conversation with Mitya is placed before the boy's departure,
  without the ambiguous reference to the earlier flood night;
- the chest retains the old postal lock; it is not the replacement archive
  door lock that Nastasya's key cannot open.

No assignments, conditions, diverts, choice IDs, selectors or scene order were
changed. The existing originality result is retained for these non-material
copy edits with the explicit justification in `ORIGINALITY_EVIDENCE.md`.

## Current causal revision and recheck — 2026-09-07

The user explicitly requested repair of the seven-year contact gap and the
insufficiently motivated global mill shutdown. The revised source establishes:

- Lada originally called Mitya through the village office. Saveliy held her
  callback number, refused to give it to Mitya after departure, and falsely
  described her wishes. This obstructed initial calls and letters, not every
  conceivable contact method for seven years. Adult Mitya acknowledges later
  choosing avoidance out of anger, fear of rejection and accumulated shame.
- Personal release affects that promise, not every bag. The archive explains
  this through Marina's established case. Yakov describes earlier mechanical
  stops and the uninformed residents helping Saveliy restart the mill.
- Public disclosure does not magically stop the shaft. In road, Yakov closes
  feed, waits for residual material to be ground, then brakes the empty drive;
  residents subsequently dismantle feed and prevent unilateral restart.
- Silence uses the same safe stopping sequence but leaves intact feed, bags
  and keys with Saveliy. Old dust and possible restart remain the danger.
  Keeper continues operating the mechanism and withholding replies.
- A release letter clears its own coercion. Foreign C4 coercion instead needs
  time away from dust: the affected road paths include recovery at the river
  until the next morning, not an instant cure caused by public confession.

The full source was re-read for knowledge, time, prop custody, choice effects
and all three endings. No further serious contradiction was identified in this
pass; this is an editorial assessment, not a guarantee. Choice IDs, cast,
scene selectors and ending identities remain unchanged. The new causal detail
is material: prior originality clearance was not extended as another copy edit.
The author subsequently authorized one additional full-current-candidate
review; narrative iteration 5 and complete-Ink iteration 6 passed with the
limitations in `ORIGINALITY_EVIDENCE.md`. The source hash below is unchanged.

## Reproducible current static checks

Reviewed Ink SHA-256:
`23c5a5fc2ce1d22b0c2d8dac06c76b26da4a75addc6f071312577293ddb7021e`.

Run from the registered story worktree:

```sh
python3 Projects/novels-chernaya-melnitsa/Art/audit_story.py
Tools/novels-tools/novels-content doctor
```

`audit_story.py` is a dependency-free, read-only interpreter for the restricted
Ink syntax used here, not an Ink compiler or a Player test. Unsupported syntax
fails the audit. It traverses choice/conditional/divert paths and checks:

- 72 complete routes, five choice groups, 12 options, 24 routes per ending;
- five choices per route, no missing divert targets or unreachable dialogue;
- `public_truth` only on road, appropriate `letter_read` state in other endings,
  and the cleared flour effect in road;
- intermediate states before C3/C4/C5: C2 exposure, C3 release, C4 re-exposure,
  sharing state and absence of public disclosure before the final choice;
- the recovery scene occurs exactly on road paths with C3 early release and
  subsequent C4 foreign exposure;
- all 13 location/illustration selectors, eight audio selectors and 11 exact
  character sprite paths exist; no obsolete Lada runtime folder.

The final `novels-content doctor` returned `configuration is valid`. Scoped
text-input whitespace/conflict-marker checks also passed, including untracked
files that ordinary `git diff --check` would not inspect. Worktree status
contained only the authorized story prefix; no shared source files were edited.
Four in-memory negative cases were rejected by `check_route`: reactivating
the cancelled promise, dropping C4 exposure, moving public disclosure before
C5, and removing required recovery. They changed copies of route records only,
not the source or runtime state.

Conservative displayed-word estimates (dialogue plus selected option labels,
excluding resource commands, unchosen labels and unvisited branches):

| Ending | Minimum | Maximum |
| --- | ---: | ---: |
| road | 4,314 | 4,516 |
| silence | 3,875 | 4,034 |
| keeper | 3,842 | 4,001 |

The 25–45 minute duration is a design target, not a measured result. Player
timing, layout, pacing and Ink compilation remain pending. Previous word counts
and the claim that text volume proved duration are superseded by this audit.

Current similarity screening passed narrative iteration 5 and complete-source
Ink iteration 6. The sixth text review was explicitly authorized by the author
for this revision only; no general protocol limit was changed. See
`ORIGINALITY_EVIDENCE.md` for sources and limitations, including the existing
*Die schwarze Mühle*/Krabat and *Czarny Młyn* title associations.
No claim of unique title or exhaustive clearance is made.

## Preserved earlier source-asset evidence

These assets were not regenerated in the editorial review:

- Five character packages and the final non-character art/audio package have
  scoped originality evidence with recorded reverse-search limitations.
- `ill02-black-flour-hand` is linked from Ink, followed by an explicit return to
  `bg05-grinding-room`.
- Liberation Sans resolves the Bubble prefab font GUID and was previously
  byte-compared with maintained SCP/TZM copies.
- Earlier file checks found 49 valid PNG headers, 11 runtime character sprites
  at `768x1280` RGBA, and eight non-empty stereo 16-bit PCM WAVs at 44.1 kHz.
- Earlier source-image review covered the cover, key backgrounds, Lada's dark
  alpha proof and Bubble surface; it is not Player visual acceptance.
- The audio generator derives its output root from its story-local path.

The earlier claim that all protagonist selectors already resolved was wrong:
the `лада`/`maincharacter` mismatch was found and corrected in this review.
The earlier follow-up also added dialogue; its claim of unchanged source is
withdrawn. Historical passes do not replace the separately documented current
review of the material rewrite.

Unity imports, real Ink compilation/source map, content validation/build and
Player/manual evidence remain deferred to the authorized final slot below.

## Deferred final slot (mandatory)

The originality hold is resolved for the unchanged candidate hash above.
Obtain new explicit human authorization under `UnityConcurrency.md` and shared
Unity/resource locks. Run atomic import/Ink compilation and source-map,
story content validate/build, catalog registration/build, Official MCP live and
restart proof, fresh Android Embedded APK, catalog-to-story emulator replay of
all five semantic choices and three endings, save/resume, smoke markers and
fallback audit, plus the Bubble/character/manual visual state matrix.

Catalog path planned by acceptance: `Projects/novels-catalog/Config/catalog.json`.
No catalog, main branch, publication, commit or external MCP config mutation was
performed in this preparation stage.
