# Колодец, который зовёт

Атомарный Unity-проект оригинальной десятиэпизодной хоррор-истории
`kolodets-kotoryy-zovet`, созданный из `Projects/novels-content-template`.

Контентный контракт проекта:

- каталоговая карточка и обложка находятся в `Config`;
- единственный `NovelContentAsset` — `Assets/kolodets-kotoryy-zovet.asset`;
- общий Ink-вход включает десять источников `Assets/Ink/s01e01.ink` —
  `s01e10.ink`;
- 30 фоновых состояний находятся в `Assets/Locations`;
- пять персонажей и их цельные варианты находятся в `Assets/Characters`;
- два эмбиентных цикла и восемь событийных звуков находятся в `Assets/Audio`.

История полностью завершается в десятом эпизоде одной из пяти развязок.
AssetBundle label назначать не требуется.

```bash
Tools/novels-tools/novels-content validate kolodets-kotoryy-zovet
Tools/novels-tools/novels-content build kolodets-kotoryy-zovet editor
```
