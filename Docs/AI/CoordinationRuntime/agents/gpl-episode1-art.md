# Agent: gpl-episode1-art

- Status: ready-with-limitations
- Task: добавить утверждённые фоны и готовые персонажные ассеты эпизода 1 GPL.
- Scope: `Projects/novels-gpl/Assets/Locations/**`, `Projects/novels-gpl/Assets/Characters/**`, GPL Ink/definition только для адресации, собственные coordination files.
- Expected files: четыре location PNG с Unity meta, production-ready character sprites с meta, команды локаций в `s01e01.ink`, скомпилированный Ink и GPL definition при необходимости.
- Base commit: `7e9c7727`.
- Note: персонажные генерации с нарисованной шахматной/чёрной подложкой отклонены и не импортируются.

## Result

- Четыре утверждённых фона добавлены в `Assets/Locations` со стабильными GUID.
- В исходный Ink добавлены пять команд `Локация`; сцена решения повторно использует столовую.
- Персонажные слои не импортированы: последний утверждённый дизайн существует только в слитном превью, а background-extraction не дал настоящего alpha.
- Unity validation заблокирована Licensing Client; compiled Ink остаётся предыдущей версии.

## Resume result

- Unity Licensing восстановлена соседним потоком.
- Четыре фона импортированы, Ink и source map явно перекомпилированы.
- `novels-content validate gpl` успешно завершён.
- Для продолжения персонажей требуется явное разрешение использовать детерминированное локальное маскирование: встроенный image generation дважды не создал настоящий alpha.
