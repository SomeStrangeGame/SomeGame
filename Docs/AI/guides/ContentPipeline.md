# Content Pipeline

Весь рабочий процесс доступен через один исполняемый файл из корня репозитория:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content plan
Tools/novels-tools/novels-content verify editor
Tools/novels-tools/novels-content validate all
Tools/novels-tools/novels-content build all android
```

Команды выполняют проекты строго последовательно: сначала каталог, затем
истории. Это важно для проектов с большим объёмом графики.

Этот документ является каноническим справочником семантики content-команд.
Общий порядок FIFO, write-lock и единственного тяжёлого процесса задаёт
[UnityConcurrency.md](../rules/UnityConcurrency.md); повторения этого инварианта
ниже являются локальными safety reminders, а не отдельными правилами очереди.

## Базовый уровень лицензии Unity

Все Unity-проекты репозитория, content pipeline, Editor-автоматизация и Player-
сборки обязаны полностью работать на Unity Personal. Запрещены любые исходники,
пакеты, сервисы, Project Settings, Build Profiles, Editor API, build-флаги и
pre/post-build шаги, чья доступность или результат зависят от Unity Pro,
Enterprise, Industry либо другой платной license entitlement.

Запрет действует и временно: нельзя включать платную функцию для импорта,
генерации, диагностики, конвертации или получения артефакта, а затем коммитить
только её результат. Обычная функция Unity не становится запрещённой лишь
потому, что Editor запущен под платной лицензией; критерием является её
доступность и одинаковая семантика на Unity Personal.

Если license-tier независимость операции не подтверждена, она считается
запрещённой до проверки или замены на Personal-совместимый путь. Отсутствующий
entitlement нельзя обходить, маскировать fallback-ом или принимать как
environment-only limitation. Такая зависимость блокирует validation/build и
фиксируется как дефект проекта или tooling.

`build` хранит отдельный Unity `Library` для `editor`, `android` и `ios`.
Активная платформа использует обычный `<project>/Library`, а неактивные кэши
лежат в игнорируемом `<project>/Build/UnityLibraryCache`. Переключение — это
быстрое перемещение каталогов на том же диске, а не повторный импорт всех
текстур. Первая сборка каждой платформы всё равно прогревает собственный кэш.
Контентный проект во время переключения должен быть закрыт в Unity.

## Минимальный changed-path gate

Перед Unity-командой сначала выполняется
`Tools/novels-tools/novels-content plan [base-ref]`. План классифицирует staged,
unstaged и untracked пути и выбирает минимально достаточную проверку:

- только `Docs/AI` — static checks, Unity не запускается;
- `Tools/unity-mcp-helper` — dependency-free unit tests, без Unity;
- один `Projects/novels-<id>` — только этот content target;
- общий `Packages/**` — catalog и production stories из текущего catalog
  `stories`, без автоматического включения WIP-проектов;
- Game scripts — один persistent MCP `editor-check --compile` и узкие EditMode
  tests;
- Player Settings/Build Profiles — Editor gate и ровно один целевой Player
  build;
- UI или визуальные assets — автоматические gates плюс отдельная ограниченная
  ручная визуальная проверка.

Детерминированную часть выполняет
`Tools/novels-tools/novels-content verify editor [base-ref]`: один `doctor`,
helper tests при необходимости, только затронутые content builds строго
последовательно и один `compose`. Финальный компактный JSON перечисляет
оставшиеся Editor/Player/manual gates. Полный лог успеха не передаётся в чат;
он читается только при failure.

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

## Android Embedded build и немедленный smoke в эмуляторе

Android Player с вшитым контентом после успешной сборки не считается
проверенным, пока тот же APK не установлен и не запущен в Android-эмуляторе.
Операция подчиняется общей FIFO/write-lock очереди и эксклюзивному Unity-
ресурсу: Editor, Hub, content build и Player build выполняются строго
последовательно. Открытый Editor можно закрывать только после явного
разрешения пользователя и проверки несохранённого состояния.

### 1. Подготовить фактический release-set

Перед build нужно повторно прочитать
`Projects/novels-catalog/Config/catalog.json`. В APK включаются каталог и
ровно истории из `stories`; незавершённый атомарный проект вне каталога не
должен случайно менять release-set.

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content build catalog android
Tools/novels-tools/novels-content build tzm android
Tools/novels-tools/novels-content build zdm android
```

`build all android` допустим, только когда все обнаруживаемые атомарные
проекты production-ready. Если он остановился на WIP-проекте, нельзя править
его config или скрывать ошибку ради APK: сверяют catalog `stories`, фиксируют
WIP в handoff и последовательно собирают только фактический release-set.

Перед каждой Unity-командой проверяются реальные Unity/Hub/import/build-
процессы и licensing IPC. Пустой `Temp/UnityLockfile` удаляется только после
подтверждения, что у него нет процесса-владельца; широкая очистка `Temp`,
`Library` или чужих cache запрещена.

### 2. Собрать development APK для эмулятора

Для локального smoke используется Embedded development build с Unity debug
keystore. Release build не должен автоматически понижаться до debug signing:
ему нужны штатные пароли production keystore.

```bash
version=$(date -u +%Y.%m.%d)
build=$(( ($(date -u +%s) - 1577836800) / 60 ))
apk="$PWD/Novels/Build/Players/$version/$build/Android/Embedded/Novels.apk"

NOVELS_PLAYER_VERSION="$version" \
NOVELS_PLAYER_BUILD_NUMBER="$build" \
Novels/Tools/build-player.sh Embedded Android "$apk" '' --development
```

Обязательные post-build проверки: exit code `0`, строка
`Build Finished, Result: Success`, существующий APK, версия/build number и
путь лога. Артефакт остаётся в ignored `Novels/Build`; бинарный APK не
коммитится.

Android Embedded читает `Application.streamingAssetsPath` внутри APK по
`jar:file://`. Этот путь должен обслуживаться через `UnityWebRequest`/
`StreamingAssetsContentSource`, а не через обычный filesystem API. Ошибки
`Content path must remain inside the configured root` или отсутствие embedded
`release.json` являются блокирующей регрессией.

### 3. Установить и сразу запустить тот же APK

Используется ARM64 AVD `Novels_Pixel_7_API_34` либо явно согласованный
эквивалент. Перед установкой подтверждают состояние `device`, а package ID
берут из текущих Player Settings/manifest APK, не угадывают по названию игры.

```bash
adb devices -l
adb -s emulator-5554 wait-for-device
adb -s emulator-5554 shell am force-stop "$package_id"
adb -s emulator-5554 install -r -d "$apk"
adb -s emulator-5554 logcat -c
adb -s emulator-5554 shell monkey \
  -p "$package_id" -c android.intent.category.LAUNCHER 1
```

Краткий `offline` сразу после перезапуска adb не считается успехом или
падением: нужно снова выполнить `wait-for-device`, а затем повторно проверить
`device`. Установка считается успешной только по `Success` от `adb install`.

### 4. Обязательный runtime/log gate

После запуска ждут завершения bootstrap и фиксируют точный PID. Logcat
очищается непосредственно перед launch, затем сохраняется по PID, чтобы
системный шум и старые падения не смешивались с текущим запуском.

```bash
sleep 15
pid=$(adb -s emulator-5554 shell pidof "$package_id" | tr -d '\r')
test -n "$pid"
adb -s emulator-5554 shell dumpsys activity activities | \
  rg 'topResumedActivity|UnityPlayerGameActivity'
adb -s emulator-5554 logcat -d -v threadtime --pid="$pid" \
  > Novels/Build/Logs/android-emulator-logcat.txt
```

Минимальный успешный smoke одновременно подтверждает:

- package process жив, а `UnityPlayerGameActivity` находится foreground;
- состояние каталога достигнуто по ожидаемому runtime-маркеру, без fallback
  ошибки загрузки;
- catalog release активирован;
- открытие истории активирует её release и доходит до episode runtime;
- нет `FATAL EXCEPTION`, ANR, native crash, Unity error-level сообщений,
  `INITIALIZATION_FAILED`, version/schema failure или content-source failure;
- приложение можно штатно остановить через `am force-stop`, после чего его PID
  исчезает, но сам эмулятор остаётся в состоянии `device`.

Для повторяемого ADB integration smoke успешный прогон не делает и не
анализирует screenshot. Его результат определяется только процессом/activity,
ожидаемыми структурированными Unity-маркерами, отсутствием блокирующих ошибок и
тайм-аутами шагов. Screenshot без эталонного сравнения не является отдельным
доказательством корректности и не добавляется в success evidence.

Runtime-маркеры имеют однострочный JSON-формат с префиксом
`[NOVELS_SMOKE] `. Поля `v`, `seq`, `runId` и `event` обязательны; остальные
поля зависят от события. Smoke-runner читает только этот префикс и проверяет
порядок событий внутри одного `runId`, не разбирая человекочитаемые сообщения.
Текущий минимальный контракт:

- `app.started`;
- `catalog.loading` → `catalog.ready` либо `catalog.load_failed`;
- `story.selected` → `release.activated`;
- `episode.selected` → `episode.ready`;
- `dialogue.ready`, опционально `choice.selected`;
- `episode.completed` или `catalog.returned`;
- `error` для любого `NovelError`;
- `fallback.used` при фактической подстановке fallback-ассета.

Любой `fallback.used` с `assetType=character` считается блокирующим smoke-
failure, даже если fallback был виден только один кадр и процесс продолжает
работать. Событие содержит `contentId`, `episodeId`, `characterId` и `reason`;
после него runner собирает failure-only screenshot/logcat/activity evidence.
Обычные текстовые bundle/cache сообщения не являются стабильным test API.

Screenshot и расширенная диагностика собираются только при failure:

- `FATAL EXCEPTION`, ANR, native crash, Unity error-level сообщение или другая
  блокирующая ошибка;
- ожидаемый маркер шага не появился за заданный сценарием тайм-аут;
- package process исчез до завершения сценария;
- foreground activity отличается от ожидаемой;
- тест не может перейти к следующему детерминированному состоянию.

При таком failure до остановки приложения сохраняют screenshot, полный logcat
окна текущего теста и `dumpsys activity`; если PID ещё жив, дополнительно
сохраняют PID-filtered logcat. Только после сбора этих артефактов screenshot
передаётся на визуальный анализ. Отсутствие screenshot при успешном прогоне —
ожидаемое поведение, а не неполная проверка.

```bash
adb -s emulator-5554 exec-out screencap -p \
  > Novels/Build/Logs/android-emulator-failure.png
adb -s emulator-5554 logcat -d -v threadtime \
  > Novels/Build/Logs/android-emulator-failure-logcat.txt
adb -s emulator-5554 shell dumpsys activity activities \
  > Novels/Build/Logs/android-emulator-failure-activity.txt
```

Строка `TrySetException` внутри info-level stack trace сама по себе не является
ошибкой: текущий cache-miss маршрут использует исключение проверки, затем
пишет `Download content ... because its cache is invalid` и обязан завершиться
`Activate content release`. Оценивать нужно полную цепочку, а не совпадение
слова `Exception`.

Сообщения эмулятора `EGL_BAD_CONFIG`, fallback с ES 3.2/3.1 и
`ASTC... is not supported, decompressing texture` допустимы только когда игра
остаётся foreground и функциональный gate проходит. Они означают, что этот
AVD не доказывает качество ASTC, размер текстур в GPU-памяти, FPS, нагрев или
реальную device-совместимость. Compression/visual/performance gate выполняется
отдельно на физическом ASTC-capable Android-устройстве.

После пользовательского smoke приложение закрывают, не выключая AVD:

```bash
adb -s emulator-5554 shell am force-stop "$package_id"
test -z "$(adb -s emulator-5554 shell pidof "$package_id" | tr -d '\r')"
adb -s emulator-5554 devices -l
```

В handoff записываются APK path/version/build/size, AVD/serial, package ID,
результат install/foreground/release/episode gate, точные блокирующие ошибки и
отдельно emulator-only warnings. Для успеха достаточно PID, activity и
log-marker evidence; screenshot не нужен. При failure дополнительно указываются
тайм-аут или нарушенное ожидаемое состояние и пути к собранным screenshot,
полному logcat и activity dump.

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

Editor читает это дерево через `FileSystemContentSource`, Remote Android/iOS —
через `HttpContentSource`, Android Embedded внутри APK — через
`StreamingAssetsContentSource`. Каталог и истории публикуются независимо.
