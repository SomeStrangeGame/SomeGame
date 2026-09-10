# Agent: `catalog-publish-locked-copy`

- Status: completed
- Task: Rebuild and publish catalog containing locked episode explanatory copy
- Scope: Novels/Assets/Novels/CatalogFlow.cs; Projects/novels-catalog/Assets/RemoteAssets/catalog/fallback.prefab; catalog build/publish outputs; Docs/AI/CoordinationRuntime/agents/catalog-publish-locked-copy.md; Docs/AI/CoordinationRuntime/HANDOFF.md
- Base commit: `eb7655d32ab6765bd96fcabb5a728a9a72ce407c`.
- Requested UTC: `2026-09-10T09:56:37Z`.
- Paused UTC: `2026-09-10T10:10:20Z`.
- Progress: checkout plus shared Unity/Catalog locks were acquired; stale Catalog `Temp/UnityLockfile` was removed after confirming no Catalog Editor process. Android catalog build was then blocked before start by foreign Unity Editor PID 63134 for `/Users/iantonishin/Kids/games-pluskidsdev-8142/web-games-wrapper`.
- Next: after that external Editor exits, register a fresh FIFO request, reacquire checkout and shared Unity/Catalog locks, build Catalog Android and Kostroma Android Remote APK, resolve exact remote destination read-only, obtain concrete destination/payload confirmation, then publish and verify. No server mutation performed.
- Build resumed UTC `2026-09-10T10:20:25Z`: Android Catalog content gate passed and Kostroma Android Remote test-signed APK build passed for `https://pureshechka.com/content`, channel `kostroma-dev`.
- Candidate: `Novels/Build/Players/automation/kostroma/Android/Remote/Novels.apk`, 30,750,116 bytes, SHA-256 `ec706395ec775dff5b059a85bd2d77e9198cd8ec0dcaf02087e9abe45294f8a2`, version `2026.09.10` (`3519980`).
- Publish preflight: public manifest retains `chernaya-melnitsa=1`, `trinadtsatyy-kolokol=1`; existing `/home/p/pureshecom/public_html/DevBuilds/Kostroma-dev.apk` is 30,750,556 bytes, SHA-256 `ad8a2cb2ad48b75ae8000bb5bb7d902fc7130a498a83de29eac3eb5a51186c8e`. Await explicit confirmation to backup and atomically replace only this APK via `pureshecom@pureshechka.com`; manifest/story trees unchanged.
- User confirmed the exact SHA-256, SSH destination, final path and backup path. Published UTC `2026-09-10T12:41:10Z` via temporary sibling plus atomic rename.
- Verification: remote and public HTTPS `https://pureshechka.com/DevBuilds/Kostroma-dev.apk` both match SHA-256 `ec706395ec775dff5b059a85bd2d77e9198cd8ec0dcaf02087e9abe45294f8a2` and 30,750,116 bytes; HTTP 200 with `application/vnd.android.package-archive`. Previous APK is preserved at `/home/p/pureshecom/rebrand-backups/20260910-catalog-lock-copy/Kostroma-dev.apk.before` with original SHA-256 `ad8a2cb2ad48b75ae8000bb5bb7d902fc7130a498a83de29eac3eb5a51186c8e`. Manifest and story trees were not changed.
- Publication attempt UTC `2026-09-10T12:00Z`: external-write approval was rejected twice by the execution policy before any bytes were uploaded. A new direct user confirmation naming the exact external host, final path, backup path and candidate SHA-256 is required. Own checkout lock/request released; server remains unchanged.
