# Current cross-chat handoff

Этот файл содержит только актуальное незавершённое состояние. Завершённая
история до ротации 2026-08-28 находится в
[`CoordinationHandoffHistory-through-2026-08-28.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-28.md).

Перед работой прочитайте этот файл полностью, затем проверьте утверждения по
текущим файлам, `git status --short`, runtime FIFO и write-lock. Архивный
handoff читается только при расследовании конкретного прежнего решения.

## Текущее состояние

- Runtime FIFO-заявок нет.
- Активного write-lock нет после завершения текущей cleanup-задачи.
- Единственная незавершённая архитектурная работа — WebGL local prototype:
  branch `prototype/webgl-local-platform`, commit `cfb92896` отсутствует в
  `main`; Unity compilation и browser smoke ещё не выполнены.
- Windows Player уже интегрирован в `main` через `f849ff22`; прежний статус
  `ready-for-integration` закрыт как устаревший.
- Story preview merge уже интегрирован через `60a13762`; прежний статус
  `active` закрыт.
- Bundle audit присутствует в `main`; прежний статус
  `ready-for-integration` закрыт.
- Локальный `main` содержит cleanup commit `6d15bec6`, который ещё не был
  отправлен в `origin/main` на момент создания этого снимка.
- В рабочем дереве сохраняются посторонние пользовательские изменения
  `.DS_Store`, `Novels/ProjectSettings/ProjectSettings.asset` и
  `Projects/novels-tzm/ProjectSettings/PackageManagerSettings.asset`; не
  включать и не откатывать их без отдельного запроса.

## Pending / risks

- WebGL prototype требует восстановления стабильного Unity Licensing,
  последовательной Unity-компиляции и browser smoke перед интеграцией.
- Перед публикацией локальных documentation commits повторно проверить
  `origin/main` и точный staged diff.

## Suggested next step

- Для обычной работы следовать `Novels/Docs/AI/README.md` и не читать архивную
  историю без конкретной причины.
- Для WebGL продолжить единственную запись из `work/parallel/` после проверки
  лицензии.

## 2026-08-28T13:31:00Z — character-hairstyle-protocol — completed

Task: дополнить общий протокол правилами проектирования причёсок.

Changed:
- `Novels/Docs/AI/rules/CharacterLayeringRules.md`: добавлены утверждение
  причёски на собранном персонаже, контроль естественного объёма, обязательное
  проектирование front/back и общее превью состава эпизода.

Validation:
- `git diff --check -- Novels/Docs/AI/rules/CharacterLayeringRules.md` — успешно.
- Commit `1671fd66` содержит только канонический документ правил.

Pending / risks:
- Публикация `main` в `origin/main` выполняется в этой же задаче.

Suggested next step:
- Использовать обновлённый порядок до производства игровых alpha-слоёв.
