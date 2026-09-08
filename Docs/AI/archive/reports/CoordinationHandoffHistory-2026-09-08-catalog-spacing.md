# Catalog spacing evidence

## 2026-09-08T08:25:23Z — catalog-header-spacing — ready-for-review

Changed fallback.prefab: vertical spacing 28 -> 8, story title top alignment,
header authored height synchronized to 68. No runtime changes or save edits.
Catalog content-gate 082251 and Editor compile 082309 passed; portrait inspected:
Novels/Assets/Build/Logs/catalog-header-spacing-20260908.png. Snapping passed;
card check passed after downloads completed (first probe ran too early).
Editor left playing at catalog start. No APK, commit or publication.

2026-09-08T08:32Z — `catalog-story-spacing`: fallback carousel bottom margin 32 plus outer gap 8 now separates stories by 40; CatalogScreen includes authored margin in adaptive row height, preserving episode 380x528 and header position. Catalog build 083018, compile 083042, live catalog/snapping passed; catalog-story-spacing-20260908.png inspected. Editor playing; no saves, Player, commits or publication.
