# Lada source repair — ready-for-final-validation, 2026-09-09

## Current complete source package —11:19UTC

Status: **ready-for-final-validation**, not Android acceptance.
Built-in generation recovered; user explicitly rejected CLI/API fallback.
All four runtime PNGs were replaced in the retained clone only, after complete
package review. Existing metas/import settings are byte-identical. Old620×620
sources are retained in `legacy-source/` and Git; nothing was deleted.

Production mapping: main←main-clean-v2.png, focused←focused-clean.png,
alarmed←alarmed-clean.png, resolve←resolve-clean.png, all RGBA1254×1254.
Targets: `Assets/Characters/maincharacter/view/whole/coat/{emotion}.png`.
`package-before-import.json` records exact file/meta hashes, GUIDs and all46
scene→Лада→coat→emotion→maincharacter/view/whole/coat/emotion→file references
across six episodes. Alarmed is already used in e1 (the older e2–e6 summary
below was incomplete); resolve is used in e6. No new fallback or Ink edit.

Validation: matte helper5 tests PASS, package geometry5 tests PASS, exact
post-copy/meta audit PASS, existing story static86 routes/13choices/3endings and
Unicode/7negative regressions PASS. Alpha bounds differ by at most1px; silhouette
IoU against master: focused0.992410, alarmed0.991203, resolve0.992606. All core RGB
pixels remain byte-identical to each corresponding generated green draft.
No runtime face/body compositing, resizing, crop or compression override.

Visual review: `package-faces.png`, `package-full-light.png`,
`package-full-dark.png`, plus individual dark focused/resolve and light alarmed
proofs inspected. Same adult identity, hair, costume, reel, satchel, stance,
leftward gaze, complete hair/hands/feet; transparent gaps retained, no pale
bottom strip. Focused tightens brows, alarmed raises brows/parts lips, resolve
uses restrained firm lips/steady eyes and is intentionally subtler. Fine warm
hair rim remains; final on-device edge/detail/readability assessment is pending.

Complete-package originality iteration2: reviewed all four current whole sprites
and face/full-body sheets against iteration1 protected traits and direct-source
comparison. No new distinctive silhouette/costume/prop combination or identity
drift introduced by expressions; no material finding. Risk low, confidence
medium, passed with the same bounded-search limitations. This is a source
production decision, not an exhaustive legal or in-game quality guarantee.

Remaining: fresh explicit final Unity/content/Android slot and actual Lada
rendering check, then the still-open story route/ending/save/audio/UI gates.
No Unity/ADB/build ran here. APK tk-20260909-1018 still contains the OLD sprites;
its blocked Android V2 evidence is historical and does not validate this repair.

## Earlier neutral-only checkpoint (historical; superseded above)

Successful built-in drafts (generated approximately11:00UTC, copied11:16UTC):

| Emotion | Original filename | Raw PNG SHA256 |
| --- | --- | --- |
| focused | exec-9e2755bc-4489-40f3-a912-a41ea3904536.png | f50b0ffed0028fdbe8cd370e9525caec5d7a48251e3b5e27273d8febe535b715 |
| alarmed | exec-13dca270-92e7-45fd-a2e9-12de69153774.png | 4ef04aac4504c16ae3e5e077b934661eaf57d6337a125c410463536d87b01cd1 |
| resolve | exec-2f372879-68f0-42fe-8e60-bea9462895c3.png | 44c3bda787642534f6651caa9ada7a49ae78f96dece7d451b77122b879b1ae9f |

Local raw copies: `{emotion}-green-draft.png`; each was an edit of the same
reviewed `main-clean-v2.png`, not of the previous emotion. Original directory
is listed below. Focused used exactly the attempt4 prompt below.

Alarmed full prompt: Use case: identity-preserve. Edit target: the supplied exact fictional Lada whole-body sprite. Produce ONE alarmed expression variant of this SAME29-year-old woman. Change only eyebrows, eyes and mouth: eyes slightly wider, inner brows raised, lips subtly parted in restrained apprehension. Not screaming, comedy, rage or exaggerated fear. Preserve facial identity, head angle and image-left gaze, chestnut ponytail/all strands, teal long coat, grey/copper scarf pattern, brown satchel, metal reel canister, both hands, dark trousers, lace-up boots, entire standing pose, painted style and cool/copper lighting. Whole body must remain registered at EXACT same size/position as supplied: square1254x1254, crowny13, solesy1212, boundsx430..867. No reframing, rescaling, anatomy changes or new props. For technical matte removal place only empty exterior and internal gaps on flat pure#00FF00 green. No green spill, shadows, ground, checkerboard, border, text or sheet. Complete uncropped head and feet. This is a subtle facial-expression edit, not a redesign.

Resolve full prompt: Use case: identity-preserve. Edit target: supplied exact fictional Lada whole-body sprite. Make ONE resolve expression variant of the SAME29-year-old archivist, for a difficult final decision. Change only facial expression: steady intent eyes looking image-left, composed brow, firm closed lips, quiet determination without anger or a smile. Preserve exact face identity/age/head angle, chestnut ponytail and loose strands, teal long raincoat, patterned grey/copper scarf, brown satchel, metal reel canister and hands, dark trousers, boots, standing pose, detailed painted neo-noir rendering, cool key and copper rim light. Keep canvas square1254x1254 and exact figure registration: crowny13, solesy1212, boundsx430..867. No zoom/crop, no pose, scale, outfit, silhouette or anatomy changes. Empty exterior and every internal gap must be solid uniform#00FF00 green for later technical alpha extraction; no green spill or shadows. No checkerboard, ground, border, text, additional props or sheet. Complete full-body character, subtle facial expression edit only; do not redesign.

At this checkpoint the neutral alpha master was prepared, with variants pending.
User explicitly approved Python background removal on2026-09-09. `matte.py`
implements measured green-key removal, bounded3px mixed-edge cleanup and residual
green suppression only in that edge band. No crop, rescale or internal redraw.
Five tests PASS, including teal/face preservation, enclosed gap, clipped-input
rejection, unknown-background rejection, determinism and fine-edge cleanup.
`main-clean.png` is first-pass evidence (rejected residual green hair/laces).
`main-clean-v2.png` is the reviewed neutral RGBA master1254×1254; alpha bounds
[430,13,868,1213],292935 core pixels byte-identical to green draft,6341 mixed
edge pixels unmixed,698 residual green pixels corrected. White/dark/purple
proofs generated; purple/light/dark visual review shows intact figure/no stripe.
PNG file SHA256: `13717205cf1579674ad47c5c116001919dfc971c0c60f4ba134882d2db7fb8ff`.
This file hash differs from the pixel-buffer hash in `main-clean-v2.json`.
No production overwrite yet; master-derived variants and final package checks
must precede import. No Unity/ADB/build in this source-only task.
Not approved for import. Existing runtime PNGs/metas, Ink, catalog and APK remain
unchanged. Android V2 defect evidence: `Art/ANDROID_ACCEPTANCE_V2.md`.

## Protected contract

Fictional archivist Lada,29; existing face, chestnut ponytail, teal long raincoat,
grey/copper-patterned scarf, brown satchel, reel canister, trousers/boots,
leftward gaze, standing pose and painterly neo-noir lighting. Whole-image repair,
not separately generated heads/body parts. Only main, focused, alarmed, resolve.
Main covers default dialogue; focused investigation is used throughout e1–e6;
alarmed accompanies disappearance/memory revelations in e2–e6; resolve appears
in the final episode. Exact selector paths remain existing coat/*.png.

All old Lada PNGs620×620. main/focused contain a full-width opaque pale row610.
New output is1254×1254 (requested2048×2048 was NOT honored). Figure detail is
visibly improved, all feet/hands retained; the neutral source alpha review passed.
Final full-package identity/contact/edge/resolution gates remain pending because
the three new variants do not exist. Master and planned-package originality
review is recorded below; old package evidence is not promoted to new variants.

## Built-in imagegen attempts (technical, not originality iterations)

| File | Input | SHA256 | Decision |
| --- | --- | --- | --- |
| main-checkerboard-rejected.png | existing runtime main.png | e615bff21a6e656398d58e788359fca171f8ad03ca39288bbe199b541a597564 | RGB with baked checkerboard; rejected |
| main-alpha-retry-rejected.png | preceding draft | 673fde1747e2c7a62ecc0c243dd79aa375111ebea973b15ad39fe272827f50a6 | RGB with baked checkerboard again; rejected |
| main-green-draft.png | first draft | 604f3f4141b744c53c08779e3aec57f79da0915c740bccd3d4bc3199a1eb6ec5 | RGB green-matte draft, not runtime-ready |

Default source directory:
`/Users/iantonishin/.codex/generated_images/01a07c22-23b1-7472-bec0-a06ac35355f3/`.
Source filenames in table order: `exec-e6740515-fc99-497a-8eef-090a860d3b3c.png`,
`exec-f7bd5c03-d367-4913-ae1b-eac29d023cc0.png`,
`exec-ee78255f-7830-4c78-b920-7d040b29f86b.png`.
Copied byte-for-byte; originals preserved. At initial draft checkpoint no image
processing had been performed. After explicit user approval, `matte.py` created
separate alpha outputs, without overwriting those drafts. The green field is
not exactly uniform: dominant samples(4,248,10)/(4,249,9); the helper therefore
measures the field and handles mixed edges. A green prompt is not alpha proof.
Do not globally desaturate green:26 retained core pixels have green dominance
above10 in eye/garment details, outside the3px cleanup ring. They are preserved,
not evidence that the transparent service background remains.

## Prompt set

### Attempt1 — old whole master is edit target

Use case: identity-preserve. Edit target: the supplied whole-body character sprite of Lada. Production repair, NOT a redesign. Reconstruct this exact fictional 29-year-old woman in clean high resolution, 2048x2048 square PNG with REAL transparent alpha background. Preserve her exact face, age, chestnut ponytail, loose hair strands, neutral thoughtful expression, gaze toward image left, teal long raincoat, grey scarf with copper pattern, dark trousers, brown lace-up boots, brown shoulder satchel and metal reel canister held at chest. Preserve the complete original standing pose, both hands, anatomy, painterly realistic neo-noir shading, cool light and restrained copper rim. Keep original normalized composition: slender full-body figure centered about x51%, crown at y2%, soles at y95%; full head, hair and feet, ample transparent side margins. Improve crisp painted facial detail, hair, fabric and boot detail; do not merely nearest-neighbor enlarge existing pixels. Remove the erroneous white horizontal strip along the bottom and all background residue. Everything outside the person/held props must have alpha0, including all canvas edges and interior gaps. No ground plane, cast shadow, black background, checkerboard pixels, border, white stripe, text, labels or additional props. Return ONLY one clean complete neutral whole-character sprite, not a sheet. Lock the character's identity and outfit to the supplied image.

### Attempt2 — first draft is edit target

BACKGROUND EXTRACTION EDIT. The supplied image is the exact edit target. It has an erroneous baked-in gray/white CHECKERBOARD. Remove ALL of that checkerboard; output an actual RGBA PNG with transparent alpha=0 outside the person. A checkerboard pattern is NOT transparency. Preserve the woman pixel-faithfully as far as possible: same fictional face, chestnut ponytail, neutral expression looking left, teal raincoat, gray patterned scarf, reel canister, shoulder bag, hands, dark trousers, boots, complete stance, lighting and colors. Do not redesign, replace the face or alter pose. All canvas edges fully transparent, full feet with margin. No white strip, no ground shadow, no background of any color. High-resolution full-body sprite, requested 2048x2048 square output, not a thumbnail; same framing. Output a single clean character sprite with genuine transparent background, not a visualization of transparency.

### Attempt3 — first draft is edit target

Precise background-only edit of the supplied fictional Lada whole-body character. Replace the entire gray/white checkerboard outside the character with a PERFECTLY UNIFORM SOLID #00FF00 chroma green background, including all internal gaps around hands, bag, hair and between legs. No checkerboard remains. No shadows, gradient, ground, noise, texture or green spill on the person. Keep the actual person IDENTICAL: same neutral face, gaze left, chestnut ponytail, teal coat, grey patterned scarf, reel canister, bag, hands, full standing pose and boots, lighting and colors, exact size and placement. This is a technical matte preparation, not new art direction. Single full-body high-resolution sprite on pure green, same square framing and all limbs complete; no text/border/white strip. Requested 2048x2048. Background must be one exactly uniform RGB green color.

## Master / planned-package originality — iteration1,2026-09-09

Scope: complete `main-clean-v2.png` and planned focused/alarmed/resolve facial
edits, with fixed pose, clothing, props and palette. Technical alpha attempts
above are not hidden originality redesign iterations.

Primary source reviewed directly in the in-app browser:
[Night School Studio official Oxenfree concept gallery](https://nightschoolstudio.com/oxenfree/#galleryOxenfree).
The enlarged five-character concept lineup shows a stylized teal-haired woman
with short orange/red jacket, slim trousers and bright boots, alongside other
casual teen silhouettes. Compared silhouette, face treatment, ponytail, coat,
palette, accessories and pose language. Lada retains a realistic adult face,
chestnut ponytail, long teal raincoat, patterned scarf, reel and satchel. Shared
ponytail/cool colours/mystery genre are generic elements; the distinctive
combination is materially different. No source art was downloaded or used as
an image-generation reference.

Finding: no material overlap in the reviewed comparison. Risk **low**,
confidence **medium**, master/planned-package gate **passed**. Scope is bounded,
not exhaustive uniqueness or legal clearance. Search text access returned429;
ordinary official-gallery browsing succeeded. Kathy Rain search yielded no
reviewed primary art, so it is not counted as evidence. No reverse-image search.
Completed variants still require their own coherent-package review for drift.

## Variant attempt4 — focused; no output

2026-09-09 approximately10:48UTC, built-in imagegen returned HTTP429 with a
protective challenge. No new image was produced; no repeated requests or
alternate endpoint bypass. Target was `main-clean-v2.png` (viewed before edit).
Submitted prompt:

Identity-preserving facial-expression edit of this exact complete fictional Lada sprite. Produce ONE whole-body focused expression variant. She is the SAME 29-year-old woman: preserve exact face identity and head angle, chestnut ponytail and all hair, teal long coat, patterned grey/copper scarf, brown satchel, metal reel canister, hands, boots, posture, lighting and painted rendering. Change ONLY eyebrows/eyes/mouth to restrained investigative concentration: slightly gathered brows, intent eyes still looking image-left, closed thoughtful lips. Not anger or smile. Body, clothes, props, silhouette and ALL registration MUST stay fixed. Same square 1254x1254 canvas and exact figure position/scale: crown y13, soles y1212, bounds x430..867. Do not zoom, crop, re-pose or alter anatomy. For technical alpha extraction, fill every area outside the person with solid uniform vivid #00FF00 green, including internal gaps. No green spill, ground or shadows. No checkerboard, border, text, additional objects or sheet. Whole character with complete hair and feet; facial-expression variation only. This is an edit of the provided original character, not a new design.

## Next bounded action

User-requested retry2026-09-09T10:54UTC used the identical focused prompt and
same verified master SHA. Built-in imagegen again returned HTTP429/protective
challenge; no image produced, no further attempts or bypass. Offered explicit
CLI/API fallback requiring locally configured OPENAI_API_KEY; not authorized
or invoked. No credentials inspected. All existing assets/proofs unchanged.

After the image service becomes available, derive focused, alarmed and resolve
from the reviewed complete master. Python authorization covers deterministic
background removal and proofs, not redrawing/assembling faces. Reacquire primary
FIFO before copying generated drafts or writing clone evidence. Inspect the
complete four-image package, full-body/face contact sheets and light/dark proofs;
only then replace the exact four runtime PNGs, preserving metas/import settings.
No new Unity/Android gate without fresh explicit final-slot approval. Current
runtime sources and diagnostic APK remain unchanged; no acceptance is claimed.
