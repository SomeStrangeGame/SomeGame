# First Snow — editorial validation, revision 5

Date: 2026-09-11. Supersedes the original static validation report, whose
three-ending reachability and 60-minute claims were not demonstrated.

Source SHA-256: `c7006807538f19f0f9b3b218914a5ce3276997918dc685d8c8165f1cd8739d72`.
Definition SHA-256: `152add48e5a8464d39678f63d86cabbb5d6aa311138d60ec0cda918934210c19`.
Checker SHA-256: `2b8750e07f7f34625309f2d73086156142f10cff128fb99d75563f498698f6fb`.

## Approved episode-cover addition

The author-approved `s01e01.png` is stored in `Config/EpisodeCovers` and bound
to episode s01e01. Its source audit and three negative probes pass; the exact
PNG matches the approved generation hash. See `episode-cover-handoff.md`.
The 704-route editorial checker was rerun and its JSON still matches the saved
evidence exactly: Ink source and branch semantics did not change.
`verify-episode-cover.cjs` confirms the refreshed story branch and canonical
main SDK both support the field. This report is not evidence of cover
export, download, UI crop or display. Definition hash above includes the binding.

## Revision 5 skill-adoption checks

The exhaustive traversal now accepts and audits canonical `Уведомление`
commands. It requires the branch-specific contact-sheet outcome, both scheduled
call notifications, reader-orientation lines, one occurrence of the 43-minute
call statement and ending-specific aftermath. `star_read` remains the historical
choice at the stop; new `star_opened` records evolving knowledge and must be true
on every completed route. The four notification texts occur only after their
supporting events, and the contact-sheet notification is confined to the branch
that earned it.

`verify-pre-unity.cjs` separately checks schema-1 website preview JSON, stable
IDs, safe relative character paths, exact file coverage, PNG signatures, source
order and speaker correspondence. It also validates dated archive filenames,
the latest manifest references and every recorded SHA-256. Website rendering,
Unity and publication are explicitly outside that check.

## Executed checks

Standalone `inkjs@2.3.2` compiled the complete episode in memory: no compiler
errors or warnings. The test replays every available choice recursively from
saved Ink states, using the actual authored conditions rather than a separate
model of the scoring rules. Every path must reach exactly one ending and the
episode completion line. Runtime errors and warnings fail the test.

704 complete paths passed:

| Ending | Paths |
| --- | ---: |
| Первый снег | 288 |
| Четыре воскресенья | 320 |
| Нерезкий, но наш | 96 |

These are exhaustive path counts, not player probabilities. There are nine
mandatory decision points and one conditional confession choice. The checker
also asserts branch-sensitive festival dialogue, permitted film capture,
separate film/digital-print sequences, the return visit for the contest photo,
the retained paper star, and removal of the future-memory/class-duration errors.
The earlier file-existence audit missed actual loader requirements. Revision 3
requires initial clothes for all three speakers, maps the declared protagonist
to `maincharacter`, follows definition art aliases and checks inherited outfits
on every emitted dialogue along every route. Nine locations and eight character
variants resolve to PNG files with valid signatures and non-zero dimensions.
It also checks the root include, authoring GUID, card ID and cover existence.
This is a bounded audit of the current story syntax against the checked-in
loader contract, not a replacement for the Unity parser or import pipeline.

Regression assertions cover the box conversation after each scene-5 choice,
destruction of disallowed contact-sheet prints, portrait restoration and consent
before teacher review, and the explicit promise about the unread reply.
Five in-memory mutation probes were rejected as expected: missing initial
clothes, missing protagonist alias, nonexistent location, inverted scene-5
consequence and a missing shared photo-shoot event. No source files were changed by those probes.
Revision 4 additionally requires Sonya's sneeze once on each photo branch and
before the later anecdote, redraws the erased constellation before extending it,
and removes the mistaken reference to a previous screen photograph.

Reproduction (install inkjs 2.3.2 outside the repository, without lifecycle
scripts; pass its actual absolute package path):

```sh
node Docs/Evidence/verify-story.cjs /path/to/inkjs Assets/Ink/s01e01.ink
```

Run from this story project root. `../Evidence/editorial-validation.json`
contains machine-readable counts, exact source hash and a witness path for
each ending. No compiled Unity payload was written by this checker.

## Duration

One route contains 6,654–7,197 displayed prose/dialogue words and 643–690
dialogue/narration lines. Counts exclude metadata, speaker labels, unvisited
alternatives and choice labels. Tokens are Unicode letter/number words; they
are not the previous whitespace count of the whole source file.

At an assumed 130–150 words/minute, text alone takes about 44–56 minutes.
Allowing an illustrative 8–12 minutes for interaction and pauses gives roughly
52–68 minutes. These speeds and overheads are planning assumptions, not measured
facts. No mandatory waits were inserted to force a duration. Actual playtime
must be timed in the real presentation; fast readers can finish sooner.

## Scope and remaining evidence

Twelve primary scene IDs and existing option IDs are preserved; one explicit
`confession_join` knot fixes nested-choice continuation. The original source
also had malformed multiline condition headers, now corrected.

Pre-Unity status is `ready-for-final-validation`. This report proves standalone
Ink compilation, traversal and source-contract addressing checks, not Unity SDK
compatibility, asset import, visual composition, canonical compiled JSON/source
map freshness, Player build or catalog acceptance. Existing images were retained;
storytelling and portrayal still need an in-game visual review. Use a fresh
playthrough: this unreleased candidate introduces state variables and does not
claim migration of old in-progress test saves.

Current archive handoff is `archive/2026-09-11_manifest_v002.json`; its
predecessor and all retained artifact hashes are checked by
`Docs/Evidence/verify-pre-unity.cjs`.
