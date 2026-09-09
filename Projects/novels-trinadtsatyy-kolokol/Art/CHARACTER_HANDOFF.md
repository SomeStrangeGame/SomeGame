# Character production handoff

All characters are fictional. Visual style: painterly neo-noir, cool key and copper rim, realistic anatomy, whole-image runtime representation.

| Selector | Protected identity | Master | Authored variants | Runtime fallback |
|---|---|---|---|---|
| Лада | 29, chestnut ponytail, teal raincoat, scarf, reel canister | `maincharacter/view/whole/coat/main.png` | main, focused, alarmed, resolve | main |
| Тим | 22, curly dark hair, rust hoodie, headlamp | `тим/view/whole/hoodie/main.png` | main, brave, frightened | main |
| Роман | 37, short dark hair/beard, charcoal field coat, evidence envelope | `роман/view/whole/fieldcoat/main.png` | main, guarded, determined, tense | main |
| Зоя | 63, silver bob, copper cardigan, hearing aid | `зоя/view/whole/cardigan/main.png` | main, grief | main |
| Ася | 26, asymmetric black/plum hair, plum jacket, headphones | `ася/view/whole/studio/main.png` | main, live, afraid | main |
| Инга | 34, auburn braid, slate work jacket, gloves | `инга/view/whole/workjacket/main.png` | main, defensive, injured, ashamed | main |

Paths are relative to `Assets/Characters/`. `main` is the SDK's default emotion
address, not the literal filename `neutral.png`. `_mainCharacter: "Лада"` maps
to `maincharacter`, not `лада`. The narrative audit moved/renamed existing
PNG files to that contract without changing their pixels or identity. There
were no imported `.meta` files to move; Unity import is still pending.
All 16 character addresses emitted by current routes resolve exactly; the four
additional main images provide the required default state for the other roles.

Provenance: built-in imagegen, original prompts recorded in task transcripts; individual masters replace a rejected overlapping lineup crop. Lada and all secondary emotional variants are identity-preserving whole-image edits of their approved masters. No modular body parts or independent face layers are used.

Visual review: all individual masters and variants show complete head, hands and feet with coherent clothing and props. `lada-contact-sheet.png` preserves Lada across four expressions; `secondary-variants-contact-sheet.png` covers the eleven newly authored secondary states. `secondary-variants-alpha-light.png` and `secondary-variants-alpha-dark.png` confirm readable silhouettes, preserved feet and clean transparent edges on contrasting static backgrounds. Every character selector used by current Ink now has an exact PNG; import settings and in-game bright/dark edge review remain final-slot gates.

Originality iteration 1: no real-person or franchise reference; silhouette/costume/palette combinations are story-specific and function-driven. External reverse-image search unavailable. Risk low, confidence medium, result passed.

Originality iteration 2: the complete current character package adds only scene-derived facial/emotional variants from the six approved fictional masters. Identity, silhouette, costume, palette, props, pose language and canvas remain master-derived; no new real-person or franchise reference was introduced. Local package review found no material overlap with other story characters; external reverse-image search remains unavailable. Risk low, confidence medium, result passed.
