# Character handoff

Runtime representation is whole-image for every character. No independently generated face, hair, clothes or body layers exist.

| Selector | Protected identity | Used appearances | Status |
|---|---|---|---|
| Яр | 22, lean, olive skin, unruly black hair, soot-stained hands, patched charcoal travel clothes | `travel/main`, `travel/alarmed`, `travel/marked` | passed |
| Лада | 24, warm brown skin, dark-auburn braid with one copper ring, green working layers, copper jug | `forager/main`, `forager/urgent`, `forager/wounded` | passed after runtime alpha repair; injury is non-graphic in narrative |
| Весна | 48, broad silhouette, gray braided crown, burgundy council wool, key belt | `council/main`, `council/stern`, `council/afraid` | passed |
| Тихон | 67, slim, clouded eyes, square white beard, patched coat, bronze bells | `bellkeeper/main`, `bellkeeper/listening` | passed |
| Мирон | 17, wiry, pale freckled face, cropped brown hair, wet russet work jacket | `memory/main`, `memory/sorrow`, `memory/echo` | passed |

NPC selectors resolve at `Assets/Characters/<lowercase-name>/view/whole/<outfit>/<variant>.png`. Яр is the definition's main character: the runtime uses `Assets/Characters/maincharacter/view/whole/travel/<variant>.png`, regardless of his displayed name. The 2026-09-07 audit moved his three existing PNGs there without changing image bytes. `main` and `sorrow` intentionally resolve to the same approved Miron memory master; `main` and `stern` intentionally resolve to the same approved Vesna authority master. Лада uses `wounded` only from the flooded-path attack onward and retains it through the endings. Voice-only memories and remote voices use narrator text. Contact-sheet source files are retained under `Art/Source/`; light/dark alpha proofs and final screen scale remain part of the authorized Unity visual slot.

## Desktop source review — 2026-09-11

Directly inspected `yar-triptych.png`, `lada-triptych.png` and
`secondary-cast-grid.png` as complete sheets. Яр and Лада retain face, hair,
costume, proportions and gaze direction across their used states; Весна, Тихон
and Мирон remain internally recognizable between paired states. Whole bodies,
hands and feet are present without an obvious second head, duplicated neck or
detached limb. This is a source-sheet review only: runtime scale, individual
PNG alpha edges on contrasting backgrounds and simultaneous sprite count remain
deferred to the Unity/Player visual gate.

## Runtime alpha repair — 2026-09-11

The fresh Android Player gate exposed baked checkerboard pixels and disconnected
edge fragments in Lada's three production crops. The defect was returned from
acceptance to character production. A deterministic mask removed only the two
near-white checker shades from the original whole variants; the leftmost 20 px
of `urgent` and 40 px of `wounded` were cropped to remove disconnected foreign
fragments. Height, identity, face, braid, costume, pose, jug and all retained
character pixels are unchanged. The preview `characters/lada.png` remains an
exact copy of `forager/urgent.png`.

The final files are RGBA PNGs with real transparent backgrounds. Originality
evidence remains applicable because this is a technical extraction of the
approved variants, not a redesign. Fresh light/dark and Android runtime evidence
must still pass before the story returns to acceptance.
