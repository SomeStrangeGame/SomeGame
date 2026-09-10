# Acceptance evidence

Status: **text-first choice correction complete in source; runtime recheck pending** — the author retained the adult dialogue treatment but rejected the large image-only choice cards. Choices are now wide vertical text buttons following the established Black Mill interaction pattern; each authored image is a compact supporting thumbnail inside its labeled button. Every APK and choice screenshot below predates this revision and is superseded as evidence of the choice layout. Full route coverage also remains pending, so the story is not yet accepted.

Fresh story-only Android content build passed on 2026-09-10: release `c296794b0b9db4b6c3470a190c9c81f70744497b1be0e89c5b16518bd78b0ef9`, chunks `af5ec61b…` and `9ffaea0e…`; log `Novels/Build/Logs/automation/content-gate-20260910T134456Z.log`. Per the author's instruction, no aggregate Embedded Player was built. This proves import/compilation/bundling, not portrait runtime appearance.

## Text-first illustrated choices — 2026-09-10

- Replaced the horizontal image-only card mode with a vertical stack of `380x96` text-first buttons.
- Choice labels are always visible at `20px`, with a reserved readable text area; authored episode choice art remains present as a `64x64` supporting thumbnail on the left.
- Reused the story-local adult dark cartographic surface as a sliced button background instead of copying the Black Mill artwork.
- Added source assertions for orientation, label visibility, button geometry and thumbnail bounds. The bounded story audit still passes all 72 routes.
- The rejected runtime layout and the correction decision are recorded in `archive/2026-09-10_manifest_v004.json` and `archive/reviews/2026-09-10_text-first-choice-redesign_v004.md`.
- Required next gate: fresh Unity import/content/player build and portrait Android verification. APK `36536684…` and `Acceptance/20260910-adult-bubble/choice-stable-v3.png` must not be used to accept this layout.

## Adult Bubble and containment correction — 2026-09-10

- Replaced the childlike mushroom/firefly/tracks/antlers treatment with a restrained near-black indigo, weathered-cartography, roots and aged-brass visual language appropriate to an adult mystical adventure.
- Kept the dialogue center broad and unobstructed. The illustrated-choice frame described by this earlier revision was subsequently rejected and replaced by the text-first layout above.
- Corrected containment across all five dialogue variants: text-safe width is `344`; standard backing expansion is `96x120`; the previously undersized `32x32` backing is now `64x104`. Dynamic vertical fitting remains enabled.
- Added source assertions for exact RGBA sprite dimensions and every dialogue variant's safe geometry. The bounded story audit passes all 72 routes; scoped `git diff --check` passes.
- Provenance and the rejected assets are preserved in `archive/2026-09-10_manifest_v003.json` and `archive/reviews/2026-09-10_adult-bubble-redesign_v003.md`.
- Required next gate: fresh Unity import/content build/player build, then portrait Android screenshots with a short line, the longest representative line, and illustrated choices. Until that passes, no earlier APK is the current candidate.

## Illustrated-choice repair and resume recheck — 2026-09-10

- Root cause: the APK contained the previously composed story bundle, in which choice PNGs loaded as `Texture2D` but `LoadAssetAsync<Sprite>` returned `null`. The story source importers were corrected and the repaired story was explicitly composed before rebuilding the player.
- Unity bundle probe confirmed the repaired bundle resolves `anchor-name`, `anchor-promise` and `anchor-trace` as non-null Sprites.
- Fresh test-signed Embedded APK: `Novels/Build/Players/automation/kostroma/Android/Embedded/Novels.apk`, 2,492,479,532 bytes, SHA-256 `96660cf4a935b6c71268700fa39102be5706a8f31f23598181bad9daee61df25`.
- Clean Android run `e58999045d79491d939906204a960943` entered through the real catalog, activated repaired release `aa5840ca2751a28b804424b8f62a49c6cd1d540c8db3cd34cfbe20729bec0bcc`, reached `s01e01` sequence 46 with `choiceCount=3`, and visibly rendered three distinct illustrated cards.
- Route A selected visible choice ID `0`. After force-stopping and relaunching only the app, the catalog offered `Продолжить`; run `92480ae3640f467f8653f2b270d0067c` restored the post-choice line «Его зовут Яр…», proving the selected branch and dialogue position survived process restart.
- Evidence: `Acceptance/20260910-choice-fix/choice-icons-visible.png` (SHA-256 `2af69518…`), `save-resume-catalog.png` (`5b3c71ad…`), `save-resume-restored.png` (`64ffce8d…`), and `runtime-logcat.txt` (`3454ccfb…`).
- No `fallback.used`, application ANR, or `FATAL EXCEPTION` marker is present for the two recorded run IDs.

## Route-matrix blocker — 2026-09-10

This finding is **superseded** by the repaired APK and evidence above. It is retained as immutable defect history; its screenshots and logs must not be used as proof of the current candidate.

- Reinstalled the immutable candidate APK (`3cf56aefc377f9f6e5078dcd26ba080993d64f396d12765af78a1a6315ab8d05`) and cleared only `ru.kostroma.novels` app data.
- Clean run `7652336f15334178a56e6700e5e8026a` entered through the real catalog flow, selected `les-zabyvshiy-tropy`, activated release `3b3d4bdf9bb374ef2fc96a27231cdfd98a02b0764b884633530c688ef30f01fd`, reached `s01e01`, and emitted `dialogue.ready` sequence 46 with `choiceCount=3`.
- After the choice UI settled for five seconds, all three authored cards remained present but their labels were completely invisible. Selecting an unlabeled branch would not be valid player or route evidence.
- Stable screenshot: `Acceptance/20260910-route-matrix/route-a/choice-labels-missing.png` (SHA-256 `f6cdb388a6d3a78f621505ff6ef3d47d5fc77c8d43523a54ad494a4fc3e9c88c`).
- Full run log: `Acceptance/20260910-route-matrix/route-a/runtime-logcat.txt` (SHA-256 `5f40626a0e2aa31d9c61e3644ad1823122f8b2bf5d6726fe125d1d2c14bd7d94`).
- Per the acceptance contract, this is returned to the story-local Bubble/Choice production owner. The remaining two routes, all twelve choice alternatives, three endings, and save/resume gate remain stale/pending until the repair is rebuilt.

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
