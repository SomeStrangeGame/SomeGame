# Adult Bubble redesign — 2026-09-10 — v003

Story: `les-zabyvshiy-tropy`

The previously integrated blue-green fairytale Bubble was rejected by the author as childlike and therefore genre-inappropriate. Its mushroom, firefly, antler and animal-track ornament has been removed from production.

The replacement uses a restrained adult mystical-adventure language: near-black indigo stone/leather surfaces, weathered cartographic lines, roots, a compass mark and thin aged-brass edges. The dialogue panel retains a broad unobstructed center. The illustrated-choice frame retains a transparent center so the authored choice image remains primary.

The image generator returned checker-backed RGB candidates. They were not imported directly. `Art/BubbleRepair/process_generated_panels.py` deterministically removed only connected neutral checker regions and produced exact RGBA production sizes: dialogue `1200x800`, choice `512x768`. The rejected production assets and raw candidates are preserved under `archive/assets/`.

The author's second finding was also corrected across all five Bubble variants: dialogue width is now `344` instead of `360–380`, four standard backgrounds use `96x120` expansion instead of `80x80`, and the undersized named-dialogue background uses `64x104` instead of `32x32`. Existing vertical `ContentSizeFitter` behavior remains intact, so long text grows while retaining explicit inset from the surface edge.

Static result: story route audit passes all 72 paths; six episode images remain distinct; both Bubble sprites have exact RGBA headers; every dialogue variant passes the text-safe geometry assertions; scoped `git diff --check` passes.

Runtime status: all APK evidence predating this source revision is superseded for Bubble appearance and containment. A fresh Unity/content/player build and portrait Android replay are required before visual acceptance.
