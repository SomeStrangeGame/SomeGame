# Parallel work: TZM shared Presentation font

- Статус: integrated
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `7aa70b46512a2f924d66067f2b3792cfb211eb2b`
- Ответственный поток: текущий чат, дедупликация Liberation Sans Regular в TZM
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Projects/novels-tzm/Assets/Presentation/Fonts/**`
- `Projects/novels-tzm/Assets/Presentation/Fonts.meta`
- три существующие копии `Presentation/*/liberationsans-regular.ttf` с `.meta`
- `Projects/novels-tzm/Assets/Presentation/bubble/screen-variant.prefab`
- `Projects/novels-tzm/Assets/Presentation/notification/screen-variant.prefab`
- `Projects/novels-tzm/Assets/Presentation/setting/screen.prefab`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- собственные coordination-файлы и новая запись `CoordinationRuntime/HANDOFF.md`
- Git index и отдельный commit

## Не изменять

- остальной арт, video/audio и их import settings
- общий SDK/runtime и `Projects/novels-zdm/**`
- чужие coordination-файлы

## Изменённые контракты

- Все TZM Presentation prefab используют один canonical Liberation Sans Regular
  из `Assets/Presentation/Fonts`.

## Выполнено

- Подтверждено, что три TTF побайтово идентичны: SHA-256
  `e5b0af421ea2bfbc1ac8d251d647268087ae82786234c57f757d1f0b90fa8b49`.
- Копия из `Presentation/setting` перенесена в
  `Presentation/Fonts/liberationsans-regular.ttf` вместе с `.meta`; canonical
  GUID `0125029842d6d993020c25af5bf725f6` сохранён.
- Две остальные копии удалены; все 14 prefab references bubble, notification и
  setting используют canonical GUID.
- Из первого chunk удалены два устаревших GUID, число assignments снизилось с
  416 до 414 без изменения 12 chunks.
- Authoring guide закрепляет единый `Presentation/Fonts` для общих шрифтов.

## Проверено

- Старые GUID не упоминаются; TTF/OTF/TTC в TZM — 1; canonical prefab
  references — 14.
- `novels-content validate tzm`, `build tzm editor` и `doctor` — успешно.
- Bundle audit: 555 root assets, 183902.2 KiB; build log содержит одну запись
  `Assets/Presentation/Fonts/liberationsans-regular.ttf`.
- Release bundle уменьшился с 188 628 082 до 188 315 864 байт (-312 218 байт);
  два удалённых TTF освобождают 700 400 байт исходников.
- Layout: 12 chunks, 414 GUID, 51/51 video, 56 unused posters.
- 101 внешний payload/card/cover файл совпал с baseline по SHA-256; изменились
  только Unity bundle и release manifest.

## Требуется при интеграции

- Визуальный UI smoke через Game не выполнялся; serialized references и content
  bundle проверены полностью.
