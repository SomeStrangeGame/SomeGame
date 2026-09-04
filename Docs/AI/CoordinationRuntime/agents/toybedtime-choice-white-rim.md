# Agent: `toybedtime-choice-white-rim`

- Status: ready-for-integration
- Task: remove the unintended white exterior rim from toybedtime choice cards.
- Scope: `Projects/novels-toybedtime/Assets/Presentation/bubble/sprites/choice-card.png` and its `.meta`, generated validation evidence, `Docs/AI/CoordinationRuntime/HANDOFF.md`, own coordination records.
- Base: `f691f613`.

## Result

- Removed the unintended white exterior rim and the generator checkerboard residue from `choice-card.png`.
- Replaced the inherited noisy alpha with an exact rounded-rectangle mask; hidden RGB outside the mask is cream-matted to prevent bilinear color bleeding.
- Android import for this small UI sprite now uses uncompressed RGBA32 (`textureFormat: 4`, `textureCompression: 0`) so block compression cannot recreate alpha speckles.
- Story-local prefab, dialogue panel, shared fallback and other stories were not changed.
- Editor verify and Android content-gate passed after one successful project-protocol recovery of a failed Unity Licensing IPC client.
- Fresh Embedded APK passed emulator visual gate at `s01e01.ink:43`: `Novels/Build/Logs/toybedtime-choice-white-rim-final.png`.
- APK: `Novels/Build/Players/toybedtime-choice-white-rim/Novels.apk`.
