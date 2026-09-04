# Agent: `catalog-restart-popup`

- Status: completed
- Task: add an authored restart-story confirmation popup to the catalog.
- Scope: catalog/application/novel runtime and view code, catalog fallback prefab, children prefab variant, tests, own coordination records and handoff.
- Order: fallback behavior and neutral prefab first; child-only visual overrides second.
- Base: `0268b5f165db75294baadc57b72cbd6d0d3e353a` (`origin/main`).
- Requested UTC: `2026-09-04T15:50:18Z`.
- Scope extension: remove the intermediate episode-selection screen; continue resolves the latest unlocked episode, restart resolves the first episode after clearing only the selected story.
- Result: fallback owns the serialized secondary button and confirmation modal; children variant overrides only inherited visuals. Catalog selection now returns continue/restart intent, and NovelRuntime resolves first or latest unlocked episode without a second screen.
- Validation: fallback catalog editor build, children catalog Android build, Novels compile, Embedded children APK, catalog smoke and device interaction passed. Cancel kept the catalog open; confirm removed the selected story save and emitted `story.selected -> episode.ready(s01e01)` with no `episode.selected` event. Evidence: `Novels/Build/Logs/automation/catalog-restart-popup-child.png`.
