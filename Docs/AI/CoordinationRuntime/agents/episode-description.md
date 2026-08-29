# Agent: episode-description

- Статус: completed
- Задача: извлечь описание эпизода из Ink и показать в каталоге эпизодов.
- Область: точечные Content runtime/editor/Game файлы и definitions TZM/ZDM.
- Ожидаемые файлы: `NovelContentAsset.cs`, `NovelDefinition.cs`, authoring,
  `CatalogFlow.cs`, `tzm.asset`, `zdm.asset` и собственные coordination-файлы.
- Результат: описание извлекается из Ink, сериализуется в Episode и
  отображается существующей карточкой выбора эпизода.
