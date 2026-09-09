# Лес, забывший тропы

Атомарный Unity-проект истории `les-zabyvshiy-tropy` серии «Ночелесье», созданный из `Projects/novels-content-template`.

- шесть эпизодов с целевым хронометражем 25–45 минут (Player-хронометраж ещё не проведён);
- пять значимых групп выбора и три достижимые концовки;
- пять цельных character packages;
- шестнадцать story-used фоновых состояний, 12 choice illustrations, audio/SFX;
- собственная Bubble presentation;
- шесть отдельных обложек эпизодов в `Config/EpisodeCovers/`, назначенных по ID через `_catalogCover` (экспорт после интеграции актуального SDK).

Unity-backed compilation, import, bundle, Android и Player visual gates отложены до отдельно разрешённого финального validation/acceptance-слота.

Обложки и проверка их привязок: `Art/EPISODE_COVERS.md`; точные запросы генератору: `Art/EPISODE_COVER_PROMPTS.md`.

Повторяемая статическая проверка без Unity: `python3 Projects/novels-les-zabyvshiy-tropy/Art/check_source.py` из корня worktree. Она обходит 72 комбинации решений и проверяет селекторы с учётом `maincharacter`; компиляцию Ink и runtime-проверку не заменяет. Исправления и оставшиеся ограничения перечислены в `Art/STORY_REVIEW.md`.
