# Parallel work: ZDM wrap mode normalization

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `zdm-wrap-mode-normalization`
- Последнее обновление: 2026-08-28

## Разрешённая область

- ZDM PNG `.meta` с `wrapU/V/W = 0/0/0` под
  `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/story/character/characters/**`;
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`;
- ZDM character trim manifest;
- 25 alias-source PNG/meta, ставшие безопасными после нормализации;
- ставшие пустыми folder meta;
- `Novels/Docs/AI/ContentAuthoringGuide.md`;
- собственные coordination status/runtime/handoff-файлы

## Не изменять

- Ink TZM/ZDM;
- TZM import settings: проект используется только как эталон;
- остальные ZDM importer settings кроме `wrapU/V/W`;
- SDK/runtime и чужие незавершённые изменения;
- Unity/validate/build не запускать из-за Licensing Client

## Контракт

- Story character PNG используют эталон TZM `wrapU/V/W = 1/1/1` (`Clamp`).
- После нормализации 25 exact-byte групп с одинаковыми остальными importer
  settings переводятся на Art Aliases; старые Ink-имена сохраняются.

## План проверки

- до/после SHA-256 всех Ink-файлов TZM/ZDM;
- доказать, что до правки 25 групп различались только `wrapU/V/W`;
- проверить все alias-пары по PNG/importer/trim geometry;
- static alias/target/GUID/manifest audit;
- `novels-content doctor`, `git diff --check`, scoped diff review.

## Выполнено

- Все 194 оставшихся на момент аудита ZDM character PNG с
  `wrapU/V/W = 0/0/0` приведены к эталону TZM `1/1/1` (`Clamp`).
- После нормализации 25 exact-byte групп получили одинаковые полные importer
  settings. Добавлены 25 Art Aliases, source PNG/meta удалены ещё на
  2 173 892 байта.
- Общий результат ZDM: 83 aliases в 69 canonical targets; удалённые PNG
  занимали 20 430 629 байт.
- Trim manifest сокращён 397 → 372; он снова точно соответствует оставшимся
  character PNG.
- `ContentAuthoringGuide.md` фиксирует общий эталон story art: Clamp по всем
  осям; Presentation-specific import settings исключены из правила.

## Проверено

- Все текущие 448 PNG/meta ZDM имеют `wrapU/V/W = 1/1/1`; в 188 surviving
  изменённых `.png.meta` diff содержит только эти три поля.
- Exact-byte duplicate groups после миграции: 0.
- Static audit: 83 уникальных alias-source отсутствуют, targets существуют,
  self/cycles и ссылки на удалённые GUID отсутствуют.
- 372 character PNG = 372 trim entries; у всех alias-пар совпадают SHA-256,
  нормализованные importer settings и trim geometry.
- Пустых каталогов Assets TZM/ZDM нет; Ink snapshots TZM/ZDM не изменились.
- `novels-content doctor`, `git diff --check` и scoped diff review: успешно.

## Требуется при интеграции

- Unity Editor, `validate zdm` и bundle build не запускались по прямому
  ограничению после сбоя Licensing Client.
- После восстановления лицензии выполнить refresh/reimport, `validate zdm`,
  `build zdm editor` и playback smoke вариантов внешности главной героини.
