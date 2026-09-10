# Лес, забывший тропы

Атомарный Unity-проект истории `les-zabyvshiy-tropy` серии «Ночелесье», созданный из `Projects/novels-content-template`.

- шесть эпизодов с целевым хронометражем 25–45 минут (Player-хронометраж ещё не проведён);
- пять значимых групп выбора и три достижимые концовки;
- пять цельных character packages;
- шестнадцать story-used фоновых состояний, 12 choice illustrations, audio/SFX;
- собственная Bubble presentation;
- шесть отдельных обложек эпизодов в `Config/EpisodeCovers/`, назначенных по ID через `_catalogCover` (экспорт после интеграции актуального SDK).
- story-owned веб-превью первого эпизода в `Config/Preview/preview.json` с тремя реально показанными персонажами и дословным фрагментом до первого выбора.

Unity-backed compilation, import, bundle, Android и Player visual gates отложены до отдельно разрешённого финального validation/acceptance-слота.

Обложки и проверка их привязок: `Art/EPISODE_COVERS.md`; точные запросы генератору: `Art/EPISODE_COVER_PROMPTS.md`.

Повторяемая статическая проверка без Unity: `python3 Projects/novels-les-zabyvshiy-tropy/Art/check_source.py` из корня worktree. Она обходит 72 комбинации решений, проверяет селекторы с учётом `maincharacter`, а также JSON, ссылки на изображения и дословное соответствие веб-превью исходному Ink; компиляцию Ink и runtime-проверку не заменяет. Исправления и оставшиеся ограничения перечислены в `Art/STORY_REVIEW.md`.
