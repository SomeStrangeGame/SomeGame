# Agent: `camera-fade-auto-hide`

- Status: completed
- Task: сделать `Камера: затемнение` временным эффектом: fade-in 0.33 s, hold 1 s, fade-out 0.33 s.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Location/{CameraActionPlan.cs,LocationController.cs,View/LocationScreen.cs}`, точный выполненный TODO в `Projects/novels-tzm/Assets/Ink/s01e01.ink`, собственные coordination records и shared handoff.
- Contract: остальные camera actions, serialized prefab fields, public Ink syntax и generated Ink outputs не менять; cancellation эпизода должна прерывать весь эффект.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree and current TZM Ink edits.
- Requested UTC: 2026-09-01T14:13:40Z
- Result: `FadeIn` is a timed dark effect with 0.33 s fade-in, 1 s hold and
  0.33 s fade-out; cancellation resets the overlay; the exact completed Ink TODO
  was removed.
- Validation: scoped `git diff --check`, fresh Novels Editor compile and
  `Tools/novels-tools/novels-content validate tzm` passed.
