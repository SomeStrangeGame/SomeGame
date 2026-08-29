# Parallel work: fallback-wardrobe-background

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 60a13762
- Ответственный поток: новый fallback гардероба с собственным фоном
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Features/Wardrobe/**`
- `Packages/NovelsContentSdk/Runtime/Features/OptionSelection/**`
- минимальные Game/runtime integration points для передачи фона
- системные fallback-ассеты гардероба и их `.meta`
- собственные coordination-записи

## Не изменять

- Ink и compiled Ink
- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`
- Android/iOS texture profiles
- посторонние изменения рабочего дерева

## Изменённые контракты

- Планируется опциональный собственный фон presentation гардероба с системным fallback.

## Выполнено

- Гардероб получил отдельный layout поверх существующего `OptionListScreen`;
  обычный Choose продолжает использовать прежнюю компоновку.
- Добавлен полноэкранный собственный фон между location и character canvases.
- Добавлены nameplate персонажа, четыре категории с активным состоянием,
  явные стрелки вариантов и сворачивание панели.
- Переключение персонажей и изменение Ink не добавлялись.
- Кнопка снятия элемента не добавлена: текущий Ink-choice contract не содержит
  безопасного отдельного решения для подтверждения пустого выбора.

## Проверено

- Scoped `git diff --check` — успешно.
- `dotnet build Novels/Novels.csproj --no-restore` после актуализации
  сгенерированного csproj — успешно, 0 warnings, 0 errors.
- После восстановления Licensing IPC Unity 6000.3.11 batch compile завершился
  штатно: Licensing Client подключён, C# ошибок нет, `Exiting batchmode
  successfully now!`.

## Требуется при интеграции

- Выполнить ручную проверку развёрнутого/свёрнутого fallback-экрана в Play Mode
  на 1080×1920.
