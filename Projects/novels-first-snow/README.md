# Первый снег

Обложка единственного эпизода `s01e01` утверждена и сохранена в
`Config/EpisodeCovers/s01e01.png`; `_catalogCover` назначен в definition.
Ветка уже содержит актуальный main SDK с полем обложки; её экспорт и показ
остаются частью отдельного финального Unity-слота.
Подробности: [episode-cover handoff](Docs/Preproduction/episode-cover-handoff.md).

Атомарный Unity-проект истории `first-snow` для SomeGame.

- Жанр: романтическая драма.
- Возраст: 12+.
- Плановый хронометраж: около 60 минут.
- Фактическая основа: полностью вымышленная.
- Границы: без эротики и трагической смерти.

Проект создан из `Projects/novels-content-template`. Разрешённый финальный
Unity-слот выполнен 2026-09-11: импорт, каноническая Ink-компиляция с source map,
atomic editor content build и bundle audit прошли. Подробное evidence находится
в [Unity validation](Docs/Evidence/unity-validation.md).

Standalone Ink compilation, exhaustive route traversal,
runtime-address/default-outfit audit и атомарная Unity-сборка проходят.
Эмулятор и ADB явно исключены пользователем для этого чата и не считаются
device evidence. Catalog/Player-интеграция, Play Mode и ручная визуальная
приёмка остаются отдельными следующими воротами.

Story-owned веб-превью находится в `Config/Preview/preview.json`: это точный
линейный фрагмент канонического начала с двумя утверждёнными персонажами.
Датированный архив принят с 2026-09-11 UTC; недоступная более ранняя история
честно отмечена как пробел, а не реконструирована.
