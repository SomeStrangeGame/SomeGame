# Agent: `toybedtime-bubble-regression`

- Status: ready-with-limitations
- Task: устранить регрессию всех Bubble экранов `toybedtime`, появившуюся после story-local prefab variant.
- Scope: `Projects/novels-toybedtime/Assets/Presentation/**`, generated `toybedtime` release, own coordination records and existing `toybedtime-choice-icons` handoff line.
- Evidence: пользовательский portrait screenshot на `s01e01.ink:43` показывает сломанные фоновые слои и отсутствующий текст до первого иллюстрированного choice; единственное presentation-изменение обычных реплик — новый минимальный prefab variant.
- Hypothesis: raw YAML overrides variant не прошли Unity-authored serialization и затронули неверные inherited fileID/components; удаление variant должно вернуть проверенный shared fallback, не затрагивая optional choice icons.
- Base commit: `a9ff1e1344599ecc16ff4df11409f479e6603085` plus current uncommitted feature work.
- Validation: remove only broken local variant, `novels-content validate/build toybedtime editor`, release audit excludes local Bubble but retains definition/backgrounds/choice sprites, fresh Novels compile, user portrait replay.
- Requested UTC: `2026-09-03T14:05:00Z`.
- Result: удалён только broken story-local Bubble variant и его пустые каталоги; shared fallback и optional illustrated choices сохранены. Повторный build после освобождения Unity database прошёл.
- Validation result: `novels-content validate/build toybedtime editor` passed; release audit содержит 6 roots (definition, 3 backgrounds, `garage`, `blocks`) и не содержит `presentation/bubble`; fresh Novels compile passed without errors; нужен пользовательский portrait replay исходной реплики и первого choice.
- Completed UTC: `2026-09-03T14:11:00Z`.
