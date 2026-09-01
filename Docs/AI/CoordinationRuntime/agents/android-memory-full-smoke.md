# Agent: android-memory-full-smoke

- Status: yielded
- Task: выполнить полный read-only memory profile текущего Android emulator smoke с управлением только существующим приложением.
- Scope: Android emulator `emulator-5554`, runtime smoke/memory evidence under ignored build logs, own coordination records and `HANDOFF.md`; tracked runtime/content files are not modified.
- Base commit: `8d3512787e6ee10bf2171edd9d21f5f0ca0b0b93`.
- Acceptance: record PSS/RSS/native/heap at stable checkpoints, peak during traversal, episode/story transition, return-to-catalog recovery, swap and memory warnings; distinguish emulator ASTC fallback from target-device behavior.
- Validation: ordered `[NOVELS_SMOKE]` telemetry, `adb shell dumpsys meminfo`, foreground activity and fresh logcat blocking markers.
- Started UTC: `2026-08-31T09:54:26Z`.
- Yielded UTC: `2026-08-31T09:58:20Z`; current APK is stale and cannot provide valid end-of-episode/recovery evidence. Observed stress baseline: GPL about 335 MiB PSS, TZM plateau about 374 MiB PSS / 464 MiB RSS, negligible swap; emulator logs ASTC8x8 fallback to decompressed RGBA. Resume only after the owner completes a full Embedded content rebuild and starts the final pass.
