# Parallel work: android-embedded-emulator

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c77278d32c8fa7b09a3d8878e23ad42daafa4
- Ответственный поток: Android Embedded player build and emulator smoke
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Novels/Assets/Editor/PlayerBuildAutomation.cs` (только development signing для Embedded Android)
- `Novels/Assets/Novels/EntryPoint.cs` (только выбор Android Embedded content source)
- `Packages/Bundles/StreamingAssetsContentSource.cs*` (read-only URI content source)
- `Novels/Build/**` (generated, ignored)
- собственные coordination records
- Android emulator runtime state

## Не изменять

- production source and assets вне указанной build-automation правки
- чужие dirty changes and coordination records

## Изменённые контракты

- Embedded Android development build временно использует Unity debug keystore; release signing не меняется.
- Android Embedded читает `jar:file://` StreamingAssets через UnityWebRequest вместо filesystem API.

## Выполнено

- Задача поставлена в runtime FIFO.
- Android Build Support и AVD подтверждены; build не запускался.
- Catalog, TZM и ZDM Android content пересобраны последовательно и встроены в development APK.
- Исправлены development signing и чтение Android `jar:file://` StreamingAssets.
- APK установлен и игра оставлена запущенной на экране каталога.

## Проверено

- Android Build Support установлен для Unity 6000.3.11f1.
- AVD `Novels_Pixel_7_API_34` ранее загрузился и доступен через adb.
- Player build: success, version `2026.08.29` (`3502656`).
- `adb install -r -d`: success; foreground `UnityPlayerGameActivity`, PID 7347.
- После исправления свежий logcat не содержит catalog/init/version/schema failures.
- Visual smoke: каталог TZM/ZDM отображается; системная fullscreen-подсказка закрыта.
- Scoped `git diff --check`: успешно.

## Требуется при интеграции

- Интегрировать только три scoped source files и `.meta`, не включая соседний dirty tree.
- GPL намеренно не включён: `schemaVersion: 1`, и текущий catalog перечисляет только TZM/ZDM.
