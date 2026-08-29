# Agent: story-content-version

- Статус: completed
- Задача: перенести ContentVersion из Episodes на уровень истории.
- Область: точечные Content runtime/editor/Game файлы и definitions TZM/ZDM.
- Ожидаемые файлы: `NovelContentAsset.cs`, `NovelDefinition.cs`,
  `StoryInkAuthoring.cs`, `NovelContentAssetEditor.cs`, `NovelRuntime.cs`,
  `NovelRuntime.Content.cs`, `NovelProgress.cs`, `tzm.asset`, `zdm.asset` и
  собственные coordination-файлы.
- Результат: версия хранится один раз на уровне истории; Content, Editor и
  Game runtime успешно скомпилированы Unity Roslyn.
