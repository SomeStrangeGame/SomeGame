# Parallel work: catalog mockup parity

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c77278d32c8fa7b09a3d8878e23ad42daafa4
- Ответственный поток: catalog-mockup-parity
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Packages/NovelsContentSdk/Runtime/Catalog/**`
- `Packages/NovelsContentSdk/Runtime/CatalogContracts/**`
- точечные story-card contract/validation файлы в `Packages/NovelsContentSdk/Editor/**`
- `Novels/Assets/Novels/CatalogFlow.cs`
- новый Game-owned reader статуса прогресса рядом с CatalogFlow
- `Novels/Assets/Novels/ApplicationTexts.cs`
- `Projects/novels-catalog/Assets/RemoteAssets/catalog/screen.prefab`
- `Projects/novels-catalog/README.md`
- `Projects/novels-tzm/Config/card.json`
- `Projects/novels-zdm/Config/card.json`
- `Projects/novels-catalog/Config/catalog.json`
- точечное изменение `bundleVersion` в `Novels/ProjectSettings/ProjectSettings.asset`
- собственные coordination files

## Не изменять

- текущие GPL, wardrobe, texture postprocessor и Android ASTC изменения других потоков
- остальные story assets и Game runtime

## Изменённые контракты

- Story-card schema 2 содержит обязательный отображаемый `genre`.
- Catalog/Game/story cards требуют клиента `0.2.0` и интегрируются атомарно.

## Выполнено

- Добавлены progress-aware `Открыть` / `Продолжить` и persisted started marker с legacy save fallback.
- Добавлены отдельный CTA, page indicator и синхронизация с центральной карточкой.
- Добавлены genre schema/data для TZM/ZDM.
- Добавлены SafeArea container и runtime anchor update; фон остаётся полноэкранным.
- Версии Catalog, TZM, ZDM и Game синхронизированы на `0.2.0`.

## Проверено

- `novels-content doctor` — успешно.
- Unity Bee Roslyn response files: `Novels.Catalog.Contracts`, `Novels.Catalog`, `Novels` — успешно, без ошибок.
- JSON schema/version assertions через `jq` — успешно.
- Prefab local fileID definitions/references audit — успешно.
- Scoped `git diff --check` — успешно.
- Unity batch validation дважды заблокирован внешним Licensing Client: channel timeout/headless entitlement, затем disposed `IServiceProvider`.

## Требуется при интеграции

- Атомарно интегрировать SDK + Game + Catalog + TZM/ZDM card schema.
- После стабильного Licensing повторить `validate catalog`, `build catalog editor`, Game compile и ручной phone/tablet mouse/touch Play Mode gate.
