# Agent: wardrobe-neutral-emotion

- Status: waiting-user-validation
- Task: не наследовать сюжетную эмоцию/позу в neutral wardrobe preview, сохраняя её для восстановления после закрытия.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterSpriteResolver.cs`, focused validation, own coordination files and shared handoff.
- Constraints: Ink и save format не менять; story appearance cache не очищать; target-aware wardrobe state сохранить.
- Base commit: `4bfd64af41d3`
- Started UTC: 2026-08-30T14:45:36Z
- Heartbeat UTC: 2026-08-30T14:51:30Z
- Result: `Wardrobe` render role использует временный чистый appearance-state, поэтому story emotion/pose не наследуются в free preview и остаются в cache для восстановления после закрытия. Первый gate не подключился к helper socket; один повторный attach compile passed без compiler errors. Pending: visual open/close.
