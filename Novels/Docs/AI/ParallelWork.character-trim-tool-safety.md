# Parallel work: character trim tool safety

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, исправление UX и границ записи sprite trim
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/CharacterSpriteAlphaTrim.cs`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- `Novels/Docs/AI/ParallelWork.character-trim-tool-safety.md`
- собственные coordination-файлы и append-only запись в `CoordinationRuntime/HANDOFF.md`

## Не изменять

- PNG, `.meta` и `sprite-trim-manifest.asset` историй
- runtime schema манифеста
- `Projects/novels-zdm/**`
- остальные shared packages и Game runtime
- чужие coordination-файлы

## Изменяемый контракт

- Inspector сначала показывает план; обновление индекса и физическая обрезка PNG становятся разными явными действиями.

## План проверки

- Unity Roslyn для актуальной TZM Editor assembly без запуска второго Unity.
- Статическая проверка веток preview/index/trim и scoped `git diff --check`.

## Выполнено

- Inspector начинает с read-only preview и показывает отдельные счётчики и
  список только тех PNG, которые действительно будут физически обрезаны.
- Обновление индекса вынесено в отдельное действие, которое сохраняет только
  manifest и никогда не открывает PNG на запись.
- Физическая обрезка недоступна при нулевом списке, требует подтверждение с
  путями и повторно проверяет полный план, SHA-256 и crop-геометрию до записи.
- Слово `Перегенерировать` удалено из актуального Inspector и инструкции;
  результаты явно сообщают количество изменённых PNG.

## Проверено

- Unity Roslyn `Novels.Character.rsp` — успешно.
- Unity Roslyn `Novels.ContentSdk.Editor.rsp` — успешно после финальной правки.
- Текущий TZM: 435 PNG и 435 хешей manifest; все SHA-256 совпадают,
  missing/hash mismatch 0/0.
- `novels-content doctor` — успешно.
- Scoped `git diff --check` — успешно.

## Требуется при интеграции

- Выполнить refresh открытого TZM Editor и визуально проверить новый Inspector.
- На текущем TZM preview должен показать 0 PNG к обрезке; обе записывающие
  кнопки должны быть disabled.
- Полный Unity validate/build не запускался из-за живого TZM UnityLockfile.
