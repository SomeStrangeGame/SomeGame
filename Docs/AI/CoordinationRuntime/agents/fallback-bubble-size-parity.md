# Agent: `fallback-bubble-size-parity`

- Status: ready-for-manual-validation
- Task: вернуть fallback bubble геометрию базового/TZM prefab вместо растянутого runtime layout.
- Scope: `Novels/Assets/Novels/Fallbacks/EpisodeUI/bubble/screen-variant.prefab`, Novels Editor lifecycle, own coordination records and shared handoff.
- Contract: TZM/content/Ink/runtime не менять; убрать только экспериментальные layout overrides; существующий dirty tree сохранить.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: 2026-09-01T11:36:51Z
- Completed UTC: 2026-09-01T11:38:56Z
- Evidence: fallback overrides `_forceLayoutRebuildAfterContentChange=1` and five `LayoutElement.m_IgnoreLayout=0`; TZM leaves those values inherited, so its fixed geometry is replaced only in fallback.
- Result: removed the fallback-only forced layout rebuild and five `m_IgnoreLayout=0` overrides. The fallback variant now inherits the same fixed RectTransform/layout geometry as the base and TZM bubble instead of splitting content into stretched strips.
- Validation: scoped serialized diff and override audit passed; attached Novels compile passed with `compilerErrors: []`. Unity PID `59162` remains open; restart Play Mode before visual replay because the active instance was created from the previous prefab state.
