# Agent: `kostroma-preview-characters`

- Status: completed
- Result: story-owned `Config/Preview` contains preview JSON plus Lada, Mitya and Nastasya PNGs; scroll-driven character staging is live in responsive portrait/landscape readers at site release `20260909-7`; old public `/preview.json` returns 404.
- Validation: Website production build, static JavaScript parse, JSON/event/asset checks, scoped diff checks and live HTTPS endpoint checks passed. User explicitly stopped waiting for the optional Unity content build; the interrupted process was stopped and its unowned zero-byte `UnityLockfile` moved to `/private/tmp/chernaya-melnitsa-UnityLockfile-interrupted-20260909T1519`.
- Task: Add story-owned character staging to web preview in portrait and landscape and deploy it
- Scope: Website/**;Projects/novels-chernaya-melnitsa/Config/Preview/**;Projects/novels-chernaya-melnitsa/Config/preview.json;remote:/home/p/pureshecom/public_html/index.html;remote:/home/p/pureshecom/public_html/site/releases/**;remote:/home/p/pureshecom/public_html/content/stories/chernaya-melnitsa/1/preview/**;remote:/home/p/pureshecom/public_html/content/stories/chernaya-melnitsa/1/preview.json
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T14:39:58Z`.
