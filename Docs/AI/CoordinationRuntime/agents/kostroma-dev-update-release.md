# Agent: `kostroma-dev-update-release`

- Status: completed
- Task: Build and publish current main Kostroma Android Remote dev APK
- Scope: Novels/Assets/Plugins/Android/AndroidManifest.xml,Novels/Build/LocalContent,Novels/Build/Players/automation/kostroma/Android/Remote,Novels/Build/Logs/automation,Projects/novels-catalog/Build,remote:/home/p/pureshecom/public_html/DevBuilds/Kostroma-dev.apk,remote:/home/p/pureshecom/rebrand-backups/20260910-kostroma-update-popup/Kostroma-dev.apk.before,Docs/AI/CoordinationRuntime/HANDOFF.md
- Base commit: `b6896bb7b188dc2f6350077ef18b70c80fbcd165`.
- Requested UTC: `2026-09-10T16:02:09Z`.

## Result 2026-09-10T16:40Z

- Rebuilt Android catalog and Kostroma Remote Player from current `main` using channel `kostroma-dev`, content root `https://pureshechka.com/content`, and test signing.
- Android smoke exposed two manifest regressions before publication: missing launcher intent, then missing `BaseUnityGameActivityTheme`. Both were fixed in `Novels/Assets/Plugins/Android/AndroidManifest.xml`; the final app launches, remains foreground, reaches `catalog.ready`, downloads all three stories, and renders the catalog. The generic smoke timed out only on its later unautomated story-selection events.
- Published APK: version `2026.09.10` (`3520345`), package `ru.kostroma.novels`, 30,190,575 bytes, SHA-256 `368d1efaf4f0579afcd832f6ea41906cefc698d435dc97f60aa551f37929251f`, v2 signature with SomeGame Test certificate.
- Atomic destination: `/home/p/pureshecom/public_html/DevBuilds/Kostroma-dev.apk`; public URL `https://pureshechka.com/DevBuilds/Kostroma-dev.apk`. Public HTTP 200, MIME and length match, downloaded SHA matches.
- Previous APK preserved at `/home/p/pureshecom/rebrand-backups/20260910-kostroma-update-popup/Kostroma-dev.apk.before`, SHA-256 `ec706395ec775dff5b059a85bd2d77e9198cd8ec0dcaf02087e9abe45294f8a2`.
- Public manifest remained unchanged: `chernaya-melnitsa=3`, `trinadtsatyy-kolokol=1`, `les-zabyvshiy-tropy=1`.
