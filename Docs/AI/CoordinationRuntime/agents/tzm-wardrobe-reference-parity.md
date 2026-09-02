# Agent: `tzm-wardrobe-reference-parity`

- Status: ready-for-user-visual-check
- Task: привести TZM wardrobe variant к утверждённому референсу, сохранив наследование от authored fallback prefab.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Projects/novels-tzm/Assets/Presentation/wardrobe/screen-variant.prefab`, approved TZM wardrobe sprite assets and metadata, `Projects/novels-tzm/Assets/tzm.asset` only if new asset GUIDs require chunk registration, own coordination records and shared handoff.
- Contract: fallback geometry/behavior remains canonical; shared runtime additions are optional and preserve default visuals when TZM overrides are absent; do not unpack or duplicate the prefab variant; do not touch Ink or saves.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: `2026-09-01T14:41:39Z`
- Result: утверждённый softened sprite kit подключён к настоящему TZM prefab variant; shared fallback сохраняет собственный вид и содержит только dormant authored icon slots плюс opt-in theme contract.
- Inheritance: `screen-variant.prefab` по-прежнему ссылается на shared fallback GUID `c70fbd96d8d6443329e9d10a73f0428a`; root не распакован и не скопирован.
- Validation: TZM validate/editor build passed; composed release содержит variant и 17 production sprites; all variant dependencies resolved in `tzm.asset`; scoped diff-check and fresh attached Novels compile passed.
- Next: ручной portrait visual replay в уже открытом Unity; при расхождении править только TZM style/layout overrides или approved sprites, не дублировать fallback.
