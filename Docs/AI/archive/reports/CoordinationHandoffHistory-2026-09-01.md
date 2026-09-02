# Coordination handoff history — 2026-09-01

## 2026-09-01T17:02:18Z — android-test-apk-publish — complete

Task: опубликовать тестово подписанный Android Remote Release и свежий Android-only content.
Result: старые hosted `Novels-remote.apk`, `Novels-dev.apk` и `/dev` удалены; новый `/dev` содержит 253 файла без Win/iOS/Mac bundles. `Novels-test.apk` — 25,618,661 bytes, version `2026.09.01` (`3507417`), v2 signer `CN=SomeGame Test`, SHA-256 `75d6148d21714650587d916434ff7e5304223b3b5a0918e24bb93019b50cc102`.
Validation: Android content gate и Remote Player build passed; remote size/checksum совпадают; HTTPS APK и catalog HEAD возвращают 200 с ожидаемыми lengths.
Published: `https://pureshechka.com/DevBuilds/Novels-test.apk`, content root `https://pureshechka.com/dev`.

## 2026-09-01T15:37:30Z — fifo-chat-heartbeat-wait — completed

Task: заменить блокирующее ожидание FIFO на минутный heartbeat текущего чата.
Changed: `UnityConcurrency.md` требует фактический current thread id, проверку сохранённой привязки, отсутствие дублей и stop/pause при завершении или ожидании решения.
Validation: scoped `git diff --check` и `Tools/somegame docs-check` passed.
Pending / risks: none; bounded polling сохранён только как fallback без heartbeat.
