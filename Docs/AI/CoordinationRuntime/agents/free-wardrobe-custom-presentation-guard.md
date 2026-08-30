# Agent: free-wardrobe-custom-presentation-guard

- Status: waiting-user-validation
- Task: открывать свободный гардероб поверх любого сюжетного образа через отдельный нейтральный preview с восстановлением исходного кадра.
- Scope: `Novels/Assets/Novels/NovelRuntime.Presentation.cs`, `Novels/Assets/Novels/NovelRuntime.StoryQueue.cs`, `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterController.cs`, focused validation, own coordination files and shared handoff.
- Constraints: Ink, save format и scripted wardrobe не менять; preview API должен принимать target героя для будущего multi-character wardrobe; существующий чужой diff в scope не перезаписывать.
- Base commit: `4bfd64af41d3`
- Started UTC: 2026-08-30T14:30:05Z
- Heartbeat UTC: 2026-08-30T14:43:15Z
- Decision: первоначальный availability guard отменён по уточнению пользователя; свободный гардероб должен быть доступен на кастомных сюжетных образах.
- Result: free wardrobe открывает отдельный neutral `Wardrobe` render request для переданного character target, затем восстанавливает исходные story request, target, visibility и position. Main-character flow передаёт пустой target; API готов к будущему выбору других героев. Fresh Novels compile passed. Pending: visual open/close на кастомном образе.
