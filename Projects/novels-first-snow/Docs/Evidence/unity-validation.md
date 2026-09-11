# Unity validation — 2026-09-11

Target: `Projects/novels-first-snow` at branch `codex/story-first-snow`, after
merging local `main` commit `00516542` into the story branch.

The user explicitly authorized the final Unity validation slot on 2026-09-11
and explicitly waived emulator and ADB checks for this chat. The waiver is a
recorded limitation, not device evidence.

## Passed gates

- Unity version: `6000.3.11f1`.
- Licensing preflight: passed; no conflict markers. Other active Editors used
  different project paths and outputs.
- `Tools/somegame story-check --build --platform editor`: passed in 43.892 s.
- Atomic content configuration validation: passed again after the build in
  4.216 s.
- Canonical Ink payload generated at `Assets/Ink/first-snow.ink.json`
  (110,628 bytes).
- Canonical source map generated at
  `Assets/Ink/first-snow.ink.json.source-map.json` (140,187 bytes).
- Content bundle audits passed for all three delivery chunks: 11, 7 and 5 root
  assets respectively.
- Editor release generated with release id
  `e6a298b7bd7fde24123098ecb7e34d3c6632a3bcfec17560c256923cbe1fe0cd`.
  It contains the compiled story, source map, episode cover and three content
  bundles.
- Unity exited from batch mode successfully.

## Non-blocking diagnostics

The fresh Unity log contains the shared Ink integration's obsolete
`PlayerSettings` API warnings, certificate/Xcode discovery diagnostics and an
initial unavailable licensing access-token message. None stopped compilation,
Ink generation, bundle audit or release generation. No compile error, Ink
compiler timeout or `INITIALIZATION_FAILED` marker was produced.

## Explicitly not covered

- Android emulator, ADB smoke and device rendering: waived by the user for this
  chat and not run as acceptance gates.
- Catalog registration, Player build, in-game Play Mode and manual visual
  acceptance: not part of this atomic story build and remain separate
  integration gates.
- The five presentation insert PNGs are included in the content bundle, but the
  story still has no authored display commands/prefabs for them; inclusion is
  not evidence that they appear in runtime.
