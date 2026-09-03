# Agent: zmt-backgrounds

- Status: ready-for-integration
- Task: создать минимальный согласованный набор исторических фонов для автономной истории ZMT.
- Scope: шесть новых PNG в `Projects/novels-zmt/Assets/Locations/`, own coordination records and `HANDOFF.md`; source Ink, config, definition, characters and shared runtime are excluded.
- Asset list: `полоцк квартира 1965.png`, `ленинск кузнецкий лаборатория 1941.png`, `воронежский фронт лето 1942.png`, `горшечное зима 1943.png`, `эвакогоспиталь 1943.png`, `полоцк яблоневый сад.png`.
- Visual contract: one consistent 16:9 visual-novel background series in cinematic historical painted realism; period-appropriate Soviet 1940s/1960s environments; no people, bodies, gore, text, logos, flags, emblems or watermarks; usable center/lower composition for later character and dialogue overlays.
- Base commit: `69d77aa9c04d` with existing uncommitted `zmt.ink` and its own coordination records preserved.
- Result: created the six scoped backgrounds with the built-in image-generation mode as one style-matched series; the Polotsk garden received one precise edit removing a modern-looking translucent row cover while preserving the rest of the frame.
- Validation: all six images visually inspected; exact asset count 6; every file is PNG, 1672x941, RGB; no people, bodies, gore, readable text, logos, flags, emblems or watermarks found; no unexpected files under the target directory; `git diff --check` passed; `Tools/novels-tools/novels-content plan` selected only `zmt` atomic content plus documentation and correctly requires a manual visual gate, which was completed. Unity import, `.meta`, config/definition and full content build remain outside this backgrounds-only scope because the standalone story has not been bootstrapped as a content project yet.
- Started UTC: 2026-09-02T09:35:50Z
- Completed UTC: 2026-09-02T09:46:48Z
