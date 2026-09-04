# SomeGame automation runner

`Tools/somegame` запускает bounded workflows и печатает ровно один компактный
JSON. Полные логи сохраняются в ignored `Novels/Build/Logs/automation`.

```bash
Tools/somegame docs-check
Tools/somegame context --task integration
Tools/somegame start-task --agent-id <id> --task <summary> --scope <exact-scope>
Tools/somegame tooling-tests
Tools/somegame story-check --agent-id <lock-owner> --target <story-id> [--build]
Tools/somegame verify --explain --base-ref origin/main
Tools/somegame verify --agent-id <lock-owner> --base-ref origin/main
Tools/somegame commit-plan
Tools/somegame finish-check --agent-id <lock-owner>
Tools/somegame git-publish --agent-id <lock-owner> [--ssh-key <local-key-path>]
Tools/somegame licensing-preflight
Tools/somegame licensing-preflight --recover --agent-id <lock-owner> --confirm-pid <exact-pid>
Tools/somegame content-gate --agent-id <lock-owner> --platform editor
Tools/somegame content-gate --agent-id <lock-owner> --target catalog --platform editor --close-hub
Tools/somegame editor-gate --agent-id <lock-owner> --project Novels --compile
Tools/somegame player-build --agent-id <lock-owner> --target Android --mode Embedded --development
Tools/somegame player-build --agent-id <lock-owner> --target Android --mode Embedded --test-signing
Tools/somegame android-smoke --agent-id <lock-owner> --apk <path> \
  --package-id <id-from-current-apk-or-player-settings>
Tools/somegame android-dev-cycle --agent-id <lock-owner> --package-id <id>
Tools/somegame clean-generated --agent-id <lock-owner> --project <exact-project> [--apply]
Tools/somegame finish-task --agent-id <lock-owner> --paths <owned-path...> \
  --summary <result> [--allow-pending]
```

`start-task` идемпотентно создаёт собственные agent/request records и получает
lock только в первой позиции FIFO. `finish-task` выполняет scoped static/unit
gates, пишет компактный handoff, завершает agent и освобождает только собственные
request/lock; незаданные runtime/manual gates блокируют release без
`--allow-pending`.

`tooling-tests` объединяет unit suites трёх локальных инструментов.
`story-check` валидирует одну атомарную историю, а с `--build` выполняет её
целевую platform-сборку. `android-dev-cycle` последовательно объединяет Embedded
Android Player build и smoke одного APK. `clean-generated` принимает точный
Unity project, по умолчанию только показывает разрешённые generated-каталоги и
удаляет их лишь с `--apply`.

Для zsh completion добавьте каталог `Tools` в `fpath`, затем выполните
`autoload -Uz compinit && compinit`; файл называется `somegame-completion.zsh`.

`docs-check`, `context`, `verify --explain`, `commit-plan` и read-only
`licensing-preflight` не требуют lock. Write workflows fail-closed проверяют
точного владельца repository write-lock. `verify` исполняет
минимальный changed-path plan, сохраняет полные логи и кэширует только полный
успех; `finish-check` ничего не закрывает и сообщает blockers до завершения.
`git-publish` публикует только текущий `HEAD` в `origin/main`: требует правильную
ветку, чистое дерево кроме собственных untracked coordination records, делает
`fetch`, запрещает автоматическую интеграцию remote-ahead истории и никогда не
использует force-push. `--ssh-key` только добавляет локальный ключ в SSH-agent;
ключ и его содержимое в репозиторий не записываются.
`docs-check` выполняет scoped diff-check только для AI docs и общего tooling;
чужие Unity-generated whitespace изменения не маскируют результат.
`content-gate --target <id>` выполняет один явно выбранный atomic build и нужен
для live-проверки в очень грязном рабочем дереве; без `--target` используется
changed-path plan.
Перед batch/Editor start runner fail-closed обнаруживает работающий Hub.
`--close-hub` разрешает отправить `TERM` только main Hub PID и дождаться
process barrier; helper-процессы отдельно не сигналятся.

`licensing-preflight --recover` отправляет только `TERM` явно переданным через
`--confirm-pid` main Unity Hub/Licensing Client PID, только при conflict markers
и отсутствии Editor. Сокеты, лицензии и caches автоматически не удаляются.

`editor-gate` по умолчанию подключается к уже открытому Editor. `--start-editor`
разрешает runner запустить один Editor; запущенный им процесс будет остановлен
при завершении, если не передан `--no-stop-editor`.

`android-smoke` получает package ID явно, ждёт упорядоченные
`[NOVELS_SMOKE]` events и сохраняет screenshot/full logcat/activity только при
failure. Для ручного сценария список ожидаемых событий меняется через
`--required-events`.

`player-build --test-signing` собирает обычный Android Player без
`BuildOptions.Development`, но использует отдельный локальный ключ из
`Novels/LocalSigning`. При первом запуске ключ и credentials создаются
автоматически и игнорируются Git; production keystore и его настройки не
изменяются. Режим несовместим с `--development` и другими платформами.
