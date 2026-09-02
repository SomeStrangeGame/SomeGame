# Agent: `tzm-wardrobe-arrow-spacing-fix`

- Status: ready-for-user-visual-check
- Task: исправить направление стрелок карусели TZM wardrobe и увеличить расстояние между кнопкой сворачивания и верхом панели.
- Scope: `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/OptionListScreen.cs`, `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/Resources/OptionListScreen.prefab`, `Projects/novels-tzm/Assets/Presentation/wardrobe/screen-variant.prefab`, focused TZM/Unity validation, own coordination records and shared handoff.
- Contract: TZM prefab остаётся variant общего authored fallback; shared fallback сохраняет текущее поведение и геометрию по умолчанию; изменение направления использует фактическую ориентацию утверждённого TZM sprite; Ink, saves и чужие assets не затрагиваются.
- Base commit: `69d77aa9c04d6b104acd5cea32c074b2778802ff` plus preserved dirty tree.
- Requested UTC: `2026-09-01T15:28:14Z`
- Evidence: пользовательский portrait screenshot показывает, что левая item-arrow направлена вправо, правая — влево, а collapse-chevron визуально касается верхней границы панели.
- Validation plan: scoped serialization/diff checks, TZM validate/editor build, attached Novels compile; ручной portrait replay остаётся пользовательским gate.
- Result: ориентация утверждённого left-pointing sprite теперь сохранена у previous и зеркалится только у next; expanded collapse position вынесена в наследуемое serialized поле, fallback сохраняет `635`, TZM variant переопределяет `700` и получает 27 px визуального зазора с учётом высоты кнопки 96 px.
- Inheritance: TZM variant по-прежнему наследует shared fallback GUID `c70fbd96d8d6443329e9d10a73f0428a`; root не распакован и не дублирован.
- Validation: scoped serialization/diff checks passed; `novels-content validate tzm` и `build tzm editor` passed; release build log содержит variant и оба arrow sprites; attached Novels compile passed без compiler errors.
- Next: ручной portrait replay пользователем; проверить направления обеих стрелок и отступ collapse-chevron над раскрытой панелью.
