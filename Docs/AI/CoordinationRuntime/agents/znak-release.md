# Agent: `znak-release`

- Status: completed
- Task: Integrate and formally accept `znak-na-dube` under the 2026-09-11 rules; no publication or push is authorized.
- Scope: `Projects/novels-znak-na-dube/**`; exact local acceptance artifacts and own coordination records. Catalog is read-only unless a later changed-path plan requires it.
- Integrated commits: `d86f2861` archive/current-story alignment; `fcc2bbb1` imported Unity metadata and compiled Ink.
- Current repair: Android runtime exposed baked checkerboard transparency and disconnected edge fragments in all three Lada variants. Character-production replaced only that background with deterministic alpha, retained identity and poses, updated the byte-identical preview derivative, and created archive checkpoint v007 (`3cda15f…`).
- Validation passed: static source model 372 complete routes / 3 endings / 18 choices / 0 unreachable lines; 7 unit tests; Android content gate `content-gate-20260911T090327Z.log` after the repair.
- Superseded APK: `Novels/Build/Players/znak-na-dube-acceptance-20260911/Novels.apk`, SHA-256 `928370a660bbb3f033e5a3dfa8b96ddab4f81ae930788b44bdbfa5f9eb4cfcc2`, 2,632,542,767 bytes. It proved real catalog-to-story activation and exposed the alpha defect, but is stale after the repair and is not acceptance evidence.
- Runner findings: `android-smoke` falsely reports `launch_failed` on this API-34 AVD because `monkey` exits non-zero for `SYS_KEYS`; exact `UnityPlayerGameActivity` launch reaches ordered `app.started`, `catalog.loading`, `catalog.ready`. First-launch fullscreen and notification system prompts must be cleared before telemetry polling.
- Queue recovery: the preceding `product-analytics-client` lock became stale after the Unity MCP process leak; with developer auto-approval it was paused and its files preserved. This request is now first and is corrected to local integration/acceptance scope only.
- Next: after FIFO acquisition, commit only the scoped Lada/docs/archive repair (exclude whitespace-only `ProjectSettings.asset` and foreign dirty files), build a fresh Embedded Kostroma APK from the new runtime HEAD and repaired story release, then replay the three-route/eight-choice/three-ending matrix on the task-owned AVD. Stop at local acceptance; do not push or publish.

- Completed UTC: `2026-09-11T09:47:14Z`.
- Validation: finish-task passed; logs: 1; pending: none.
