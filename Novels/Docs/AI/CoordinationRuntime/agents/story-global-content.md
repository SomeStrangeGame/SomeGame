# Agent: story-global-content

- Статус: completed
- heartbeat_utc: 2026-08-24T11:40:00Z
- Завершено UTC: 2026-08-24T11:40:00Z
- Результат: story-global миграция и Android-сжатие завершены; обе истории
  валидируются и собираются, Android bundles уменьшены суммарно на 48,0%.
- Задача: сохранить один Unity AssetBundle на историю, вынести Ink, видео и
  аудио в файловые payloads и заменить episode/shared-адресацию Unity-контента
  единым story-global namespace.
- Область:
  - `Novels/Docs/AI/ParallelRefactoringCoordination.md` — атомарная поправка:
    по явному запросу пользователя разрешить продолжительное ожидание без
    write-lock с интервалом проверки не менее 60 секунд и общим таймаутом
    10 минут;
  - `Packages/NovelsContentSdk/Runtime/ContentAddressing/ContentAddressConvention.cs`
  - `Packages/NovelsContentSdk/Runtime/ContentAddressing/ContentAddresses.cs`
  - точные runtime loaders и validation rules, использующие episode/shared
    адресацию, после инвентаризации;
  - `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/**`;
  - `Projects/novels-tzm/Assets/RemoteAssets/content/tzm/**`;
  - отдельный `ParallelWork.story-global-content.md`;
  - собственные runtime coordination files.
- Не изменять:
  - `Projects/novels-catalog/**`;
  - Game runtime без отдельного атомарного блока;
  - Ink, видео и аудио до подтверждения их текущего файлового контракта.
- Атомарные блоки:
  0. После освобождения текущего владельца изменить политику ожидания и
     проверить непротиворечивость с FIFO/write-lock: не более 10 минут
     непрерывного polling, до 10 проверок с минутным интервалом.
  1. Инвентаризация адресного API и новый контракт без миграции assets.
  2. Миграция и последовательная проверка `zdm`.
  3. Миграция и последовательная проверка `tzm`.
  4. Texture compression и сравнение размеров.
- Зависимость: освобождение `novels-simplification`, поскольку его scope
  пересекается с Content SDK и адресацией.
- Создано UTC: 2026-08-24T10:35:20Z
