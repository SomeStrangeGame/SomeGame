# Multi-project split

Актуальный план разделения проекта. Документ является точкой опоры для
последовательного переноса и заменяет прежнюю схему с embedded-контентом.

## Целевая система

| Часть | Владелец | Результат |
|---|---|---|
| Game runtime, save, cache, fallbacks | `novels-game` | APK/AAB/IPA |
| Общие runtime-контракты и Editor pipeline | `novels-content-sdk` | Unity Package |
| Шаблон новой истории | `novels-content-template` | Unity project template |
| Визуальный каталог | `novels-catalog` | Catalog UI bundle |
| TZM | `novels-tzm` | Один Story bundle на платформу |
| ZDM | `novels-zdm` | Один Story bundle на платформу |
| Локальная оркестрация | `novels-tools` | CLI |

## Неподвижные правила

- Обратная совместимость со старым delivery-форматом не требуется.
- История собирается целиком; эпизодных bundles и `shared` в целевой схеме нет.
- Исходные данные карточки и обложка принадлежат проекту истории.
- Catalog UI содержит только представление; центральный реестр содержит только
  `storyId`, порядок и доступность.
- Editor читает готовые releases из `Build/LocalContent`.
- Android и iOS читают те же контракты с HTTP(S)-сервера.
- Runtime не читает authoring assets через `AssetDatabase`.
- Контентные bundles не попадают в `StreamingAssets` или Player.
- Сервер сборки пока не создаётся; build и publish запускаются локально.

## Разрешённые зависимости

```text
novels-game    -> ContentSDK Runtime
novels-catalog -> ContentSDK Runtime + Editor
novels-tzm     -> ContentSDK Runtime + Editor
novels-zdm     -> ContentSDK Runtime + Editor
```

SDK не зависит от Game или конкретной истории. Истории не зависят друг от
друга. Catalog UI не знает конкретных историй.

## Runtime delivery

`IContentSource` является единственной инфраструктурной границей:

- Editor: `FileSystemContentSource(Build/LocalContent)`;
- Player: `HttpContentSource(baseUrl)`.

Разбор release, проверка SHA-256, кеширование, атомарная активация и загрузка
AssetBundle остаются общими.

## Форматы публикации

```text
catalog/
  registry/catalog.json
  ui/Remote/<platform>/release.json
  ui/Remote/<platform>/<bundles>

stories/<storyId>/
  card.json
  cover.<extension>
  Remote/<platform>/release.json
  Remote/<platform>/<story-bundle>
  Files/<sha256>.bin
```

Локальный root полностью повторяет серверный контракт. Editor выбирает свою
платформенную секцию через тот же `ContentPlatform`, что и Player.

## Карта текущего владения

### Game

- `Assets/Novels/Application*`, `EntryPoint`, `NovelRuntime*`;
- `CatalogFlow`, `ContentDeliveryFlow`, `Save`;
- `Assets/Novels/Fallbacks`;
- стартовая сцена и bootstrap.

### ContentSDK Runtime

- `ContentAddressing`, `Content`, `Diagnostics`;
- `BubbleContracts`, `ChooseContracts`, `WardrobeContracts`;
- Story execution/queue и общие UI controllers;
- Base Episode UI;
- инфраструктурная граница `IContentSource` из пакета Bundles.

### ContentSDK Editor

- `Assets/Editor` за исключением Player-specific build orchestration;
- анализ Ink, валидация, scaffolding, сборка и release generation.

### Catalog

- `Assets/RemoteAssets/catalog`;
- визуальная часть `Assets/Novels/Catalog/View` после выделения runtime-контракта.

### TZM / ZDM

- соответствующие корни `Assets/RemoteAssets/content/<storyId>`;
- соответствующие Ink, audio и video authoring sources.

### Удалено после переключения

- Game-owned content build profile, validator и bundle pipeline;
- `StreamingAssets` и authoring `RemoteAssets` из Game;
- embedded test Player, seed builder и Local/Embedded/Hybrid delivery branches;
- старый ScriptableObject-каталог и монолитные ServerRoot tools;
- эпизодные delivery groups и отдельные loading bundles.

## Порядок переноса

1. Перевести Editor на `Build/LocalContent`, не меняя release-контракт.
2. Исключить установку built release в `StreamingAssets/Remote`.
3. Выделить ContentSDK package и формальные catalog/card contracts.
4. Перейти на один bundle истории.
5. Вынести template, TZM, ZDM и Catalog проекты.
6. Добавить `novels-tools` для build-local/compose-local/publish-local.
7. Удалить старый delivery и authoring content из Game. Выполнено.
8. Выполнить Editor, Android и iOS сквозную проверку.
