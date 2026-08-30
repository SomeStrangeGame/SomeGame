# Agent: free-wardrobe-effective-selection

- Status: waiting-user-validation
- Task: использовать эффективный текущий wardrobe-стиль как fallback начального индекса, если equipped-запись отсутствует.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterController.cs`, `Novels/Assets/Novels/NovelRuntime.StoryQueue.cs`, focused validation, own coordination files and shared handoff.
- Constraints: save имеет приоритет; открытие вкладки не меняет образ; defaults и future character target учитывать.
- Base commit: `4bfd64af41d3`
- Started UTC: 2026-08-30T15:04:00Z
- Heartbeat UTC: 2026-08-30T15:08:30Z
- Result: если save не содержит equipped item, free wardrobe initial index берётся из эффективного explicit/default значения CharacterController; для TZM hair это `Распущенные`, а не первый алфавитный `За плечами`. Save остаётся приоритетом, initial preview выключен. После двух transport failures финальный live Novels compile passed без errors. Pending: visual reopen hair tab.
