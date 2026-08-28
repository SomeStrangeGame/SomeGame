# Parallel work: windows-player-build

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: main
- Базовый commit: 8f7082db
- Ответственный поток: текущий чат Novels
- Последнее обновление: 2026-08-26

## Разрешённая область

- `Novels/Tools/build-remote-player.sh`
- `Novels/Tools/build-embedded-test-player.sh`
- `Novels/Assets/Editor/PlayerBuildAutomation.cs`
- `Novels/Assets/Editor/RemotePlayerBuildGuard.cs`
- `Novels/Assets/Novels/EntryPoint.cs`
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`
- `Packages/NovelsContentSdk/Editor/AtomicContentBuild.cs`
- `Tools/novels-tools/novels-content`
- `Novels/Build/Players/Windows/**` (generated, ignored)
- собственные coordination runtime-файлы

## Не изменять

- Game runtime
- атомарные authoring-ассеты контентных проектов
- чужие coordination-файлы

## Изменённые контракты

- Remote Player build tool принимает target `Windows` и передаёт Unity target
  `Win64`.
- Atomic content pipeline принимает `windows`, строит
  `BuildTarget.StandaloneWindows64` и публикует platform key `Win`.
- Embedded test build читает контент из
  `StreamingAssets/NovelContent` и не создаёт HTTP-конфигурацию.

## Выполнено

- Добавлена поддержка Windows/Win64 в Player и atomic content tooling.
- Добавлен отдельный embedded test workflow без HTTP и удалённого сервера.
- Собраны Windows-бандлы каталога, TZM и ZDM.
- Собран автономный Windows x86_64 Player в
  `Novels/Build/Players/WindowsOffline`.

## Проверено

- `zsh -n` для трёх build-скриптов: успешно.
- `git diff --check`: успешно.
- Windows content releases: catalog, TZM и ZDM присутствуют.
- Unity Player build: Success, 2139,3 MiB.
- `file Novels.exe`: PE32+ GUI x86-64 for MS Windows.

## Требуется при интеграции

- Запустить папку целиком на Windows; macOS не может выполнить `.exe`.
