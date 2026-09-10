# «Волчья пошлина»

Атомарный content-проект истории `volchya-poshlina` для каталога «Кострома».

Исходный Ink находится в `Assets/Ink`, цельные персонажи — в
`Assets/Characters`, двенадцать сюжетных фонов — в `Assets/Locations`, а локальный
Bubble и звук — в `Assets/Presentation` и `Assets/Audio`.

Сценарий содержит 10 групп решений, 28 вариантов и 4 концовки. Статический
перебор Ink проверил все 26 244 сочетания. За один маршрут отображается
2 846–3 693 слов реплик, описаний и выбранных ответов (служебные команды
исключены). Час прохождения этим объёмом не подтверждён: при условных
150–200 словах в минуту это около 14–25 минут чистого чтения, без пауз и UI.

Результаты повторного аудита, исправления и ограничения:
[Art/STORY_AUDIT.md](Art/STORY_AUDIT.md).

Обложка единственного эпизода `s01e01` хранится отдельно в
`Config/EpisodeCovers/s01e01.png` и назначена через `_catalogCover`.
Общая `Config/cover.png` сохранена для карточки истории и fallback.
[Интеграция с main](Art/INTEGRATION.md),
[обложка: manifest, prompts и проверка](Art/EPISODE_COVER.md).

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate volchya-poshlina
Tools/novels-tools/novels-content build volchya-poshlina editor
```
