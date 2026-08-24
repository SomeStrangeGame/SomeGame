# Content Pipeline

Весь рабочий процесс доступен через один исполняемый файл из корня репозитория:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate all
Tools/novels-tools/novels-content build all android
```

Команды выполняют проекты строго последовательно: сначала каталог, затем
истории. Это важно для проектов с большим объёмом графики.

## Команды

Проверить окружение и конфигурацию без Unity:

```bash
Tools/novels-tools/novels-content doctor
```

Запустить полную Unity-валидацию одного проекта или всех проектов:

```bash
Tools/novels-tools/novels-content validate tzm
Tools/novels-tools/novels-content validate all
```

Собрать один проект или всё содержимое для одной платформы:

```bash
Tools/novels-tools/novels-content build catalog editor
Tools/novels-tools/novels-content build tzm android
Tools/novels-tools/novels-content build all ios
```

Допустимые платформы: `editor`, `android`, `ios`. После успешной сборки CLI
сам обновляет `Novels/Build/LocalContent`; отдельной команды compose больше нет.

Сборка пересоздаёт только `Remote/<выбранная-платформа>` и сохраняет ранее
собранные платформы. Поэтому последовательные `build all android` и
`build all ios` формируют единое серверное дерево. Content-addressed `Files`
дополняются; конкретный release использует только перечисленные в нём payloads.

Опубликовать уже собранное дерево:

```bash
Tools/novels-tools/novels-content publish /absolute/server/root
```

## Конфигурация

- каталог: `Projects/novels-catalog/Config/catalog.json`;
- история: `Projects/novels-<storyId>/Config/card.json`;
- `minimumClientVersion` хранится прямо в соответствующем JSON;
- отдельного `Config/build.json` больше нет;
- новый story-project обнаруживается по `Config/card.json`.

Каждый атомарный проект создаёт ровно один bundle. Ручные AssetBundle labels в
Inspector не используются: SDK явно включает содержимое `Assets/RemoteAssets`.
Ink и прочие потоковые файлы записываются как content-addressed payloads.

## Отчёт валидации

Валидатор не останавливается на первой найденной проблеме. За один проход он
собирает полный отчёт и выводит:

- одно сгруппированное сообщение со всеми предупреждениями;
- одно сгруппированное сообщение со всеми ошибками.

При наличии ошибок сборка не начинается.

Правила реализуют `IContentValidationRule` и последовательно получают один
`ContentProject` и один `ValidationReport`. Новую проверку следует оформлять
отдельным небольшим правилом и добавлять в список `ContentValidator`; создавать
ещё одну точку запуска или собственный формат лога не нужно.

Базовая последовательность правил:

```text
ProjectStructureRule
  -> ConfigurationRule
  -> StoryRule / CatalogRule
  -> BundleRule
```

Story-specific исключения не добавляются в общий SDK. Они должны оставаться
данными соответствующего контентного проекта.

## Выходное дерево

```text
LocalContent/
  catalog/
    registry/catalog.json
    ui/Remote/<platform>/release.json
    ui/Remote/<platform>/<bundle>/<version>
  stories/<storyId>/
    card.json
    cover.<extension>
    Files/<sha256>.bin
    Remote/<platform>/release.json
    Remote/<platform>/<bundle>/<version>
```

Editor читает это дерево через `FileSystemContentSource`, Android/iOS — через
`HttpContentSource`. Каталог и истории публикуются независимо.
