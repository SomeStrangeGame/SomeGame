# Coordination handoff history — 2026-09-03

Archived from the current handoff during full-tree publication.

- `toybedtime-story`: сказка `toybedtime` «Последний кубик не спит» для совместного чтения взрослого и ребёнка 2–4 лет опубликована в `origin/main` content-коммитом `7e8cf0b3`. Один эпизод, два бинарных выбора в обычном Bubble, один тёплый финал, три согласованных фона одной комнаты и вертикальная обложка; shared runtime и Presentation-prefabs не менялись. Story/catalog validate и editor builds, release asset audit, visual/source safety review и scoped diff-check passed. Ручной in-game replay не запускался.
- `final-main-publication`: завершённый `.agents/skills/somegame-create-story/**` принят из соседней задачи, повторно прошёл docs/staged-diff checks и зафиксирован атомарным коммитом `cdfc4fce` поверх publication evidence `d90b63a7`; финальный handoff — `ab01b4af`. Canonical non-force push подтверждён: `localSha == remoteSha == ab01b4afec15c35d76c8387f18027f6d1f83b0fe`. Pending: none.
- `somegame-create-story-skill`: добавлен проектный `.agents/skills/somegame-create-story` с режимами guided/auto-approve, обязательным авторским free-form жанром и отдельной factual-basis осью. Workflow требует канонический checkout, отдельную ветку от актуального `main` без worktree, identity-first персонажей, сюжетные костюмы/эмоции, Ink/catalog integration и scoped validation. `docs-check`, YAML/frontmatter, reference links, TODO/whitespace и `git diff --check` passed. Штатный `quick_validate.py` не стартует в доступных Python из-за отсутствующего `PyYAML`; его проверки повторены системным YAML-парсером. Изменения не коммитились и не публиковались.
- `publish-all-local-main`: весь локальный dirty tree опубликован в `origin/main` как атомарные tooling/docs, runtime/shared packages и production content commits с последующим non-force merge четырёх remote-коммитов. `git diff --check`, automation tests, editor content builds для catalog и всех 12 stories, fresh Novels Editor compile и finish-check passed; EditMode test assemblies отсутствуют. После одного штатного licensing recovery-loop (TERM только конфликтующего PID 57057; license/cache/socket не удалялись) Unity gates стабильны. `localSha == remoteSha == 8a75e39e9bb0f976311d18d472b2762d221c07a7`; существующие manual visual gates остаются в handoff.
- `tzm-s01e01-smoke-fixes`: устранён источник массового fallback Салли — несуществующие в каноническом и переданном read-only арт-наборе `Сандали` больше не применяются как аксессуар; исправлена опечатка эмоции `снисхождение`. Финальный экран получил уменьшенный end-of-episode текст, сдвиг единственной кнопки и принудительный layout rebuild в TZM bubble variant. `novels-content validate tzm`, `git diff --check` и два Android Embedded player build passed. Первый контрольный проход дошёл до `s01e01.ink:1905` без `fallback.used`; финальный APK запущен без crash/error/fallback, но повторный полный проход остановился у сохранённого последнего кадра `s01e01.ink:169`: video layer перекрывает следующий dialogue и не принимает продолжение. Это отдельный runtime-дефект `Кат-сцена (стоп)`, поэтому визуальная проверка нового финального layout остаётся pending. Evidence: `Novels/Build/Players/automation/Android/Embedded/Novels.apk`, `Novels/Build/Logs/automation/tzm-s01e01-smoke-fixes-20260902.log`, `Novels/Build/Logs/automation/tzm-s01e01-smoke-fixes-blocked-stop-frame-20260902.png`.
- `gpl-episode2-art-integration-smoke`: commit `a92ba3f0` завершил арт эпизода: три фона и четыре цельных Павла (`main`, раненый, с рычагом, двойник), Ink selectors, meta и full/face dark/light contact sheets. Alpha/dimensions/GUID и визуальные края passed. Повторный GPL validate заблокирован Unity licensing (`LicenseClient-iantonishin` channel отсутствует, headless license не найден); own batch остановлен, lockfile удалён. После восстановления Hub нужны validate, compiled Ink, editor/android content, Embedded APK и episode-two smoke.

## 2026-09-01T16:54:00Z — tzm-charon-clothed-scene — ready-for-integration
Task: показать Харона одетым в сцене первого появления.
Changed: во всех четырёх репликах сцены закреплён существующий вид `основной`; удалён только TODO `в одежде`.
Validation: scoped line/TODO audit, `git diff --check` и `novels-content validate tzm` passed.
Pending / risks: none; TODO `актуализировать с текстом` сохранён.

## 2026-09-01T15:55:00Z — tzm-ink-accessory-splashes-bun — ready-for-integration
TZM Ink: Салли снимает аксессуар после сна в катере; `брызги` оставлены без runtime-правок; под водой добавлен `пучок`. Три TODO удалены, остальные не менялись.
Validation: scoped diff/TODO audit clean; `novels-content validate tzm` passed.

## 2026-09-03T09:45:00Z — choice-horizontal-layout — ready-with-limitations

Runtime choice-кнопки в TZM и fallback bubble теперь копируют горизонтальные
anchors/pivot текста и центрируются по его X, сохраняя рассчитанное положение
ниже реплики. Scoped diff-check и fresh Novels compile прошли; пользователь
проверяет кадры `s01e01.ink:136` и `sobibor.ink:163` вручную.
