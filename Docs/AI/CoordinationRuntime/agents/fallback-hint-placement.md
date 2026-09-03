# Agent: `fallback-hint-placement`

- Status: ready-with-limitations
- Task: prevent fallback Hint bubbles from covering character faces.
- Scope: `Packages/NovelsContentSdk/BaseUI/Base/bubble/screen.prefab`, `Projects/novels-tzm/Assets/Presentation/bubble/screen-variant.prefab`, narrow validation, own coordination records and handoff.
- Contract: fallback Hint moves below the face zone; TZM stays at its current authored position.
- Evidence: Sobibor `sobibor.ink:297` screenshot; fallback Hint root fileID `6710638279854274581` is anchored at Y `100`, while the historically aligned character face occupies the same band.
- Request: `20260903T101557Z-fallback-hint-placement`.
- Requested/Acquired UTC: `2026-09-03T10:15:57Z`
- Result: fallback Hint root moved from Y `100` to `-80`; TZM variant explicitly preserves Y `100` for the same root.
- Validation: scoped prefab `git diff --check` passed; fresh Unity 6000.3.11f1 Novels compile passed with zero compiler errors. First gate attempt hit the recurring MCP startup timeout; bounded retry succeeded.
- Limitation: user will visually replay Sobibor `sobibor.ink:297`.
