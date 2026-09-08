# Handoff history — 2026-09-08 publication

## 2026-09-08T07:37:00Z — catalog-scroll-snap — ready-for-review

Added gesture-only CatalogScrollSnap to authored story/episode ScrollRects:
0.36s vertical / 0.22s horizontal, bounded velocity projection, wheel settling,
interrupt/cancel on new press, settings and inline reset. Last-card padding added.
Compile status completed without errors. Loaded zdm/tzm passed snapping, catalog,
settings and isolated hold regressions; original volume restored, saves untouched.
The first synthetic snap probe used screen pixels instead of canvas units; corrected
probe passed. No prefab/content changes: bundle rebuild unnecessary. No APK/tablet test.
Evidence: Novels/Assets/Build/Logs/catalog-snap-{final,second}-20260908.png inspected.
Editor left in Play Mode at catalog start. No commit, publish or scene save.
