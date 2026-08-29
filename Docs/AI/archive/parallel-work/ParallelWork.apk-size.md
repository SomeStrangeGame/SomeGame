# Parallel work: apk-size

- Статус: integrated
- Архивировано аудитом: 2026-08-28; реализация присутствует в `main` (`f849ff22`) или его истории
- Ветка: main
- Ответственный поток: уменьшение первоначального Android APK
- Последнее обновление: 2026-08-24

## Разрешённая область

- Android Player settings и build profile
- `Novels/Assets/Editor/PlayerBuildAutomation.cs`
- встроенные fallback-ресурсы
- shader stripping settings

## Не изменять

- `Projects/novels-*`
- `Packages/NovelsContentSdk/**`
- каталог и runtime-логику историй

## Изменённые контракты

- Публикация и AAB не входят в задачу.
- Release Android использует IL2CPP, только ARM64, engine stripping и не ниже Medium managed stripping.

## Выполнено

- Подтверждено, что content-проекты исключаются из staging Player-сборки.
- Release остаётся без Development-флага и debug keystore.
- Android managed stripping повышен до Medium.
- Android fallback-текстуры ограничены 1024 px и ASTC 6x6; ненужные physics shapes отключены.
- Shader stripping уже работает в автоматическом режиме; список preloaded shaders пуст.
- `CodeStrippingFix` сохранён: он защищает типы Unity из удалённых AssetBundle от engine stripping.
- Найдены два одинаковых файла Liberation Sans по 344.5 KiB, но их дедупликация затрагивает базовые prefab общего SDK и вынесена из этого изолированного изменения.

## Проверено

- `git diff --check` — без ошибок.
- Текущий Editor.log — новых ошибок компиляции не найдено.
- Предыдущий remote development APK: 64.3 MiB; это baseline, не финальный результат оптимизации.

## Требуется при интеграции

- После закрытия Unity собрать release APK через `Tools/build-remote-player.sh` и сравнить размер с baseline.
- На Android проверить загрузку удалённого UI: `CodeStrippingFix` намеренно сохранён.
