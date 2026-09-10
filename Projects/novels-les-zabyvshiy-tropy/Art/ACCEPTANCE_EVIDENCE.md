# Acceptance evidence

Status: **blocked** — runtime launch succeeds, but the character-alpha visual gate fails.

## Android gate — 2026-09-10

- User explicitly authorized the final Unity/Android slot and auto-approval.
- Story Editor, Catalog Editor, story Android, required catalog/player builds passed.
- Final tested APK: `Novels/Build/Players/automation/kostroma/Android/Embedded/Novels.apk`, 2,464,982,621 bytes, SHA-256 `739b8a1253f975edecfb48235a4e31d2839ff38ada4fde9321f9b1d8672e254f`.
- Device: `emulator-5554`, Android API 34, 1080×2400. Install completed with `Success`; `lastUpdateTime=2026-09-10 10:57:55`.
- Clean run `b73ff9a4816a488193233fe52969a81d` reached `catalog.ready` with three stories, downloaded `les-zabyvshiy-tropy`, selected it, activated release `f00d58404e0da22b238f241d690fea9b6145107ab3f5c7056d9fee13308fa900`, reached `episode.ready(s01e01)` and repeated `dialogue.ready`. No `fallback.used`, FATAL exception or ANR marker appears in the saved log.
- The catalog card, title, six-episode count and episode-one cover render correctly.

## Corrections made during the gate

The first runtime frame exposed dark-brown text on a dark-blue story-local Bubble. Six dialogue text states were changed to a light cold tone. A later long line exposed `32px` overflow; all dialogue text states were restored to the established `22px` size while the distinct choice-button size remained unchanged. The final `dialogue-readable.png` proves the long sampled line is readable and contained.

## Blocking finding

At least Asya's runtime whole-character sprite has destructive internal transparency: face, hands and scattered highlights remain while most dark clothing/body pixels disappear and the location shows through. The source inventory confirms this is not a Unity selector fallback: the intended selector resolves, but the imported PNG alpha is sparse (`main.png`: 380,187 fully transparent pixels of 393,216; mean alpha approximately 6.8/255). The same source-generation pattern is present across the 20 character PNGs, with mean alpha approximately 4.0–118.4/255, so the package must be treated as affected until every used variant passes light/dark alpha proofs.

Expected: one coherent, opaque whole-character silhouette with transparency only outside the character. Actual: large holes inside the body and clothing. Evidence: `Acceptance/20260910-android/character-alpha-failure.png` and the production PNGs under `Assets/Characters/`.

The built-in background-extraction edit was attempted for the neutral Asya master but returned HTTP 429 before producing an artifact. No unsafe opaque rectangle, unrelated fallback, or identity-changing replacement was substituted. Per `CharacterLayeringRules.md` and the acceptance skill this blocks acceptance and further route/endings claims.

## Preserved evidence

- `Acceptance/20260910-android/catalog-card.png`
- `Acceptance/20260910-android/dialogue-readable.png`
- `Acceptance/20260910-android/character-alpha-failure.png`
- `Acceptance/20260910-android/runtime-logcat.txt`
- Story Android log: `Novels/Build/Logs/automation/content-gate-20260910T075524Z.log`
- Player log: `Novels/Build/Logs/automation/player-20260910T075548Z.log`

Next required owner: `$somegame-create-character` together with `$imagegen`. Repair the neutral identity/outfit masters and every used variant as true transparent cutouts while preserving selector paths and `.meta` GUIDs; generate full-body/face contact sheets plus light/dark alpha proofs. After that material art change, repeat a freshly authorized exact-story Unity/Android gate and the planned episode/choice/ending matrix. The story must not be marked accepted before those checks pass.
