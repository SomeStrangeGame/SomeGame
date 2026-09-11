# Character handoff

Runtime representation is whole-image for every character. No independently generated face, hair, clothes or body layers exist.

| Selector | Protected identity | Used appearances | Status |
|---|---|---|---|
| Яр | 22, lean, olive skin, unruly black hair, soot-stained hands, patched charcoal travel clothes | `travel/main`, `travel/alarmed`, `travel/marked` | passed |
| Лада | 24, warm brown skin, dark-auburn braid with one copper ring, green working layers, copper jug | `forager/main`, `forager/urgent`, `forager/wounded` | passed; injury is non-graphic in narrative |
| Весна | 48, broad silhouette, gray braided crown, burgundy council wool, key belt | `council/main`, `council/stern`, `council/afraid` | passed |
| Тихон | 67, slim, clouded eyes, square white beard, patched coat, bronze bells | `bellkeeper/main`, `bellkeeper/listening` | passed |
| Мирон | 17, wiry, pale freckled face, cropped brown hair, wet russet work jacket | `memory/main`, `memory/sorrow`, `memory/echo` | passed |

NPC selectors resolve at `Assets/Characters/<lowercase-name>/view/whole/<outfit>/<variant>.png`. Яр is the definition's main character: the runtime uses `Assets/Characters/maincharacter/view/whole/travel/<variant>.png`, regardless of his displayed name. The 2026-09-07 audit moved his three existing PNGs there without changing image bytes. `main` and `sorrow` intentionally resolve to the same approved Miron memory master; `main` and `stern` intentionally resolve to the same approved Vesna authority master. Лада uses `wounded` only from the flooded-path attack onward and retains it through the endings. Voice-only memories and remote voices use narrator text. Contact-sheet source files are retained under `Art/Source/`; light/dark alpha proofs and final screen scale remain part of the authorized Unity visual slot.
