# Episode covers — 2026-09-08

Scope: ten episode-specific catalog images explicitly requested by the author.
Source of scenes: current revised `Assets/Ink/s01e01.ink`–`s01e10.ink`, not the
historical narrative package. Existing cover, backgrounds and character art
remain unchanged. This does not resolve the separate narrative-plan discrepancy.

## Frozen production manifest

All rows are required. Target: opaque, full-bleed portrait PNG, requested
1024×1536 (actual output dimensions verified after generation). Realistic
painterly contemporary northern village, muted slate blue / damp timber /
restrained warm light. No people, faces, monsters, gore, logos, embedded titles,
episode numbers, readable document text, decorative symbols or black bars.
Main subject must remain legible at thumbnail size and under center cropping.
No outcome-specific destruction, burial or restored-memory imagery.

| Episode | Title | Scene-derived subject | Target file / `_catalogCover` |
| --- | --- | --- | --- |
| s01e01 | Обратный адрес | Unsigned envelope and old key at a weathered village house entrance | s01e01.png |
| s01e02 | Плёнка без голоса | Cassette and portable player on the dark kitchen table, dry tap behind | s01e02.png |
| s01e03 | Карточка Алексея | Medical card and open wooden records drawer in the rural clinic | s01e03.png |
| s01e04 | Человек с чужой фамилией | Unfinished wooden boat and plane on the workshop bench | s01e04.png |
| s01e05 | Сухое русло | Sagging plank bridge over a shallow, nearly dry creek at dusk | s01e05.png |
| s01e06 | Дождь говорит | Rain on the old concrete dam and rusted sluice handwheel | s01e06.png |
| s01e07 | Фраза из будущего | Portable recorder on a dry ledge beside the pump collector grille | s01e07.png |
| s01e08 | Четыре узла | Water tower above village roofs in predawn rain, distribution pipe below | s01e08.png |
| s01e09 | Право не помнить | Pencil and decision sheets on a shop counter, one ruled entry crossed out | s01e09.png |
| s01e10 | Что вернёт вода | Old well windlass and wet timber in morning light before the final decision | s01e10.png |

Destination for approved images: `Config/EpisodeCovers/`.
Generation: built-in image tool, one call per asset; pilot s01e01 before the
remaining images. Full prompts, output provenance and review results follow.

## Integration boundary

The current main contract (`ContentPipeline.md`, episode-cover section) uses
ordinary PNG/JPEG files, not Unity sprites or bundle dependencies. `_catalogCover`
must be assigned by stable episode ID. The story worktree still predates this
SDK contract. No Unity session is authorized; definition assignment, preview
export and catalog UI acceptance remain pending until safe main integration
and the separately authorized final validation slot. Do not hand-edit generated
preview data or fabricate Unity validation. A file in this directory alone is
not proof of runtime assignment.

## Status

Ten images generated, visually reviewed and copied unchanged into
`Config/EpisodeCovers/`. All are distinct opaque RGB PNGs, 1024×1536.
The existing general cover, backgrounds and character images were not changed.
See [gallery](episode-covers.html) and [full prompts/provenance](episode-cover-prompts.json).

## Visual review — completed

Pilot s01e01 established the object-led portrait composition before production
of the remaining nine images. Initial s01e02 and s01e03 introduced snowy outdoor
scenery: both were rejected and corrected through the built-in image editor.
The final ten-image package has consistent unfrozen autumn scenery.
No pixel processing, external source images, rasterized titles or logos were
added. Tiny indistinct prop markings on recorder buttons and distant shop
containers are incidental texture, not readable story text or branding.

The final package was inspected image by image: envelope/key, audio cassette,
records drawer, model boat, damaged footbridge, sluice wheel, digital recorder,
water tower, decision sheet and well windlass are distinct focal subjects.
The medical card and decision sheet reveal no personal information.
The last cover shows an intact windlass before the decision, without depicting
the result of any ending. Subject placement is intended for portrait display;
actual catalog cropping remains part of the pending Unity/catalog acceptance.

## Originality review — covers only

Status: passed for this ten-cover package after one full-package originality
review; no substantial similarity findings in the checked comparisons.
This is a bounded creative review, not a guarantee of uniqueness or legal clearance.
It does not replace the pending originality review of the revised Ink text.

Method: descriptive web search, direct reading of official source pages and
visual inspection of their linked images in the browser. No unpublished story
text or generated image was uploaded to an external search service.

- [The Ring, Paramount official page](https://www.paramountpictures.com/movies/the-ring):
  the directly inspected linked steelbook/poster image centers a vertical VHS
  cassette and bright circular emblem. Our s01e02 is a diagonal compact audio
  cassette in a domestic kitchen, without an emblem, screen, countdown or figure.
  Our s01e10 focuses on a side-view timber windlass; no circular well-bottom
  view or apparition is used. Tape/well motifs remain general genre adjacency,
  not evidence of matching visual execution.
- [The Vanishing of Ethan Carter, developer screenshots](https://www.theastronauts.com/2013/09/reveal-first-game-screenshots/):
  the directly inspected linked
  [dam screenshot](https://www.theastronauts.com/wordpress/wp-content/uploads/2013/09/TVoEC_ScreenShot_13_Dam.jpg)
  shows a monumental curved masonry dam in a wide elevated lake panorama.
  Our s01e06 uses a close foreground handwheel at a small straight rural sluice
  in rain. Different scale, structure, viewpoint and focal hierarchy; autumn
  foliage and atmospheric rural infrastructure are generic overlaps.
- The other eight covers were reviewed within the complete package for repeated
  distinctive composition, franchise motifs, recognizable characters/branding
  and narrative spoilers. None was identified. A records card, boat model,
  bridge, tower, key and pencil are commonplace props and structures.

Confidence: moderate. No reverse-image search was available; comparisons are
limited to descriptive discovery and the inspected official material, not an
exhaustive image database. The two season corrections were visual-continuity
iterations, not unresolved originality findings.

## Handoff

Use the ten filename mappings above after the worktree is safely brought onto
the SDK contract from main. Assign through the supported authoring workflow,
then export preview and verify every episode card plus missing-image fallback
in the authorized Unity/catalog slot. This step has NOT been performed.
The gallery is a static asset review, not runtime evidence.

Static checks:

```bash
python3 -B Projects/novels-kolodets-kotoryy-zovet/Art/tests/test_episode_covers.py
python3 -B Projects/novels-kolodets-kotoryy-zovet/Art/tests/test_continuity.py
```
