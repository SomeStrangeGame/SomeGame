# Parallel work: android-integration-test-protocol

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c77278d32c8fa7b09a3d8878e23ad42daafa4
- Ответственный поток: Android emulator integration-test protocol
- Последнее обновление: 2026-08-29

## Разрешённая область

- `Docs/AI/guides/ContentPipeline.md`
- собственные coordination records

## Не изменять

- runtime/build automation implementation
- чужие dirty changes and coordination records

## Изменённые контракты

- Успешный ADB-тест не создаёт screenshot; success определяется PID/activity, ожидаемыми log-маркерами, отсутствием блокирующих ошибок и тайм-аутами.
- Screenshot, полный logcat и activity dump создаются только при ошибке, тайм-ауте или нарушении ожидаемого состояния.

## Выполнено

- `ContentPipeline.md` переведён с обязательного visual gate на экономный failure-only visual diagnostic flow.
- Перечислены условия failure и обязательные диагностические артефакты.

## Проверено

- Scoped `git diff --check` и проверка обязательных формулировок.

## Требуется при интеграции

- Не возвращать обязательный screenshot в success evidence без отдельного эталонного visual-test контракта.
