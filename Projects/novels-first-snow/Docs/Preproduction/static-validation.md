# First Snow — editorial validation, revision 2

Date: 2026-09-07. Supersedes the original static validation report, whose
three-ending reachability and 60-minute claims were not demonstrated.

Source SHA-256: `f830ee4a271388a04e754dac1be97581358562e64da204a35b651bced1a2357b`.

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

Reproduction (install inkjs 2.3.2 outside the repository, without lifecycle
scripts; pass its actual absolute package path):

```sh
node Docs/Evidence/verify-story.cjs /path/to/inkjs Assets/Ink/s01e01.ink
```

Run from this story project root. `../Evidence/editorial-validation.json`
contains machine-readable counts, exact source hash and a witness path for
each ending. No compiled Unity payload was written by this checker.

## Duration

One route contains 6,256–6,764 displayed prose/dialogue words and 632–677
dialogue/narration lines. Counts exclude metadata, speaker labels, unvisited
alternatives and choice labels. Tokens are Unicode letter/number words; they
are not the previous whitespace count of the whole source file.

At an assumed 130–150 words/minute, text alone takes about 42–52 minutes.
Allowing an illustrative 8–12 minutes for interaction and pauses gives roughly
50–65 minutes. These speeds and overheads are planning assumptions, not measured
facts. No mandatory waits were inserted to force a duration. Actual playtime
must be timed in the real presentation; fast readers can finish sooner.

## Scope and remaining evidence

Twelve primary scene IDs and existing option IDs are preserved; one explicit
`confession_join` knot fixes nested-choice continuation. The original source
also had malformed multiline condition headers, now corrected.

This report proves standalone Ink compilation and traversal, not Unity SDK
compatibility, asset import, visual composition, canonical compiled JSON/source
map freshness, Player build or catalog acceptance. Existing images were retained;
storytelling and portrayal still need an in-game visual review. Use a fresh
playthrough: this unreleased candidate introduces state variables and does not
claim migration of old in-progress test saves.
