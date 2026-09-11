# Agent: `story-candidate-freshness`

- Status: waiting-human-approval
- Task: Final acceptance for volchya-poshlina
- Scope: Projects/novels-volchya-poshlina; Projects/novels-catalog/Config/catalog.json; final editor/android evidence
- Base commit: `d4505b07d5a91a26cae053d016e9800ae9a48d0f`.
- Requested UTC: `2026-09-11T09:05:23Z`.
- Checkpoint UTC: `2026-09-11T09:25:00Z`.
- Story and Catalog editor/android content builds passed. Fresh Kostroma Embedded
  APK: SHA-256 `aacb9de11677dfdb7323a0eeb03ca6a2dea49a703e40541847ad4d7c82a1b19d`,
  110797544 bytes. Clean task AVD exited after boot; stable emulator-5554 has an
  older `ru.kostroma.novels` signed incompatibly. Explicit approval is required
  before uninstalling that exact package because app data may be lost.
