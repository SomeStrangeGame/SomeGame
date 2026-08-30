# Agent: wardrobe-choice-filter

- Status: waiting-user-validation
- Task: не сохранять обычные сюжетные item choices как wardrobe items и безопасно очистить старые ложные записи.
- Scope: `Novels/Assets/Novels/StoryExecution/ChoiceSelectionHandler.cs`, `Novels/Assets/Novels/Save/SaveSystem.cs`, `Novels/Assets/Novels/NovelRuntime.StoryQueue.cs`, focused validation, own coordination files and shared handoff.
- Constraints: Ink и save format не менять; настоящие wardrobe choices сохранять; старые записи удалять только после сверки с доступными assets категории.
- Base commit: `4bfd64af41d3`
- Started UTC: 2026-08-30T13:43:00Z
- Heartbeat UTC: 2026-08-30T13:49:00Z
- Result: unlock теперь требует явный `StoryChoiceAction`; ordinary item choices
  не записываются. Свободная категория после успешной загрузки asset-вариантов
  удаляет старые несовпадающие save entries. Scoped diff check и fresh Novels
  compile passed. Pending: user replay/open-close validation.
