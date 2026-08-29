# Parallel work: unity-mcp-fallback

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`
- Ответственный поток: Experiments-1 — Official Unity MCP fallback helper
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Tools/unity-mcp-helper/**`
- `Docs/AI/work/parallel/ParallelWork.unity-mcp-fallback.md`
- Собственные записи `unity-mcp-fallback` в `Docs/AI/CoordinationRuntime/**`

## Не изменять

- Unity scenes, prefabs, runtime and content assets.
- Чужие coordination/status records и весь существующий dirty tree.

## Изменённые контракты

- Планируется локальный read-only fallback вокруг Official Unity MCP; игровой runtime не меняется.

## Выполнено

- Реализован dependency-free Python JSON-RPC client и persistent Unix-socket daemon.
- Добавлены fail-closed allowlist, read/write policy, lock ownership guard,
  bounded reconnect, таймауты, protocol/server checks, response cap и JSONL log rotation.
- Добавлены компактные summaries и raw `--format json`.
- Добавлены fake MCP tests и документация эксплуатации/удаления fallback.

## Проверено

- `python3 -m unittest discover -s Tools/unity-mcp-helper/tests -v` — 8 tests passed.
- Persistent Official Unity MCP smoke — `editor_status`, `get_scene_hierarchy`,
  `console` успешно в одной сессии; сцена не dirty, Console без errors.
- `git diff --check` — успешно.

## Требуется при интеграции

- Не добавлять write-tools без отдельного точного scope и проверки под write-lock.
- Удалить fallback после стабильного появления native `unity_novels` namespace.
