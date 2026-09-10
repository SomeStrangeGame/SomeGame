# Agent: `catalog-custom-reset`

- Status: ready-with-limitations
- Task: Remove all custom catalog variants and their build wiring while preserving fallback and story registry
- Scope: Projects/novels-catalog/Assets/RemoteAssets/catalog/children;Projects/novels-catalog/Assets/RemoteAssets/catalog/nochelessie;Projects/novels-catalog/Assets/RemoteAssets/catalog/scp;Projects/novels-catalog/README.md;Packages/NovelsContentSdk/Runtime/Catalog/CatalogAddresses.cs;Novels/Assets/Editor/PlayerBuildAutomation.cs;Novels/Tools/build-player.sh;Tools/somegame-tools/runner.py;Tools/somegame-tools/tests/test_runner.py;Docs/AI/plans/SlavicMysticismMvp.md
- Base commit: `c5a431e30ff857a00e70fd50ed14ae0d559995ad`.
- Requested UTC: `2026-09-07T15:13:18Z`.

- Completed UTC: `2026-09-07T15:16:20Z`.
- Result: removed all `children`, `nochelessie` and `scp` catalog variants,
  their selector/build wiring and the temporary Nochelessie Player identity;
  preserved `fallback.prefab`, story projects and `Config/catalog.json`.
- Validation: scoped `git diff --check`, shell syntax and all 37 runner tests
  passed. Catalog content build was not completed because the Catalog project
  is open in Unity; fallback redesign is intentionally the next separate task.
