# Publication correction validation — first-snow v2

Recorded UTC: 2026-09-11T13:14:00Z

- Channel: `kostroma-dev`
- Public story version: `first-snow/2`
- Channel manifest SHA-256: `25657e8bae53cdd605371e27b85a679a44d1d2df0b1439285c0b6a8d5d481625`
- Story file count: 19
- Card release stage: `beta`
- Website preview: `preview/preview.json`, `lesha.png`, and `sonya.png`
- APK action: retained unchanged

The Android content build and Unity content validation passed. The immutable
story tree was checksum-compared before and after publication, then the channel
manifest was switched last. Public HTTP checks passed for the preview manifest
and both character images. Live browser inspection confirmed the beta badge,
the reading button, and the opened linear excerpt through its final continuation
message. Emulator and ADB validation remained explicitly waived.
