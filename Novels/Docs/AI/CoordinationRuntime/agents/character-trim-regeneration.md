# Agent: character-trim-regeneration

- Статус: completed
- Задача: добавить безопасную перегенерацию и Inspector-кнопки trim manifest.
- Область:
  - `Packages/NovelsContentSdk/Editor/CharacterSpriteAlphaTrim.cs`
  - `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterSpriteTrimManifest.cs`
  - `Novels/Docs/AI/ContentAuthoringGuide.md`
- Ожидаемые изменения: хеш уже обрезанного PNG, миграция старых записей без повторной обрезки, проверка/перегенерация из Inspector.
- Результат: безопасная hash-aware перегенерация и Inspector-кнопки готовы;
  Unity Roslyn compile и статическая проверка 890 существующих записей успешны.
- Последнее обновление: `2026-08-27T15:28:44Z`
