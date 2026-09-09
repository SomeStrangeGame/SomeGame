# Agent: `tk-catalog-restore`

- Status: completed
- Task: Restore Black Mill alongside Trinadtsatyy Kolokol in Kostroma dev manifest
- Scope: Remote-only /home/p/pureshecom/public_html/content/kostroma-dev.json; exact ordered set chernaya-melnitsa=1, trinadtsatyy-kolokol=1; verify both immutable story roots; APK and story files unchanged; own coordination only
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T15:57:14Z`.

- Result: restored ordered channel set `chernaya-melnitsa=1`, `trinadtsatyy-kolokol=1`; APK and immutable story trees unchanged.
- Validation: both remote card and Android release files existed before switch; staged manifest SHA-256 `cdd0e705d591e99b50c0249dd52ad557ca5b4c806d6b3c959764ac7a2bde2b0e`; public manifest matches and both story endpoints return HTTP 200.
- Rollback: the accidental single-TK manifest is preserved at `/home/p/pureshecom/rebrand-backups/20260909-kostroma/content/kostroma-dev.json.only-trinadtsatyy-kolokol`.

- Completed UTC: `2026-09-09T16:00:17Z`.
- Validation: finish-task passed; logs: 1; pending: none.
