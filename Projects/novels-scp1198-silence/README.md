# Тише, Нина

Атомарный Unity-проект коммерческой хоррор-истории `scp1198-silence`,
созданный из `Projects/novels-content-template`.

История основана на статье “SCP-1198” автора Drewbear и распространяется по
лицензии Creative Commons Attribution-ShareAlike 3.0. Источник:
https://scp-wiki.wikidot.com/scp-1198

Это производная работа по вселенной SCP Foundation. SCP Foundation и исходная
статья также доступны по CC BY-SA 3.0:
https://creativecommons.org/licenses/by-sa/3.0/

Проект не является официальным продуктом SCP Wiki и не одобрен автором исходной
статьи. Оригинальные персонажи, сюжетные ветки, диалоги и иллюстрации этой
истории распространяются на тех же условиях CC BY-SA 3.0.

Контентный контракт проекта:

- `Config/card.json` с `schemaVersion`, `minimumClientVersion`, `storyId`,
  `title`, `description` и `cover`;
- `Config/cover.<extension>`;
- один `NovelContentAsset` в `Assets/scp1198-silence.asset`;
- Ink и скомпилированный JSON в `Assets/Ink`;
- персонажи в `Assets/Characters`, локации в `Assets/Locations`.

AssetBundle label назначать не требуется. Проверка и сборка выполняются из
корня общего репозитория:

```bash
Tools/novels-tools/novels-content validate scp1198-silence
Tools/novels-tools/novels-content build scp1198-silence editor
```
