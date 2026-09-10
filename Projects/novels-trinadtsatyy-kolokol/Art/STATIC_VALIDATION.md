# Static validation handoff

Status: source audit and current full narrative/Ink originality passed;
**Unity/Player acceptance pending**. Changes are
limited to `Projects/novels-trinadtsatyy-kolokol/**`. No Unity, catalog, shared
SDK, content build or Player operation was run. This is not release acceptance.

## Current candidate preparation — 2026-09-09

Owner: current story task, `tk-source-candidate-20260909`.
Input HEAD: `1120f84256c5b2613392e2631c3a485fd01044ed`; registry base:
`d31d993572d30a1c38cbf1a8e1dde94eb10f1f5f`.
Scope: preserve and commit the already reviewed story-local changes under
`Projects/novels-trinadtsatyy-kolokol/` in its registered worktree, update this
handoff and README, then refresh the shared `story-candidate` manifest with
the clean resulting SHA. This continues the author's requested integration;
it does not authorize publication or a Unity/Android slot.

The canonical FIFO is now empty, but its checkout is still on
`codex/story-batch-chernaya-melnitsa` at `f1721a63e0462958912795316e63a184e21dd74e`
with foreign SDK, runtime, catalog and story changes. An empty queue does not
transfer ownership of that dirty tree. Do not copy this story's dirty files
there, switch its branch, stash its changes, or merge unrelated work.
The supported next integration input is a clean story commit and the refreshed
`.git/somegame-runtime/candidates/trinadtsatyy-kolokol.json` manifest.

Pre-commit checks passed: 86 source routes and seven negative self-tests; story-only diff;
unchanged narrative/Ink originality fingerprints; original master moves
preserve bytes; six episode-cover hashes match their approved manifest;
local Markdown links. No source text, images or Unity serialization are changed
by this preparation. Final runtime acceptance is still pending.

## Previous full review scope — 2026-09-08

Owner: current story task, `tk-originality-exception-20260908`.
Base: `1120f84256c5b2613392e2631c3a485fd01044ed`, registered story worktree,
exact owned prefix `Projects/novels-trinadtsatyy-kolokol/`.
The author explicitly authorized one additional complete review after the
five-iteration Ink limit. Scope: this handoff, `ORIGINALITY_EVIDENCE.md`,
`NARRATIVE_PACKAGE.md`, `APPROVED_ASSETS.md`, `EPISODE_COVERS.md`, `../README.md`.
Review covers the complete narrative and six Ink sources, with read-only local
comparison and compact external searches. No story/art rewrite, shared checkout,
Unity, catalog, Git commit or publication is included. Current active shared
owner is `app-branding-profiles`; its scope does not claim this story prefix.
Validation: source checker with negative self-tests, scoped diff and local links.
Status: full review passed; evidence and current gate references recorded.
Post-edit verification passed: 86 complete source routes, all seven negative
self-tests, scoped `git diff --check`, local Markdown links in all six documents.
Episode Ink hashes and all six episode-cover PNG hashes are unchanged.
Integration remains deferred while the canonical checkout has an active foreign
owner; no shared lock was acquired or foreign state modified by this review.

## Previous preparation scope — 2026-09-08

Owner: current story task, `tk-preacceptance-docs-20260908`.
Base: `1120f84256c5b2613392e2631c3a485fd01044ed` in the registered
`codex/story-trinadtsatyy-kolokol` worktree. Author requested continued work
with auto-approval. Scope: this handoff, `APPROVED_ASSETS.md` and `../README.md`.
These story-local documentation edits require no shared checkout lock; no
external owner claims this story prefix in the inspected coordination records.
The occupied canonical checkout and its foreign changes are outside this scope.

Corrected stale inventory: six episode covers now have a manifest entry and
discoverable handoff; the Bubble accent is static, not an implemented animation.
Images, Ink, definition and existing character/audio files remain unchanged.
Validation: scoped diff/Markdown-link checks and source regression check.
Historical status: documentation preparation complete; originality decision
was required at that point and is now resolved by the current review above.

## Reproduce the source check

From this story project, run `python3 Art/check_story.py --self-test`.

The dependency-free, read-only checker parses the limited Ink syntax actually
used here, walks source choices/conditions/assignments/diverts, and fails on
unsupported syntax. It is not an Ink compiler and does not emulate Unity's
presentation, save loading or Ink-to-episode export. Its output includes SHA-256
hashes of all six episode sources so the checked candidate is identifiable.

Current result:

- 32 routes through the five binary pre-final decisions;
- 86 complete routes: 22 «Город слышит», 32 «Тихая смена», 32 «Дежурная по нулю»;
- six decisions per complete route, all thirteen named options covered;
- all six episodes visited in order, each ending terminates at `END`;
- final `evidence` range 6–11;
- 3 388–3 518 visible words per route, using whitespace tokenization;
- 16 location IDs → 16 PNGs; 16 audio IDs → unique WAVs;
- 16 emitted character-state addresses resolve exactly, including inherited
  expressions; all six default `main.png` images exist.

Assertions reject absent Tim/Inga dialogue in episode 6, publication without
`evidence >= 8`, any extra Tim/Inga availability restriction, a stale Tim flag
after his return in the watch ending, and a final choice reaching the wrong
ending. These checks protect state-dependent dialogue, not arbitrary prose
meaning: the narrative audit below was performed separately by reading it.

Seven reproducible in-memory negative checks reject publication depending on
Tim or Inga, publication without enough evidence, absent Tim or Inga speaking,
a wrong final divert, and failure to record Tim's return. They never write
fixtures or mutate the story files. Publication has 18 saved-Tim and 4 missing-
Tim routes, and independently 8 present-Inga and 14 absent-Inga routes.

## Narrative corrections

- Established Roman's arrival, the archive checkout, Asya/Zoya accompanying
  the group and the rescuers' arrival. Removed mobile reception in the vault.
- Separated Inga's recent access signature from the seventeen-year-old order.
  Explained Levin's initial consent followed by forced confinement, and Zoya's
  preserved recording instead of simultaneous destruction and preservation.
- Showed the broadcast warning the contractor, and the quiet route preserving
  the service-channel recording. The choice has visible consequences.
- Made the map/key route visible; introduced Nina's map correction before it
  is used. Roman previously found a blocked entrance, not an already accessible
  zero room that would make the investigation redundant.
- Distinguished the 03:13 switch, recorded bell playback and 04:13 closure.
  Tim's cable route, disappearance, possible second rescue and dialogue agree.
- Saved Inga accompanies the party; otherwise rescuers keep her at the collapse.
  A lost journal stays lost; the backup is opened only when Inga is present.
- Extracted twelve victims and Levin before the final decision. Nina's fate
  and the remaining missing people are explicit in each ending. Her token is
  not simultaneously in Roman's possession and on her chain.
- Put the recorded future phrase before its first human utterance; disconnected
  recording inputs before naming people, distinguishing this test from answering
  a lure. The dispatcher ending explicitly accepts the cost of answering.
- Removed the silent failed-publication → destruction fallback. Publication is
  offered only when eligible, and both remaining options explain their costs.
  Public documents omit victims' names; full evidence goes to investigators.
- Preserved already-carried evidence after destruction. Removed unsupported
  one-route duration claims and synchronized the narrative/presentation docs.

## Follow-up continuity audit

The second read corrected only episode 4–6 prose; choice IDs, conditions,
assignments, media selectors and all ending routes remain unchanged:

- The missing ten archive reels are accounted for by the old destruction act.
- Lada's daytime drawing and the evening emergency now form one chronology.
- The key is used by its current holder and returned; the radio is explicitly
  collected and switched off before it appears among the carried evidence.
- Roman's roof hypothesis comes from the map, not an unsupported claim that
  Nina's voice was recorded before her disappearance.
- Quiet-radio players immediately send the rescue instruction to responders.
- Unknown rescued people have evacuation entries, not invented known names
  and case numbers. Closing false doors is not equated with rescuing everyone.
- Search gaps replace the claim that searches must have stopped before a
  disappearance, which contradicted Tim's route. Prediction versus suggestion
  stays unresolved: hearing a phrase and then repeating it proves neither.

At that earlier pass: 82 routes. The current counts above supersede that result.
No new art, metadata or Unity output was produced by this follow-up audit.

## Final wording check

The next full read found no new major branch contradiction. Three precision
edits clarify the existing premise/timeline: the opening recording is from
today, the bell (not the tower) was removed, 03:30 → 04:13 is 43 minutes, and
the 04:05 line refers to the next live cycle rather than denying the 03:13 one.
The synopsis now uses the same bell/tower distinction. No scene, dialogue,
choice, variable, condition or media command was added or removed.
At that earlier pass: 82 routes. The current counts above supersede that result.

## Author-requested final-choice correction

- Publication now depends on sufficient evidence only. Both shutdown methods
  close the remaining routes for Tim, Nina and every other missing person;
  publication preserves the archive but does not rescue missing Tim off-screen.
- The missing-Tim publication branch contains no Tim dialogue and explicitly
  leaves him missing. The watch ending records his actual return in `saved_tim`.
- Inga's password opens the contractor's backup journal, not the old system's
  manual breakers. The independent old recorder exports records/address links
  as each line stops; the evidence is needed to reconstruct the correct order.
  This also explains why copying everything first and smashing the membrane
  is not equivalent to the orderly archival shutdown.
- Final captions/warnings state the closed-route cost and the permanent watch.
  Named choice IDs, episode IDs and existing variables are preserved.
- Full source control checks and targeted final-branch reread found no further
  definite contradiction in this revision. This is not a guarantee of absence.

## Runtime address corrections

The local SDK's `ContentAddressConvention.CharacterWholeVariant` uses `main`
as its default, and `CharacterSpriteResolver` maps the main role to
`maincharacter`. Existing `neutral.png` masters were renamed to `main.png`;
Lada's directory moved from `лада` to `maincharacter`. Matching Ink selectors
and handoffs were updated. No pixels or imported asset GUIDs changed: these
PNGs have not yet been imported into Unity.

There are 20 whole-image PNGs including defaults. Eleven secondary emotional
variants from the preceding production pass have static review evidence at:

- `Assets/Evidence/characters/secondary-variants-contact-sheet.png`;
- `Assets/Evidence/characters/secondary-variants-alpha-light.png`;
- `Assets/Evidence/characters/secondary-variants-alpha-dark.png`.

Only the six approved visible roles are emitted. Off-screen guard, Levin,
unknown recorded man and child Lada use narration with quoted speech.

## Originality and stable IDs

Current narrative iteration 5 and Ink iteration 6 passed on 2026-09-08 after
the author explicitly authorized one additional full review beyond the Ink
limit. The historical counter is preserved, not reset. The complete current
package, six episodes and all endings were reviewed; the 60-file local literal
comparison found no eight-word matches. Meaningful genre overlaps and source
limitations are recorded separately in `ORIGINALITY_EVIDENCE.md`.
Character iteration 2 and the unchanged visual package are unaffected.

Story ID `trinadtsatyy-kolokol`, six episode IDs, knots and named choice IDs
remain stable. Conditions, captions and prose changed in this unreleased
candidate; existing test saves must be checked in the final slot.

## Deferred final slot

Prerequisites and current state:

1. Complete: current full narrative/Ink originality evidence is recorded after
   the explicitly authorized additional review. No further author decision
   is outstanding for this review or the prepared six-cover art package.
2. Prepare a reviewed clean story-only commit/candidate under the integration
   protocol, preserving all current dirty assets. Integrate with the current
   shared SDK in the canonical checkout only after the active owner/FIFO allows
   it. The 2026-09-09 continuation prepares the clean story-local commit and
   refreshes its candidate manifest; this is not catalog registration, merge,
   publication or runtime acceptance.
3. Obtain the separate current authorization for one final Unity/Android slot
   and the required shared resource locks. The core explicitly excludes
   general `auto-approve` from that authorization.

Inside that slot, after the prerequisites are met:

1. Import moved/new Assets PNGs and generate their `.meta` files. The six
   catalog covers in Config are ordinary external PNGs, not Unity Sprites.
2. Assign `_catalogCover` through Inspector by the six stable episode IDs
   listed in [EPISODE_COVERS.md](EPISODE_COVERS.md). They are not assigned yet.
3. Compile root `Assets/Ink/trinadtsatyy-kolokol.ink` (includes `s01e01.ink`
   through `s01e06.ink`), generating `.ink.json` and `.ink.json.source-map.json`.
4. Run story validation and the editor content build, including cross-episode
   visit-count conditions, guarded choices and episode-cover preview/export.
5. Acceptance owns catalog registration and preserves existing entries. This
   registration must precede the final APK/catalog-to-story acceptance run;
   final evidence cannot truthfully precede all catalog registration.
6. Build a fresh Android Embedded APK and record its hash, build time, package,
   release and exact emulator/API/serial. Replay through the real catalog,
   covering every episode, distinct choice branch and all three endings.
   Include missing-Tim publication, absent-Inga publication, Tim's return in
   the watch ending, both two-choice/three-choice final screens and save/resume.
7. Check character fallback, alpha edges, Bubble wrapping/tap areas, cover
   crop/fallback, audio loop seams/mix, safe areas and actual first-route time.
   Record the corresponding runtime markers and bounded visual evidence.
8. Record acceptance result; missing/stale Android evidence remains blocking.
   No publication is implied by these checks.
