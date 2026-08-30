# Agent: gpl-fullbody-character-offset

- Status: ready-for-integration
- Task: сместить полнофигурных персонажей GPL вниз локальным presentation override без изменения других историй.
- Scope: `Projects/novels-gpl/Assets/Presentation/character/`, собственные coordination records, `HANDOFF.md` при завершении.
- Expected change: единый вертикальный offset корня character-screen GPL; все runtime-слои сохраняют взаимную регистрацию.
- Base commit: `4bfd64af41d3`.
- Validation: scoped diff, GPL prefab inspection, Unity compile/editor visual gate при доступном Editor.
- Result: создан project-local character-screen variant с единым `Y = -120`;
  другие истории и shared SDK не менялись.
- Validation: configuration valid; scoped prefab diff/check passed. Unity content
  phase blocked by read-only Package Manager database while Novels Editor was active;
  GPL editor rebuild и visual replay остаются ручным gate.
- Resume: пользователь попросил выполнить GPL editor rebuild и проверку.
- Final validation: GPL editor release `c08a534aa2a1c2a017a4209ddcb3d8df5d327a1c201b34827987aa3d0382a517`
  built and composed; project-local character prefab listed in release; fresh Novels
  Editor compile passed with no compiler errors. Manual visual replay remains.
- Repro: первый offset был применён к root Screen Space Overlay Canvas и не влиял
  на фактическую геометрию; исправление переносится на дочерний `Viewport`.
- Corrected result: `Viewport` offset `Y = -120`, root Canvas offset reset to zero;
  GPL release `8b65bdd9f6e42386bcce4f3f3259615a17232af6ef1ab16401f84f4f3fae59bf`
  built/composed, fresh Novels compile passed, Editor left open for visual replay.
- Heartbeat UTC: 2026-08-30T16:38:00Z.
- Resume: по live Play Mode персонаж требует ещё `100 px` вниз; fallback choices
  воспроизводят overlap после динамического текста, требуется принудительный layout rebuild.
- Scope extension: GPL character prefab plus `BubbleScreen.cs`; existing wardrobe diff preserved.
- Heartbeat UTC: 2026-08-30T16:47:30Z.
- Result: GPL Viewport offset increased to `Y = -220`; fallback `BubbleScreen`
  forces active root layout after dynamic text and button-pool updates.
- Validation: live repro captured in Play Mode; GPL release
  `33ca6c97122a3016795e1e3a6c7643311e71e5076195c35e978c751f42ade0c7`
  built/composed; fresh Novels Editor compile passed with no compiler errors.
- Pending: visual replay at Ink lines 58 and the three-choice frame.
- Resume: user reported TZM custom bubble regression caused by the global layout rebuild;
  scope narrows the behavior to the game fallback prefab only.
- Result: shared base defaults restored; rebuild is opt-in and enabled only by the
  Novels fallback bubble variant. TZM custom presentation remains opted out.
- Validation: TZM and GPL editor builds passed; TZM release `48adc0d999f2b6a13d0eba8ba18fb00f94b35f9863226ad1173c8eb3e179c192`,
  GPL release `33ca6c97122a3016795e1e3a6c7643311e71e5076195c35e978c751f42ade0c7`;
  fresh Novels compile passed without compiler errors; Editor left open.
- Pending: visual replay of TZM line 63 and GPL three-choice fallback.
