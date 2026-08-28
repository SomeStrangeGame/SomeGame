# Authoring атомарных историй

Этот документ — стартовая точка для разработчика, сценариста или
контент-менеджера, который впервые открыл `novels-tzm` или `novels-zdm`.

## Что это за проекты

`Projects/novels-<storyId>` — не самостоятельные игры. В них нет игровых сцен и
собственного runtime-кода. Каждый проект хранит одну историю и производит один
AssetBundle для выбранной платформы вместе с файловыми payloads и
`release.json`. Исполняющий код находится в Game и общих пакетах.

```text
Ink + PNG + prefab + definition
                |
                +--> novels_content_<storyId>

StreamingAssets
                +--> Files/<sha256>.bin

bundle + files  +--> release.json
```

Перед любым изменением pipeline прочитайте
`ParallelRefactoringCoordination.md`. Текущие команды сборки и публикации
описаны в `ContentPipeline.md`, а поддерживаемый предметный синтаксис — в
`InkSyntax.md`. После автоматической проверки пройдите
[ручной чек-лист](ManualContentChecklist.md).

## Первые пять минут

1. Откройте `Config/card.json`: это публичная карточка и минимальная версия
   клиента.
2. В пилотном TZM откройте `Assets/tzm.asset`: это единая точка настройки
   runtime истории, эпизодов и aliases.
3. Откройте `Assets/Ink/tzm.ink`: он включает эпизодные Ink-файлы.
4. Unity-ресурсы TZM находятся непосредственно в `Assets/Characters`,
   `Assets/Locations`, `Assets/Choices` и `Assets/Presentation`.
5. До сборки запустите `Tools/novels-tools/novels-content validate <id>`.

Не пытайтесь запускать content-проект кнопкой Play: сцены намеренно отсутствуют.

## Карта проекта

```text
Projects/novels-tzm/
  Assets/
    tzm.asset                     контракт истории и эпизодов
    Ink/                          Ink source, compiled story и source map
    Characters/                   персонажи и trim manifest
    Locations/                    фоны
    Choices/                      изображения выборов
    Presentation/                 story-specific UI и его зависимости
      Fonts/                      общие шрифты Presentation-prefab
      setting/                    настройки UI истории
    Video/                        внешние видео
    Audio/                        внешнее аудио, если используется
  Config/
    card.json                     storyId, заголовок, версия и обложка
    cover.*                       обложка истории
  Build/LocalContent/             сгенерированный результат
```

`novels-zdm` пока остаётся на legacy-layout
`Assets/RemoteAssets/content/zdm/**` и
`Assets/StreamingAssets/noveltexts/zdm/**`. Его миграция выполняется отдельным
последовательным блоком после проверки TZM; смешивать две структуры внутри
одного story-проекта нельзя.

`Build`, `Library`, `Temp`, `Logs` и `obj` — генерируемые директории. Не
редактируйте их как источник данных и не очищайте во время параллельной Unity-
валидации другого проекта.

## Что редактируется, а что генерируется

| Данные | Роль | Редактировать вручную |
| --- | --- | --- |
| эпизодные `.ink` | авторский сценарий | да |
| корневой `<id>.ink` | список `INCLUDE` | да, осторожно |
| `<id>.ink.json` | исполняемая история | нет, результат компиляции |
| `*.source-map.json` | связь runtime с исходником | нет |
| `Config/card.json` | карточка и совместимость | да |
| `<id>.asset` | эпизоды и aliases | через Unity Inspector |
| PNG/prefab/MP4 | визуальный контент | да, сохраняя адресацию |
| `.meta` | GUID и import settings Unity | не удалять и не копировать вслепую |
| `Build/LocalContent` | публикационный результат | нет |

Source Ink остаётся authoring-входом и не публикуется. Runtime payload содержит
скомпилированный Ink и source map.

## Обязательные и опциональные части

| Часть | Статус | Правило |
| --- | --- | --- |
| `Config/card.json` | обязательна | `storyId` совпадает с ID definition |
| одна `Config/cover.*` | обязательна | имя совпадает с `card.cover` |
| один `NovelContentAsset` | обязателен | перечисляет хотя бы один эпизод |
| compiled Ink | обязателен | имя выводится из ID истории |
| episode source Ink | authoring-контракт | порядок задаётся `INCLUDE` корневого Ink |
| location/character assets | по использованию | имя является частью runtime-адреса |
| story presentation prefabs | опциональны | история может использовать общий presentation Game |
| видео | опциональны | `tzm` использует, `zdm` — нет |
| аудио | опционально | допустимые отсутствия задаются через silent audio IDs |

`tzm` — основной пример истории с видео, aliases и presentation overrides.
`zdm` — пример большой статической истории без видео и почти без собственных
presentation-prefabs.

`Assets/Presentation` предназначен только для story-specific UI/prefab и их
зависимостей. Арт персонажей и локаций участвует в расчёте чанков только из
`Assets/Characters` и `Assets/Locations` (для legacy-layout — из стабильных
`story/character/characters` и `story/location/locations`). Старые вложенные
каталоги `Presentation/character/characters` и
`Presentation/location/locations` не являются runtime roots и не должны
восстанавливаться при импорте. В TZM они удалены после успешных validation и
bundle build; актуальный story art находится только в поддерживаемых корнях.

Общие шрифты Presentation-prefab хранятся один раз в `Presentation/Fonts`.
Не копируйте один TTF в каталоги отдельных prefab: все ссылки должны вести на
единый font asset с одним Unity GUID.

### Постеры видеолокаций и чанки

В Inspector `NovelContentAsset` группа `Не используется` хранит Unity-ассеты,
которые намеренно остаются в проекте, но не публикуются ни в чанках, ни среди
root assets обычного story bundle. Повторный расчёт чанков сохраняет это
исключение, а проверка запрещает одновременно назначить один GUID в чанк и в
`Не используется`.

Строка получает вычисляемую метку `Постер видео`, если это PNG из `Locations`
и для его технического ID существует прямой или разрешённый через video alias
MP4. Метка служит пояснением в Inspector и не добавляет serialized-поле в
story asset.

Для TZM туда входят только PNG-постеры локаций, которым соответствует реальный
`Assets/Video/<location>.mp4`; video aliases учитываются так же, как прямые
имена. Если видео для локации нет, её PNG остаётся обычным ассетом чанка.
Runtime сначала разрешает URL видео: при отсутствии URL загружается PNG, а при
наличии видео переходный однотонный экран остаётся видимым до готовности первого
кадра. Сами исключённые PNG и их `.meta` не удаляются.

Повторный расчёт определяет порядок не по произвольному совпадению имени в
тексте, а через общий parser команд Ink. Фоны и видео учитываются только в
`Локация` / `Кат-сцена`, audio — только в его командах, а character art — по
персонажу и конкретным кандидатам view/emotion/clothes/hair/accessory, включая
варианты из гардероба и дополнительных веток гардероба. Дефолтные clothes,
hair и accessory персонажа назначаются на его первое взрослое появление, даже
если их имена не повторены в реплике. Перед сопоставлением видео разрешаются
aliases. Поэтому название локации в аннотации или обычной реплике не переносит
её ассеты в ранний чанк.

## Адресация ресурсов

Технические ID нормализуются в Unicode Form C, обрезаются и приводятся к
нижнему регистру. Путь остаётся контрактом: переименование директории или файла
может сломать команду Ink даже при неизменной картинке.

В TZM автор работает с короткими физическими путями:

```text
Characters/<character>/view/<view>/main.png
Characters/<character>/view/<view>/emotions/<emotion>.png
Characters/<character>/clothes/<clothes>/1.png
Characters/<character>/hairs/<front|back>/<hair>/<color>.png
Characters/<character>/accessories/<front|middle|back>/<item>.png
Locations/<location>.png
Choices/<item>.png
Presentation/<feature>/screen-variant.prefab
```

Сборщик отображает их в стабильные runtime-адреса:

```text
story/location/locations/<location>.png
story/choose/items/<item>.png
story/character/characters/<character>/view/<view>/main.png
story/character/characters/<character>/view/<view>/emotions/<emotion>.png
story/character/characters/<character>/clothes/<clothes>/1.png
story/character/characters/<character>/hairs/<front|back>/<hair>/<color>.png
story/character/characters/<character>/accessories/<front|middle|back>/<item>.png
story/presentation/<feature>/screen-variant.prefab
```

### Art Aliases без изменения Ink

Поле `Алиасы арта` в `NovelContentAsset` связывает старый runtime-путь с одним
каноническим PNG. Оба значения задаются относительно `content/<story-id>` и
включают расширение, например:

```text
story/choose/items/старое имя.png
  -> story/choose/items/каноническое имя.png
```

Runtime разрешает alias до загрузки Choice, Location или Character sprite.
Физический alias-source после миграции может отсутствовать; конечный target
обязан существовать и не должен находиться в `Не используется`. Сборщик
публикует только канонический target, даже если старый PNG временно оставлен в
source tree. Дубликаты alias, self-reference, циклы и отсутствующие targets
останавливают validation.

Такой alias позволяет сохранить исторический Ink побайтово неизменным. Для
персонажей объединяйте только exact-byte дубликаты с одинаковыми original size,
crop и trimmed hash в `sprite-trim-manifest.asset`. Пары разных персонажей,
`front/back`, `main/emotion` и `main/view` не считаются автоматически
взаимозаменяемыми, даже если их PNG сейчас совпадают: для них требуется явное
смысловое решение автора. Если подтверждённый alias связывает emotion с тем же
адресом, что и main body, runtime считает emotion успешно разрешённой для
validation/fallback, но не передаёт второй одинаковый слой на экран.

Для персонажей runtime пробует основной слой и кандидаты, переданные в Ink.
Если обязательный набор не разрешился, используется missing-character fallback.
Для отсутствующего фона текущий runtime получает missing-background. Поэтому
такой контент может остаться запускаемым, но валидатор всё равно должен сообщать
проблему.

Если `front` и `back` одной причёски доказанно совпадают побайтово, имеют
одинаковые importer settings и одну trim-геометрию, а изображение должно
рисоваться одним слоем, оставляйте только `front`. Не создавайте alias
`back -> front`: иначе один sprite будет отрисован дважды. Отсутствующий
опциональный `back` runtime воспринимает как пустой слой.

## Безопасное изменение Ink

1. Меняйте исходный episode `.ink`, а не compiled JSON.
2. Используйте команды и алиасы из `InkSyntax.md`.
3. Сохраняйте ID knot, stitch и именованных выборов, если они уже могли попасть
   в сохранение игрока.
4. Не исправляйте массово регистр, `ё/е` или пробелы без проверки всех ссылок.
5. После компиляции проверяйте, что обновились compiled story и source map.
6. Запускайте `validate <storyId>` до `build`.

Исторический Ink содержит терпимые варианты синтаксиса, например `..:`,
пробелы перед `...:`, пустые селекторы и разные написания эмоций. Новый контент
должен использовать канонический синтаксис; очистка старого корпуса выполняется
отдельно, чтобы не менять сюжетное поведение случайно.

## Безопасное добавление ресурса

1. Выберите корневой семантический каталог: `Locations`, `Characters`,
   `Choices` или `Presentation`.
2. Используйте каноническое имя из Ink без расширения.
3. Добавьте файл вместе с созданным Unity `.meta`.
4. Не создавайте копию существующего файла ради второго эпизода: Unity-ресурс
   уже глобален в пределах истории. Для второго имени используйте alias.
5. Убедитесь, что texture importer применил Sprite, отключённые mipmaps и
   Read/Write, а также Android ASTC 6×6 и iOS ASTC 8×8. Для story art во всех
   content-проектах используйте эталон TZM: `TextureWrapMode.Clamp` по всем
   осям (`wrapU/V/W = 1/1/1` в `.meta`). Presentation-specific настройки к
   этому правилу не относятся.
6. После добавления персонажа, одежды, эмоций, волос или аксессуаров выберите
   `Characters/sprite-trim-manifest.asset`. Сначала нажмите
   `1. Проверить изменения (без записи)`: действие только читает PNG и показывает
   точное количество и список файлов, которые потребуют физической обрезки.
   Затем выберите одно из независимых действий:

   - `2. Обновить индекс без изменения PNG` — записывает только хеши и удаляет
     устаревшие записи manifest; ни один PNG не открывается на запись;
   - `3. Обрезать N новых/заменённых PNG` — доступно только при ненулевом `N`,
     повторно показывает список и требует подтверждение.

   Манифест читает поддерживаемые PNG из своей папки и всех подпапок. Те же
   операции report/apply доступны из CLI:

   ```bash
   Tools/novels-tools/novels-content trim-sprites <story-id> report
   Tools/novels-tools/novels-content trim-sprites <story-id> apply
   ```

   Физическая обрезка сохраняет оригиналы только реально изменяемых PNG в
   `Build/SpriteTrimBackup/<timestamp>`, не меняет `.meta` и обновляет один
   `sprite-trim-manifest.asset` истории. Перед записью инструмент повторно
   проверяет хеши и геометрию показанного плана; при расхождении операция
   отменяется без записи. Хеш подтверждает неизменность уже обработанного PNG.
   Файл с уже сохранённым crop-размером получает только новый хеш через действие
   обновления индекса, поэтому старые записи мигрируют без повторной обрезки.
   Заменяющий арт нужно добавлять на исходном авторском холсте, а не заранее
   обрезать вручную.
7. Проверьте ссылку валидатором и только затем собирайте bundle.

## Команды рабочего процесса

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate tzm
Tools/novels-tools/novels-content validate zdm
Tools/novels-tools/novels-content build tzm editor
Tools/novels-tools/novels-content build zdm editor
```

Тяжёлые Unity-команды для `tzm` и `zdm` выполняются строго последовательно.
`build all` также обрабатывает проекты последовательно.

## Перед передачей изменения

- в diff нет `Library`, `Temp`, логов и случайных `.DS_Store`;
- не появился `Config/build.json`;
- не вернулся ручной AssetBundle label;
- source Ink, compiled story и source map согласованы;
- новые ссылки на assets разрешаются;
- размер bundle сравнен с baseline из `ContentSizeBaseline.md`;
- `git diff --check` проходит;
- в статус-файле потока перечислены проверки и известные предупреждения.
- смысл и визуальная целостность проверены по `ManualContentChecklist.md`.
