# gpl-setting-font-reference

- Status: `ready-for-integration`
- Base: `4bfd64af41d3`
- Scope: `Projects/novels-gpl/Assets/Presentation/setting/screen.prefab`; generated GPL local content; own coordination records and handoff.
- Task: fix invisible GPL setting-screen text caused by an invalid built-in font reference.
- Validation: GPL validate/build, fresh Novels compile, manual story-open gate.

- Result: both legacy `Text` fields now reference Unity's built-in font with
  `type: 0`; GPL release `69eb5ad758d671bb0674497136c888138cc4c8f03090b3dea226fec63cf755e4` built successfully.
- Evidence: the failed run reached `episode.selected` and waited for the setting
  selection with no runtime error; the transparent button and both text fields
  had unresolved `type: 3` built-in font references, explaining the gray-only UI.
- Pending: repeat the GPL card click and confirm visible title/button.
