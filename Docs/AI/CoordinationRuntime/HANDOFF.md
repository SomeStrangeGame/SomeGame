# Current cross-chat handoff

Только актуальное незавершённое состояние. Предыдущий полный snapshot сохранён
в Git на commit `b910242f`; завершённые результаты сведены в
[`CoordinationHandoffHistory-through-2026-08-30.md`](../archive/reports/CoordinationHandoffHistory-through-2026-08-30.md).

## Ready for integration or validation

### Android content and runtime

- `android-astc8`: postprocessor интегрирован; требуется актуальное измерение
  bundle size только при следующей Android content сборке.
- `android-embedded-emulator` + `runtime-smoke-telemetry`: исходники в `main`;
  следующий Embedded APK smoke должен проверить полный `[NOVELS_SMOKE]` flow и
  считать character `fallback.used` блокирующим.

### UI and character runtime

- `catalog-mockup-parity`: код в `main`; нужен один bounded Play Mode visual gate.
- `wardrobe-interaction-fix`: код в `main`; нужен visual gate 1080×1920.
- `character-whole-variants`: production art contract принят; runtime adapter
  одного полного sprite address ещё не реализован.

## Blocked / limitations

- `gpl-clothes-only-story`: Ink/source map в `main`, но проверка зависит от трёх
  утверждённых цельных clothing sprites.
- `gpl-character-art`: цельные Lea/Mark/Vera assets отсутствуют.
- `catalog-playmode-review`: paused до явного продолжения ручного visual review.
- WebGL prototype остаётся только в `prototype/webgl-local-platform`, commit
  `cfb92896`; compilation и browser smoke не выполнены.

## Runtime rules

- Перед работой использовать `Tools/somegame context --task <type>` и проверить
  полный Git/FIFO; архив читать только для конкретной регрессии.
- Затем выполнить `Tools/somegame verify --explain`; тяжёлые gates запускать
  только по плану и под точным write-lock.
- Успешные результаты передавать компактно; полные логи читать при failure.
