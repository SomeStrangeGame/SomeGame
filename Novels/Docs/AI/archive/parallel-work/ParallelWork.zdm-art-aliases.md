# Parallel work: ZDM exact art aliases

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `zdm-art-aliases`
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/definition/zdm.asset`
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/story/character/sprite-trim-manifest.asset`
- только exact-byte duplicate PNG/meta ZDM с одинаковыми importer settings и trim geometry;
- folder meta ставших пустыми каталогов ZDM;
- три заранее проверенных пустых hair-каталога TZM и их folder meta;
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- собственные coordination status/runtime/handoff-файлы

## Не изменять

- `Projects/novels-zdm/Assets/StreamingAssets/noveltexts/zdm/**`
- `Projects/novels-tzm/Assets/Ink/**`
- PNG с отличающимися importer settings, даже если их байты совпадают;
- SDK/runtime: общий alias contract уже реализован предыдущими блоками;
- чужие незавершённые изменения и coordination-файлы

## Изменяемый контракт

- Старые ZDM character addresses продолжают приниматься через story-level
  Art Aliases и разрешаются в canonical exact-byte targets.
- Физически удаляется только alias-source, совпадающий с target по PNG,
  importer settings и trim geometry.

## План проверки

- до/после SHA-256 всех Ink-файлов TZM и ZDM;
- exact-byte/import/trim аудит каждой alias-пары;
- static alias/target/manifest/GUID audit;
- `novels-content doctor`, `git diff --check` и scoped diff review;
- Unity/validate/build отложены до восстановления Licensing Client.

## Выполнено

- В `zdm.asset` добавлены 58 story-level Art Aliases, ведущих в 45
  канонических targets.
- Удалены 58 alias-source PNG/meta общим исходным размером 18 256 737 байт.
- ZDM trim manifest сокращён с 455 до 397 записей; удалены только source
  entries.
- Полностью пустые деревья `анпу` и `стражники` вместе с шестью folder meta
  удалены после проверки GUID-ссылок.
- В TZM удалены три ранее проверенных пустых hair-каталога, затем ставшая
  пустой родительская `hairs/back`; удалены четыре неиспользуемых folder meta.

## Проверено

- Каждая из 58 пар повторно проверена против `HEAD`: совпадают SHA-256 PNG,
  importer settings без GUID и trim geometry.
- Static alias audit: 58 уникальных sources отсутствуют, все targets
  существуют, self/cycles и ссылки на 58 удалённых GUID отсутствуют.
- 397 оставшихся ZDM character PNG точно соответствуют 397 адресам trim
  manifest.
- Среди оставшихся 473 ZDM PNG нет exact-дубликатов с одинаковыми importer
  settings. 25 побайтовых совпадений с различным `wrap mode` сохранены.
- Пустых каталогов под Assets TZM/ZDM не осталось.
- SHA-256 snapshots Ink TZM и ZDM совпадают с исходными.
- `Tools/novels-tools/novels-content doctor` и `git diff --check`: успешно.

## Требуется при интеграции

- Unity Editor, `validate zdm` и bundle build не запускались по прямому
  ограничению после сбоя Licensing Client.
- После восстановления лицензии выполнить refresh/compile, `validate zdm`,
  `build zdm editor` и playback smoke сцен с Гором/Анпу и Стражниками.
