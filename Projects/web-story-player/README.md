# SomeGame Web Story Player

Один общий Unity WebGL Player для запуска отдельной истории с сайта. Сайт
передаёт стабильный `storyId` и разрешённую версию release через JavaScript;
Player загружает только выбранную историю.

## Текущий статус

`episode progression implemented; browser acceptance recorded below; full player pending`

Переход первого эпизода во второй проверен 2026-09-12 во встроенном браузере.
Исправлен существующий marker mismatch «Чёрной мельницы»: definition теперь
задаёт `...: КОНЕЦ СЕРИИ`, как все четыре Ink-эпизода. После локального WebGL
rebuild на отдельной версии `smoke-boundary-20260912` получены completion s01e01,
next_episode_available и запуск s01e02. Реплика ветки поиска Мити подтвердила
перенос Ink state. Reload восстановил s01e02 с двумя решениями и той же репликой;
browser error log пуст. Native/web compile и Player build из предыдущего шага
не менялись. Ранний NextEpisode (отказ) и безопасный ReturnToSite проверены ранее.
Прежний `smoke` save, содержащий решения за старой границей, не удалён.
Перед публикацией исправленного контента нужна отдельная оценка совместимости
старых native сейвов; contentVersion в этой локальной правке не повышалась.

Проект содержит `WebPlayerEntryPoint`, browser event bridge и управляемую
`WebStoryRuntimeSession`. Каждый принятый launch создаёт собственный
`EpisodeRuntime`, cancellation lifetime и динамический runtime root. Повторный
идентичный launch отклоняется, а новый launch корректно отменяет прежнюю сессию.
Нормальная смена сессии отключает её ввод, отменяет чтение, ждёт завершения
загрузки/чтения и записи последнего сейва, затем освобождает scope.
Во время ожидания новый `Launch` отклоняется с `session_stopping` (не ставится
в очередь). При `session_stop_failed` новая история не запускается: snapshot
сохраняется в памяти, следующий `Launch` повторяет запись перед переключением.
Не закрывать/перезагружать вкладку при этой ошибке: неподтверждённый snapshot
ещё не гарантированно сохранён. Закрытие вкладки не может ждать async stop.
Подключён same-origin WebGL content source: manifest проходит общую проверку,
а первый чанк — загрузку и проверку целостности. Одновременно скачивается один
чанк; временный кеш ограничен бюджетом 64 MiB. Кеш не является хранилищем сейвов.
Отдельный IndexedDB adapter подтверждает запись после commit транзакции.
WebEpisodePlayer подключает Ink, авторские экраны и общую StoryPresentation.
Последний открытый эпизод читается напрямую; ordinary advance и choice ждут commit IndexedDB.
При reload проверяется совместимость и воспроизводится сохранённая очередь.
Повреждённый/несовместимый сейв не удаляется. `content_ready` всё ещё не означает
готовность чтения: после него идут `resume_loaded`, `reading_started`, `dialogue_ready`.

## Владение UI

Стартовая `Assets/WebPlayer/WebPlayer.unity` намеренно не содержит Canvas,
EventSystem или экранов приложения. `WebPlayerEntryPoint` создаётся до загрузки
сцены через `RuntimeInitializeOnLoadMethod`, принимает конфигурацию сайта и в
передаёт её в отдельную runtime-сессию.

Runtime root создаётся динамически только после принятия launch configuration.
Камера и EventSystem создаются под этим root; экраны создают существующие
контроллеры и освобождают вместе со scope сессии. EntryPoint не знает
структуру UI конкретной истории и не создаёт каталог.

Первый профиль — `media-free-v1`: проект намеренно не подключает Unity audio и
video modules и не содержит медиаресурсов. Исходники медиа в story projects не
изменяются.

Общий Content SDK подключён с символами `NOVELS_MEDIA_FREE` и
`NOVELS_STORY_PLAYER_ONLY` для WebGL и Standalone (Editor).
Первый исключает audio assembly и video execution в общем LocationController:
фон использует статичную картинку или существующий missing-background fallback,
без разрешения media URL и ожидания cutscene. Второй исключает каталог и
content-authoring Editor assembly. Story projects этих символов не получают.
Общие Bubble/Choose/Character/Location/Notification подключены к сессии;
`link.xml` сохраняет динамически загружаемые SDK views. `WebPlayerBuild` перед
сборкой генерирует ignored defaults из существующих SDK экранов и native
fallback art/font. Только из generated Location удаляется VideoPlayer;
исходный prefab не меняется. Произвольные story-local prefab с video components
этим не очищаются и требуют отдельной проверки. Гардероб не создаётся.

Проверенный сценарий: реальная «Чёрная мельница», первый эпизод, три варианта
первого выбора; выбрана ветка поиска Мити. Reload восстановил 61 решение и ту же
реплику. Missing release → retry без reload также восстановил чтение. Проверка
выполнена во встроенном браузере на локальной development-сборке, не на сайте.
Переходы между эпизодами и возврат предоставлены как команды/события для сайта;
production кнопки и UX ошибки сейва/повтора ещё не подключены. Полная история, все чанки, память мобильного браузера,
quota/offline и разные story-local layouts не прошли acceptance.

## Контракт запуска

Страница создаёт Player, затем вызывает:

```javascript
unityInstance.SendMessage('WebStoryPlayer', 'Launch', JSON.stringify({
  storyId: 'chernaya-melnitsa',
  storyVersion: '5',
  manifestUrl: '/content/stories/chernaya-melnitsa/5/webgl/release.json',
  locale: 'ru',
  profile: 'media-free-v1',
  returnUrl: '/apps/kostroma/stories/chernaya-melnitsa/'
}));
```

Player отправляет события браузеру:

```javascript
window.addEventListener('somegame:web-player-event', event => {
  console.log(event.detail);
});
```

Доступны `player_ready`, `launch_accepted`, `launch_rejected`, `story_loading`,
`runtime_session_ready`, `content_requested`, `content_ready`,
`runtime_session_stopping`, `runtime_session_cancelled` и `runtime_error`. После ошибки контента разрешён
повторный корректный `Launch` без перезагрузки Player.

На границе эпизода сначала записываются решения, затем атомарно completion
и Ink entry state следующего эпизода. Только после подтверждения выдаются
`episode_completed` (code = episodeId) и `next_episode_available` либо
`story_completed`. Сайт вызывает `SendMessage('WebStoryPlayer','NextEpisode','')`:
текущая сессия безопасно закрывается, следующая восстанавливается из прогресса.
До завершения эта команда отклоняется; произвольный episodeId не принимается.
Обычный Launch/reload выбирает последний открытый эпизод. Завершённая история
повторно не проигрывается автоматически; restart пока не реализован.

`SendMessage('WebStoryPlayer','ReturnToSite','')` ждёт save/stop, затем выдаёт
`return_to_site` с маршрутом в `code`. Навигацию выполняет сайт, не Unity.
`returnUrl` необязателен (по умолчанию `/`), допускает только простой same-origin
путь из латинских букв, цифр, `/`, `_`, `-`; внешние URL/query/fragment не принимаются.
При ошибке сейва событие возврата не выдаётся: повторить команду после устранения
ошибки, не перезагружая страницу с неподтверждённым snapshot.

## Локальная проверка

`Tools/serve-smoke.py` — только локальный development host, не production-сервер.
Он обслуживает готовый `Build/WebGL` и существующий WebGL release
`novels-chernaya-melnitsa`, не изменяя исходники истории. Запуск из project root:

```sh
python3 Projects/web-story-player/Tools/serve-smoke.py
```

Открыть `http://127.0.0.1:8767/` во встроенном браузере Codex или Яндекс Браузере.
В каталоге версии ожидаются `release.json`, `<bundle-name>/<hash>` и `Files/`.
Server отображает только разрешённые build-каталоги, слушает loopback.

Дополнительные локальные fault-injection кнопки (не production bridge):
`Switch during next save` задерживает доставку commit acknowledgement на 1,5 с
и запрашивает replacement + конкурирующий launch. Проверено: stopping → rejection
→ acknowledgement → cancelled → следующий accepted; возврат восстановил 62 решения.
`Block save writes` имитирует отказ readwrite transaction; повторный launch
оставался заблокирован `session_stop_failed`. После `Allow save writes` + retry
записан удержанный snapshot; reload восстановил 63 решения. Это имитация ошибки,
не тест реального исчерпания квоты или незавершённой браузерной транзакции.

Проверено 2026-09-11 во встроенном браузере Codex: настоящий Player принимает
launch и загружает первый чанк до `content_ready`; duplicate/external URL
отклоняются; отсутствующая версия даёт `content_failed`; последующий корректный
launch снова достигает `content_ready` без reload. Повтор той же версии может
использовать кеш — negative smoke использует отдельную отсутствующую версию.

`Tools/save-smoke.html` проверяет сам JS adapter с реальным IndexedDB:
write/transaction commit → reload → exact read → delete → missing key.
Это не проверка Unity↔JS save bridge и не восстановление прохождения истории.
Облачной синхронизации пока нет; очистка данных сайта удалит локальные сейвы.

## Unity

- Version: `6000.3.11f1`.
- Target: WebGL, single-threaded.
- Initial memory: 128 MiB, growth allowed to 512 MiB; уточняется по результатам
  мобильного spike.
- Compression: Brotli with decompression fallback until hosting serves
  precompressed artifacts with correct headers.
- Official Unity Pipeline: `com.unity.pipeline` `0.5.0-exp.1`.
- Optional MCP server: `unity_web_story_player`.
- Exact project path:
  `/Users/iantonishin/Fork/SomeGame/Projects/web-story-player`.

Первый разрешённый Unity import, compile/reload и повторный cold-start MCP probe
пройдены на `6000.3.11f1`. Development WebGL Player build и браузерная загрузка
контента также пройдены. Первый эпизод, выбор и Unity↔IndexedDB reload smoke
пройдены на development build. Публичный сайт не изменён.

## Следующая интеграционная граница

Общие lifecycle, очередь, replay validator и UI-контроллеры уже используются
обоими плеерами. Общий NovelProgress подключён через Novels.Progress; сайт
владеет NextEpisode/ReturnToSite кнопками и обработкой событий. Подключение этих
контролов подготовлено локально в `Website/public/player`: один прямой URL,
opt-in config, выход после flush и повтор при ошибке сейва без reload. Инструкция
и evidence — `Website/public/player/README.md` (Website — отдельный Git-репозиторий).
Production пока не включён; каталог→плеер и новая NextEpisode-кнопка требуют E2E.
Demand-loaded чанки не имеют ещё web progress UI
и полного memory/performance acceptance. Delayed acknowledgement и имитация
ошибки/повтора сохранения проверены; реальные quota/offline, закрытие вкладки
во время записи и полноценная смена опубликованных версий требуют отдельных проверок.

### Общие сохранения

`Novels.Save` уже перенесён в `Packages/NovelsStoryRuntime/Save` с сохранением
assembly GUID и бинарного формата. `WebDecisionSaveSession.Open` предварительно
читает запись для story/release/episode из IndexedDB и возвращает тот же
`SaveSystem`, которым пользуется приложение. Передать отдельно release version
для адреса хранилища и content version из definition для проверки envelope.
После каждого checkpoint и до уничтожения сессии необходимо ожидать
`Decisions.FlushAsync()`: он ждёт завершения writer и commit IndexedDB.
`RetryFlushAsync()` повторяет запись последнего snapshot после storage failure.
`FlushSynchronously` для такого backend намеренно запрещён. Повреждённый или
несовместимый save остаётся нетронутым и не считается новым прохождением.

Adapter подключён к первому эпизоду. Протокол Unity↔JS содержит явный `found`,
поскольку `JsonUtility` не сохраняет различие JSON null и пустой строки для
этого callback. Браузерный smoke проверил новый сейв, advance, choice, reload
и восстановление 61 решения. Закрытие вкладки посреди записи не гарантирует
commit; уже подтверждённые checkpoints остаются основой восстановления.
