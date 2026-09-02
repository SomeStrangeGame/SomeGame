# Agent: `bubble-fallback-gameobject-reference`

- Status: ready-for-manual-validation
- Task: исправить подтверждённую неверную сериализованную ссылку общего bubble fallback.
- Scope: `Novels/Assets/Novels/Novels.unity`, Novels Editor lifecycle, own coordination records and shared handoff.
- Contract: сохранить прямую схему shared fallback -> TZM variant; заменить только ошибочный component fileID на root GameObject fileID; unrelated dirty tree сохранить.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: 2026-09-01T12:15:07Z
- Evidence: `EntryPoint._fallbackBubble` has type `GameObject`; Inspector shows None with component fileID `2630654022111875827`; shared prefab root GameObject fileID is `3712146450823894313`.
- Completed UTC: 2026-09-01T12:19:15Z
- Result: `_fallbackBubble` now references shared prefab root GameObject fileID `3712146450823894313`; direct shared fallback -> TZM variant inheritance is preserved.
- Validation: screenshot/field-type/root-ID evidence confirmed the cause; scene was clean before restart; fresh Novels import and compile passed without missing-reference, initialization or compiler errors. Manual Play reproduction remains.
