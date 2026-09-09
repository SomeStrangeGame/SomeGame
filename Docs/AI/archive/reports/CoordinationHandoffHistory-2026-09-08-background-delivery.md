# Preserved settings handoff — 2026-09-08

## 2026-09-08T07:00:00Z — catalog-settings-popup — ready-for-review

Task: Replace decorative book with fixed gear and neutral settings popup embedded
in fallback.prefab. No genre styling and no generated runtime UI.
Changed: prefab/gear SVG+PNG, CatalogSettingsPopup/ICatalogSettings/controller wiring,
ApplicationAudioSettings and Game composition, existing validation, Catalog README/memory.
Global volume uses AudioListener.volume and a separate versioned PlayerPrefs key;
close/pause/dispose flush preferences. Footer URLs are authored on popup component.
Validation: final content-gate 20260908T065759Z and editor-gate 20260908T065824Z
passed. Live catalog regression and settings checks passed, including real pointer
input, mute/35%/save/reopen, fixed gear, modal block/restore, backdrop/cancel and labels.
Evidence: Novels/Assets/Build/Logs/catalog-settings-popup-final-20260908.png inspected.
Original volume preference restored after validation; story saves untouched.
Editor left playing with settings open. Recovery copies preserved under
Novels/Build/Logs/automation/catalog-settings-popup[-final]-recovery-20260908.backup.
Pending: actual privacy/terms/support HTTPS URLs (empty/disabled with explicit notice),
user visual review and Android/tablet checks. No commit, publish or scene save.

## 2026-09-08T07:06:00Z — catalog-settings-copy-trim — ready-for-review

Removed the popup title, reading subtitle and missing-links notice as requested;
removed notice binding, compacted empty header space and corrected Catalog README.
Catalog content-gate 20260908T070425Z and editor-gate 20260908T070437Z passed.
Live settings regression passed; final Game View visually inspected:
Novels/Assets/Build/Logs/catalog-settings-trimmed-20260908.png.
Editor left playing with popup open; original volume restored; no story saves changed.
Footer URLs remain pending, without an in-popup notice. No commit/publish.
