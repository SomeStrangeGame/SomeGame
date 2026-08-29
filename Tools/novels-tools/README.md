# novels-content

Единственный CLI для контентных Unity-проектов:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content plan [base-ref]
Tools/novels-tools/novels-content verify [editor|android|ios] [base-ref]
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

`plan` классифицирует staged, unstaged и untracked пути и возвращает компактный
JSON: затронутые content targets, необходимость Editor compile/tests, Player
build и ручного visual gate. Список путей ограничен первыми 40 элементами;
`path_count` сохраняет полный размер dirty set.

`verify` выполняет только детерминированные gates плана: helper unit tests и
последовательные content builds затронутых targets с одним `doctor` и одним
`compose`. Editor/Player/manual gates не запускаются скрыто: они явно остаются
в финальном JSON и выполняются агрегированным MCP `editor-check` либо целевой
Player-командой под общим write-lock.
