# Static validation handoff

Status: **ready-for-final-validation** (not accepted, not runtime-validated).

Completed on 2026-09-06 without launching Unity, content build, Player, ADB or emulator:

- exact story/card ID and required atomic scaffold paths checked;
- Ink structural audit: 10 knots, 12 inter-knot diverts, balanced conditional braces, 14 authored choice options across 6 meaningful decision points, 3 ending knots and one shared cliffhanger;
- prose size: 2,568 whitespace-delimited tokens/words; estimated 25–35 minutes at visual-novel reading/decision pace;
- all 10 referenced location IDs resolve to production PNGs;
- every used character/outfit/variant selector resolves to a whole-image PNG;
- all 5 referenced audio IDs resolve to 44.1 kHz stereo non-empty WAV files;
- character alpha audit confirms transparent pixels and opaque subject pixels in every runtime sprite;
- Bubble prefab GUID audit confirms unique local sprite GUIDs and an existing shared base-prefab GUID;
- card JSON parses; `storyId`, definition ID and project path agree;
- scoped `git diff --check` passed after staging.

Deferred by `UnityConcurrency.md` until a separate current human authorization for the single final slot:

- Unity import and generated `.meta` files for new assets;
- Ink compilation, compiled story JSON and source map;
- Official Unity MCP live/restart proof for unique server `unity_novels_znak_na_dube` (the Pipeline package is inherited from the maintained template; adding the user-level Codex server entry is a shared/out-of-story scope);
- `novels-content build znak-na-dube editor`, selector/bundle validation and catalog registration;
- story-local Bubble nine-slice, safe-area, contrast and long-copy manual review;
- full light/dark alpha, scale, gaze and composition contact-sheet review in runtime;
- fresh Android Embedded APK, catalog-to-story emulator replay of all semantically distinct paths and all three endings.

Catalog is intentionally unchanged. Acceptance/integration is therefore not claimed.
