# Parallel work: video-solid-color-null-fix

- Статус: ready-with-limitations
- Ветка: experiment/story-preview-streaming
- Базовый commit: d8853bf96f0c80d1e52c34e3ece8705cbc7018ac
- Ответственный поток: текущий чат — падение TZM после гардероба перед видео «Причал»
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Location/BackgroundPresentationController.cs`
- `Packages/NovelsContentSdk/Runtime/Features/Location/View/LocationScreen.cs`
- `Packages/NovelsContentSdk/Runtime/Features/Location/View/LocationLayout.cs`
- `Novels/Docs/AI/ParallelWork.video-solid-color-null-fix.md`
- собственные записи `Novels/Docs/AI/CoordinationRuntime/**`

## Не изменять

- `Projects/novels-*/Assets/Ink/**`
- story assets и bundles
- остальные пользовательские изменения рабочего дерева

## Изменённые контракты

- `LocationScreen.ClearImage()` явно очищает фон без попытки рассчитать layout
  для отсутствующего спрайта.

## Выполнено

- Причина подтверждена по runtime stack trace и Editor log.
- `ShowSolidColor` больше не передаёт `null` в `SetImage`; переходы к видео и
  однотонному фону используют безопасную очистку.
- Старый sprite reference освобождается перед показом видео.

## Проверено

- Unity Roslyn по актуальному `Novels.Location.rsp` — успешно.
- `SetImage(null)` в runtime отсутствует; scoped `git diff --check` — успешно.
- `dotnet build --no-restore` неприменим без Unity-generated
  `project.assets.json`; это не ошибка C#-компиляции.

## Требуется при интеграции

- Открытый Editor не выполнил refresh после внешней package-правки; macOS не
  разрешила автоматическое нажатие меню без Assistive Access.
- Выйти из Play Mode, выполнить обычный refresh/recompile и повторить переход
  `Гардероб -> Причал`.
