# Parallel work: webgl-local-prototype

- Статус: ready-for-integration
- Ветка: prototype/webgl-local-platform
- Базовый commit: main
- Ответственный поток: текущий чат Novels
- Последнее обновление: 2026-08-25

## Разрешённая область

- `Novels/Assets/Novels/**`
- `Novels/Assets/Editor/**`
- `Novels/ProjectSettings/**` только при прямой необходимости WebGL build
- `Packages/NovelsContentSdk/Editor/**` только для добавления WebGL content target
- `Packages/Bundles/**` только для WebGL-safe выполнения существующих файловых операций
- `Tools/novels-tools/**` только для добавления WebGL content target
- `Novels/Docs/AI/work/parallel/ParallelWork.webgl-local-prototype.md`
- `Novels/Docs/AI/CoordinationRuntime/agents/webgl-local-prototype.md`
- собственная runtime-заявка, write-lock и запись в `HANDOFF.md`

## Не изменять

- `Projects/novels-*/Assets/**`
- Ink и authoring content
- чужие coordination status/runtime-файлы
- Android/iOS production delivery

## Изменённые контракты

- Планируется локальный WebGL target и persistent editor prototype без сети.

## Выполнено

- Создана отдельная ветка `prototype/webgl-local-platform`, commit `cfb92896`.
- Добавлен WebGL target в atomic content pipeline и CLI.
- Добавлена единая команда Editor `Novels/Prototype/Build & Preview WebGL`.
- Добавлены локальные email/password account и analytics в persistent storage.
- Файловые операции сохранений и Bundles адаптированы к отсутствию threads и
  `DownloadHandlerFile` в WebGL.

## Проверено

- `novels-content doctor`: успешно.
- `zsh -n Tools/novels-tools/novels-content`: успешно.
- `git diff --check`: успешно.
- Unity batch compile заблокирован локальным Unity Licensing Client:
  `Unsupported protocol version '1.18.0'`; до компиляции скриптов не дошёл.

## Требуется при интеграции

- После восстановления Unity license: Unity compilation, затем команда
  `Novels/Prototype/Build & Preview WebGL` и browser smoke test.
