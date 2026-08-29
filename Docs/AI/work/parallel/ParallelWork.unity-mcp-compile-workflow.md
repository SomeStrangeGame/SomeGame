# Parallel work: unity-mcp-compile-workflow

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`
- Ответственный поток: Experiments-1 continuation — Official Unity MCP compile workflow
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Tools/unity-mcp-helper/**`
- `Docs/AI/work/parallel/ParallelWork.unity-mcp-compile-workflow.md`
- Собственные записи `unity-mcp-compile-workflow` в `Docs/AI/CoordinationRuntime/**`

## Не изменять

- Unity scenes, prefabs, runtime, project settings and content assets.
- Чужие coordination/status records и существующий dirty tree.

## Изменённые контракты

- `recompile` доступен только как lock-gated workflow с bounded polling,
  pre/post Console checks и Git-state guard.
- `recompile_status` остаётся read-only.

## План проверки

- Unit tests: lock deny/allow, completed, timeout and compile-error outcomes.
- Реальная компиляция без изменения Unity source/assets.
- Final Console/scene/Git checks и остановка всех тестовых процессов.

## Выполнено

- Manifest расширен ровно двумя командами: lock-gated `recompile` и read-only
  `recompile_status`.
- Добавлена команда `compile` с bounded polling, pre/post Console error check,
  Git-state guard и стабильными `compile_failed`/`compile_timeout` errors.
- Policy errors не маскируются recovery polling и возвращаются немедленно.

## Проверено

- Syntax и unit tests: 14/14 успешно.
- Daemon без `--agent-id`: немедленный `lock_not_owned`, Unity mutation не выполнена.
- Daemon с владельцем lock: `recompile=up_to_date`, status `up_to_date`, 1 poll,
  0 Console errors до/после, unexpected Git changes отсутствуют.
- Финальная сцена clean; Console без errors, только две прежние warning.
- Helper, Editor, Hub и Licensing Client остановлены.

## Требуется при интеграции

- Всегда запускать daemon с текущим стабильным `agent-id`; не переносить owner id
  между задачами.
- Следующим отдельным scope можно добавить test-run workflow с теми же bounded
  status polling и post-check принципами.
