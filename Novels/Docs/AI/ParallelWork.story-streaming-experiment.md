# Parallel work: story streaming experiment

- Статус: completed (experimental handoff)
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `ce28b7e0c6ead5ec7a8dee712dc6800d83698d4c`
- Ответственный поток: preview/chunk streaming test version
- Последнее обновление: 2026-08-25

## Разрешённая область

- `Packages/Bundles/**`
- `Packages/NovelsContentSdk/Editor/**`
- `Novels/Assets/Novels/**`
- `Novels/Assets/Editor/**`
- `Novels/Docs/AI/ParallelWork.story-streaming-experiment.md`
- `Projects/novels-tzm/Assets/Editor/**`
- экспериментальные build-артефакты TZM вне Git

## Не изменять

- `Projects/novels-catalog/**`
- `Projects/novels-zdm/**`
- authoring Ink и исходный арт TZM
- чужие status-файлы

## Изменённые контракты

- Экспериментальная сборка включается только через `NOVELS_STREAMING_EXPERIMENT=1`.
- Добавлены имена preview-бандла и delivery-групп `<story>-preview`, `<story>-media`.
- Без feature flag production-сборка сохраняет прежний монолитный контракт.

## Атомарные блоки

1. bootstrap/preview build output;
2. runtime preview fallback and prefetch;
3. Editor simulation and OnGUI diagnostics;
4. TZM experimental build and validation.

## Выполнено

- Созданы отдельная ветка и worktree.
- Для TZM собраны preview-бандл (текстуры до 256 px) и исходный full-бандл.
- Ink включён в preview-группу; видео и аудио вынесены в media-группу.
- Runtime начинает историю с preview и догружает full-арт в фоне.
- Текущий фон при готовности full перечитывается и плавно заменяется без
  пересборки контента; во время видео обновляется скрытый poster.
- Video RenderTexture создаётся после `Prepare()` по native-размеру MP4, а не
  по уменьшенному preview-poster; depth-buffer для UI-видео отключён.
- Добавлен компактный OnGUI HUD: Ink source, FPS, frame time, RAM, tier,
  прогресс и скорость загрузки.
- Добавлены кнопки `Cold App` и `Warm`; cold использует новый namespace кеша
  и возвращает приложение в каталог без переустановки.
- В Editor поддержана реальная симуляция канала через `NOVELS_SIMULATED_MBITS`.

## Проверено

- `novels-content doctor` — success.
- TZM editor build — success; release `c74e1cd2...` скомпонован в
  `Novels/Build/LocalContent`.
- Размеры: preview group 36 297 428 B; full-art 246 683 737 B;
  media 287 268 760 B (51 файл).
- Unity batchmode compile основного проекта — success, C# errors отсутствуют.
- После сборки импортёры TZM восстановлены; authoring art/meta не изменены.
- `git diff --check` — success.

## Запуск теста

1. Запустить Unity Editor с переменной, например
   `NOVELS_SIMULATED_MBITS=20`.
2. Открыть проект `Novels` этого worktree и войти в TZM.
3. Сравнивать первый вход, `Warm` и `Cold App` по HUD.

## Ограничения прототипа

- Preview пока строится из всего текущего art-бандла в уменьшенном качестве,
  а не только из будущего chunk 0.
- Live-upgrade персонажей и их слоёв пока не реализован; для них full
  используется при следующих загрузках визуальных ассетов.

## Требуется при интеграции

- Не интегрировать эксперимент в production до сравнительных замеров Legacy/Preview.
