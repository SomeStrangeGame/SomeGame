# Agent: `tk-web-release`

- Status: completed
- Task: Create canonical web preview and publish only Trinadtsatyy Kolokol to Kostroma dev without rebuilding the compatible Remote APK
- Scope: Primary own coordination/HANDOFF; retained clone `/private/tmp/tk-integration-X88NJZ` story Preview and exact single-story staged content; remote immutable story v1 and `kostroma-dev.json`; existing APK preserved
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T15:30:00Z`.

- Result: published `trinadtsatyy-kolokol=1` as the only Kostroma dev story; website preview has 22 canonical Ink blocks and three real character images. Existing APK was not rebuilt or replaced because Remote mode reads story IDs and versions from the channel manifest.
- Validation: source checker PASS (86 routes, 13 options, 3 endings); all 40 staged files checksum-matched; public manifest/release/preview hashes match local; cover, preview character, Ink payload and APK endpoints return HTTP 200.
- Release: content release `834935a0ec81038b97267abf6f4c3563381021c828d26c46e14ce8622a74ee37`, schema 5, minimum client 0.2.0. Previous manifest backed up at `/home/p/pureshecom/rebrand-backups/20260909-kostroma/content/kostroma-dev.json.before-trinadtsatyy-kolokol`.
- Limitations: user explicitly retained previously accepted visual/audio limitations and chose to test the existing APK manually; no new Unity build or emulator smoke was run.

- Completed UTC: `2026-09-09T15:55:22Z`.
- Validation: finish-task passed; logs: 1; pending: none.
