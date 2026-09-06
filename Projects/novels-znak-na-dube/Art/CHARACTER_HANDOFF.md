# Character handoff

Runtime representation is whole-image for every character. No independently generated face, hair, clothes or body layers exist.

| Selector | Protected identity | Used appearances | Status |
|---|---|---|---|
| Яр | 22, lean, olive skin, unruly black hair, soot-stained hands, patched charcoal travel clothes | `travel/main`, `travel/alarmed`, `travel/marked` | passed |
| Лада | 24, warm brown skin, dark-auburn braid with one copper ring, green working layers, copper jug | `forager/main`, `forager/urgent`, `forager/wounded` | passed; injury is non-graphic in narrative |
| Весна | 48, broad silhouette, gray braided crown, burgundy council wool, key belt | `council/main`, `council/stern`, `council/afraid` | passed |
| Тихон | 67, slim, clouded eyes, square white beard, patched coat, bronze bells | `bellkeeper/main`, `bellkeeper/listening` | passed |
| Мирон | 17, wiry, pale freckled face, cropped brown hair, wet russet work jacket | `memory/main`, `memory/sorrow`, `memory/echo` | passed |

Every Ink selector has a corresponding file at `Assets/Characters/<lowercase-name>/view/whole/<outfit>/<variant>.png`. `main` and `sorrow` intentionally resolve to the same approved Miron memory master; `main` and `stern` intentionally resolve to the same approved Vesna authority master because no separate neutral state is shown. Contact-sheet source files are retained under `Art/Source/`; light/dark alpha proofs and final screen scale remain part of the authorized Unity visual slot.
