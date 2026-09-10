# Acceptance evidence

Status: **character gate passed; full route acceptance pending** — the source and Android runtime character-alpha blocker is repaired. The episode/choice/ending matrix is still required before acceptance.

## Character Android recheck — 2026-09-10

- Recovered Unity Licensing by stopping the stale mutex-owning Licensing Client and starting a clean batchmode session.
- Fresh test-signed Embedded build passed: `Novels/Build/Players/automation/kostroma/Android/Embedded/Novels.apk`, 2,464,944,117 bytes, SHA-256 `3cf56aefc377f9f6e5078dcd26ba080993d64f396d12765af78a1a6315ab8d05`.
- Installed and tested the actual APK package `ru.kostroma.novels`, version `2026.09.10` (`3519910`), on `emulator-5554`, Android API 34, 1080×2400; `lastUpdateTime=2026-09-10 12:13:33`.
- Clean run `8ec8172644774311a15e38326a0a81d7` reached `catalog.ready` with three stories, selected `les-zabyvshiy-tropy`, activated release `3b3d4bdf9bb374ef2fc96a27231cdfd98a02b0764b884633530c688ef30f01fd`, reached `episode.ready(s01e01)` and repeated `dialogue.ready`.
- `nika-alert.png` shows a coherent full Nika pose. `asya-stern.png` rechecks the exact character/state family that exposed the original failure and shows continuous clothing, body and legs without location bleed-through.
- The saved clean-run log contains no `fallback.used`, `FATAL EXCEPTION`, fatal smoke error, or application ANR marker.
- Evidence: `Acceptance/20260910-alpha-recheck/nika-alert.png`, `Acceptance/20260910-alpha-recheck/asya-stern.png`, and `Acceptance/20260910-alpha-recheck/runtime-logcat.txt`.

## Character alpha repair — 2026-09-10

- Replaced destructive sparse alpha with 20 reviewed, pose-specific foreground masks; no generated replacement RGB was used.
- Verified all 20 production PNGs retain decoded RGB pixels exactly against the pre-repair Git revision.
- Preserved every `512×768` canvas, selector/outfit/state path and Unity `.meta` GUID.
- Reviewed the five neutral identities on both dark and light backgrounds in `AlphaRepair/dark-light-proof.png`.
- Reviewed every expression/pose on alternating dark and light backgrounds in `AlphaRepair/all-variants-proof.png`; the intentional translucent `яр/fading` treatment remains contained inside a coherent silhouette.
- Reproduction data and per-file hashes are recorded in `AlphaRepair/repair-report.json`; the deterministic alpha-only applicator is `AlphaRepair/apply_character_masks.py`.
- Fresh Android content-gate passed after the repair (`content-gate-20260910T085729Z.log`).
- The earlier failed signing/licensing attempts are superseded by the successful test-signed build and clean runtime evidence above.

## Android gate — 2026-09-10

- User explicitly authorized the final Unity/Android slot and auto-approval.
- Story Editor, Catalog Editor, story Android, required catalog/player builds passed.
- Final tested APK: `Novels/Build/Players/automation/kostroma/Android/Embedded/Novels.apk`, 2,464,982,621 bytes, SHA-256 `739b8a1253f975edecfb48235a4e31d2839ff38ada4fde9321f9b1d8672e254f`.
- Device: `emulator-5554`, Android API 34, 1080×2400. Install completed with `Success`; `lastUpdateTime=2026-09-10 10:57:55`.
- Clean run `b73ff9a4816a488193233fe52969a81d` reached `catalog.ready` with three stories, downloaded `les-zabyvshiy-tropy`, selected it, activated release `f00d58404e0da22b238f241d690fea9b6145107ab3f5c7056d9fee13308fa900`, reached `episode.ready(s01e01)` and repeated `dialogue.ready`. No `fallback.used`, FATAL exception or ANR marker appears in the saved log.
- The catalog card, title, six-episode count and episode-one cover render correctly.

## Corrections made during the gate

The first runtime frame exposed dark-brown text on a dark-blue story-local Bubble. Six dialogue text states were changed to a light cold tone. A later long line exposed `32px` overflow; all dialogue text states were restored to the established `22px` size while the distinct choice-button size remained unchanged. The final `dialogue-readable.png` proves the long sampled line is readable and contained.

## Superseded blocking finding

At least Asya's runtime whole-character sprite has destructive internal transparency: face, hands and scattered highlights remain while most dark clothing/body pixels disappear and the location shows through. The source inventory confirms this is not a Unity selector fallback: the intended selector resolves, but the imported PNG alpha is sparse (`main.png`: 380,187 fully transparent pixels of 393,216; mean alpha approximately 6.8/255). The same source-generation pattern is present across the 20 character PNGs, with mean alpha approximately 4.0–118.4/255, so the package must be treated as affected until every used variant passes light/dark alpha proofs.

Expected: one coherent, opaque whole-character silhouette with transparency only outside the character. Actual: large holes inside the body and clothing. Evidence: `Acceptance/20260910-android/character-alpha-failure.png` and the production PNGs under `Assets/Characters/`.

This source finding is repaired by the pose-specific alpha package above. No unsafe opaque rectangle, unrelated fallback, or identity-changing replacement was substituted. Runtime evidence below remains historical and must not be reused as proof of the repaired sprites.

## Preserved evidence

- `Acceptance/20260910-android/catalog-card.png`
- `Acceptance/20260910-android/dialogue-readable.png`
- `Acceptance/20260910-android/character-alpha-failure.png`
- `Acceptance/20260910-android/runtime-logcat.txt`
- Story Android log: `Novels/Build/Logs/automation/content-gate-20260910T075524Z.log`
- Player log: `Novels/Build/Logs/automation/player-20260910T075548Z.log`

Next required step: execute the planned episode/choice/ending matrix. The story must not be marked accepted before those checks pass.
