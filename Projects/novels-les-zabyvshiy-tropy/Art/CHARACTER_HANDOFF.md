# Character production handoff

Five original whole-image packages were produced from scene-derived identity briefs. Runtime roots are `Assets/Characters/<selector>/view/whole/<outfit>/`.

Technical correction (2026-09-07): Nika's selector is `maincharacter`, as required by `CharacterAssetProfile` and `CharacterSpriteResolver` for the declared protagonist. Her four unchanged PNGs were moved from `ника` to `maincharacter`; the displayed name and identity remain Ника. Other selectors are `ася`, `филя`, `лада`, `яр`. No production pixels or existing Unity GUIDs changed.

| Character | Protected identity | Produced states |
| --- | --- | --- |
| Ника | angular kind face, dark loose bun, compact athletic silhouette, moss field coat, map satchel | main, alert, tender, determined |
| Ася | square face, brow scar, blond braid, sturdy ranger silhouette, red wrist cord | main, stern, guilty, relieved |
| Филя | freckled teen face, copper hair, lanky silhouette, ochre windbreaker, whistle | main, grinning, afraid, focused |
| Лада | round observant face, ash bob, slight child silhouette, mist raincoat with tree rings | main, curious, sad, luminous |
| Яр | lean young hiker, wavy dark hair, faded blue jacket, red thread | main, fading, hopeful, exhausted |

Provenance: each four-state sheet was generated as one identity-preserving image, then deterministically cropped; dark studio matte was keyed to transparent alpha. Files retain no third-party source material. Alpha is present, but edge quality and exact in-game scale remain a deferred Player visual gate.

Originality iteration 1: compared facial construction, silhouettes, costume structure, palettes, accessories and pose language against generic ranger/adventure archetypes available from model knowledge; no direct visual search was available. Genre-functional boots, coats, packs and compass are non-distinctive. The combined red promise cord, blank-map cartography, tree-ring raincoat and five-way restrained palette is original in this package. Risk low; confidence medium; limitation: no reverse-image search. Gate: `passed`.

Runtime audit: every Ink `(outfit, state)` pair resolves to a produced file; fallback `main` exists for each outfit. Contact sheets are preserved by shared generation provenance outside the repository; final files are individual PNGs. Required light/dark alpha proofs, feet, gaze and screen composition remain for the authorized final slot.
