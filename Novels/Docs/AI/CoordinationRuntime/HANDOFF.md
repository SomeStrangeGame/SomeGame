# Cross-chat handoff log

Перед работой прочитайте этот журнал полностью и проверьте относящиеся к задаче
утверждения по текущим файлам и `git status`. Перед освобождением write-lock
добавьте новую запись в конец файла по формату из
`../ParallelRefactoringCoordination.md`.

## 2026-08-24 — coordination-runtime — completed

Task: Дополнить существующую координацию атомарной FIFO-очередью, handoff и
правилами безопасной последовательной работы.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлен операционный
  протокол.
- `Novels/Docs/AI/CoordinationRuntime/`: создано стартовое пространство
  очереди.

Validation:
- Ручная сверка с переносимым протоколом `skazbuka`: ключевые правила
  перенесены без замены проектной интеграционной очереди.

Pending / risks:
- Механизм остаётся кооперативным: чат должен открыть репозиторий от его корня,
  чтобы получить инструкции из `AGENTS.md`.

Suggested next step:
- Использовать runtime-очередь при следующей изменяющей или тяжёлой задаче.

## 2026-08-24T10:11:08Z — catalog-coordination-rules — completed

Task: Формализовать безопасное расширение области владения и межпроектные
атомарные scope.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлены правила
  временной передачи владения, отдельного межпроектного scope, приоритета
  активного владельца и атомарных блоков внутри расширенной задачи.

Validation:
- `git diff --no-index --check /dev/null
  Novels/Docs/AI/ParallelRefactoringCoordination.md`: whitespace-ошибок нет.
- Ручная сверка с существующей runtime FIFO-очередью: новый раздел использует
  `write-lock` как единственное разрешение записи и не создаёт второй механизм
  блокировки.

Pending / risks:
- Механизм кооперативный: временный scope должен быть объявлен до изменения
  shared-файлов.

Suggested next step:
- Для переноса Catalog size audit создать отдельный
  `ParallelWork.bundle-audit.md` с точными shared-файлами.

## 2026-08-24T10:25:30Z — bundle-audit — ready-for-integration

Task: Перенести контроль размера и состава Catalog bundle из локального
Editor-кода в общий Content SDK.

Changed:
- `Packages/NovelsContentSdk/Editor/ContentBundleAudit.cs`: общий аудит root
  assets, фактического размера и Catalog dependencies/budget.
- `Packages/NovelsContentSdk/Editor/ContentPipeline.cs`: аудит подключён после
  создания bundle и до записи `release.json`.
- `Projects/novels-catalog/Assets/Editor/**`: локальный audit удалён вместе с
  папкой и meta.
- `Projects/novels-catalog/README.md`: отдельное Unity-меню заменено описанием
  автоматической проверки.
- `Novels/Docs/AI/ParallelWork.bundle-audit.md`: записан scope и результат.

Validation:
- `novels-content doctor`: успешно.
- `novels-content validate catalog`: успешно.
- `novels-content build catalog editor`: успешно.
- Unity log: `Content bundle audit passed`; размер 6606 байт (6,5 КиБ).
- Catalog не содержит локальных C#-файлов.
- `git diff --check`: успешно.

Pending / risks:
- Для Story bundle audit пока только проверяет root assets, файл и размер без
  отдельного size budget; поведение сборки Story не менялось.

Suggested next step:
- Интеграционному координатору принять единым блоком общий audit и удаление
  локального Catalog audit.

## 2026-08-24T11:09:42Z — novels-simplification — completed

Task: Последовательно упростить Game runtime, Content SDK, UI, адресацию и
валидацию Novels, принять готовый bundle audit и пересобрать Editor-контент.

Changed:
- `Novels/Assets/Novels/**`: линейный application flow, предметный lifetime
  каталога, отдельный `ReplayValidator`, единый dialogue frame.
- `Packages/NovelsContentSdk/Runtime/**`: общие операции адресации и поиска
  слоёв; удалены три пустые contract assemblies; Choose и Wardrobe используют
  общий lifecycle через композицию, оставаясь самостоятельными фичами.
- `Packages/NovelsContentSdk/Editor/**`: неизменяемый результат инспекции
  проекта; принят общий `ContentBundleAudit` из предыдущей задачи.
- `Novels/Docs/AI/**`: актуализированы обзор, короткий план и история волны.

Validation:
- Unity 6000.3.11f1 batch compile Game runtime и Editor assemblies: успешно,
  C#-ошибок нет.
- `novels-content doctor`: успешно.
- `novels-content validate all`: Catalog, TZM и ZDM успешно.
- `novels-content build all editor`: application, TZM и ZDM успешно; свежая
  локальная композиция находится в `Novels/Build/LocalContent`.
- `git diff --check`: успешно; ссылок на удалённые contract assemblies нет.

Pending / risks:
- Play Mode, визуальные размеры и полный игровой маршрут автоматически не
  проверялись; пользователь выполнит ручной smoke test в Editor.
- Тесты намеренно не добавлялись и не запускались согласно правилам проекта.
- Editor-сборка не заменяет Android/iOS player build.

Suggested next step:
- Открыть Novels, пройти каталог → выбор истории → эпизод для TZM и ZDM и
  проверить Console; при успехе зафиксировать изменения отдельным коммитом.

## 2026-08-24T12:21:13Z — novels-simplification-2 — completed

Task: Выполнить шесть согласованных упрощений runtime, OptionList и Content SDK
validation, затем уточнить самостоятельное ожидание FIFO.

Changed:
- `NovelRuntime*`: удалены промежуточные bootstrap/session-типы, маршрут виден
  непосредственно в `Init` и preparation.
- `StoryExecution/**`, `StoryQueue/**`: простые операции переведены на
  `DelegateStoryOperation`; stateful выбор сохранён отдельными типами.
- `ContentProjectValidation.cs`: линейный validation без rule-интерфейса.
- `OptionSelection/**`: статическая разметка хранится в системном prefab,
  динамически создаются только карточки.
- Choose, Wardrobe и Catalog принимают малые зависимости напрямую.
- `ParallelRefactoringCoordination.md`: FIFO ожидается автоматически до
  таймаута; занятая очередь сама по себе не завершает ход.

Validation:
- Unity 6000.3.11f1 batch compile: успешно, C#-ошибок нет.
- `novels-content validate all`: Catalog, TZM и ZDM успешно.
- `git diff --check`: успешно.
- C# объём проверяемой области: 9628 → 9305 строк.

Pending / risks:
- Нужен ручной Play Mode smoke test Choose и Wardrobe, включая повторное
  открытие, прокрутку, preview и confirm.
- Android/iOS player build не запускался; форматы Ink/save/release не менялись.
- Тесты не добавлялись и не запускались согласно правилам проекта.

Suggested next step:
- Выполнить ручной UI smoke test, затем интегрировать
  `ParallelWork.simplification-wave-2.md`.

## 2026-08-24T11:12:00Z — story-global-content — completed block 0

Task: Разрешить ограниченное продолжительное ожидание FIFO по явному запросу
пользователя.

Changed:
- `Novels/Docs/AI/ParallelRefactoringCoordination.md`: добавлен режим ожидания
  без write-lock с интервалом не менее 60 секунд, таймаутом 10 минут и не более
  чем 10 проверками.

Validation:
- `git diff --check` для изменённых coordination-файлов: успешно.
- Ручная сверка с FIFO/write-lock: ожидание не даёт права записи и не позволяет
  удерживать lock.

Pending / risks:
- Нет; режим включается только явным запросом пользователя или координатора.

Suggested next step:
- Освободить lock атомарного блока 0 и продолжить блок 1
  `story-global-content` через обычную FIFO.

## 2026-08-24T11:20:00Z — story-global-content — completed blocks 1-2

Task: Ввести единый story-global адресный контракт и мигрировать ZDM.

Changed:
- `Packages/NovelsContentSdk/Runtime/ContentAddressing/**`: Unity-assets теперь
  адресуются только через `content/<story>/story/**`.
- Character/runtime loaders: удалён episode/shared fallback.
- `Projects/novels-zdm/Assets/RemoteAssets/content/zdm/**`: `shared` перенесён
  в `story`, локации дедуплицированы и собраны в story-global каталог.

Validation:
- `git diff --check`: успешно.
- `Tools/novels-tools/novels-content validate zdm`: успешно.
- `Tools/novels-tools/novels-content build zdm editor`: успешно.

Pending / risks:
- 12 исключённых ZDM PNG (64 409 963 байта) временно сохранены в
  `/tmp/novels-zdm-story-global-20260824T1115Z`.
- TZM ещё не мигрирован.

Suggested next step:
- Отдельным lock-блоком мигрировать и собрать TZM.

## 2026-08-24T11:40:00Z — story-global-content — ready-for-integration

Task: Завершить story-global миграцию двух историй и первую итерацию сжатия.

Changed:
- `Projects/novels-tzm/**` и `Projects/novels-zdm/**`: Unity-assets собраны в
  `content/<story>/story/**`; episode/shared каталоги удалены.
- `NovelContentTextureImporter.cs`: Android/iOS ASTC 6×6, Max Size 4096.
- Документация authoring/size: зафиксированы новый контракт и измерения.

Validation:
- `validate zdm`, `build zdm editor`, `validate tzm`, `build tzm editor`:
  успешно.
- Android build ZDM: 111 465 312 B, экономия 64,3%.
- Android build TZM: 304 933 451 B, экономия 37,6%.
- Суммарный Android bundle: 416 398 763 B вместо 800 659 912 B, экономия
  384 261 149 B (48,0%).
- `git diff --check`: успешно.

Pending / risks:
- Нужен ручной визуальный quality gate ASTC на устройствах.
- iOS override задан, но iOS build в этой итерации не запускался.
- Обратимые копии исключённых дубликатов временно находятся в
  `/tmp/novels-zdm-story-global-20260824T1115Z` и
  `/tmp/novels-tzm-story-global-20260824T1121Z`.

Suggested next step:
- Выполнить визуальный smoke test и затем iOS size build; видео оптимизировать
  отдельным блоком, не смешивая с Unity bundle.

## 2026-08-24T11:53:00Z — texture-postprocessor-rename — completed

Task: Сделать назначение Unity texture postprocessor очевидным новичкам.

Changed:
- `Packages/NovelsContentSdk/Editor/NovelContentTexturePostprocessor.cs`:
  прежний AssetPostprocessor переименован без изменения поведения.
- `.meta` переименован вместе с C#-файлом, GUID сохранён.
- Size-документация использует новое имя.

Validation:
- `Tools/novels-tools/novels-content validate zdm`: успешно.
- `git diff --check`: успешно.
- Ссылок на старое имя в актуальном коде и документации нет; исторический
  handoff оставлен неизменным.

Pending / risks:
- Нет.

Suggested next step:
- Использовать `NovelContentTexturePostprocessor` как единственную общую точку
  автоматических import settings нового story art.

## 2026-08-24T13:00:00Z — tzm-video-crf18 — ready-for-integration

Task: Удалить звук и сжать все TZM-видео H.264 CRF 18.

Changed:
- 51 MP4 в `Projects/novels-tzm/Assets/StreamingAssets/novelsvideos/tzm/`:
  перекодированы `libx264 -preset slow -crf 18`, audio streams удалены.
- Size-документация и `ParallelWork.tzm-video-crf18.md`: добавлены измерения.
- TZM Android, Mac и iOS releases пересобраны.

Validation:
- 51/51: H.264/YUV420p, исходные resolution/FPS, duration delta ≤ 0,04 с.
- Audio streams: 0.
- Видео: 346 350 427 B → 287 268 760 B, −59 081 667 B (−17,06%).
- `validate tzm` и `build tzm android|editor|ios`: успешно.
- `git diff --check`: успешно.

Pending / risks:
- Требуется ручной visual/loop quality gate.
- iOS Unity bundle вырос до 305 236 981 B против baseline 212 013 875 B;
  это отдельная регрессия texture profile, не video payload.
- Оригиналы сохранены в `/tmp/novels-tzm-video-originals-20260824T1236Z`.

Suggested next step:
- Визуально принять видео, затем отдельным атомарным блоком исправить iOS
  texture profile и повторно измерить bundle.

## 2026-08-24T13:12:00Z — ios-texture-profile — ready-for-integration

Task: Устранить регрессию iOS story bundle после общего ASTC 6×6.

Changed:
- `NovelContentTexturePostprocessor.cs`: Android оставлен ASTC 6×6, iOS
  переведён на ASTC 8×8; importer version увеличена до 4.
- Size/status документация актуализирована.

Validation:
- `build zdm ios`: успешно, bundle 66 039 336 B вместо baseline 116 864 846 B.
- `build tzm ios`: успешно, bundle 181 345 930 B вместо baseline 212 013 875 B.
- Временная регрессия TZM до 305 236 981 B устранена.
- `git diff --check`: успешно.

Pending / risks:
- Нужен ручной iOS visual quality gate ASTC 8×8.

Suggested next step:
- Проверить лица, волосы, UI и градиенты на iOS-устройстве.

## 2026-08-24T11:43:46Z — continuous-wait-policy — completed

Task: Формализовать непрерывное ожидание и автоматическое возобновление.

Changed:
- `ParallelRefactoringCoordination.md`: для явного требования «не
  останавливаться» добавлены повторяемые ограниченные периоды ожидания без
  удержания `write-lock`.
- После освобождения ресурса поток обязан сам перечитать handoff, FIFO и
  состояние репозитория, затем продолжить исходную задачу без нового сообщения
  пользователя.
- Занятая очередь и долгая сборка явно не считаются самостоятельным блокером.

Validation:
- `git diff --no-index --check`: успешно.
- Правило сохраняет интервал не менее 60 секунд и не более 10 проверок за один
  период ожидания.

Pending / risks:
- Нет.

Suggested next step:
- Применять непрерывное ожидание только при явном терминальном требовании
  пользователя или координатора.

## 2026-08-24T12:30:57Z — catalog-simplification — ready-for-integration

Task: Упростить Catalog, кроме prefab.

Changed:
- `CatalogContracts.cs`, `ContentProjectValidation.cs`, `CatalogFlow.cs` и
  `Config/catalog.json`: registry schema 2 хранит упорядоченный массив строк;
  `order`, `enabled` и `CatalogRegistryEntry` удалены.
- `Projects/novels-catalog/Packages/**`: удалена только неиспользуемая прямая
  зависимость `com.unity.2d.sprite`.
- `Projects/novels-catalog/README.md`: добавлены отдельные сценарии изменения
  списка историй и внешнего вида.
- `ParallelRefactoringCoordination.md`: зафиксирован schema-2 контракт.

Validation:
- `novels-content validate catalog`: успешно.
- Unity 6000.3.11f1 batch compile `Novels`: успешно.
- `novels-content build catalog editor`: успешно; bundle audit пройден.
- Отрицательная проверка подтвердила обязательность JSON module; зависимость
  восстановлена до финальной успешной сборки.
- Prefab не изменялся; scoped `git diff --check` успешен.

Pending / risks:
- Schema 2 несовместима со старым registry reader; registry и обновлённый
  клиент должны интегрироваться и публиковаться вместе.
- Play Mode не запускался, поскольку визуальное поведение не менялось.

Suggested next step:
- Интегрировать SDK, Game и Catalog одним контрактным блоком, затем выполнить
  обычный ручной маршрут открытия каталога.
