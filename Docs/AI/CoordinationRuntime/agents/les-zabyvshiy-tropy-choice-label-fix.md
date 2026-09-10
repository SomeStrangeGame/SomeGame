# Agent: `les-zabyvshiy-tropy-choice-label-fix`

- Status: paused-for-external-integration
- Task: Diagnose and repair invisible labels in story-local Choice presentation
- Scope: Projects/novels-les-zabyvshiy-tropy/Assets/Choices/*.png.meta; Projects/novels-les-zabyvshiy-tropy/Art/check_source.py; Projects/novels-les-zabyvshiy-tropy/Art/Acceptance/**; Projects/novels-les-zabyvshiy-tropy/Art/ACCEPTANCE_EVIDENCE.md; Projects/novels-les-zabyvshiy-tropy/archive/**
- Base commit: `eb7655d32ab6765bd96fcabb5a728a9a72ce407c`.
- Requested UTC: `2026-09-10T10:12:59Z`.

- Confirmed root cause: all 12 choice PNGs were imported as ordinary Texture assets, so runtime `LoadAssetAsync<Sprite>` returned null while the story-local image-led prefab hid label objects.
- Changed: all 12 `Assets/Choices/*.png.meta` now use single-Sprite UI import, mipmaps off and alpha transparency; `Art/check_source.py` enforces these invariants.
- Validation: static audit passes 72 routes, 12 choice icons and all importer invariants; scoped diff check passes.
- Pending: Unity import/content build and a fresh Android Embedded APK/runtime replay. Do not build while foreign catalog changes are dirty because that would contaminate the candidate.
