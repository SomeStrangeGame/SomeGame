# После последнего света

Атомарный Unity-проект романтической визуальной новеллы
`after-the-last-light`.

История рассчитана на взрослую аудиторию: Мире 19 лет, Рену 20 лет.
В ней есть чувственное романтическое напряжение, но нет откровенных сцен.

Проект содержит:

- `Config/card.json` с `schemaVersion`, `minimumClientVersion`, `storyId`,
  `title`, `description` и `cover`;
- `Config/cover.<extension>`;
- один `NovelContentAsset` и контент в `Assets/RemoteAssets`;
- Ink в `Assets/StreamingAssets`.

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate after-the-last-light
Tools/novels-tools/novels-content build after-the-last-light editor
```
