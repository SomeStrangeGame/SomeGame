# Agent: single-category-wardrobe-tabs

- Status: ready-for-integration
- Task: скрыть лишние вкладки в одиночном сценарном гардеробе, сохранив multi-category и free wardrobe.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/WardrobeController.cs`, focused validation, own coordination files and shared handoff.
- Constraints: не менять Ink, project prefabs и существующую логику multi-category/free wardrobe; сохранить чужой dirty diff файла.
- Base commit: `4bfd64af`
- Started UTC: 2026-08-30T17:01:33Z
- Heartbeat UTC: 2026-08-30T17:04:15Z
- Result: одиночный scripted wardrobe передаёт UI только текущую вкладку; multi-page и free wardrobe используют прежние списки категорий. Fresh Novels compile passed, Editor оставлен открытым.
