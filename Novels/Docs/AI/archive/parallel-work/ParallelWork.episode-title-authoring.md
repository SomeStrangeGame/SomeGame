# Parallel work: episode title authoring

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aa`
- Ответственный поток: Episode title extraction from Ink
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/StoryInkAuthoring.cs`
- `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/definition/tzm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Novels/Docs/AI/archive/parallel-work/ParallelWork.episode-title-authoring.md`
- собственные файлы в `Novels/Docs/AI/CoordinationRuntime/`

## Не изменять

- runtime StoryCommands и список metadata/ignored prefixes в этом блоке
- исходные и скомпилированные Ink/source-map артефакты
- остальные поля definitions, включая разметку чанков
- Catalog и чужие status-файлы

## Изменённые контракты

- Кнопка `Обновить эпизоды` берёт title из первой строки Ink формата
  `... (Название истории): Серия N: Название эпизода`.
- Если строка отсутствует, остаётся fallback `Сезон N, эпизод M`.

## Атомарные блоки

1. Editor-only title parser.
2. Точечная миграция titles TZM/ZDM.
3. Editor compilation и handoff.

## Выполнено

- `StoryInkAuthoring` читает каждый episode source один раз, берёт первое
  авторское название из narrator-строки `Серия N: ...` и использует прежний
  ID-based title только как fallback.
- Titles TZM/ZDM обновлены по текущим Ink: 6 авторских + 1 fallback для TZM,
  11 авторских для ZDM.
- Исходные Ink и остальные episode metadata не менялись.

## Проверено

- Сверка authoring regex с root INCLUDE и двумя definitions — все 18 titles
  совпали (TZM 7, ZDM 11).
- Unity 6000.3.11f1 Roslyn по TZM `Novels.ContentSdk.Editor.rsp` — успешно.
- C# whitespace check и scoped diff check — успешно; в Unity YAML сохранены
  прежние trailing spaces вне title-строк.

## Требуется при интеграции

- Package refresh и визуальная проверка списка Episodes.
- Отдельным атомарным блоком перенести ignored metadata prefixes в story
  asset. Рекомендуемый контракт: `_ignoredStoryLinePrefixes` в каждом
  `NovelContentAsset`, передаваемый в live parser и `ReplayValidator`.
- Для текущих историй явно задать `Название`, `Серия`, `Описание`, `Жанры`,
  `Аннотация`, `Статы`. Parser должен принимать и `Префикс: значение`, и
  `Префикс значение`, потому что ZDM использует обе формы.
- Общий `StoryCommands` не должен зависеть от Content SDK; он принимает только
  переданную коллекцию строк. В этом блоке runtime parser намеренно не менялся.
