# Parallel work: story streaming experiment

- Статус: implementation complete; manual cold/warm smoke pending
- Ветка: `experiment/story-preview-streaming`
- Базовый commit: `ce28b7e0c6ead5ec7a8dee712dc6800d83698d4c`
- Ответственный поток: preview/chunk streaming test version
- Последнее обновление: 2026-08-25

## Разрешённая область

- `Packages/Bundles/**`
- `Packages/NovelsContentSdk/Editor/**`
- `Novels/Assets/Novels/**`
- `Novels/Assets/Editor/**`
- `Docs/AI/archive/parallel-work/ParallelWork.story-streaming-experiment.md`
- `Projects/novels-tzm/Assets/Editor/**`
- экспериментальные build-артефакты TZM вне Git

## Не изменять

- `Projects/novels-catalog/**`
- `Projects/novels-zdm/**`
- authoring Ink и исходный арт TZM
- чужие status-файлы

## Изменённые контракты

- Экспериментальная сборка включается только через `NOVELS_STREAMING_EXPERIMENT=1`.
- Добавлены имена preview-бандла, delivery-групп `<story>-preview`,
  `<story>-chunk-N` и предиктивных `<story>-media-N`.
- Без feature flag production-сборка сохраняет прежний монолитный контракт.

## Атомарные блоки

1. delivery-plan contract and build-time Ink/asset analysis;
2. `preview`, `chunk-0..N` bundle build output;
3. continuous runtime chunk queue and predictive media prefetch;
4. live-upgrade backgrounds and character layers;
5. diagnostics and realistic Editor network simulation;
6. TZM experimental rebuild and cold/warm validation.

Публичное имя облегчённого стартового чанка — `preview`. Отрицательные номера
чанков в manifest, release и runtime API не используются.

## Выполнено

- Созданы отдельная ветка и worktree.
- Build-time анализ читает исходный Ink в порядке использования ассетов и
  формирует последовательность чанков; ветвления покрываются candidate-наборами.
- Для TZM собран `preview` только из ассетов `chunk-0` (текстуры до 256 px),
  а полный арт разделён на `chunk-0..12`.
- Ink включён в preview-группу; видео и аудио вынесены в media-группу.
- Runtime начинает историю с preview и непрерывно догружает очередь
  `chunk-0 -> media-0 -> chunk-1 -> media-1 ...`; требуемый сейчас чанк
  получает приоритет.
- Текущий фон при готовности full перечитывается и плавно заменяется без
  пересборки контента; во время видео обновляется скрытый poster.
- Video RenderTexture создаётся после `Prepare()` по native-размеру MP4, а не
  по уменьшенному preview-poster; depth-buffer для UI-видео отключён.
- Добавлен компактный OnGUI HUD: Ink source, FPS, frame time, RAM, tier,
  прогресс и скорость загрузки.
- Добавлены кнопки `Cold App` и `Warm`; cold использует новый namespace кеша
  и возвращает приложение в каталог без переустановки.
- В Editor поддержана реальная симуляция канала через `NOVELS_SIMULATED_MBITS`.
- Симуляция также учитывает latency/jitter; доступны переменные
  `NOVELS_SIMULATED_LATENCY_MS` и `NOVELS_SIMULATED_JITTER_MS`.

## Проверено

- `novels-content doctor` — success.
- TZM editor streaming build — success; release `b3cddd9d...` скомпонован в
  `Novels/Build/LocalContent`.
- Результат: 1 preview-бандл, 13 art-чанков, 51 предиктивная media-группа.
- Preview-бандл: 1 795 434 B (1.71 MiB); preview delivery group вместе с
  bootstrap/Ink: 4 338 014 B (4.14 MiB).
- Unity batchmode compile основного проекта — success, C# errors отсутствуют.
- После сборки импортёры TZM восстановлены; authoring art/meta не изменены.
- `git diff --check` — success.

## Запуск теста

1. Запустить Unity Editor с переменной, например
   `NOVELS_SIMULATED_MBITS=20`.
2. Открыть проект `Novels` этого worktree и войти в TZM.
3. Сравнивать первый вход, `Warm` и `Cold App` по HUD.

## Ограничения прототипа

- Автоматическая Unity content validation после успешной сборки отдельно
  блокируется локальным Unity Licensing Client (`Unsupported protocol version`);
  compile и построение всех bundle/release артефактов завершились успешно.
- Требуется ручной cold/warm PlayMode smoke после открытия Editor пользователем.

## Требуется при интеграции

- Не интегрировать эксперимент в production до сравнительных замеров Legacy/Preview.
