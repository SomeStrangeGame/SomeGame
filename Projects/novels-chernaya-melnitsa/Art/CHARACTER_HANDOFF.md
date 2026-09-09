# Character production handoff

Status: user accepted residual v6 contours as non-blocking on2026-09-08.
They are unchanged, not fixed. APK39049f824adf verified releasefb7c51d4b322.
Large Yakov guarded sleeve ribbon is gone and Lada215 fringe reduced, but
Yakov217 retains red/maroon trousers edge and a greenish left-sleeve contour.
See latest `ACCEPTANCE_EVIDENCE.md`; no further fix in this validation slot.
All11 v6 PNG/meta hashes remain verified;15tests/build/runtime loading pass,
not full story acceptance. Remaining runtime plan: `ACCEPTANCE_PLAN.md`.
Current source proofs: `Art/EdgeCleanup/v6/README.md`.

Historical v3 source correction: all 11 runtime PNGs had edge-only colour/alpha
cleanup; 14,046 contaminated edge pixels corrected, plus two detached extraction
scraps removed (Saveliy229px and Yakov guarded244px). Canvas768x1280, actual body
silhouettes, interior artwork, all meta/GUIDs, prefab composition and canonical
ASTC8 policy remain unchanged. Source comparisons on dark/light/blue backgrounds
and seven regression fixtures pass. Details, exact hashes and before/after proofs:
`Art/EdgeCleanup/README.md`. Earlier identity sheets/alpha proofs below are
historical; use EdgeCleanup evidence for current production pixels. No new
generation or design change; originality applicability is documented separately.

## Historical Player failure (before source correction)

2026-09-08 composition APK: raised characters at1.2 scale and lower Bubble
preserve readable faces in inspected Lada/Nastasiya scenes. Lada at Ink172/204
shows jagged brown/magenta edges around hair/clothing on dark backgrounds.
Evidence: `Novels/Build/Logs/automation/chernaya-composition-20260908/`
`replay-155242.png`, `replay-155348.png`. Residual coloured pixels are also
visible in source `maincharacter/view/whole/coat/wary.png` and the earlier dark
alpha proof. ASTC contribution has not been isolated. Source PNGs and canonical
import policy remained unchanged during that slot. The source correction above
now requires a fresh actual Player gate. The earlier “clean at review scale”
statement is not a current bundled-texture pass.

All characters use coherent whole-image variants on a common `768x1280`
transparent canvas. Variants were produced as identity-consistent sheets, then
split and chroma-keyed deterministically; no independently generated body parts
or runtime layers are used.

## Protected identities and runtime audit

| Selector | Protected identity | Used variants | Runtime address |
|---|---|---|---|
| maincharacter (Лада) | 29, ash-blond bob, angular face, long dark restorer coat, satchel and folding ruler | main, wary, guilt, resolve | `Characters/maincharacter/view/whole/coat/<variant>.png` |
| яков | 26, tied dark waves, narrow face, handmade-toggle vest, flour-darkened hands | main, guarded, alarm, honest | `Characters/яков/view/whole/work/<variant>.png` |
| настасья | 61, lined face, black headscarf, charcoal wool, rye-stitched postal satchel | main | `Characters/настасья/view/whole/wool/main.png` |
| савелий | 54, square tired face, immaculate blue-black rain cape, ledger cord | main | `Characters/савелий/view/whole/raincoat/main.png` |
| митя | 24, sandy hair, pale work jacket, unbranded geometric postal patch | main | `Characters/митя/view/whole/jacket/main.png` |

Every requested state has an exact whole-image runtime file. Supporting-cast
lines explicitly request `main`; missing-expression fallback is no longer
used as a substitute for authored selectors. Past Mitya dialogue is narrated
without an adult sprite; his current portrait appears only in the epilogue.
Lada's runtime PNGs were moved byte-for-byte to `maincharacter`, the path used
by `CharacterSpriteResolver` for the main-character role. Art/identity evidence
remains in `Art/Characters/лада`. The unused
planned Lada `exhausted`/`wet` variants were removed from the production
manifest rather than represented by duplicate or unreviewed art.

## Visual evidence

- Lada: `Art/Characters/лада/identity-production-sheet.png`; Yakov:
  `Art/Characters/яков/identity-production-sheet.png`, plus light/dark alpha
  proofs in the same identity directories (not the runtime selector directory).
- Nastasya, Saveliy and Mitya: `identity-master.png` plus light/dark proofs.
- Full bodies, feet and props remain inside the canvas; generated sheets show no
  duplicate limbs or identity changes across Lada/Yakov variants.
- A first dark-background review found magenta fringe; the mask and hidden RGB
  were rebuilt and the repeated proof is clean at review scale. Actual bundled
  texture review on dark and light scenes remains mandatory.

Originality: separate character screen in `ORIGINALITY_EVIDENCE.md`, `passed`,
low risk / medium confidence. Generator sources remain in the task's Codex
generation store and are identified in `GENERATION_PROVENANCE.md`; rejected
opaque-background sheets are not referenced by runtime or duplicated into Git.
