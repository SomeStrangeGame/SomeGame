# Atomic Novel Content Project

Скопируйте шаблон в `Projects/novels-<storyId>` и добавьте:

- `Config/card.json` с `schemaVersion`, `minimumClientVersion`, `storyId`,
  `title`, `description` и `cover`;
- `Config/cover.<extension>`;
- один `NovelContentAsset` и контент в `Assets/RemoteAssets`;
- Ink в `Assets/StreamingAssets`.

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate <storyId>
Tools/novels-tools/novels-content build <storyId> editor
```
