# Тише, Нина

Атомарный Unity-проект коммерческой хоррор-истории `scp1198-silence`,
созданный из `Projects/novels-content-template`.

История основана на статье “SCP-1198” автора Drewbear и распространяется по
лицензии Creative Commons Attribution-ShareAlike 3.0. Источник:
https://scp-wiki.wikidot.com/scp-1198

Контентный контракт проекта:

- `Config/card.json` с `schemaVersion`, `minimumClientVersion`, `storyId`,
  `title`, `description` и `cover`;
- `Config/cover.<extension>`;
- один `NovelContentAsset` и контент в `Assets/RemoteAssets`;
- Ink в `Assets/StreamingAssets`.

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate scp1198-silence
Tools/novels-tools/novels-content build scp1198-silence editor
```
