# Parallel work: runtime-smoke-telemetry

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c77278d32c8fa7b09a3d8878e23ad42daafa4
- Ответственный поток: machine-readable runtime smoke telemetry
- Последнее обновление: 2026-08-29

## Разрешённая область

- точечные telemetry-файлы и integration points в `Novels/Assets/Novels/**`
- `Packages/Bundles/**` только для release activation marker при необходимости
- `Packages/NovelsContentSdk/Runtime/Features/Character/**` только для character fallback marker
- `Docs/AI/guides/ContentPipeline.md`
- собственные coordination records

## Не изменять

- сцены, prefabs и serialized assets
- Ink/content assets
- соседние runtime-контракты и чужие dirty changes

## Изменённые контракты

- Добавлен версированный однострочный `[NOVELS_SMOKE]` JSON event contract с `v`, `seq`, `runId`, `event`.
- Фактическая подстановка missing character создаёт `fallback.used` и является блокирующим smoke failure.

## Выполнено

- Scope поставлен в FIFO; реализация не начиналась из-за пересекающегося `wardrobe-interaction-fix`.
- После ограниченного периода ожидания собственная заявка оставлена в очереди.
- Добавлены lifecycle/catalog/story/episode/dialogue/choice/error/fallback события.
- `ContentPipeline.md` дополнен machine-readable event contract и failure-only diagnostics.
- Существующие чужие изменения в тех же runtime-файлах сохранены.

## Проверено

- Unity 6000.3.11f1: compile `up_to_date`, 0 Console errors before/after, no unexpected Git delta.
- Editor status ready, compile/reload false, Play Mode stopped; сцена `Novels` clean.
- Scoped `git diff --check` и required-event audit — успешно.
- Runtime Android/ADB прогон новых маркеров не выполнялся; требуется следующий APK smoke.

## Требуется при интеграции

- Интегрировать только telemetry hunks вместе с уже ожидающими соседними runtime changes.
- На следующем Android Embedded smoke проверить последовательность событий и failure branch для `fallback.used`.
