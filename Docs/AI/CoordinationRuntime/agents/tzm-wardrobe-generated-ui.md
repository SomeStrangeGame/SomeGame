# Agent: tzm-wardrobe-generated-ui

- Status: ready-with-visual-gate-blocked
- Task: подключить утверждённый по референсу сгенерированный UI-kit к гардеробу TZM и убрать пустую белую область.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Projects/novels-tzm/Assets/Presentation/wardrobe/**`, `Projects/novels-tzm/Art/UI/wardrobe-ui-kit-source.png`, `Projects/novels-tzm/Assets/tzm.asset` при необходимости chunk assignment, focused validation/build, own coordination records and handoff.
- Contract: меняется только opt-in TZM reference layout; общий fallback и другие истории сохраняют текущий вид и поведение; Ink/save/wardrobe logic не меняются; Unity Personal-compatible assets/API only.
- Base commit: `8f89f27b0f01` plus preserved shared dirty tree.
- Requested UTC: `2026-08-31T17:19:32Z`.
- Acquired UTC: `2026-08-31T17:20:00Z`.
- Result: rejected whole-atlas runtime path removed. Approved reference art is retained as `Art/UI/wardrobe-ui-kit-source.png`; production UI now uses separate transparent/sliced panel, tab, primary/secondary button, character-arrow and four category-icon sprites. Visibility, active tab, labels, counts and cancel state remain dynamic.
- Validation: `novels-content validate tzm` passed; fresh TZM editor build passed and contains all nine separate sprites plus the prefab. The same shared SDK code compiled during atomic build. Fresh Novels editor-check was blocked before readiness by Unity Licensing `Access token is unavailable`; no compiler errors were reported, but Play visual acceptance remains pending.
- Next: restore Unity Hub sign-in/license, start Novels Editor and repeat the same wardrobe frame.
