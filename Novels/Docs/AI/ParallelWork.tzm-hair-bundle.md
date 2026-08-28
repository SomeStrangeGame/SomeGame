# Parallel work: tzm-hair-bundle

- Статус: ready-for-integration
- Ветка: experiment/story-preview-streaming
- Базовый commit: d8853bf96f0c80d1e52c34e3ece8705cbc7018ac
- Ответственный поток: текущий чат, оптимизация волос TZM
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/ExperimentalStreamingPlan.cs`
- `Projects/novels-tzm/Assets/tzm.asset`
- `Projects/novels-tzm/Assets/Characters/sprite-trim-manifest.asset`
- три exact-byte `Projects/novels-tzm/Assets/Characters/maincharacter/hairs/back/**/блонд.png` и их `.meta`
- `Novels/Docs/AI/ContentAuthoringGuide.md`
- собственные coordination status/runtime/handoff-файлы

## Не изменять

- `Projects/novels-tzm/Assets/Ink/**`
- остальные content-проекты
- чужие незавершённые изменения

## Изменённые контракты

- Расчёт использования character art учитывает ветки гардероба и дефолтную внешность персонажа.
- Exact-byte `back` волос не публикуется и удаляется, если совпадает с `front`; alias между слоями не создаётся.

## Выполнено

- Usage planner распознаёт дополнительные hair/clothes/accessory/view-варианты
  в именованных Ink gather-ветках без повторного заголовка `Гардероб`.
- Дефолтные clothes/hair/accessory назначаются на первое взрослое появление
  персонажа; main-character normalizes в `maincharacter`.
- `Ободок`, `Бант`, `Мальвинка` и дефолт Салли находятся в chunk 0; оба слоя
  дефолтных волос Алексы — в chunk 2.
- 19 физически сохранённых, но не используемых hair PNG добавлены в
  `Не используется`; бесхозных hair PNG не осталось.
- Три exact-byte `back`-копии удалены вместе с `.meta`; соответствующие
  manifest entries и chunk GUID удалены, `front` сохранены без aliases.
- Итог TZM: 12 чанков / 410 GUID, 75 unused GUID, 418 trim entries, 35 hair
  PNG (16 chunk + 19 unused), 535 source roots / 460 published roots.

## Проверено

- SHA-256, размеры, importer settings и trim geometry трёх front/back пар:
  совпадают.
- Unity Roslyn по изменённому `ExperimentalStreamingPlan.cs` с актуальным
  `Novels.Content.dll`: успешно.
- `dotnet build Novels/Novels.Content.csproj --no-restore`: 0 warnings/errors.
- Static YAML/meta audit: duplicate/missing/unused overlap/unassigned hair — 0.
- `Tools/novels-tools/novels-content doctor`: успешно.
- SHA-256 всех 20 файлов `Assets/Ink`: идентичен baseline.
- `git diff --check`: успешно.

## Требуется при интеграции

- После восстановления Unity Licensing Client выполнить refresh, validation и rebuild TZM bundle.
- Фактический новый bundle size не измерен; Unity по прямому ограничению не запускался.
