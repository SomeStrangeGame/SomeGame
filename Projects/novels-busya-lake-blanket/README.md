# Капелька Буся и одеяло для озера

Атомарный Unity-проект истории `busya-lake-blanket`, созданный из
`Projects/novels-content-template`.

Контентный контракт проекта:

- `Config/card.json` с `schemaVersion`, `minimumClientVersion`, `storyId`,
  `title`, `description` и `cover`;
- `Config/cover.<extension>`;
- один `NovelContentAsset` и контент в `Assets/RemoteAssets`;
- Ink в `Assets/StreamingAssets`.

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate busya-lake-blanket
Tools/novels-tools/novels-content build busya-lake-blanket editor
```
