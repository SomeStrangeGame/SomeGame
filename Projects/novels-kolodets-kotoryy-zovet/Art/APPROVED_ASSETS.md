# Approved art and audio manifest

## Story identity

- Story: `kolodets-kotoryy-zovet` — «Колодец, который зовёт».
- Tone: restrained psychological village horror, 16+, without gore.
- Visual grammar: contemporary northern Russian village, wet wood, iron,
  birch, muted blue-grey and brown palette, documentary wide compositions,
  realistic painterly treatment, no text, logos or franchise marks.

## Approved package

- Characters: 22 transparent whole-character PNGs across Mира, Лёша, Тихон,
  Вера and Соня. Each runtime outfit includes `main.png`; every used expression
  is an identity-preserving scene-derived variant.
- Locations: 36 title-free gameplay backgrounds covering 22 compositions and
  rain, night, dawn, morning and interior-lighting states. After the 2026-09-07
  continuity revision, source Ink contains 47 location cues (including
  mutually exclusive final scenes) and references 32 unique backgrounds.
  The added scene-derived set covers the
  village cemetery, fire lookout, closed shop, water tower, old bus interior
  and damaged village dam.
- Cover: `Config/cover.png`, a portrait crop of the approved rain-state well
  composition, 1360×1920, title-free.
- Audio: two original 48-second seamless stereo PCM ambience loops and eight
  short original stereo PCM event sounds, all at 48 kHz.

The baseline raster package above was generated specifically for this story on 2026-09-06 with
OpenAI's built-in image generation tool. No external image was supplied as a
reference, no named artist or studio was requested, and no searched image was
incorporated. Whole-character outputs were alpha-checked; a failed opaque
checkerboard draft and an identity/costume-drifting Sonya draft were rejected
before import. Audio was synthesized locally from elementary noise, oscillator
and envelope primitives; it contains no samples, music, speech or third-party
recordings.

During author review, Vera's first `stern` variant was rejected because its
leather shoulder cape, zipper and utility details drifted from her approved
charcoal wool workcoat. A targeted correction reproduced the right wardrobe but
returned an opaque checkerboard and was also rejected. The final runtime
`stern.png` uses Vera's approved stern-faced master pixels exactly, retaining
the correct outfit and genuine alpha channel.

The six expansion backgrounds were generated from blank prompts after the
reader-duration revision. Each maps to an added investigative or character
scene; none is an unattached decorative variant.

`NARRATIVE_PACKAGE.md` preserves the early approved design. Its episode and
ending map differs from the implemented story; see `CONTINUITY_REVIEW.md`.
The continuity revision retained all pixels and audio. Four existing background
assets are now unused by source Ink: `bg01-bus-stop`, `bg06-abandoned-club`,
`bg10-pump-dawn`, `bg21-bus-interior-predawn`. They are retained historical
production assets, not new requirements or evidence of runtime coverage.

## Episode-cover addition — 2026-09-08

Ten distinct 1024×1536 opaque portrait PNGs are now present in
`Config/EpisodeCovers/`, one per episode ID. They were generated with the
built-in image tool and visually/originality-reviewed as a complete package;
two initial snowy drafts were corrected before copying. The existing
baseline art is unchanged. See [manifest and review](EPISODE_COVERS.md) and
[gallery](episode-covers.html). Asset-file readiness does not imply catalog
assignment: definition assignment, preview export and Unity acceptance remain
pending. These files are ordinary catalog images, not Unity Sprite assets.
