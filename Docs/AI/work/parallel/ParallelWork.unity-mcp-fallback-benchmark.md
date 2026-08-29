# Parallel work: unity-mcp-fallback-benchmark

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`
- Ответственный поток: Experiments-1 continuation — Official Unity MCP fallback benchmark
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Tools/unity-mcp-helper/**`
- `Docs/AI/work/parallel/ParallelWork.unity-mcp-fallback-benchmark.md`
- Собственные записи `unity-mcp-fallback-benchmark` в `Docs/AI/CoordinationRuntime/**`
- Временный marker в `Novels/Assets/Novels/ApplicationRuntime.cs` для реального
  domain reload; marker удаляется сразу после reload, исходный content hash и
  Git diff обязаны восстановиться точно.

## Не изменять

- Unity scenes, prefabs, runtime and content assets.
- Чужие coordination/status records и существующий dirty tree.

## Изменённые контракты

- Только локальная diagnostic-команда benchmark; игровой runtime и MCP allowlist не меняются.

## План проверки

- Unit tests и syntax check helper.
- 30 циклов read-only MCP calls в одной persistent-сессии.
- Recovery после domain reload и полного перезапуска Editor без сохранения изменений проекта.
- Сравнение scoped git state до/после.

## Выполнено

- Добавлена команда `benchmark --iterations N` с success rate, median/p95/max,
  handshake count, paired raw/summary size samples и Git-state guard.
- Daemon status теперь показывает число MCP handshakes.
- README дополнен воспроизводимым benchmark workflow и оговоркой, что
  character reduction является proxy, а не точным tokenizer count.

## Проверено

- Unit tests и syntax check: 9/9 успешно.
- Реальный benchmark: 90/90 read-only calls, median 97.30 ms, p95 100.86 ms,
  max 119.88 ms; один persistent handshake, новых handshakes во время run нет.
- Paired samples: 3961 raw chars против 1661 summary chars, reduction proxy 58.07%.
- Domain reload: persistent daemon восстановил `editor_status=ready`, handshake
  count остался 1; transient marker удалён, исходный content/diff hash восстановлен.
- Полный Editor restart: PID `99125` заменён на `884`; тот же daemon вернул
  `ready` без нового handshake.
- Финальная сцена не dirty; Console без errors, только две прежние warning.
- Benchmark helper, Editor, Hub и Licensing Client остановлены.

## Требуется при интеграции

- Benchmark report остаётся runtime artifact в `/tmp`, в Git его не добавлять.
- Периодически повторять одинаковые 30 циклов и сравнивать success rate/p95/reduction.
