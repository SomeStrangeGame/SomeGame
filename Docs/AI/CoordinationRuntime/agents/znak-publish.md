# Agent: `znak-publish`

- Status: completed
- Task: Publish corrected Znak na Dube v2 content-only to Kostroma dev; device smoke delegated to user
- Scope: Novels/Build/ChannelContent/stories/znak-na-dube/2/**; Novels/Build/ChannelContent/dev.json; remote:/home/p/pureshecom/public_html/content/stories/znak-na-dube/2; remote:/home/p/pureshecom/public_html/content/kostroma-dev.json; remote:/home/p/pureshecom/rebrand-backups/20260911-znak-v2/kostroma-dev.json.before; Docs/AI/CoordinationRuntime/agents/znak-publish.md; Docs/AI/CoordinationRuntime/HANDOFF.md
- Base commit: `fc32d84c4ae70ba36f048ed0f4bc81e4e9b35e91`.
- Requested UTC: `2026-09-11T10:25:30Z`.

- User decision: stop emulator validation and delegate final device smoke to the
  user; publish content-only to Kostroma dev. APK, production and Git push are
  unchanged.
- Published immutable story version: `znak-na-dube/2`, with releaseId
  `db6d0ba698f7c36b417d1c0de8c10b7df8508e8bd690ea0f7fd5c25afb84451f`.
- Public channel manifest SHA-256:
  `9233a17fc0a75697f0b61dc22a8af639ad836b952b1002e901ca9361d3f8cf87`;
  ordered map retains `chernaya-melnitsa=3`, `trinadtsatyy-kolokol=1`,
  `les-zabyvshiy-tropy=1`, `nevesta-izo-lda=2`, adds
  `znak-na-dube=2`, and retains `volchya-poshlina=1`.
- Rollback manifest:
  `/home/p/pureshecom/rebrand-backups/20260911-znak-v2/kostroma-dev.json.before`
  (previous SHA-256 `e9f2900d96706b0944427ed2275c77d945f675637da38d6fcfe2e688d45dc6c5`).
- Verification: local/remote story checksum dry-run has no differences; public
  manifest and release hashes match local; card, cover, catalog preview,
  episode cover, both bundles, release and Ink payload return HTTP 200.
- Pending: user-owned final device smoke only.

- Completed UTC: `2026-09-11T10:54:37Z`.
- Validation: finish-task passed; logs: 1; pending: none.
