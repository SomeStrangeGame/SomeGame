# Agent: free-wardrobe-equipped-index

- Status: waiting-user-validation
- Task: синхронизировать начальную подпись/индекс свободного гардероба с фактически надетой вещью без автоматического preview.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/WardrobePresentation.cs`, `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/WardrobeController.cs`, `Novels/Assets/Novels/NovelRuntime.StoryQueue.cs`, focused validation, own coordination files and shared handoff.
- Constraints: Ink и save format не менять; переключение вкладки не должно менять образ; порядок unlocked items сохранить.
- Base commit: `4bfd64af41d3`
- Started UTC: 2026-08-30T14:56:33Z
- Heartbeat UTC: 2026-08-30T15:02:45Z
- Result: free category получает equipped value из save и позиционирует compact carousel на соответствующем item id без initial preview; порядок вариантов и образ не меняются от одного открытия вкладки. Первый attach не дождался helper socket; один повторный compile passed без compiler errors. Pending: visual tab-open check.
