# Agent: tzm-s01e01-smoke-fixes

- Status: completed
- Request: `20260902T091241Z-tzm-s01e01-smoke-fixes`
- Base commit: `69d77aa9c04d`
- Scope: diagnose and fix TZM s01e01 Sally character fallbacks using `/Users/iantonishin/Downloads/Визуал ТЗМ 2` as read-only source; fix end-of-episode portrait layout; update only exact TZM assets/Ink/presentation or minimal shared UI runtime proven necessary; generated validation/build evidence; own coordination records and handoff.
- Preserve: all unrelated dirty changes, especially GPL and active `mamkin-story`; do not modify the Downloads source folder; preserve existing Unity `.meta`/GUID when replacing assets.
- Validation: source/art/GUID/content-chunk audit, scoped diff check, `novels-content validate tzm`, Unity compile when runtime/UI changes require it, Android Embedded rebuild, full s01e01 emulator smoke with zero character fallback and visual end-screen check.
