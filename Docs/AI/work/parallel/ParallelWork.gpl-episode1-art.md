# Parallel work: gpl-episode1-art

- Статус: ready-for-integration
- Ветка: main
- Базовый commit: 7e9c7727
- Ответственный поток: интеграция арта первого эпизода «Голос подо льдом»
- Последнее обновление: 2026-08-28

## Разрешённая область

- `Projects/novels-gpl/Assets/Locations/**`
- `Projects/novels-gpl/Assets/Characters/**`
- `Projects/novels-gpl/Assets/Ink/**`
- `Projects/novels-gpl/Assets/gpl.asset`
- собственные записи в `Docs/AI/CoordinationRuntime/**`
- `Docs/AI/work/parallel/ParallelWork.gpl-episode1-art.md`

## Не изменять

- общие SDK и pipeline
- другие content-проекты
- посторонние изменения рабочего дерева

## Изменённые контракты

- Не планируются.

## Выполнено

- Утверждённый список ограничен четырьмя фонами: медблок, коридор, столовая, нижний уровень/буровая.
- Проверено, что старый layered-набор персонажей не соответствует последним утверждённым причёскам.
- Два автоматических background-extraction прохода отклонены: PNG не содержит настоящего alpha-канала.
- Четыре утверждённых PNG добавлены в `Assets/Locations` с отдельными `.meta`.
- В `s01e01.ink` подключены `медблок`, `коридор`, `столовая` и `нижний уровень`; сцена решения повторно использует `столовая`.

## Проверено

- `sips -g hasAlpha` для отклонённых генераций — `no`.
- Все фоны имеют размер 1672×941 и не требуют alpha.
- `Tools/novels-tools/novels-content doctor` — успешно.
- `git diff --check` по GPL scope — успешно.
- `novels-content validate gpl` — заблокирован Unity Licensing: unsupported protocol `1.18.0`, затем отсутствует `com.unity.editor.headless`.

## Требуется при интеграции

- После восстановления Unity Licensing реимпортировать `.meta`, перекомпилировать Ink и запустить `validate gpl`.
- Подготовить персонажные слои из последнего утверждённого цельного арта с настоящим alpha и обратной сборкой; старый layered-набор не использовать.

## Возобновление после восстановления лицензии

- Четыре location PNG успешно импортированы Unity.
- `gpl.ink.json` и source map явно перекомпилированы через временный Editor helper; helper удалён.
- Повторный `Tools/novels-tools/novels-content validate gpl` — успешно.
- Остаётся персонажный блок: встроенный image generation не выдал настоящий alpha. Нужен отдельный подтверждённый метод локального маскирования, затем зарегистрированные слои и обратная сборка.
