# Parallel work: character trim regeneration

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `bfee19aac01ce2ec39f113265b79dfd255f12260`
- Ответственный поток: текущий чат, безопасная перегенерация trim manifest
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/CharacterSpriteAlphaTrim.cs`
- `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterSpriteTrimManifest.cs`
- `Docs/AI/ContentAuthoringGuide.md`
- собственные coordination-файлы и запись в `CoordinationRuntime/HANDOFF.md`

## Не изменять

- PNG и существующие `sprite-trim-manifest.asset` историй в рамках реализации инструмента
- `Projects/novels-zdm/**`
- остальные shared packages, Game runtime и контентные ассеты
- чужие coordination-файлы

## Изменённые контракты

- Generated trim entry хранит SHA-256 обработанного PNG для определения
  идемпотентности; runtime-геометрия и asset address не меняются.

## Выполнено

- `sprite-trim-manifest.asset` получил собственный Inspector с действиями
  `Проверить арты` и `Перегенерировать трим`.
- Перегенерация рекурсивно читает поддерживаемые PNG из папки манифеста,
  пропускает совпавший хеш и не режет повторно файл с уже сохранённым
  crop-размером.
- Старые записи без хеша мигрируют без изменения PNG; новые и заменённые арты
  на исходном холсте получают backup, trim и новый хеш.
- Удалённые PNG удаляются из полностью пересобранного манифеста.
- При ошибке изменённые PNG и манифест восстанавливаются из backup.
- Authoring guide обновлён под Inspector-first workflow; CLI сохранён.

## Проверено

- Unity Roslyn по актуальному TZM `Novels.Character.rsp` — успешно.
- Unity Roslyn по актуальному TZM `Novels.ContentSdk.Editor.rsp` с новой
  runtime reference — успешно.
- Текущие manifests: TZM 435/435 и ZDM 455/455 записей разрешаются в PNG;
  размер каждого PNG совпадает с сохранённым crop, missing/mismatch 0/0.
- `novels-content doctor` — успешно.
- Scoped `git diff --check` — успешно.

## Требуется при интеграции

- Unity refresh и визуальная проверка кнопок на `sprite-trim-manifest.asset`.
- В TZM нажать `Проверить арты`, затем `Перегенерировать трим`: первый apply
  должен добавить 435 хешей без изменения PNG.
- После сохранения обновлённого manifest пересобрать content bundle.
- Полный Unity build не запускался, потому что TZM Editor уже открыт
  пользователем; второй Unity запрещён протоколом.
