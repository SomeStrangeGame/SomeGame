# First Snow — editorial handoff

Status: `ready-with-limitations`.

Integrated acceptance evidence is recorded in
`../Evidence/acceptance-validation.md`. The story is registered in the local
Catalog and story/Catalog builds plus a fresh Novels Editor compile pass. The
remaining limitations are the explicitly waived emulator/ADB gate and manual
visual acceptance through the real Player flow.

## Unity validation — 2026-09-11

The separately authorized final Unity slot passed for the atomic story project:
Unity `6000.3.11f1` imported the assets, generated the canonical Ink JSON and
source map, audited three content chunks and produced the editor release. The
post-build atomic validation also passed. Exact artifact sizes, release id,
diagnostics and remaining gates are recorded in
`../Evidence/unity-validation.md`.

The user explicitly waived emulator and ADB checks for this chat. This removes
those gates from the requested run but does not turn them into passed device
evidence. Catalog registration, Player/Play Mode and manual visual acceptance
were not performed. The five presentation inserts were bundled but still have
no authored runtime display commands/prefabs, so they must not be claimed as
visible.

Current archive manifest:
`archive/2026-09-11_manifest_v003.json`. Date naming, predecessor, referenced
files and SHA-256 values pass the static completeness check; known pre-adoption
history gaps remain explicit.

## Revision 5 — current story-skill adoption

The canonical opening now identifies the protagonist, school context, immediate
contest goal and Sonya's relationship without relying on production notes.
Scene 4 makes the fear behind deleting Miya's preferred portrait explicit and
retains a distinct emotional consequence on both branches. Four selective
notifications follow actual events. `star_read` preserves the historical stop
choice while `star_opened` tracks knowledge after the deferred first-call
reading. All three endings retain their thresholds but now show a different
concrete next action. The approved single approximately sixty-minute format is
unchanged.

The mandatory story-owned preview is complete at
`Config/Preview/preview.json`, with a spoiler-safe canonical opening excerpt and
only byte-identical approved school images for Lesha and Sonya. The dated archive
starts from surviving material on 2026-09-11; unavailable earlier process files
are recorded as gaps, not reconstructed. Updated full-text originality review
passes at low risk / medium confidence.

Static result: 704 routes, endings 288/320/96, zero Ink compile warnings/errors;
preview/source/archive checks pass. Unity import, canonical compiled JSON/source
map, browser rendering, catalog display, measured playtime and runtime visual
acceptance remain deferred. No catalog mutation, publication or Unity operation
was performed.

## Episode cover — author-approved addition

Added `Config/EpisodeCovers/s01e01.png` and `_catalogCover: s01e01.png` for the
single episode. Full provenance, generation prompt, bounded originality review
and the author's approval are in `episode-cover-handoff.md`; the production
manifest and art handoff reference it. The source cover audit and three mutation
probes pass. The existing 704-route editorial evidence remains unchanged and
was verified again; the definition hash in static-validation.md was refreshed.

The refreshed branch contains the current SDK episode-cover field. Do not claim
the cover is already visible in the catalog: export and display still require
the final Unity slot. No catalog registration, publication or Unity operation
was performed for this addition.

## Revision 4 — scene and branch continuity

Reviewed the complete current episode again. Fixed the opening's foreknowledge
of the falling box and clarified how it is caught; removed the attempt to sit
on the projector's occupied crate; made equipment return explicit; reconciled
the mixed contact sheet with its later remaining non-Miya frames; restored the
erased constellation before adding a point; marked the time jump to the last
concert announcement and the move behind the curtain; corrected the film/camera
pronoun and the first screen photograph incorrectly described as a reshoot;
finished print processing before the delayed call. Clarified the occupied-hands
passage at the stop.

Sonya's sneeze was previously authored only for lower_camera but recounted in
both branches. It now happens in their shared continuation; assertions check
its presence once and before the anecdote on every route. This is an event
placement fix, with unchanged decisions, variables and ending thresholds.

704 routes pass, with 288/320/96 endings and no Ink warnings or runtime errors.
Five negative probes pass, including removal of the shared sneeze. Evidence
and source/checker hashes were refreshed. Definition, asset files and the
remaining Unity/visual gates are unchanged; no commit was created.

## Revision 3 — repeated review and corrections

Base for this review: `46e064cef4aec919526ca8fa920989b94560c01d` plus the
five existing uncommitted pre-Unity audit/documentation changes, all preserved.
Scope remains `Projects/novels-first-snow/**`; no commit or candidate registry
update was requested. Current evidence describes working files, not the old
registered commit. Current source hash and definition/checker hashes are in
static-validation.md.

Fixed the scene-5 reference to an offer absent on one branch; Miya's knowledge of
the last film frame on a branch where she was never told; duplicate sitting and
the put-away phone in scene 7; ambiguous projector ventilation and next-day
timing; return of borrowed equipment and payoff of the backup lights; camera,
phone, baggage and star provenance; the missed Wednesday conversation; actual
restoration, teacher-review consent and paper selection before printing; removal
of disallowed contact-sheet prints as well as negatives; contest submission
before its deadline; the previously unspoken promise about the star; duplicated
fourth-call introduction and a grammatical mismatch in the final reply.

Found and fixed two story-definition blockers using existing data contracts:
`CharacterSpriteResolver` remaps Лёша to `maincharacter`, while
`CharacterSpriteSetLoader.LoadWholeVariant` requires non-empty initial clothes
before examining selectors. Three art aliases preserve the existing PNG paths;
three school-outfit defaults enable first appearance. Shared SDK files were
only inspected. Card/episode descriptions now match the five-day setup.

All 704 routes still pass with unchanged ending distribution (288/320/96).
The expanded checker validates inherited outfits and aliases along every route;
four negative probes confirm failures for the two definition defects, a missing
background and a wrong choice consequence. `doctor` and scoped diff checks pass.
The full revised candidate received a renewed originality review; see
content-originality-review.md. No new variables, choices or endings were added.

Remaining gates: Unity import/serialization and canonical Ink/source-map
generation, then visual/runtime acceptance and measured playtime. Existing
stills remain stylized character portraits, not proof of every narrated pose or
prop action. The five presentation inserts are source art without authored
display commands/prefabs in this candidate; they must not be claimed as visible
or visually accepted. No Unity operation was started.

## Revision 2 — retained editorial history

Task: user-authorized correction of the review findings and expansion toward
the requested hour-long first-love story. Owner: current primary story thread.
Base commit: `590283ba4c839422b32f5b3bc3101a56db5739a8`.
Scope: this story's Ink, narrative/production documentation and local evidence.

Corrected unreachable third ending, malformed conditional syntax and nested
choice continuation; removed impossible future knowledge; aligned five-day
calendar, class history, phone/film distinction, processing time, consent,
costume transitions, contest-photo provenance and star ownership. Prior
admission is remembered by subsequent arguments. Negative disposal is shown.

Added events with observable consequences: Sonya's rehearsal and stage debut,
Miya's return of a classmate's unfinished play, practical projector recovery,
handoff to younger volunteers, next-day photograph processing, a separate
contest shoot and chronological relationship calls. Reduced repeated
aphorisms and duplicated summaries. Boundaries remain fictional romance, 12+,
no erotic content, tragic death or love triangle.

Validation: standalone Ink compilation, all 704 routes and source-tree
location/whole-character selector resolution passed; see
static-validation.md and ../Evidence/editorial-validation.json. The previous
validation report has been superseded. Existing PNGs remain unchanged and
are not newly claimed to match every revised narrated action.

Pending: canonical Unity compilation/source map and visual acceptance,
plus measured playtime. No catalog publication or shared-runtime changes
were performed as part of the editorial rewrite.
