# Колодец, который зовёт

Атомарный Unity-проект оригинальной десятиэпизодной хоррор-истории
`kolodets-kotoryy-zovet`, созданный из `Projects/novels-content-template`.

Текущая редакция: сценарный аудит и исправления от 2026-09-07 —
[отчёт и ограничения](Art/CONTINUITY_REVIEW.md). Изменён source Ink;
скомпилированные JSON и source map пока относятся к `b5c73d39`.
Перед запуском исправленной версии необходимы повторная проверка оригинальности
полного текста, компиляция и финальная runtime-приёмка. Старый candidate manifest
не включает эти незакоммиченные исправления.

Обложки эпизодов от 2026-09-08: подготовлены десять отдельных PNG в
`Config/EpisodeCovers/` — [галерея](Art/episode-covers.html),
[привязка по ID и результаты проверки](Art/EPISODE_COVERS.md).
Назначение в определении истории и проверка отображения в каталоге пока
не выполнены; для этого нужен актуальный SDK и разрешённый слот Unity.

Контентный контракт проекта:

- каталоговая карточка и обложка находятся в `Config`;
- единственный `NovelContentAsset` — `Assets/kolodets-kotoryy-zovet.asset`;
- общий Ink-вход включает десять источников `Assets/Ink/s01e01.ink` —
  `s01e10.ink`;
- 36 фоновых состояний находятся в `Assets/Locations`;
- пять персонажей и их цельные варианты находятся в `Assets/Characters`;
- два эмбиентных цикла и восемь событийных звуков находятся в `Assets/Audio`.

История полностью завершается в десятом эпизоде одной из пяти развязок.
AssetBundle label назначать не требуется.

Проверка условий и ссылок исходного текста без Unity (не заменяет Ink runtime):

```bash
python3 -B Projects/novels-kolodets-kotoryy-zovet/Art/tests/test_continuity.py
```

Следующие команды запускают Unity и выполняются только в финальном разрешённом слоте:

```bash
Tools/novels-tools/novels-content validate kolodets-kotoryy-zovet
Tools/novels-tools/novels-content build kolodets-kotoryy-zovet editor
```
