# Project memory

## Назначение

`SomeGame` — репозиторий Unity-приложения визуальных новелл, общего Content SDK,
локального build tooling и отдельных Unity-проектов каталога и историй.

Канонический workspace и coordination root — Git-корень `SomeGame`; локальная
родительская папка `Fork` является только контейнером и не считается проектом.

## Состав репозитория

| Путь | Ответственность |
| --- | --- |
| `Novels` | Game runtime, стартовая сцена, save/cache/fallbacks и Player |
| `Packages/NovelsContentSdk` | общие runtime-контракты и Editor content pipeline |
| `Packages/NovelInk` | Ink-команды, runtime и source map |
| `Packages/Bundles` | release, content source, integrity, cache и bundles |
| `Projects/novels-catalog` | визуальный каталог и catalog bundle |
| `Projects/novels-<storyId>` | атомарный authoring-проект одной истории |
| `Projects/novels-content-template` | шаблон нового story project |
| `Tools/novels-tools` | последовательная локальная оркестрация контента |
| `Tools/unity-mcp-helper` | bounded взаимодействие с живым Unity Editor |
| `Docs/AI` | канонические AI-протоколы, память и история |

Текущие production stories определяются реестром каталога, а не вручную
поддерживаемым списком в memory bank.

## Постоянные ограничения

- Unity Editor, импорт и сборки — единый эксклюзивный тяжёлый ресурс репозитория.
- Рабочее дерево может одновременно содержать изменения нескольких потоков;
  незнакомые изменения не откатываются и не включаются в чужой scope.
- Game не владеет authoring content pipeline; истории и Catalog собираются
  отдельными проектами через общий SDK.
- Generated `Library`, `Build`, `Temp`, логи и publish output не являются
  контрактом и не переносятся в memory bank.
- Текущие версии, ветка и dirty state всегда проверяются по файлам/командам.

Подробности: [UnityProjectContext.md](../architecture/UnityProjectContext.md) и
[MultiProjectSplitPlan.md](../architecture/MultiProjectSplitPlan.md).
