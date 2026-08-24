# Parallel work: simplification-wave-2

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 0477677f4c1196737dddf2594594e8329d4c563e
- Ответственный поток: этот чат, шесть согласованных упрощений Novels
- Последнее обновление: 2026-08-24

## Разрешённая область

- `Novels/Assets/Novels/NovelRuntime*.cs`
- `Novels/Assets/Novels/NovelBootstrapProcess.cs*`
- `Novels/Assets/Novels/NovelSession.cs*`
- `Novels/Assets/Novels/StoryExecution/**`
- `Novels/Assets/Novels/StoryQueue/**`
- `Packages/NovelsContentSdk/Editor/ContentProjectValidation.cs`
- `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/**`
- `Packages/NovelsContentSdk/Runtime/Features/Choose/**`
- `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/**`
- связанные prefab/meta только для общего `OptionListScreen`
- документация и собственные coordination runtime files

## Не изменять

- Ink, save/release форматы и технические адреса.
- Игровые размеры и поведение существующего UI.
- Catalog/TZM/ZDM authoring content.
- Character и Location runtime этой волной.

## Изменённые контракты

- Ожидается только сокращение внутренних API; публичное поведение сохраняется.

## Выполнено

- Validation переведён на линейный маршрут без rule-интерфейса и одноразовых
  классов.
- Удалены `NovelBootstrapProcess`, `NovelStartSession` и `NovelSession`.
- Простые StoryOperation заменены `DelegateStoryOperation`.
- Статическая OptionList-разметка перенесена в общий Resources prefab.
- Малые Dependencies Choose, Wardrobe и Catalog заменены прямыми конструкторами.
- Правило FIFO обновлено: ожидание выполняется самостоятельно до таймаута.

## Проверено

- Unity 6000.3.11f1 batch compile — успешно.
- `novels-content validate all` — Catalog, TZM и ZDM успешно.
- `git diff --check` — успешно.
- Рассматриваемый C# объём: 9628 → 9305 строк.

## Требуется при интеграции

- Ручной Play Mode smoke test Choose/Wardrobe.
