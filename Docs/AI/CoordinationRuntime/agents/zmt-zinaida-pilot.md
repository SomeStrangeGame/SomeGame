# Agent: zmt-zinaida-pilot

- Status: superseded
- Task: создать один проверочный цельный спрайт молодой Зинаиды Туснолобовой в фронтовом образе 1942 года по production-соглашениям GPL.
- Scope: `Projects/novels-zmt/Assets/Characters/зинаида/view/whole/front/main.png`, own coordination records and `HANDOFF.md`; additional characters, variants, Ink selectors, `.meta`, config/definition and shared runtime are excluded.
- Visual contract: 1024x1536 RGBA full-body sprite on transparent background; GPL cinematic realistic rendering and framing; historically grounded likeness and Red Army medical-service field uniform; neutral determined pose; no gore, weapon, text, logos or invented decorations.
- Reference: public-domain 1941 portrait linked to the Russian State Catalogue record via Wikimedia Commons; existing GPL Pavel/Mark whole sprites are style and framing references only.
- Base commit: `69d77aa9c04d`; existing uncommitted ZMT Ink/backgrounds and coordination records preserved.
- Result: created one neutral whole-body front-line Zinaida sprite from the archival 1941/1942 likeness references in the GPL whole-character composition and rendering style. Built-in image generation produced a checkerboard RGB backdrop, so the imagegen skill's documented local chroma-key extraction path was used to obtain real alpha without changing the accepted character design.
- Validation: face/pose/anatomy/uniform and full silhouette visually inspected against archival and GPL references; clean dark-background edge proof passed; exact scoped asset count 1; production file is PNG 1024x1536 RGBA with genuine alpha; `git diff --check` passed; `Tools/novels-tools/novels-content plan` selected `zmt` atomic content and the completed manual visual gate. Unity import, `.meta`, Ink selectors and content build remain outside this single-pilot scope.
- Started UTC: 2026-09-02T09:55:00Z
- Completed UTC: 2026-09-02T10:22:58Z
- Superseded UTC: 2026-09-02T10:37:27Z by `zmt-zinaida-layers`; the approved whole image remains the canonical art master under `Projects/novels-zmt/Art/Zinaida/Master/front-neutral.png`, while the incorrect NPC-style production path was removed.
