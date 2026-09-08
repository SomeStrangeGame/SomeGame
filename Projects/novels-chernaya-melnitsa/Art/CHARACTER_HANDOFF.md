# Character production handoff

Status: passed for static production; in-Player visual gate deferred.

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
