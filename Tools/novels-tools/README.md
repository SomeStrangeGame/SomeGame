# novels-content

Единственный CLI для контентных Unity-проектов:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate <catalog|story-id|all>
Tools/novels-tools/novels-content build <catalog|story-id|all> <editor|android|ios>
Tools/novels-tools/novels-content publish <destination-directory>
```

`build` автоматически компонует результат для Game. Проекты обрабатываются
последовательно, поэтому команда безопасна для больших наборов контента.

Для `editor`, `android` и `ios` сохраняются независимые Unity `Library`.
Активный кэш находится в обычном `<project>/Library`, неактивные — в
`<project>/Build/UnityLibraryCache/<platform>`. Первая сборка платформы остаётся
холодной, следующие не переимпортируют все текстуры после сборки другой
платформы. Перед переключением проект должен быть закрыт в Unity.
Для полного холодного импорта удалите обычный `<project>/Library` и каталог
`<project>/Build/UnityLibraryCache`; оба содержат только генерируемые данные.
