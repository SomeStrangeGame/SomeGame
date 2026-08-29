# Agent: tzm-shared-presentation-font

- Статус: completed
- Задача: Объединить три идентичные копии Liberation Sans Regular TZM в один общий font asset.
- Область: TZM Presentation fonts, три presentation prefab, `tzm.asset`, authoring-документация и собственные coordination-файлы.
- Ожидаемый результат: один шрифт в `Assets/Presentation/Fonts`, сохранённый canonical GUID, без missing references; validation и bundle build успешны.

- Результат: три копии Liberation Sans сведены к одному font asset; все 14
  prefab references используют сохранённый GUID, validation/build успешны.
