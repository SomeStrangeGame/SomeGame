# Parallel work: TZM semantic character aliases

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `tzm-semantic-art-aliases`
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Character/CharacterSpriteSetLoader.cs`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Projects/novels-tzm/Assets/Characters/sprite-trim-manifest.asset`
- шесть заранее подтверждённых exact-byte alias-source PNG и их `.meta`:
  три `царь/**`, emotion Атлана, view Фила и view Алексы;
- ставшие пустыми `Characters/царь/**` folder meta;
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- собственные coordination status/runtime/handoff-файлы

## Не изменять

- `Projects/novels-tzm/Assets/Ink/**`
- `Projects/novels-zdm/**`
- остальные character assets и import settings канонических PNG
- чужие незавершённые изменения и coordination-файлы

## Изменяемый контракт

- Шесть старых character addresses разрешаются через story-level aliases в
  канонические exact-byte targets.
- Если main body и emotion после alias resolution имеют один адрес, runtime
  считает emotion разрешённой, но не передаёт второй одинаковый слой на экран.

## План проверки

- до/после SHA-256 всех 20 Ink-файлов;
- exact-byte/import/trim аудит шести пар;
- статический аудит aliases, targets, chunk GUID и manifest;
- доступная не-Unity компиляция Character/Content assemblies;
- `novels-content doctor`, `git diff --check` и scoped diff review;
- Unity/validate/build отложены до восстановления Licensing Client.

## Выполнено

- В `tzm.asset` добавлены шесть подтверждённых aliases: короткий `царь`
  канонизирован в `царь таддеус`; emotion Атлана — в его main; view Фила и
  Алексы — в соответствующие canonical view.
- `CharacterSpriteSetLoader` сохраняет фактические разрешённые адреса main и
  emotion. Fallback проверяет исходный полный набор, после чего совпадающий
  emotion исключается из результата для экрана.
- Удалены шесть alias-source PNG/meta общим исходным размером 8 925 949 байт.
  Пустая папка `Characters/царь` и три folder meta также удалены.
- Из trim manifest удалены только шесть source entries; 418 → 412.
- Chunk layout переведён на canonical GUID в самом раннем месте: 12 чанков,
  410 → 406 назначений. Группа `Не используется` осталась 75 GUID.
- Source/published roots ожидаются 529/454; среди оставшихся Character PNG
  exact-byte duplicate groups больше нет.

## Проверено

- До удаления у каждой из шести пар совпадали SHA-256, importer settings,
  original size, crop и trimmed hash.
- Static alias audit: 23 уникальных aliases, 23/23 sources отсутствуют,
  23/23 targets существуют и не входят в unused; duplicates/self/cycles — 0.
- Chunk/unused audit: 406/75 GUID, duplicates/missing/overlap — 0.
- Unity 6000.3.11f1 Roslyn последовательно скомпилировал
  `Novels.ContentAddressing`, `Novels.Content` и `Novels.Character` без
  ошибок.
- `Tools/novels-tools/novels-content doctor` и `git diff --check`:
  успешно.
- SHA-256 всех 20 файлов `Assets/Ink` идентичен baseline; Ink не изменён.

## Требуется при интеграции

- Unity Editor, `validate tzm` и bundle build не запускались по прямому
  ограничению после сбоя Licensing Client.
- После восстановления лицензии выполнить refresh/compile,
  `validate tzm`, `build tzm editor` и playback smoke сцен с Царём,
  Атланом, Филом и подростком Алексой.
- `dotnet build Novels.Character.csproj --no-restore` недоступен без
  generated `project.assets.json`; вместо него успешно выполнена прямая
  компиляция Unity Roslyn по актуальным Bee response files.
