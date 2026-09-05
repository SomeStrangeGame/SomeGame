# Acceptance evidence

## Final artifact

- Story: `scp1198-silence` — «Тише, Нина».
- Final Android artifact: `Novels/Build/Players/scp1198-silence/Novels.apk`.
- Final artifact size: 2,248,256,132 bytes.
- Build mode: Android `Embedded`, test signing, generated 2026-09-04.

## Build and content gates

- `story-check --build --platform editor`: passed.
- Story `content-gate --platform android`: passed after the final Ink correction.
- Catalog `content-gate` passed for editor and Android with `scp1198-silence` registered as the fifteenth story.
- A fresh Embedded Android player build passed after final character-address and Ink corrections.

## Device smoke

Target: `Novels_Pixel_7_API_34`, serial `emulator-5554`.

The initial automatic smoke timed out at the non-interactive catalog because the runner does not select a card. It still confirmed `app.started`, `catalog.loading` and `catalog.ready`. The same installed artifact was then driven through the real catalog with ADB input.

Final clean run `93110907a53e4812875b93fb4227d765` confirmed:

- `catalog.ready` with `storyCount=15`;
- visible catalog card and cover for `scp1198-silence`;
- `story.selected`, `release.activated`, `episode.ready` and `dialogue.ready`;
- all six choice stages became interactable with counts `3`, `3`, `3`, `2`, `2` and `4`;
- Nina and Kirill rendered from story assets; no `fallback.used` event occurred in the clean run;
- one complete archive route selected the fourth final decision and emitted `episode.completed`, followed by `catalog.returned`;
- no `INITIALIZATION_FAILED` event occurred in the clean run.

Screenshots and transient logcat evidence were visually inspected during the run. Automation artifacts remain under the ignored `Novels/Build/Logs/automation` workspace.

## Reachability audit

The final Ink source has four explicit terminal knots:

- `witness_ending`, reachable through `submit_evidence` when `evidence >= 3`;
- `protocol_ending`, reachable either through `trust_doctor` or insufficient evidence;
- `chorus_ending`, reachable directly through `broadcast_file`;
- `silence_ending`, reachable directly through `destroy_source` and exercised on Android.

Each terminal knot contains the exact configured marker `...: КОНЕЦ СЕРИИ` and an explicit `-> END`. The three reconverging choice groups contain explicit gather markers, and every non-terminal route has an explicit divert to its successor.

## Defects found and closed

Runtime acceptance found and closed two issues before this final evidence was recorded:

1. Character directories used author-facing Latin names instead of the runtime addresses `maincharacter` and `кирилл`, causing a missing-character fallback. The directories and their `.meta` files were renamed together to preserve GUIDs.
2. Three Ink choice groups lacked explicit gather markers, causing `ran out of content` after selection. All three gathers were added, recompiled and exercised past their former failure points.

The final status recorded here applies only after both corrections and the subsequent rebuild.

## Character facing correction

Post-release art review on 2026-09-05 found that Nina's approved whole-image
master faced screen-left, contrary to the project composition rule for a main
character. The master was deterministically mirrored in place, preserving its
Unity `.meta` identity, 1024 × 1536 canvas and alpha channel. Final visual review
confirms Nina now faces screen-right. Kirill was inspected but not modified: as
a secondary character, he already faces screen-left.

## Emotion package

The 2026-09-05 expansion adds five authored emotions for Nina (`alarmed`,
`suspicious`, `overwhelmed`, `determined`, `exhausted`) and four for Kirill
(`concerned`, `wary`, `urgent`, `resolute`) in addition to each neutral master.
Every asset is a complete 1024 × 1536 RGBA figure. Nina remains screen-right;
Kirill remains screen-left. Identity, hair, outfit, pose, scale and framing were
compared on full-body and face contact sheets.

The first transparent-background pilots were rejected because they contained a
painted RGB checkerboard. Accepted variants were regenerated on uniform chroma
green, masked deterministically and subjected to green despill. Light and dark
proof sheets show clean silhouettes without the original green fringe.

Ink now explicitly selects character presentation at 25 story beats, including
24 uses of the new emotions. The master Ink
was compiled with the repository's own `Ink-Libraries.dll`; the compiled JSON
contains every selector. Unity imported all nine PNGs and generated their
`.meta` files, preserving the existing two masters. The exact-project Pipeline
compile gate passed without compiler errors after the conflicting Unity
Licensing Client processes were recovered. The final Android story content
build then passed with all emotion selectors and assets included.

## Wardrobe package

The 2026-09-05 wardrobe pass adds three whole-image patient states for Nina
(`main`, `alarmed`, `exhausted`) and four whole-image protective states for
Kirill (`main`, `concerned`, `resolute`, `urgent`). All are 1024 × 1536 RGBA.
Nina remains screen-right and Kirill remains screen-left. A generated Kirill
`protective/wary` draft was rejected before import because its gaze crossed
toward screen-right.

Ink now establishes both costume changes in the story: the archived first-cycle
recording and protocol ending show Nina in patient clothing, while Kirill dons
the protective coverall only after entering the exposed sector. Every added
selector resolves to a production file; no unused outfit/emotion matrix was
generated. Dark and light full-body proof sheets were visually reviewed for
identity, clothing continuity, direction, anatomy, feet and alpha edges.
Unity imported all seven PNGs and generated stable `.meta` files. The
exact-project editor compile gate, Atomic Content SDK validation and Android
story content build all passed. Compiled Ink contains both patient selectors
and all eight protective presentation uses; each referenced appearance resolves
to an approved 1024 × 1536 RGBA file.
