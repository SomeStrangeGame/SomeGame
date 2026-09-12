# Web Story Player: план media-free MVP и подготовки хостинга

Статус: planning baseline. Документ не разрешает изменение runtime, сборку,
публикацию сайта или контента. Каждая такая операция выполняется отдельным scope
по общей очереди и применимым validation/release gates.

## Цель

Добавить веб как равноправный способ чтения отдельных историй, сохранив сайт
каталогом, а текущее мобильное приложение — отдельным клиентом того же story
runtime. Первый WebGL-релиз не воспроизводит и не доставляет аудио или видео.

Media-free — обратимое продуктовое ограничение, а не удаление исходников:

- аудио- и видеофайлы остаются в story projects и Git;
- новые платформенные releases не включают media payload;
- Player не создаёт audio/video runtime и не инициирует media requests;
- authored media-команды остаются допустимыми и исполняются как no-op;
- возвращение медиа требует нового capability/profile и пересборки контента,
  но не повторного производства или импорта сохранённых исходников.

## Границы системы

```text
Website
  catalog / SEO / auth / save API / analytics / launch configuration
      |
      | storyId, release, locale, short-lived session
      v
Web Story Player
  one story / Ink / dialogue / choices / static art / local save
      |
      v
WebGL story release
  manifest / Ink / prefabs / static art / hashes

Mobile application
  catalog + the same shared story runtime + platform adapters
```

В Unity Player не входят каталог, регистрация, платёжный интерфейс, APK updater,
push notifications, гардероб, аудио и видео. Сайт не исполняет Ink и не
дублирует игровой runtime.

## Media-free контракт

### Авторские проекты

Истории могут продолжать содержать `Assets/Audio`, `Assets/Video`,
`Config/CatalogVideos`, audio-команды Ink и video presentation metadata. Эти
данные считаются dormant source material.

Новые истории не обязаны производить аудио и видео до отдельного изменения
продуктового профиля. Проверка completeness не должна требовать dormant media.

### Content pipeline

Профиль `media-free-v1` должен:

1. Исключать audio/video из usage plan, chunks, loose media, release manifest и
   checksum tree.
2. Не считать исключённые исходники orphaned или ошибкой публикации.
3. Не включать story/episode catalog video в card payload.
4. Требовать статический фон для каждого video-only presentation. Отсутствие
   статического fallback является ошибкой сборки, а не runtime-сюрпризом.
5. Записывать в release capabilities явное отсутствие медиа:

   ```json
   {
     "profile": "media-free-v1",
     "capabilities": ["ink.v1", "static-art.v1", "choices.v1", "save.v1"]
   }
   ```

Старые опубликованные releases остаются immutable. Они не становятся
media-free автоматически и заменяются только новой версией при отдельной
публикации.

### Runtime

- `music`, `sound` и `ambient` являются успешными no-op и не пишут ложные
  playback errors.
- Очистка/остановка audio channel также является no-op.
- Background presentation всегда выбирает static image.
- Video alias и video URL не разрешаются и не загружаются.
- Player не создаёт `AudioSource`, `VideoPlayer` и `RenderTexture` для медиа.
- Отсутствие static fallback останавливает загрузку истории с понятным кодом
  content compatibility error.
- Формат старого save не меняется только из-за отключения медиа.

Предпочтительная реализация — общий immutable runtime profile, передаваемый при
composition, а не условные директивы WebGL внутри story logic. Начальное
значение `media-free-v1` одинаково для WebGL и мобильного приложения.

## WebGL content target

Каждый атомарный story project остаётся authoring/content project и получает
дополнительный target `webgl`. Он выпускает AssetBundles для общего Player, но
не самостоятельный `.wasm`.

```text
content/stories/<story-id>/<version>/webgl/
  release.json
  bundles/
  checksums.json
```

Требования совместимости:

- точная поддерживаемая Unity version фиксируется в Player manifest;
- story release сообщает минимальную версию Player и capability set;
- Player отвергает неизвестную обязательную capability до загрузки bundles;
- Android, iOS и WebGL releases собираются независимо из одних исходников;
- URL каждой версии immutable;
- активный manifest переключается атомарно и не изменяет старую версию.

## URL-контракт сайта

Рекомендуемая публичная структура:

```text
/apps/<app-slug>/
/apps/<app-slug>/stories/<story-slug>/
/apps/<app-slug>/stories/<story-slug>/play/

/webgl/player/<player-version>/...
/content/stories/<story-id>/<story-version>/webgl/...
/api/player/session
/api/saves/<story-id>
```

Страница истории индексируется и содержит нормальный HTML: заголовок,
аннотацию, обложку, автора, возрастной рейтинг и структурированные данные.
Маршрут `/play/` предназначен для приложения и может быть `noindex`. Unity
canvas не должен быть единственным индексируемым содержимым.

Player получает launch configuration после загрузки shell, а не доверяет
произвольному manifest URL из query string:

```json
{
  "storyId": "chernaya-melnitsa",
  "storyVersion": "5",
  "playerVersion": "1",
  "manifestUrl": "/content/stories/chernaya-melnitsa/5/webgl/release.json",
  "locale": "ru",
  "profile": "media-free-v1",
  "sessionToken": "short-lived-token",
  "returnUrl": "/apps/kostroma/stories/chernaya-melnitsa/"
}
```

Сайт разрешает только manifest URL собственного allowlisted content root.
Session token не сохраняется внутри Unity assets или URL.

## Фактическое состояние публичного хостинга

Read-only HTTP-аудит `https://pureshechka.com/` выполнен 2026-09-11. Сервер во
время проверки сообщал `nginx/1.31.3`.

| Возможность | Состояние | Вывод |
| --- | --- | --- |
| HTTPS | работает | подходит |
| HTTP/2 | работает | подходит |
| gzip HTML/JS | работает | оставить fallback |
| Brotli | не подтверждён: `Accept-Encoding: br` вернул несжатый JS | настроить до WebGL release |
| CORS | `Access-Control-Allow-Origin` отсутствует | не мешает same-origin; нужен для CDN/subdomain |
| OPTIONS | отвечает `200`, но без CORS allow headers | не является рабочим preflight |
| Byte ranges | работает, тест вернул `206` и корректный `Content-Range` | подходит |
| HTML/JS/JSON/APK MIME | корректны | WebGL MIME проверить отдельно |
| Cache-Control | явные правила отсутствуют | настроить по типу ресурса |
| Security headers | CSP, nosniff и связанные заголовки не обнаружены | добавить до публичного запуска |

На текущем сайте также установлен `noindex,nofollow`. Это правильно для
закрытого preview, но должно быть снято с публичных страниц каталога перед
этапом привлечения поисковой аудитории. Клиентский password splash не является
защитой содержимого и не используется для конфиденциальных материалов.

## Требования к серверу

### MIME и content encoding

Минимальная матрица:

| Файл | Content-Type | Дополнительно |
| --- | --- | --- |
| `.wasm` | `application/wasm` | `nosniff` |
| `.framework.js` | `text/javascript` | Brotli/gzip |
| `.data` и bundles | `application/octet-stream` | Brotli только после замера |
| `.json` | `application/json` | manifest cache policy |
| `.symbols.json` | `application/json` | не публиковать в production без необходимости |
| `.br` variant | тип исходного файла | `Content-Encoding: br` |
| `.gz` variant | тип исходного файла | `Content-Encoding: gzip` |

Нельзя отдавать `.wasm.br` как обычный download без правильных
`Content-Type`/`Content-Encoding`. Нельзя одновременно динамически сжимать уже
предсжатый файл. Если управляемый hosting не поддерживает `brotli_static`, MVP
может использовать gzip или Unity decompression fallback, но это считается
временным ухудшением скорости, а не готовой Brotli-конфигурацией.

### Cache policy

- версионированные Player-файлы и bundles:
  `Cache-Control: public, max-age=31536000, immutable`;
- release/checksum manifest по immutable version URL: допустим immutable;
- channel/active manifest: `Cache-Control: no-cache` с ETag;
- HTML запуска: `Cache-Control: no-cache`;
- save/session API: `Cache-Control: no-store`;
- все ответы сохраняют корректный ETag или Last-Modified.

Service Worker не входит в первый MVP. До его появления достаточно HTTP cache
и IndexedDB. Это снижает риск смешивания старого shell и нового content
manifest.

### CORS

При размещении сайта, Player и content на `https://pureshechka.com` CORS не
нужен для их взаимной загрузки. Same-origin является предпочтительным MVP.

При появлении CDN или отдельного asset-domain CORS включается только на
`/webgl/` и `/content/`:

- allow origin: точные production/staging origins, не отражённый произвольный;
- methods: `GET`, `HEAD`, `OPTIONS`;
- headers: только реально используемые request headers;
- expose: `Content-Length`, `Content-Range`, `ETag`;
- credentials выключены для публичных immutable assets;
- `Vary: Origin` при нескольких разрешённых origins.

Save API использует отдельную auth/CORS policy и не получает wildcard origin с
credentials.

### Security headers

До публичного MVP настроить:

- `X-Content-Type-Options: nosniff`;
- `Referrer-Policy: strict-origin-when-cross-origin`;
- ограниченный `Permissions-Policy`;
- HSTS только после подтверждения, что все требуемые поддомены работают по
  HTTPS;
- CSP, разрешающую только фактически необходимые `script-src`, `connect-src`,
  `img-src`, `font-src`, `worker-src` и `frame-src`.

Финальная CSP строится после появления WebGL template: Unity может потребовать
`blob:` для workers/resources. `unsafe-eval` не добавляется заранее; если
конкретная сборка требует его, сначала проверяется другой build/template mode.

Первый Player собирается без WebGL threads. Поэтому COOP/COEP и
cross-origin isolation не являются требованиями MVP. При последующем включении
threads понадобятся отдельный аудит `SharedArrayBuffer`,
`Cross-Origin-Opener-Policy` и `Cross-Origin-Embedder-Policy`, а также
совместимость всех внешних ресурсов.

## Требования к сайту вокруг Unity

### Запуск и восстановление

- responsive контейнер с корректной safe area и изменением ориентации;
- пользовательская кнопка запуска вместо автоматического старта тяжёлой
  загрузки;
- HTML progress/error UI работает до и независимо от Unity;
- retry не перезагружает всю страницу, когда можно повторить content request;
- выход возвращает на каноническую страницу истории;
- обновление страницы восстанавливает локальный checkpoint;
- ошибка несовместимой версии предлагает обновить Player, а не начать заново.

### Сейвы

- local-first storage через WebGL persistent storage/IndexedDB;
- явный flush после checkpoint и при уходе страницы, насколько позволяет
  браузер;
- облачная синхронизация только через HTTPS API сайта;
- MySQL недоступен непосредственно браузеру или Unity;
- short-lived session token передаётся после bootstrap;
- optimistic revision предотвращает молчаливое перетирание прогресса;
- API ограничивает размер payload, rate и пару user/story;
- save schema и content version сохраняются отдельно.

### Наблюдаемость

Минимальные события:

- `play_requested`;
- `player_download_started/completed/failed`;
- `story_download_started/completed/failed`;
- `story_started`;
- `episode_started/completed`;
- `choice_made` без текста чувствительного пользовательского ввода;
- `save_local_completed/failed`;
- `save_sync_completed/conflicted/failed`;
- `story_completed`;
- `player_error` с bounded code, player/story version и без token/save payload.

Сайт должен различать отказ загрузки Player, отказ story content, runtime error
и save sync error. Эти классы имеют разные продуктовые последствия.

### Доступность и SEO

- каталог и story pages работают без Unity;
- кнопка запуска доступна с клавиатуры и имеет видимый focus;
- canvas имеет доступное имя и соседний HTML exit/error control;
- page title, description, canonical URL, Open Graph и schema.org формируются
  из story metadata;
- `robots.txt` и page metadata согласованы;
- sitemap содержит приложения и страницы историй, но не `/play/`, immutable
  content и API routes;
- закрытый staging защищается server-side и остаётся `noindex`.

## Этапы реализации

### 1. Media-free contract

- добавить общий runtime/content profile;
- сделать audio-команды no-op;
- запретить video resolution/playback;
- исключить media из releases;
- добавить статическую проверку video-only backgrounds;
- покрыть профиль dependency-free/EditMode tests.

Результат: мобильное приложение ведёт себя как раньше, кроме отсутствия аудио и
видео; сохранённые исходники не входят в новый release.

### 2. WebGL content pipeline

- добавить `webgl` в CLI, target resolver и Library cache allowlist;
- выпускать WebGL bundles и manifest;
- добавить compatibility/capability validation;
- собрать одну короткую и одну тяжёлую историю;
- зафиксировать реальные размеры и peak memory.

### 3. Player vertical slice

- создать отдельный Unity project;
- вынести/подключить общий story runtime;
- реализовать browser content/save/navigation adapters;
- добавить JS bootstrap и события;
- проверить одну историю на desktop и реальных телефонах.

### 4. Hosting staging

- создать неизменяемый staging release;
- настроить MIME, compression и cache policy;
- проверить same-origin загрузку;
- применить security headers;
- выполнить негативные тесты: отсутствующий bundle, битый hash, старая версия,
  offline, прерванная загрузка и reload.

### 5. Site integration

- добавить индексируемую страницу истории;
- добавить `/play/` shell;
- подключить локальные и облачные сейвы;
- включить аналитику воронки;
- оставить мобильное приложение альтернативной кнопкой;
- снять preview `noindex` только с выбранных публичных страниц после
  продуктового решения.

## Приёмка хостинга

Перед первым WebGL staging release проверяются фактические публичные ответы:

1. HTML запуска возвращает `200`, `no-cache` и требуемые security headers.
2. `.wasm` возвращает `application/wasm`.
3. Brotli-клиент получает `Content-Encoding: br`; клиент без Brotli получает
   рабочий fallback.
4. Повторный запрос immutable asset получает cache hit или `304`.
5. Range request возвращает `206` и правильный `Content-Range`.
6. Same-origin Player загружает manifest и bundles без CORS ошибок.
7. Разрешённый staging origin проходит CORS, посторонний origin — нет, если
   cross-origin delivery включён.
8. Несуществующие `.wasm`/bundle URL возвращают настоящий `404`, а не HTML SPA.
9. Старый Player release продолжает открываться после публикации нового.
10. Корневая страница не переключается до полной проверки immutable payload.

## Решения, которые откладываются

- физическое удаление audio/video assets;
- WebGL threads и COOP/COEP;
- Service Worker/offline pre-cache;
- отдельный CDN/domain;
- возвращение аудио и видео;
- монетизация внутри Player;
- публикация или перепубликация существующих историй.

## Definition of done для media-free WebGL MVP

- одна короткая и одна тяжёлая существующая история проходят полностью;
- ни Player, ни story release не содержат аудио/видео payload;
- в Network panel отсутствуют media requests;
- первый интерактивный экран загружается в установленный после spike бюджет;
- reload восстанавливает checkpoint, offline не уничтожает сейв;
- desktop Chrome/Firefox/Safari и согласованная матрица реальных мобильных
  браузеров проходят smoke;
- серверные MIME, Brotli/fallback, cache, range и security checks подтверждены
  на публичном staging URL;
- мобильный клиент на том же shared runtime проходит regression scenario;
- опубликованные старые releases и исходные media assets сохранены.
