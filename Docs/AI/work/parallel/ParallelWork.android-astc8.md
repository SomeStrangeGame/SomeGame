# Parallel work: android-astc8

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c7727
- Ответственный поток: Android ASTC 8×8 postprocessor и измерение story bundles
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`
- `Docs/AI/guides/ContentAuthoringGuide.md`
- `Docs/AI/plans/ContentSizeOptimization.md`
- `Docs/AI/plans/ContentSizeBaseline.md`
- `Docs/AI/work/parallel/ParallelWork.android-astc8.md`
- собственные записи в `Docs/AI/CoordinationRuntime/**`

## Не изменять

- `Projects/novels-*/Assets/**`
- Game runtime и Player settings
- посторонние пользовательские изменения рабочего дерева

## Изменённые контракты

- Планируется единый Android story-art профиль ASTC 8×8 вместо ASTC 6×6.

## Выполнено

- Общий postprocessor переведён на Android ASTC 8×8; версия увеличена для
  гарантированного реимпорта существующего story art.
- Актуальные authoring/size руководства синхронизированы с новым профилем.

## Проверено

- Текущий владелец политики подтверждён по коду и документации.
- `Tools/novels-tools/novels-content doctor` — конфигурация валидна.
- `git diff --check` по файлам задачи — успешно.
- Licensing IPC восстановлен по каноническому протоколу без удаления лицензии
  или cache: штатно закрыт Hub, удалены два точных stale socket без владельца.
- Android TZM и ZDM последовательно переимпортированы и собраны успешно.
- Текущий bundle total: TZM 27 792 318 B, ZDM 56 445 255 B.

## Требуется при интеграции

- Выполнить ручной visual quality gate лиц, волос, одежды, alpha-краёв и
  градиентов. Bundle delta включает также текущий chunking/exclude-unused
  pipeline и не является чистым ASTC A/B.
