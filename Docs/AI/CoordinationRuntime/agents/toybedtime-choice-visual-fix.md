# Agent: `toybedtime-choice-visual-fix`

- Status: ready-with-limitations
- Task: исправить кастомные choice-кнопки, у которых фон перекрывает изображение и подпись.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/Bubble/View/ChoiceButtonIcon.cs`, own coordination records and existing toybedtime handoff line.
- Evidence: после корректного Sprite import кастомный жёлтый background появляется, но обе кнопки визуально пусты; обычный Bubble остаётся исправен.
- Validation: shared SDK compile, toybedtime validate/build if needed, portrait replay, scoped diff-check.
- Base: `a9ff1e134459`
- Requested UTC: `2026-09-03T14:30:22Z`.
- Result: отдельный перекрывающий `ChoiceBackground` больше не создаётся; helper использует исходный фон Button и гарантирует порядок icon-before-text над фоном.
- Validation result: live Novels Editor завершил auto-compile (`ready`), compiler console errors empty; scoped `git diff --check` passed. Pending: user portrait replay.
- Completed UTC: `2026-09-03T14:36:00Z`.
