# Agent: `nevesta-catalog`

- Status: completed
- Task: Publish nevesta-izo-lda v1 content-only to the Kostroma dev channel; emulator explicitly skipped
- Scope: generated staging under /private/tmp/nevesta-release-staging; remote:/home/p/pureshecom/public_html/content/stories/nevesta-izo-lda/1; remote:/home/p/pureshecom/public_html/content/kostroma-dev.json; recoverable manifest backup; exact coordination records; Projects/novels-catalog/Config/catalog.json remains read-only
- Base commit: `56fa70a014d37bddf596a4dae631533b557001c5`.
- Requested UTC: `2026-09-11T08:21:57Z`.

- User decision: publish content-only and explicitly skip emulator validation; APK remains unchanged.
- Published: immutable `stories/nevesta-izo-lda/1`, 32 files, checksum dry-run clean.
- Public channel: ordered map `chernaya-melnitsa=3`, `trinadtsatyy-kolokol=1`, `les-zabyvshiy-tropy=1`, `nevesta-izo-lda=1`; manifest SHA-256 `2e3b56245c6ea412bc79a8e3e5d9c7ea7492be0b1b473758d0fd529ba575d7f0`.
- Rollback: previous manifest SHA-256 `663b4c96c3e06eb0ca87895d6e2a2c6aa9447d5a9ff744821d0cdc6f06dde632` at `/home/p/pureshecom/rebrand-backups/20260911-nevesta-v1/kostroma-dev.json.before`.
- Public verification: manifest, all retained cards/releases, new card/cover/preview/episode cover/release/bundle/Ink payload returned HTTP 200 with expected lengths; public and remote manifest hashes match.
- Limitation: no APK build, ADB, emulator smoke or device visual gate was run for this publication; this is not full story acceptance.
- Completed UTC: `2026-09-11T08:54:00Z`.
