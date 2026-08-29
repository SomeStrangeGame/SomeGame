# Agent: episode-schema-dedup

- Статус: completed
- Задача: перенести дублирующий StoryPath из Episodes на уровень истории.
- Область: точечные Content runtime/editor/Game файлы и definitions TZM/ZDM.
- Ожидаемые файлы: `NovelContentAsset.cs`, `NovelDefinition.cs`,
  `StoryInkAuthoring.cs`, `NovelContentAssetEditor.cs`, validator,
  `NovelRuntime.cs`, `NovelRuntime.NovelPreparation.cs`, `tzm.asset`,
  `zdm.asset` и собственные coordination-файлы.
- Результат: `StoryPath` хранится один раз на уровне истории; Content,
  Editor и Game runtime скомпилированы Unity Roslyn.
