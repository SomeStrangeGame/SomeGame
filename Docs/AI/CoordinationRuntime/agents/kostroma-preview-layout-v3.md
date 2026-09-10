# Agent: `kostroma-preview-layout-v3`

- Status: completed
- Task: Make preview characters replace exclusively through offstage-right transitions and raise portrait framing
- Scope: Website/app/globals.css; Website static build/deploy snapshot; own coordination and handoff. No Unity, story content, or skill changes.
- Base commit: `f1721a63e0462958912795316e63a184e21dd74e`.
- Requested UTC: `2026-09-09T15:38:27Z`.
- Result: non-active preview characters now exit fully to the right and remain hidden; incoming characters enter from the same edge; portrait framing was raised without restoring top clipping. Website build and live release `20260909-11` checks passed; Unity was not used.
