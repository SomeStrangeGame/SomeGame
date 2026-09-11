# Generation provenance

Built-in image generation was used for nine source artifacts: cover, Yar triptych, Lada triptych, secondary cast 3×2 grid, two environment/story 3×2 grids, Bubble UI 2×2 kit, choice-icon 3×2 grid, and the extension location triptych `locations-grid-c.png`. Final prompts specified original painterly dark fantasy, charcoal/pine/oxidized-bronze/bone-white palette, the forked-root empty-eye motif, exact panel geometry, no named artist/franchise, no text/logo/watermark and scene-specific subjects from `ART_MANIFEST.md`.

The extension triptych was generated on 2026-09-07 as an exact three-column atlas: abandoned charcoal boundary with smoking pits and living roots beneath ash; half-submerged path shrine with seven broken and one intact name tablets plus a bronze bell; subterranean root archive with hanging wooden memory tablets and eight central vacancies. The 2172×724 source was deterministically split into three 724×724 runtime locations: `charcoal-boundary.png`, `drowned-shrine.png`, and `root-archive.png`.

Source files are preserved under `Art/Source/`; runtime crops are under `Assets/Characters`, `Assets/Locations`, `Assets/Choices`, `Assets/Presentation`, and `Config/cover.png`. Cropping did not composite unrelated identities or independently generated body parts. The Lada checker background and the deliberately generated secondary-cast chroma field were deterministically converted to alpha; no character pixels were borrowed or recomposed.

Audio was synthesized at 44.1 kHz stereo PCM WAV by `Art/render_audio.py`. The 48-second bed uses periodic unresolved low-mid intervals, sparse deterministic stick-slip root events and loop-safe modulation. Four SFX are separate renders: pulse, root creak, bronze response and cliffhanger sting.
