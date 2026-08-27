# Content Pipeline

Весь рабочий процесс доступен через один исполняемый файл из корня репозитория:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate all
Tools/novels-tools/novels-content build all android
```

Команды выполняют проекты строго последовательно: сначала каталог, затем
истории. Это важно для проектов с большим объёмом графики.

`build` хранит отдельный Unity `Library` для `editor`, `android` и `ios`.
Активная платформа использует обычный `<project>/Library`, а неактивные кэши
лежат в игнорируемом `<project>/Build/UnityLibraryCache`. Переключение — это
быстрое перемещение каталогов на том же диске, а не повторный импорт всех
текстур. Первая сборка каждой платформы всё равно прогревает собственный кэш.
Контентный проект во время переключения должен быть закрыт в Unity.

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
Inspector не используются. Для пилотного TZM SDK берёт Unity-ресурсы из
коротких каталогов `Assets/Characters`, `Assets/Locations`, `Assets/Choices`,
`Assets/Presentation` и definition из корня `Assets`; legacy-проекты пока
продолжают использовать `Assets/RemoteAssets`. Физические пути при сборке
отображаются в стабильные runtime-адреса, поэтому структура проекта не является
публичным ключом AssetBundle.

TZM Ink и медиа берутся из `Assets/Ink`, `Assets/Video`, `Assets/Audio` и
публикуются под прежними namespaced-путями `noveltexts/<id>`,
`novelsvideos/<id>`, `novelsaudio/<id>`. Legacy `StreamingAssets` остаётся
поддержанным до отдельной миграции остальных историй.

## Отчёт валидации

Валидатор не останавливается на первой найденной проблеме. За один проход он
собирает объективные ошибки и выводит их одним сообщением, сгруппированным по
стабильному коду. При наличии ошибок сборка не начинается.

Тип атомарного проекта определяется единственным файлом-маркером:
`Config/catalog.json` для каталога или `Config/card.json` для истории. Оба файла
одновременно и отсутствие обоих считаются ошибкой.

`ContentValidator` последовательно проверяет конфиг, definition/Ink и bundle,
после чего возвращает компактный `ContentBuildPlan` для сборки. Новую
объективную проверку добавляйте в этот маршрут; собственная точка запуска или
отдельный формат лога не нужны.

Базовая последовательность правил:

```text
project marker
  -> configuration
  -> story / catalog content
  -> bundle inputs
```

Story-specific исключения не добавляются в общий SDK. Они должны оставаться
данными соответствующего контентного проекта.

Визуальное соответствие, смысл сцены и качество переходов автоматикой не
определяются. Их проверяют по
[ManualContentChecklist.md](ManualContentChecklist.md).

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
