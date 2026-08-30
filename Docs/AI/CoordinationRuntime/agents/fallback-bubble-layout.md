# fallback-bubble-layout

- Status: `ready-for-integration`
- Base: `4bfd64af41d3`
- Scope: `Packages/NovelsContentSdk/BaseUI/Base/bubble/screen.prefab`; removal of `Projects/novels-gpl/Assets/Presentation/bubble/**`; GPL local content; own coordination records/handoff.
- Task: make fallback bubble choices flow below dynamic dialogue text and remove the accidental TZM bubble copy from GPL.
- Validation: GPL validate/build, fresh Novels compile, portrait three-choice fallback visual replay.

- Result: removed the accidental TZM bubble directory from GPL. In the shared
  base fallback prefab, the five dialogue description LayoutElements now
  participate in their parent VerticalLayoutGroups, so dynamic choices flow
  below text instead of overlapping it.
- Validation: GPL validate/build passed; release `7fe86a…e7283` contains no
  custom bubble; clean Novels restart/compile passed.
- Pending: replay the three-choice portrait frame and confirm layout visually.
