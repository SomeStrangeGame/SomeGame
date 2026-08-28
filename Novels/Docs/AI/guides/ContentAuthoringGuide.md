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
2. Откройте `Assets/RemoteAssets/content/<id>/definition/<id>.asset`: это список
   эпизодов, главный персонаж, пути Ink и aliases.
3. Откройте `Assets/StreamingAssets/noveltexts/<id>/<id>.ink`: он включает
   эпизодные Ink-файлы.
4. Найдите Unity-ресурсы истории в
   `Assets/RemoteAssets/content/<id>/story/`.
5. До сборки запустите `Tools/novels-tools/novels-content validate <id>`.

Не пытайтесь запускать content-проект кнопкой Play: сцены намеренно отсутствуют.

## Карта проекта

```text
Projects/novels-<id>/
  Assets/
    RemoteAssets/content/<id>/
      application/setting/       настройки UI истории
      definition/<id>.asset      контракт истории и эпизодов
      story/                      все Unity-ресурсы истории
    StreamingAssets/
      noveltexts/<id>/            Ink source, compiled story и source map
      novelsvideos/<id>/          внешние видео, если история их использует
      novelsaudio/<id>/           внешнее аудио, если история его использует
  Config/
    card.json                     storyId, заголовок, версия и обложка
    cover.*                       обложка истории
  Build/LocalContent/             сгенерированный результат
```

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
| `definition/<id>.asset` | эпизоды и aliases | через Unity Inspector |
| PNG/prefab/MP4 | визуальный контент | да, сохраняя адресацию |
| `.meta` | GUID и import settings Unity | не удалять и не копировать вслепую |
| `Build/LocalContent` | публикационный результат | нет |

До отдельного решения Game нельзя исключать source Ink или source maps из
production payloads: необходимость этих файлов должна быть подтверждена по
runtime-потребителям.

## Обязательные и опциональные части

| Часть | Статус | Правило |
| --- | --- | --- |
| `Config/card.json` | обязательна | `storyId` совпадает с ID definition |
| одна `Config/cover.*` | обязательна | имя совпадает с `card.cover` |
| один `NovelContentAsset` | обязателен | перечисляет хотя бы один эпизод |
| compiled Ink | обязателен | путь задаётся в definition |
| episode source Ink | authoring-контракт | `sourcePath` должен указывать на реальный файл с точным регистром |
| location/character assets | по использованию | имя является частью runtime-адреса |
| story presentation prefabs | опциональны | история может использовать общий presentation Game |
| видео | опциональны | `tzm` использует, `zdm` — нет |
| аудио | опционально | допустимые отсутствия задаются через silent audio IDs |

`tzm` — основной пример истории с видео, aliases и presentation overrides.
`zdm` — пример большой статической истории без видео и почти без собственных
presentation-prefabs.

## Адресация ресурсов

Технические ID нормализуются в Unicode Form C, обрезаются и приводятся к
нижнему регистру. Путь остаётся контрактом: переименование директории или файла
может сломать команду Ink даже при неизменной картинке.

Основные шаблоны:

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

Для персонажей runtime пробует основной слой и кандидаты, переданные в Ink.
Если обязательный набор не разрешился, используется missing-character fallback.
Для отсутствующего фона текущий runtime получает missing-background. Поэтому
такой контент может остаться запускаемым, но валидатор всё равно должен сообщать
проблему.

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

1. Выберите семантический каталог внутри `story`: `location`, `character`,
   `choose` или `presentation`.
2. Используйте каноническое имя из Ink без расширения.
3. Добавьте файл вместе с созданным Unity `.meta`.
4. Не создавайте копию существующего файла ради второго эпизода: Unity-ресурс
   уже глобален в пределах истории. Для второго имени используйте alias.
5. Убедитесь, что texture importer применил Sprite, отключённые mipmaps и
   Read/Write, а также Android ASTC 6×6 и iOS ASTC 8×8.
6. После добавления эмоций, волос или аксессуаров выполните сначала отчёт,
   затем безопасную обрезку прозрачных полей:

   ```bash
   Tools/novels-tools/novels-content trim-sprites <story-id> report
   Tools/novels-tools/novels-content trim-sprites <story-id> apply
   ```

   Команда сохраняет оригиналы в `Build/SpriteTrimBackup/<timestamp>`, не
   меняет `.meta` PNG и обновляет один `sprite-trim-manifest.asset` истории.
   Повторный запуск идемпотентен: уже обработанные файлы пропускаются.
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
