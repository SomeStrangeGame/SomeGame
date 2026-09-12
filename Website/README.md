# Кострома — сайт и браузерный плеер

Сайт хранится в основном репозитории SomeGame, каталог `Website/`.

- `app/` — React/Vinext-версия для разработки.
- `hosting/` — действующая статическая оболочка для pureshechka.com. При изменении
  взаимодействий поддерживать обе оболочки согласованными.
- `public/player/` — единый WebGL reader; доступные истории определяет config.json.
- Unity player/bundles не включаются в Git сайта: публикация переиспользует
  проверенные immutable URL. Контент не смешивается с версиями native-каталога.

Проверка: `npm ci`, `npm run build`, `npx tsc --noEmit --incremental false`.
Затем `node scripts/build-hosting.mjs <site-release> <reader-release>` создаёт
`dist/hosting/<site-release>/`. Запускать после Vinext build, который очищает dist.
Пример идентификаторов: `20260912-21` и `20260912-01`.

Публикация: загрузить новые каталоги site/releases и player/releases; проверить
хеши. Сохранить предыдущие index.html и player/config.json вне public_html.
Атомарно переключить player/config.json, player/index.html, затем корневой
index.html последним. Не менять существующие bundle/release URL или dev-channel.
Rollback переключает только сохранённые входные файлы, не удаляя старые релизы.

Сайт сохраняет текущую заставку закрытого просмотра и noindex. Заставка работает
на клиенте, не является защитой данных; reader доступен по прямой ссылке.
SSH-ключи, пароли сервера и локальные env в репозиторий не помещать.

Прежние метаданные отдельного Git сайта сохранены локально в
`.git/website-repository-backup-20260912` основного репозитория; не публикуются.
