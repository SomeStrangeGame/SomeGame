# WebGL route validation handoff history

Historical entries preserved verbatim; current validation supersedes their pending gates where explicitly stated in HANDOFF.md.

## 2026-09-12 — codex-web-completion — source-fix pending validation

Existing WebGL build reproduced s01e02 reading_failed after choice 0 (go with Yakov), retry resume_loaded=136 fails identically; no chunk_1 HTTP request. WebEpisodePlayer.Sprite calls TryGetBundledSprite without acquiring selected chunk; Scope.EnsureOwned rejects it. Added await GetAssetBundle + cancellation check (3 lines, web-only). Static review/diff check only; compiled artifact unchanged. Save preserved at 127.0.0.1:8767, smoke-boundary-20260912; next: rebuild WebGL then Retry from same save, verify chunk_1/2 and full ending. No publication/commit/Unity launched. Own smoke server stopped and checkout released for queued retirement integration. See Website/public/player/README.md for exact repro.

## 2026-09-12T09:45:44Z — codex-web-e2e — completed

Task/changed: local catalog→reader→next UI E2E. Website app/page.tsx uses same-origin /content (public manifest HTTP200 lacks localhost CORS); opt-in Vite dev proxy connects existing public cards and localhost WebGL, no hosting changes. serve-smoke.py injects debug controls on local player route; production config remains disabled. README updated; unrelated dirty state preserved.
Validation: actual catalog link → fresh s01e01 → choice 1 search Mitya → episode_completed → new NextEpisode button → s01e02; two actions → ReturnToSite → real catalog → reopen restores resume_loaded=2 and Mitya-route paragraph. Desktop1280×720/portrait390×844/landscape844×390 inspected, browser error logs empty. TypeScript noEmit/incremental=false, production Website build and scoped diff checks passed. Existing Player/content build reused; no Unity started.
Pending: public deployment/headers/compression, full episodes/routes, real mobile devices and storage quota/offline/tab-close remain unaccepted. No publication, commit, media/save deletion. Next: plan release candidate hosting checks and final-story/performance acceptance; publication requires separate authority. Own local servers stopped at teardown; queue released.

## 2026-09-12T09:10:23Z — codex-web-controls — completed

Task: local website reader controls and save-error UX. Changed: nested Website repo public/player (single player route/config, NextEpisode, safe ReturnToSite, retry without reload); optional catalog link in app/page.tsx, existing dirty changes preserved. Local serve-smoke.py serves actual reader/build and injects fault controls only for smoke=1; README documents integration and limits.
Validation: Website npm run build, npx tsc --noEmit, reader.js node --check and scoped diff checks passed. In-app real s01e02 resume; simulated write failure → clear error → retry → latest action restored without reload; ReturnToSite navigated to /. Visual checks 1280×720, 390×844, 844×390 passed (short landscape scrolls). Expected injected storage error appears in Unity development console.
Pending / risks: config remains disabled; no publication/commit/media or save deletion. Catalog→reader and new NextEpisode UI end-to-end still pending; prior Unity NextEpisode evidence remains valid. Real storage quota/offline/tab-close, hosting release headers and full story acceptance remain open. Website is a separate nested Git repo. Local server stopped; no Unity process started.
Suggested next step: local catalog integration and episode-boundary UI E2E, then separately authorized hosting rollout.
