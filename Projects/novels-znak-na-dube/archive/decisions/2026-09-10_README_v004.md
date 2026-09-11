# Ночелесье: Знак на дубе

Атомарная история `znak-na-dube`: один эпизод тёмного фэнтези примерно на 34–45 минут,
8 значимых точек выбора и 3 достижимых финала. Исходный Ink находится в
`Assets/Ink/`; карточка и обложка — в `Config/`; runtime-арт, Bubble UI и аудио — в
`Assets/`. Утверждённый narrative package, provenance и проверки лежат в `Art/`.

Редакция 2026-09-10: [полный читаемый сценарий](Art/SCENARIO.md),
[изменения и ограничения](Art/REVISION_HANDOFF.md). Превью начала находится в
`Config/Preview/preview.json`; dated checkpoints — в `archive/`.
Длительность расчётная, а история архива до его внедрения неполна.

Текущий статус: `ready-for-final-validation`. Статическая подготовка завершена, но Unity import,
компиляция Ink, generated `.meta`, content build и runtime/manual acceptance отложены до единого
финального Unity-слота. История пока не зарегистрирована в каталоге.

Повторяемая проверка графа, предметов и ссылок на ассеты без Unity:

```bash
python3 Projects/novels-znak-na-dube/Art/check_story.py
python3 Projects/novels-znak-na-dube/Art/test_story_checks.py
python3 Projects/novels-znak-na-dube/Art/archive_revision.py verify
```

Она проверяет ограниченный синтаксис исходника и все 372 полных маршрута; официальную
компиляцию Ink и визуальную приёмку не заменяет. Результаты и ограничения:
[STATIC_VALIDATION.md](Art/STATIC_VALIDATION.md). После исправлений состояния предметов
начинайте новое прохождение, а не загружайте сохранение от прежнего кандидата.

Команды финального слота выполняются из корня репозитория только после отдельного разрешения:

```bash
Tools/novels-tools/novels-content validate znak-na-dube
Tools/novels-tools/novels-content build znak-na-dube editor
```
