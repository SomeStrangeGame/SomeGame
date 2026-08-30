# Agent: gpl-vera-integration

- Status: ready-for-integration
- Task: импортировать утверждённый art Веры episode 1 в atomic Unity project GPL.
- Scope: `Projects/novels-gpl/Assets/Characters/вера/**`; Vera default outfit in `Projects/novels-gpl/Assets/gpl.asset`; точечные Vera presentation labels в `Projects/novels-gpl/Assets/Ink/s01e01.ink` и compiled Ink outputs only through the canonical compiler; собственные coordination records; shared `HANDOFF.md`; ротация уже завершённых записей в существующий handoff archive для соблюдения лимита.
- Base commit: `4bfd64af41d3`.
- Inputs: `Projects/novels-gpl/Art/Vera/LayerPilot/**`, `OutfitPilot/**`, `Variants/station_{urgent,hides_hands,pain}.png`.
- Contract: common head contains head/hair/neutral face only; each clothes layer contains neck, outfit, hands and footwear; facial patches alter only registered face area; substantially different poses remain whole variants.
- Validation: PNG geometry/alpha, content validation, GPL editor content build, resolver addresses, scoped diff and visual gate evidence.
- Result: imported common head, station and `drilling_insulated` clothes, facial patches `alarmed`/`guarded`/`pain`, and whole variants `urgent`/`hides_hands`/`pain_pose`; Vera defaults to station clothes and episode selectors exercise all six presentations.
- Evidence: all PNG inputs are 1024×1536 RGBA; copied head/clothes SHA-256 match approved sources; `novels-content validate gpl` and `build gpl editor` passed twice after final stable asset-id rename; Mac release lists every expected Vera address; compiled Ink contains all six selectors; `docs-check` and scoped `git diff --check` passed.
- Pending: bounded in-game visual gate after the GPL/Novels Editor is intentionally opened; no Editor was left running by this task.
