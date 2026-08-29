# Agent: story-path-convention

- Статус: completed
- Задача: удалить ручной compiled Ink path и вывести его из story ID.
- Область: точечные Content runtime/editor/Game файлы и definitions TZM/ZDM.
- Ожидаемые файлы: `NovelContentAsset.cs`, `NovelDefinition.cs`, Inspector,
  authoring, validator, runtime consumers, `tzm.asset`, `zdm.asset` и
  собственные coordination-файлы.
- Результат: сериализуемый compiled Ink path удалён; runtime выводит
  `<story-id>.ink.json`, authoring проверяет имя корневого Ink.
