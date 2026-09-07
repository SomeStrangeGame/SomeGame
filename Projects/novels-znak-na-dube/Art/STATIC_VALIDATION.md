# Static validation handoff

Status: **ready-for-final-validation** (not accepted, not runtime-validated).

Completed on 2026-09-06 and refreshed after the 2026-09-07 length extension, without launching Unity, content build, Player, ADB or emulator:

- exact story/card ID and required atomic scaffold paths checked;
- Ink structural audit: 13 knots, 15 inter-knot diverts plus terminal `END`, balanced conditional braces, 18 authored choice options across 8 meaningful decision points, 3 ending knots and one shared cliffhanger;
- source size: 5,006 whitespace-delimited words including Ink control text; 4,101 displayed words before mutually exclusive branch removal; manual shortest-route audit yields a conservative 3,500+ displayed-word route because all three extension scenes are mandatory and only choice/conditional alternatives are excluded; estimated 25–35 minutes at roughly 110–140 effective displayed words per minute including decisions and transitions;
- all 13 referenced location IDs resolve to production PNGs; the three extension backgrounds are 724×724 and were visually checked for composition, embedded text, logos and watermarks;
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
