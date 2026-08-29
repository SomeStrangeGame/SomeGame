# Parallel work: unity-mcp-editmode-tests

- Статус: ready-with-limitations
- Ветка: main
- Базовый commit: `7e9c77278d32c8fa7b09a3d8878e23ad42daafa4`
- Ответственный поток: Experiments-1 continuation — Official Unity MCP EditMode tests
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Tools/unity-mcp-helper/**`
- `Docs/AI/work/parallel/ParallelWork.unity-mcp-editmode-tests.md`
- Собственные записи `unity-mcp-editmode-tests` в `Docs/AI/CoordinationRuntime/**`

## Не изменять

- Unity scenes, prefabs, runtime, tests, project settings and content assets.
- Чужие coordination/status records и существующий dirty tree.

## Изменённые контракты

- EditMode test run доступен только как lock-gated workflow.
- Status polling read-only, bounded; filter передаётся явно и без shell expansion.

## План проверки

- Unit tests: lock deny, pass/fail/timeout, filter forwarding and policy error.
- Реальный минимальный EditMode run на существующих тестах либо честный empty-suite result.
- Final Console/scene/Git checks и остановка тестовых процессов.

## Выполнено

- Allowlist расширен read-only `list_tests`/`test_status` и lock-gated `run_tests`.
- Добавлена команда `editmode-tests`: только mode=editor, explicit tests выключены,
  bounded async polling, filter/filter_type, compact failures, Console/Git guards.
- Empty suite намеренно возвращает non-success `outcome=no_tests`, а не ложный pass.

## Проверено

- Syntax/unit tests: 19/19 успешно.
- Без `--agent-id`: немедленный `lock_not_owned`, test run не запускается.
- `list_tests(mode=editor)`: корректная schema, фактически 0 тестов.
- Реальный run: completed за 1 poll, total=0, outcome=no_tests, 0 Console errors,
  no Git delta; команда корректно завершилась non-success.
- Финальная сцена clean; Console без errors, две прежние warning.
- Helper, Editor, Hub и Licensing Client остановлены.

## Требуется при интеграции

- Инфраструктура готова, но quality gate ограничен отсутствием EditMode-тестов.
- Следующая полезная задача — добавить минимальные реальные тесты выбранного
  проекта/контракта отдельным scope, затем повторить workflow до `outcome=passed`.
