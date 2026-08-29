# Parallel work: TZM remove legacy Presentation art

- Статус: integrated
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `1bba19c974829f19c6f21437fa4bc120abfc1e88`
- Ответственный поток: текущий чат, физическое удаление уже исключённого legacy Presentation art
- Последнее обновление: 2026-08-27

## Разрешённая область

- `Projects/novels-tzm/Assets/Presentation/character/characters/**`
- `Projects/novels-tzm/Assets/Presentation/character/characters.meta`
- `Projects/novels-tzm/Assets/Presentation/location/locations/**`
- `Projects/novels-tzm/Assets/Presentation/location/locations.meta`
- `Docs/AI/ContentAuthoringGuide.md`
- `Docs/AI/archive/parallel-work/ParallelWork.tzm-remove-legacy-presentation-art.md`
- собственные coordination-файлы и новая запись в `Docs/AI/CoordinationRuntime/HANDOFF.md`
- Git index и отдельный commit текущего блока

## Не изменять

- родительские `Presentation/character`, `Presentation/location`, их prefab и UI dependencies
- `Projects/novels-tzm/Assets/tzm.asset` и актуальные story roots
- `Projects/novels-zdm/**`, общий SDK и runtime
- чужие coordination-файлы

## Изменённые контракты

- Нет: удаляются только физические копии арта, которые уже исключены из TZM chunks и не являются prefab dependencies.

## Выполнено

- Удалены `Presentation/character/characters`,
  `Presentation/location/locations` и их folder meta: 713 PNG / 711 938 459
  байт, всего 1551 tracked-файл с вложенными Unity meta.
- Родительские Presentation-prefab и 10 используемых UI-изображений сохранены.
- `ContentAuthoringGuide.md` закрепляет, что legacy-каталоги TZM не следует
  восстанавливать.

## Проверено

- До удаления: `novels-content validate tzm` и `build tzm editor` — успешно.
- После удаления: те же validation/build — успешно.
- Layout до/после: 12 чанков, 416 GUID, 51/51 MP4, 56 unused-постеров;
  SHA-256 `tzm.asset` остался
  `95032a7a122c6bf2c0159e76ae86f51ab96e76c0d992ee448297ff73ce3e4882`.
- Presentation audit после удаления: 19 файлов, 10 изображений и все 10 только
  как prefab dependencies; unreachable/direct legacy — 0.
- 838 удалённых GUID не упоминаются за пределами удалённых деревьев.
- Composed TZM tree: 704828 → 648100 KiB; 101 неизменяемый файл совпал по
  SHA-256, изменились только release manifest и Unity bundle.

## Требуется при интеграции

- Визуальный Play Mode smoke не выполнялся: атомарный content-проект не содержит
  сцен и проверяется через Game.
