---
name: somegame-create-character
description: Design, originality-screen, produce, import, and visually validate a coherent character for a SomeGame story, including identity master, used outfits, emotions, poses, runtime selectors, and production handoff. Use for new story characters or a material new character package in an existing story; do not use for backgrounds, narrative design, or minor asset fixes.
---

# Create a SomeGame character

Use this skill together with `$somegame-workflow`. Use `$imagegen` whenever AI
raster generation or editing is required. Treat the approved narrative package,
the exact story project, author decisions, current character-layering rules, and
manual-content checklist as authoritative. This skill realizes character
requirements; it does not rewrite the story to justify a preferred design.

Read [character package](references/character-package.md) before defining or
producing a character.

## Establish the character contract

Start from the character's actual narrative function, scenes, relationships,
emotional arc, appearance states, and factual constraints. Confirm the stable
character name/selector, story project, art style, approval mode, and whether
the runtime representation is whole-image or genuinely requires independent
layers. Ask only for an unresolved decision that would materially change
identity, sensitivity, historical integrity, or production scope.

Record protected identity traits before image production: face, apparent age,
body proportions, hair, distinctive features, silhouette, palette, posture,
scale, and any author-defined representation constraints. For a real person,
preserve the evidence and reconstruction classifications supplied by the story
package; never turn an unsupported visual invention into documented fact.

## Approve the identity master

Create a neutral, standing, full-body master on transparency, suitable for the
target visual-novel composition. Unless a scene explicitly requires it, avoid
cropped anatomy, embedded background, furniture, unintended props, or a
back-facing default. Approve identity, anatomy, style, proportions, gaze,
canvas, scale, foot position, and alpha edges before producing variants.

All later assets must be identity-preserving edits or derivations of this
approved master. Do not repair incompatible generations by combining unrelated
heads, bodies, hair, clothing, or facial parts.

## Pass the visual-originality gate

Apply `Docs/AI/rules/OriginalityReviewProtocol.md` and the visual criteria in
[character package](references/character-package.md) to the identity master and
planned appearance package before bulk variants. Compare the distinctive
combination of silhouette, costume, facial design, palette and styling. Preserve
narrative identity and factual constraints; for real people assess artistic
execution rather than treating recognizable likeness as a defect.

## Produce only used appearance states

Derive an appearance inventory from approved scenes. Keep outfit, outfit
condition, emotion, pose, age/state transformation, screen side, and gaze as
separate dimensions. For each outfit, approve one neutral whole reference from
the identity master, then derive only the combinations the story actually uses.
Do not generate a speculative emotion/outfit matrix.

Prefer coherent whole variants. Create modular runtime layers only when the
story contract requires independent customization, and then obtain them by
deterministically separating one coherent approved image on a registered
canvas. Never independently generate interchangeable body parts.

## Import and prove runtime resolution

Place only approved production files in the exact story project. Preserve
existing Unity `.meta` identity on replacement and use the current import
pipeline for new assets. Do not create per-file compression policy or manual
AssetBundle labels that compete with shared tooling.

For every used appearance, prove the chain:

```text
scene -> character selector -> outfit -> emotion/pose -> runtime address -> file
```

Validate exact resolution and documented fallback behavior. Produce full-body
and face contact sheets plus light/dark alpha proofs, then inspect identity,
anatomy, hair, clothing continuity, emotional readability, gaze, scale, feet,
edges, and layered seams. Run the bounded in-game visual gate when required by
the changed-path plan. Automated geometry or alpha checks do not replace visual
review.

## Hand off

Return the approved character brief, protected identity traits, identity master,
scene-derived appearance inventory, exact files and selectors, master-to-variant
provenance, runtime-resolution audit, contact sheets, alpha proofs, import and
validation evidence, visual-originality iteration log and final gate result,
rejected or draft work, and unresolved manual gates.

Missing required variants or identity drift block the character package. Do not
hide them with unrelated fallbacks, modify narrative requirements, author Ink,
add the story to the catalog, or publish changes from this skill.
