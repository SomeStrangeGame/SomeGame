# Kolodets publication acceptance — 2026-09-11

Status: `blocked`.

## Candidate

- Story: `kolodets-kotoryy-zovet`, content version `1`.
- Android story release: `a64bd55e15e034a468aaa4541fa639a2d8a63e7a8f909a53fbce2b9da7e0c1d4`.
- Catalog release: `b532649eed82dd06ec853d7fdfb60432808b6a933e3de145587c1006e8fb25ef` with seven stories.
- Embedded test APK: `Novels/Build/Players/kolodets-acceptance-20260911/Novels.apk`.
- APK size: `2667175086` bytes; SHA-256: `38266d8db323ce4b9ae589c85f0c4eebffdb236ef9b585184b09f8f081eff8f3`.
- Device: `emulator-5554`, `sdk_gphone64_arm64`, Android API 34.

## Passed evidence

- Fresh Android story content build passed after canonical Ink recompilation.
- Android catalog build passed with the story appended after the six existing entries.
- A first APK attempt exposed an incomplete generated release set: missing
  `volchya-poshlina/card.json`. The missing published dependency was rebuilt;
  no production source changed.
- The final APK built and installed successfully. Structured smoke reached
  `app.started -> catalog.loading -> catalog.ready` with seven stories.
- Real catalog interaction displayed the Kolodets card and episode cover, then
  emitted `story.selected -> release.activated -> episode.ready(s01e01) -> dialogue.ready`.
- The activated release ID is exactly the candidate release above. No
  `fallback.used`, `error`, fatal exception, ANR, initialization failure, or
  content-source failure was observed in the successful run.
- `catalog-card.png` and `episode-start.png` preserve the successful portrait
  catalog/story evidence. `runtime-logcat.txt` preserves the successful run.

## Blocking finding

At the first three-option choice in `s01e01`, the third option extends beyond
the lower portrait safe area. Its label and control are clipped at the bottom
of the 1080x2400 screen. See `choice-overflow.png`.

This is a required runtime/manual visual gate. The remaining choice, route,
ending, save/resume, and episode matrix was stopped after the first blocking
finding and is not claimed as complete. Acceptance must not repair the layout;
return the story-local Bubble/choice presentation to its production owner,
rebuild the candidate and rerun the complete Android matrix.

No channel tree, mutable manifest, website story tree, or APK was uploaded.
The public `kostroma-dev.json` remains unchanged and the expected Kolodets v1
URLs continue to return 404.
