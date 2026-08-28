# Parallel work: TZM art aliases and safe duplicate migration

- Статус: ready-for-integration
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `d8853bf96f0c80d1e52c34e3ece8705cbc7018ac`
- Ответственный поток: `tzm-art-aliases`
- Последнее обновление: 2026-08-27

## Разрешённая область

- runtime/content contract и централизованное разрешение адресов арта в
  `Packages/NovelsContentSdk/**`;
- передача resolver в runtime игры в `Novels/Assets/Novels/**`;
- editor validation, Inspector и фильтрация bundle roots;
- `Projects/novels-tzm/Assets/tzm.asset`;
- `Projects/novels-tzm/Assets/Characters/sprite-trim-manifest.asset`;
- удаление только 17 заранее подтверждённых exact-byte дубликатов Choices и
  Characters вместе с их `.meta`;
- authoring guide, собственные coordination-файлы и append-only handoff.

## Не изменять

- любые `.ink`, скомпилированный Ink и source map;
- `Projects/novels-zdm/**`;
- неоднозначные exact-byte пары разных персонажей, `front/back`,
  `main/emotion` и `main/view`;
- import settings канонических PNG;
- чужие coordination-файлы.

## Изменяемый контракт

- Story asset хранит логические `Art Aliases` в пространстве `story/...`.
- Runtime разрешает alias до загрузки Choice, Location и Character sprite.
- Сборщик не включает физический alias-source среди bundle roots.
- Validation отклоняет дубликаты, циклы и отсутствующие конечные targets.
- Ink остаётся неизменным; старые имена продолжают работать через aliases.

## План проверки

- статический аудит aliases/targets, GUID чанков и trim metadata;
- проверка удаления ровно 17 exact-byte alias-source PNG и `.meta`;
- аудит неизменности всех файлов `Assets/Ink` по SHA-256;
- не-Unity компиляция доступных generated projects с `--no-restore`;
- `novels-content doctor`, `git diff --check` и scoped diff review;
- Unity validation/build отложены до восстановления лицензии.

## Выполнено

- В story contract добавлены `Art Aliases` с нормализацией, duplicate/self/cycle
  validation и централизованным разрешением адреса до загрузки Choice,
  Location и Character sprite.
- Build roots исключают authoring-unused и физические alias-source; editor
  validation требует существующий конечный target.
- В TZM добавлено 17 aliases. Удалено 17 exact-byte alias-source PNG и их
  `.meta`: три Choices, четыре опечатки `раздражеие`, восемь child-вариантов с
  суффиксом `1`, `Алекса/с улыбкой` и `Барбара/хитрая улыбка`.
- Chunk GUID переведены на canonical targets; assignments 414 → 408.
- Из trim manifest удалены только 14 alias-записей после подтверждения полного
  совпадения geometry/hash; 435 → 421.
- Неоднозначные different-character, `front/back`, `main/emotion` и
  `main/view` exact duplicates оставлены без изменений.

## Проверено

- Static TZM audit: 17 уникальных aliases, targets существуют, sources удалены,
  cycles/duplicates отсутствуют; 408 уникальных chunk GUID и 56 unused GUID
  разрешаются.
- SHA-256 snapshot всех 20 файлов `Assets/Ink`: побайтово неизменен.
- `Novels.ContentAddressing.csproj` и `Novels.Content.csproj`: build succeeded,
  0 warnings / 0 errors.
- `novels-content doctor` и `git diff --check`: успешно.

## Требуется при интеграции

- Unity, `validate tzm` и `build tzm editor` намеренно не запускались по прямому
  ограничению пользователя после падения Licensing Client.
- Restore generated Character/Editor projects зависал и был отменён; после
  восстановления Unity выполнить штатную компиляцию, validation и bundle build.
- Ожидаемый static root count после удаления: 538 source roots и 482 roots
  после исключения 56 unused; подтвердить фактическим build.
