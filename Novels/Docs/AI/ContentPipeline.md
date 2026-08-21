# Content Pipeline

## Быстрая работа в Editor

- Валидация: `Novels > Content > Validate`.
- Полная сборка AssetBundle: `Novels > Content > Build All Bundles`.
- Полная очистка runtime-кеша: `Novels > Content > Clear Cache`.
- Пересоздание локального UI: команды в `Novels > UI`.

## Терминал

Из корня `Novels`:

```bash
Tools/validate-novels.sh validate
```

Проверяет authoring и существующий built output.

```bash
Tools/validate-novels.sh content
```

Собирает и валидирует контент по активному `NovelContentBuildProfile`.

```bash
Tools/release-novel-content.sh --local-only
```

Создаёт локальный Android/iOS release и готовый к публикации каталог `Build/NovelContent/ServerRoot`, ничего не загружая на сервер.

## Результат публикации

`ServerRoot` содержит:

```text
ServerRoot/
  deployment.json
  Files/<sha256>
  Remote/Android/release.json
  Remote/Android/<bundles>
  Remote/iOS/release.json
  Remote/iOS/<bundles>
```

На сервер переносится содержимое `ServerRoot` без переименования каталогов.

## Player

Embedded Android APK для проверки без сервера:

```bash
Tools/build-embedded-test-player.sh
```

Remote Player:

```bash
Tools/build-remote-player.sh Android https://example.test/content Build/Players/Novels.apk
Tools/build-remote-player.sh iOS https://example.test/content Build/Players/iOS
```

Editor всегда читает `StreamingAssets`. Обычный Player требует HTTP(S) root. Embedded test Player создаётся отдельным скриптом и не меняет рабочий проект.

## Порядок проверки

```text
индексация проекта
  -> анализ Ink и INCLUDE
  -> проверка ссылок на контент
  -> сборка bundles во staging
  -> построение release/deployment manifests
  -> проверка размеров и SHA-256
  -> атомарная публикация результата
```

Warnings собираются в одно сгруппированное сообщение и не останавливают сборку. Errors также группируются, после чего сборка прекращается.
