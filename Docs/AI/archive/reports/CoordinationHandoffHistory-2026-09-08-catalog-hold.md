## 2026-09-08T06:17:00Z — catalog-hold-reset — ready-for-review

Replaced destructive confirmation click with a continuous 2-second hold and
horizontal button fill; the inline warning/dependent episode reset scope is unchanged.
Release, exit, drag, movement, disable and focus loss cancel and clear progress.
Changed: SDK Card/HoldToConfirm, fallback prefab, existing live validation and README.
Validation: content-gate 20260908T061404Z and editor-gate 20260908T061431Z passed;
no compiler/Console errors. Live catalog checks passed (2 stories, one Continue).
Hold validation passed on a cloned real bundle button with counter-only callback:
click/submit and cancellations safe; second pointer isolated; full hold fires once.
Visually inspected Novels/Assets/Build/Logs/catalog-hold-reset-20260908.png.
Editor left playing on Tzm with confirmation closed; no user progress reset.
Recovery backup preserved at Novels/Build/Logs/automation/catalog-hold-reset-recovery-20260908.backup.
Pending: user review/touch-device check; no commit, publish or scene save.
