# Refactoring History

Архив прежних архитектурных волн, решений и результатов проверок. Документ сохранён для контекста, но не описывает проект целиком в его текущем состоянии. Актуальная точка входа: [UnityProjectContext.md](UnityProjectContext.md).

<!-- unity-onboarding:generated:start -->

## Simplification wave completed on 2026-08-24

- `ApplicationRuntime` теперь выражает линейный цикл каталог → история без
  искусственной state machine; загруженный каталог оформлен как явный lifetime.
- Проверка replay вынесена в `ReplayValidator`, а построитель диалога вычисляет
  единый кадр представления до формирования очередей Bubble и Character.
- Повторяющиеся алгоритмы поиска слоёв персонажа и построения адресов сведены к
  общим локальным операциям; результат инспекции контентного проекта стал
  неизменяемым.
- Удалены три пустые contract assemblies Bubble/Choose/Wardrobe. Их контракты
  живут в соответствующих feature assemblies, при этом Choose и Wardrobe
  остаются независимыми фичами.
- Общий `OptionListController` устранил дублирование lifecycle Choose/Wardrobe,
  а `OptionListScreen` хранит одну коллекцию карточек вместо параллельных
  списков. Размеры и оформление UI не менялись.
- Unity runtime и Editor assemblies скомпилированы; `doctor`, валидация Catalog,
  TZM и ZDM прошли; Editor bundles всех трёх атомарных проектов пересобраны.
  Ручная проверка игрового маршрута оставлена владельцу проекта.

## Simplification wave completed on 2026-08-21

- The redundant `Novels.Waiting` feature, factory, and assembly were removed. StoryQueue now receives the episode token and performs the same scaled-time wait directly.
- `Novels.UITransitions` moved to the reusable `somegame.ui-transitions` package with its assembly GUID preserved.
- Ink reading, source mapping, typed story contracts, and command parsing moved together to the independent `somegame.novel-ink` package. Existing assembly GUIDs, namespaces, and runtime APIs remain unchanged.
- Unused `somegame.localization` and `somegame.sodata` dependencies were removed from this project; their source packages remain available to other projects.
- Character address forwarding was removed. Runtime now uses the same `ContentAddresses`/`ContentAddressConvention` boundary as Editor validation, and repeated resolved sprite probes no longer reload the same asset.
- Dialogue composition uses a named tuple instead of a one-use result DTO and one shared role-to-layout rule. Camera syntax is a data table instead of a long conditional chain.
- Validation reports no longer pretend to be mutable string collections. Validators receive the concrete typed report directly.
- SHA-256 calculation is owned by `Bundles.ContentHash`, and reusable deployment-manifest construction lives in the editor-only `Bundles.Editor` assembly.
- Generated-solution runtime and Editor compilation completed with 0 errors and 0 warnings. Tests and AssetBundle builds were not run.

## Architecture wave completed on 2026-08-19

- Ink audio commands contain a bare file name without an extension. Editor validation and delivery indexing require exactly one matching `.wav`, `.mp3`, or `.ogg` file; runtime resolves the physical format from the pinned release and rejects missing or ambiguous matches.
- `LocalePolicy` owns the `en` fallback and the supported `en`/`ru` locale set. Catalog and story authoring require complete values for every supported locale. Application-level bootstrap/catalog strings live in the built-in `ApplicationLocalizationData.asset`, so they remain available before remote content loads.
- Editor dependency discovery builds a recursive Ink source graph with `INCLUDE` cycle/missing-file diagnostics. Audio, background, speaker, and camera references retain their source file, line, and authored text through validation.
- Editor content snapshots use immutable `ContentBuildFile` values and create mutable release DTOs only at the JSON boundary. Numeric client-version parsing is centralized in `Bundles.ClientVersion`.
- Save format version 2 persists typed `StoryDecision` records (`Advance` or an integer choice ID). The byte sentinel and the 0-254 choice-ID limit were removed. Version-1 saves are intentionally rejected; no migration is provided.
- Unity 6000.3.11f1 and the affected generated C# projects compiled with 0 errors and 0 warnings. Authoring validation completed without errors; complete built-output validation still requires rebuilding the existing schema-4 Android release and producing the missing iOS release.

## Project Summary

- Root: `/Users/iantonishin/Fork/SomeGame/Novels`
- Unity: 6000.3.11f1 (`3000ef702840`)
- Product: single-scene visual-novel player driven by compiled Ink and AssetBundles.
- Last analyzed: 2026-08-17; baseline commit `2002ab14f2c42a180eb1b1ae306f46003285f7a0`.

## Confirmed Environment

- Render pipeline: unresolved. Graphics Settings reference an SRP asset and `URPProjectSettings.asset` exists, but no URP package or matching tracked pipeline asset was found.
- Input: legacy Input Manager (`activeInputHandler: 0`).
- Content targets: Android and iOS are built from the same checked-in profile into one deployable `ServerRoot`.
- Important dependencies: Ink, UniTask, uGUI, plus local `somegame.*` packages for bundles/cache, disposal, loading, localization, settings, logs, and ScriptableObject data.

## Structure And Assemblies

- `Assets/Novels`: runtime composition, feature/domain assemblies, views, and the only scene.
- `../../Packages/NovelInk`: typed story contracts, command parser, Ink reader, and source-map support. The package preserves the established `Novels.Story*` namespaces.
- `Assets/RemoteAssets`: bundled authoring content. Every technical address below this root is canonical lowercase. Application-owned assets are grouped under `content/{contentId}/application`, episode-owned assets under `content/{contentId}/episodes/{episodeId}`, definitions under `content/{contentId}/definition`, while `catalog` and `loading` remain top-level shared bundles.
- `Assets/StreamingAssets`: Ink JSON, audio/video, and built remote bundle payloads.
- `Assets/Editor/CreateAssetBundles.cs`: manual Android AssetBundle builder/cache clearer.
- `../../Packages/*`: shared local packages; moving `Novels` alone breaks the relative `file:` dependencies.
- `Novels`: broad composition-root assembly referencing feature assemblies and shared packages.
- `Novels.Content`: immutable `NovelDefinition` and `EpisodeDefinition` configuration independent of scene serialization.
- `Novels.Diagnostics`: neutral error code, severity, source, and exception contracts.
- `Novels.Editor`: editor-only validation of loaded novel configuration and Android bundle output.
- Feature assemblies: package-owned `StoryContracts`, `StoryCommands`, and `StoryProcessor`, plus project-owned `StoryQueue`, `QueueProcess`, `Bubble`, `Character`, `Location`, `Notification`, `Audio`, and `Save`, with separate View assemblies where applicable.

## Scenes And Startup

- Only enabled build scene: `Assets/Novels/Novels.unity`.
- Scene `EntryPoint.OnEnable()` initializes UniTask's player loop, caps FPS at 30, creates the concrete `IContentSource`, captures `Application.persistentDataPath` on the main thread, constructs `ApplicationRuntime`, and starts an exception-observing session wrapper.
- `ApplicationRuntime` owns the shared bundle service, local bootstrap/retry UI, remote catalog, story selection, and the currently active novel runtime. `Entity.Init()` owns one selected story and creates `NovelBootstrapProcess`, which coordinates application preparation, New Game/Continue selection, episode preparation, and episode execution through delegates.
- The scene contains no concrete story ID. Startup loads `novelcatalog.asset` and its selection screen from `novels_catalog`; `Catalog.Entity/View` presents the active `tzm` and `zdm` entries. Bundle and definition addresses are derived from the selected content ID. The loaded definition then presents an explicit episode selection; each episode supplies one episode-lifetime bundle.
- `StoryCommandParser` converts legacy colon-delimited Ink lines into typed commands; `NovelProcess` maps them to queued actions for location/cut-scene, audio, camera, waits, notifications, character presentation, dialogue, and choices.

## Architecture And Conventions

- Application, selected-novel, and episode lifetimes are distinct. `EpisodeRuntime` owns a linked token and cancels it before disposing `EpisodeScope`; all episode UI, waits, media, queue work, and asset awaits receive that token.
- Story execution returns `EpisodeRunResult` with `Completed`, `Failed`, or `Cancelled` status. `ApplicationRuntime` decides whether to stop or return to the catalog and is the reporting boundary for fatal execution results.
- Every external Ink/audio/video resolution requires the active `ContentReleaseSnapshot`; no release-less or `legacy` content-file path remains.
- `ContentDeliveryCoordinator` can prepare one release delivery group with file/byte progress, cancellation, disk-space validation, integrity verification, and cache pinning. A pinned group raises the effective LRU floor above the default 512 MiB cache limit when necessary.
- Release schema 5 carries `Embedded`, `Hybrid`, or `Remote` delivery mode and assigns both AssetBundles and external files to delivery groups. External files retain stable logical addresses while their immutable server payloads use `Files/{sha256}` paths. The build profile controls mode, embedded groups, publish/player-seed roots, and optional hard budgets; the current checked-in profile uses Remote delivery.
- `Novels.ContentAddressing` owns `ContentPackageConvention`, `ContentAddressConvention`, and immutable episode-scoped `ContentAddresses`. Bundle names, definition paths, delivery-group IDs, runtime loading, character resolution, and Editor validation share these dependency-free conventions; the former `PathGetter` assembly has been removed.
- `EntryPoint` captures immutable `ApplicationEnvironment` values and owns the explicit scene Camera reference. Runtime composition receives locale, client version, platform, persistent path, and camera without ambient lookups.
- `NovelCiValidation.ValidateExistingContentBatch` and `BuildAndValidateContentBatch` provide non-test batch automation for content validation and complete artifact production.
- `ContentBuildTransaction` makes bundle output, validation, publish artifacts, and optional player seeds one rollback boundary. Runtime releases use candidate/active promotion: a remote manifest becomes active only after application content and catalog assets load successfully.
- Remote request behavior is defined by `ContentRequestPolicy`; HTTP requests have bounded timeout/retry/backoff and typed failure categories, while Editor StreamingAssets access remains single-attempt local delivery.
- Ink source is the only source of episode audio, background, and speaker dependencies. `StoryDependencyAnalyzer` parses authored command lines, resolves declared string variables, derives matching video files during content builds, and rejects empty or statically unresolved resource references. It no longer scrapes compiler-private Ink JSON strings.
- `Bundles.MediaFileConvention` owns only the fixed `.mp4` video extension. Ink audio references are extensionless names; Editor validation and runtime release lookup require one unambiguous matching audio payload.
- Player schema support is centralized in `ContentCompatibility`. Deployment URLs are generated into a staging-only `ContentRuntimeConfiguration` Resources asset instead of being serialized into the startup scene.
- Shared cancellation-aware view animations live in the `somegame.ui-transitions` package. Location background playback/cut-scene behavior lives in `BackgroundPresentationController`, while image/camera/dialogue geometry lives in `LocationLayout`; existing UI dimensions and serialized timing/layout values are unchanged.

- Content invariants are enforced at model boundaries: episode IDs, media IDs, and normalized locale codes must be unique case-insensitively.
- Bundle assets are addressed by immutable `BundleAssetAddress` values. Required loads fail explicitly, while optional sprite probes use a separate API.
- `CharacterSpriteResolver` owns appearance state and character sprite-address resolution; `Character.Entity` is limited to presentation orchestration.
- `BundlePayloadLoader` owns release-based payload acquisition and integrity, while `BundleStore` owns bundle records, leases, and asset caching. Runtime delivery uses the platform `release.json` exclusively; legacy per-bundle pointer/manifest files are no longer produced or consumed.
- Serialized `ContentReleaseDto` objects exist only at the JSON boundary. Runtime code receives an immutable `ContentReleaseSnapshot`, and `ContentReleaseFingerprint` calculates its canonical ID from deterministically sorted metadata.
- Editor validation is composed from `PrefabContentValidator`, `BuiltReleaseValidator`, and `StoryReferenceValidator`. In addition to authoring structure and release integrity, it scans Ink for statically resolvable background, character, and audio references.
- Root composition is split between `Entity.NovelPreparation` for immutable prepared resources and preloading, and `Entity.EpisodeComposition` for feature construction and episode execution.
- Locale fallback is deterministic but never ambient: every catalog/content resolution call supplies the session locale explicitly.
- `NovelContentBuildProfile` defines build targets, compatibility metadata, output root, and reporting budgets. Builds return platform-neutral `ContentBuildResult` values used by validation and publish-artifact creation.

- Confirmed manual composition root: a partial `Novels.Entity` wires feature entities using nested `Ctx` structs and delegates; no DI container.
- Confirmed Entity/View split: disposable plain-C# feature entities drive uGUI `Screen` MonoBehaviours.
- Confirmed command queue with async and immediate/replay modes; replay input is persisted as typed `StoryDecision` records.
- Story syntax parsing is isolated in `Novels.StoryCommands`; each command exposes only its specialized payload (`Dialogue`, `Background`, `Audio`, `Notification`, `Camera`, or `Wait`). Dialogue presentation, choice actions, character control arguments, background options, and camera actions are converted from authored strings to typed `StoryContracts` values at this boundary. Free-form character asset candidates remain strings because their resource category is resolved during sprite lookup. `StoryProcessor` converts Ink choices to the neutral `StoryChoice` contract. `StoryQueue` accumulates commands and builds executable queue batches, while `NovelProcess` only receives steps and executes ready batches through delegates.
- A dialogue may have empty speaker/text when Ink exposes choices without a prompt; character presentation is updated only when the dialogue contains speaker or text content.
- `StoryQueue.StoryCommandQueueBuilder` maps each non-dialogue story command to one executable queue item. `StoryQueue.DialogueQueueBuilder` owns the last-character state and returns explicit queue items that belong before and after the accumulated commands. It resolves the configured main-character name and typed dialogue presentation into a neutral `StorySpeakerRole` and `StoryCharacterPosition`; `StoryQueue.Entity` is the sole owner of command accumulation and final batch composition. Character, Bubble, and QueueProcess consume the resolved role instead of comparing authored speaker strings.
- Dialogue background alignment remains a neutral `StoryDialogueAlignment` through StoryQueue and QueueProcess; `Location.Entity` owns the conversion to Unity's `TextAlignment` used by its View.
- `QueueProcess.Executor` owns sequential execution and draining of completed queue batches. It converts the optional saved choice into a typed `QueueExecutionContext`; every `IQueue` has one `Run(context)` entry point and selects live or replay behavior through `QueueExecutionMode`. The executor is created through the root `Entity.QueueProcess` partial factory; `NovelProcess` receives only its execution delegate and remains responsible for story progression. `Save.Entity` owns the immutable initial-choice snapshot and its replay cursor, exposing the next saved choice through a delegate without leaking its collection.
- `QueueExecutionContext` carries the episode-lifetime cancellation token. Queue commands are immutable after construction, validate required delegates in their constructors, and cancel user-input waits without leaving the executor suspended.
- Notifications preserve their non-blocking story behavior through a Notification-owned FIFO dispatcher. The dispatcher serializes presentation, observes cancellation and exceptions, and replaces queue-level fire-and-forget work.
- `Save.Entity` receives byte-storage operations through delegates and no longer references the Cache assembly. The root `Entity.SaveSystem` factory owns the Cache adapter. Cache owns filesystem path resolution, atomic byte writes, existence checks, and exact-key deletion; filesystem paths are converted to `file://` URLs only at the Bundles video boundary.
- Save storage writes and accepts only a versioned binary envelope containing content identity, content version, and typed decision records. Saves in obsolete formats or belonging to another episode/version are not replayed.
- `StoryProcessor.ReadNext()` exposes typed `Content`, `Choices`, and `Completed` control flow. `NovelProcess` handles completion before parsing and terminates without adding an artificial queue item.
- Story completion asks `StoryQueue.TryComplete()` for a final batch so commands authored after the last Dialogue are not discarded. No empty batch or artificial command is emitted.
- `StoryCommand` is a closed polymorphic hierarchy. Every concrete command owns exactly one valid payload; the public parser facade and authored syntax remain unchanged.
- Choices exposed beside a non-dialogue/metadata Ink line are attached to an empty `Dialogue`, preserving choice presentation without visible speaker text.
- `EntryPoint` owns the cancellation token for one enabled novel session and cancels it before root disposal. Runtime feature entities destroy the screens and audio objects they instantiate, while session-bound loading, animation, request, wait, and execution operations observe cancellation.
- `SettingProcess` returns a typed `SettingSelection`, uses one cancellation-aware completion source, and receives localized UI text through a delegate. Localization is loaded before the settings screen; New Game side effects remain owned by the composition flow.
- The shared Bundles package validates UnityWebRequest results, observes session cancellation, uses versioned persistent-cache keys, resolves the active platform rather than a hard-coded Android bundle key, and owns StreamingAssets URL conversion. Audio requests apply the same result validation and no longer send response-only CORS headers.
- Bundles can create an episode `Scope`. The scope records owned bundle names, uses bundle-qualified asset-cache keys, unloads its bundles with `Unload(false)`, and removes their cached asset references at episode completion/disposal.
- Runtime parse, save, initialization, and queue-execution failures use `Novels.Diagnostics.NovelError` with Warning, Recoverable, or Fatal severity. Existing feature logs remain available for local diagnostics.
- `Novels/Validate Content` checks the catalog asset and screen, catalog IDs/titles and uniqueness, every referenced `NovelContentAsset`, bundle assignments, compiled Ink, AudioMixer, media, and built Android version files. Bundle building validates authoring before staging and built output after the atomic swap.
- Shared `BaseDisposable` is idempotent, marks itself disposed before child teardown, cleans every owned disposable even after an exception, and immediately disposes items added after owner disposal.
- `Location.VideoPlayback` owns VideoPlayer subscriptions and the active RenderTexture. It prepares video with timeout and session cancellation, releases GPU resources on replacement/disposal, and reports readiness/completion/failure to the unified live/immediate background flow in `Location.Entity`.
- `Character.Entity` owns per-character appearance state keyed by resolved character identity. It resolves a complete sprite set before touching the View, and `ShowCharacterQueue` rebuilds the same character presentation in live and replay modes while varying only the show animation.
- `StoryCommands.Entity` remains the public parser facade; `StoryPrefixParser` owns prefix/argument tokenization and `StoryCommandMapper` owns conversion of authored arguments into typed contracts.
- `Location.Entity` receives its target Camera through its construction context instead of querying `Camera.allCameras` during playback.
- `Novels.Bubble.Contracts` defines neutral Dialogue, Wardrobe, and Choose presentations passed through StoryQueue and QueueProcess to Bubble. The composition root does not map Bubble DTOs; `Bubble.Entity` alone converts each neutral contract into its UI-specific View model. Wardrobe and Choose contracts are intentional empty markers for future fields, and their authored trigger literals are centralized in `BubbleTriggers`.
- Lifetime ownership uses custom `BaseDisposable` and `.AddTo(this)`; async APIs use UniTask.
- Namespaces follow feature folders. Private fields are `_camelCase`; serialized references are `[SerializeField] private`; braces use Allman style; XML docs and nullable annotations are absent.

## Architecture wave completed on 2026-08-17 (catalog-scale runtime)

- Saves are namespaced as `Saves/{contentId}/{episodeId}/SaveChoice`. The former global `SaveChoice` is intentionally ignored; each story and episode starts with its own save namespace.
- `ApplicationRuntime` owns a `DisposableSlot<Entity>` for exactly one active novel. Completed novels are removed immediately instead of remaining referenced by the application lifetime stack.
- Catalog bootstrap retries only transport and integrity failures. Client/content-schema incompatibility and authoring failures escape to the fatal error boundary instead of being presented as network retries.
- `Remote/{platform}/release.json` atomically pins the catalog, every bundle version/hash, all Ink, audio, and video files, the minimum client version, and the content schema. One release remains fixed for the complete application session, with a cached last-known-good fallback.
- Bundle loading is deduplicated per bundle/version. Scopes receive reference-counted leases, so a bundle is unloaded only after its last non-persistent consumer is gone.
- Bundles download to temporary files, are checked with streaming SHA-256, committed atomically, and opened with `AssetBundle.LoadFromFileAsync`; bundle bytes are no longer duplicated through `LoadFromMemoryAsync`.
- Ink, audio, and video use the same release-aware file cache. Files are integrity-checked, addressed under the release ID, and pruned with a 512 MiB LRU budget.
- Novel preparation returns immutable `PreparedNovelResources` instead of mutating a partially initialized bootstrap state and stores only a preserved story-text preload task.
- Application-level UI strings are owned by `ApplicationLocalization`; episode titles have localized authoring entries with the previous `_title` retained as a fallback.
- The local bootstrap UI is shipped in the player as `Assets/Resources/Novels/BootstrapScreen.prefab`, with code generation retained as a safe fallback. The remote catalog uses a `ScrollRect` and cards with title, description, availability status, and enabled state.
- The Android content builder now emits the release manifest after staged bundle generation. The Editor validator checks the release, pinned bundle versions, all delivered content-file descriptors, the local bootstrap prefab, and the existing authoring rules.

## Architecture wave completed on 2026-08-17 (delivery hardening)

- `Bundles.Entity` is now a stable facade over focused `ContentReleaseProvider`, `BundleStore`, `BundledAssetCache`, `ContentFileStore`, and `ContentIntegrityVerifier` collaborators. Missing bundles/assets fail through typed content exceptions instead of mixing callback errors and null results.
- Required sprite requests remain strict, while character-layer candidate probing uses the explicit nullable `TryGetBundledSprite` path because absence in a particular appearance category is expected fallback behavior.
- Bundle and external-file SHA-256 checks execute away from the Unity main thread. Immutable release paths are remembered after successful verification for the current session, and content-cache pruning runs only after a new file is committed.
- Media resolution is immutable and episode-scoped. Each `Bundles.Scope` owns its prefix/manifest resolver, so preparing another story cannot overwrite global media configuration.
- `ContentReleaseValidator` is shared by runtime, Editor validation, and the build pipeline. It validates schema/client versions, duplicate names and paths, normalized paths, SHA metadata, and delivery-group totals.
- The Editor `ContentFilePolicy` is the only source of deliverable external-file rules. Hidden files and unsupported extensions (including `.DS_Store`) are excluded from releases and publish artifacts.
- Release files carry story delivery-group metadata. The build reports total/group/file budgets, warns about oversized payloads and large WAV files, and supports streamed OGG audio at runtime.
- A successful content build now atomically creates one complete ignored deploy artifact under `Build/NovelContent/ServerRoot`. Platform-specific releases and bundles live under `Remote/<platform>`; deduplicated immutable Ink/audio/video payloads live under `Files/{sha256}`. Unity bundle manifests, source Ink, and redundant story JSON names are excluded. The directory contents map directly to the configured remote server root.
- Music and ambient clips are destroyed when their channel is cleared, closing the previous orphaned-clip lifetime gap.
- Generated bootstrap/catalog prefabs share `GeneratedPrefabWriter`, which enforces a non-zero root scale and atomic Unity prefab serialization. Existing UI dimensions, reference resolutions, and layout values remain unchanged by design.
- `Novels.Locale` owns the session locale and deterministic locale fallback shared by application strings, catalog text, and episode titles.

## Architecture wave completed on 2026-08-17

- Two unresolved merge-conflict blocks were removed from folder metadata while preserving the existing folder GUIDs.
- `EpisodeScope` now owns episode-only screens, processors, audio, waits, notifications, and the bundle scope; application services remain owned by the root entity.
- `PriorityLoader` centralizes temporary background-loading priority changes used during bootstrap.
- The active content release declares available video files. Videos are resolved and cached only when first shown.
- `StreamingAssetsSource` and `MediaResolver` were extracted from `Bundles.Entity`; sentinel URLs and eager PNG-to-MP4 probing were removed.
- Bundle cache directories retain only the active version after a successful load.
- Content identity/version can be authored through additive scene fields with backward-compatible fallbacks.
- Save writes use a single coalescing background writer; the versioned envelope and atomic Cache write remain unchanged.
- Audio reuses one source per channel, streams music/ambient, and keeps a bounded sound-effect cache.
- StoryQueue delegates are grouped into location, audio, localization, bubble, choice, and character capability ports.
- Audio and unsupported-camera failures use `NovelError`; shared-package transport logs remain package-neutral.
- An isolated Unity 6000.3.11f1 import/compile completed without C# errors or warnings. Tests and Play Mode were not run.

## Architecture wave completed on 2026-08-17 (authoring and runtime ownership)

- `NovelCatalogAsset` is the remote authoring source for available stories and localized card text. It lives in the application-owned `novels_catalog` bundle together with the `Catalog.View` screen. The selected entry supplies content ID, bundle, and full asset address; `EntryPoint` contains only composition-root configuration.
- `NovelContentAsset` is the authoring source for one novel's identity, story bundle, episodes, media, AudioMixer, and content versions. A story-level bundle owns its definition and application assets; every episode declares one episode-lifetime bundle. Only catalog and the main loading screen remain in shared bundles.
- `Bundles.Entity` can load bundles before a novel prefix is known. Its media resolver is configured later with the prefix and episode media manifest, after `NovelContentAsset` has loaded. This keeps the local `StreamingAssets` source compatible with its future replacement by a remote server/CDN.
- `NovelDefinition`, episode collections, and silent-audio IDs expose defensive read-only collections.
- `EpisodeRuntime`, created through the root partial factory, owns the episode scope and guarantees save flushing at episode completion.
- `SaveWriter` exposes an observable `FlushAsync`; episode completion and application pause wait for pending coalesced writes. Disposal retains the synchronous last-resort flush.
- Audio uses dedicated loop sources for Music/Ambient and a four-voice Sound pool. Sound clips are not evicted while a voice is using them.
- Authored `тишина` is an explicit silent-audio ID that stops its channel without requesting a nonexistent file.
- Bubble choice buttons are pooled and rebound instead of destroyed and instantiated for every dialogue.
- Bundles records the exact names present in every loaded bundle and resolves requested addresses case-insensitively through this catalog before loading.
- Shared Bundles failures use package-neutral `BundleFailure`; the composition root adapts them to `NovelError`. Notification and video playback failures now use typed errors directly.
- Story command tokenization supports escaped separators in arguments, while the Editor validator scans compiled Ink strings for parse errors and missing static audio.
- AssetBundle building uses a Library staging directory, validates the build manifest, and swaps output only after all requested targets succeed. Console history is no longer cleared.
- Isolated Unity compilation and `NovelContentValidator.ValidateBatch` completed without errors. Tests were not created or run.

## Testing And Tooling

- Latest refactoring validation: isolated Unity 6000.3.11f1 compilation and existing-content validation passed; two complete Android content rebuild/validation passes succeeded with schema 4, 4 bundles, and 48 referenced external files. Tests and Play Mode were not run. A staging Android Player build was attempted but failed in the external platform pipeline with `Curl error 60` (certificate CN mismatch), so APK/device behavior remains unverified.

- The checked-in generated release must be rebuilt after removing the obsolete `tzm_1` package; until then its bundle list is stale and is not current runtime evidence.
- `Tools/validate-novels.sh` exposes `validate` and `content` batch commands. `Tools/build-remote-player.sh` builds Android/iOS from an isolated temporary project copy with novel StreamingAssets excluded and injects the required remote HTTP(S) root only into that copy.
- Unity 6000.3.11f1 isolated batch compilation and `NovelCiValidation.BuildAndValidateContentBatch` completed successfully for schema 4 after the explicit-delivery-ownership wave. The generated Android publish artifact is about 572 MiB. Tests were not created or run.

- The latest Android content release is `74a2b7e3706d6afceab3dc0ad76591433c72a56056a103ebfe57f317c093732a`: 4 bundles and 48 referenced external files. The complete build-and-validation batch passed authoring, bundle, release, byte-size, and SHA-256 checks.
- A final isolated Unity batch compile and `NovelContentValidator.ValidateBatch` completed successfully after the explicit-content-contracts wave. Tests were not created or run, and no UI asset or dimension was changed.

- Unity Test Framework is present transitively, but no EditMode or PlayMode tests were found.
- No project-local CI/test command was found. Tests were not created or run, and no scene/prefab was saved.
- Unity regenerated response/project files for the architecture wave. Every changed asmdef compiled through Unity Roslyn, and the complete generated solution compiled with 0 errors and 0 warnings. Tests were not created or run. A separate Unity batch-mode import remained unavailable because the project was already open in another Editor instance; runtime behavior remains unverified.

## Risks And Unknowns

- Render-pipeline configuration may be stale or incomplete; validate inside the Editor before graphics work.
- Bundle names remain authored string contracts, but loaded asset addresses are resolved against an exact per-bundle catalog. Shared story speakers and arguments are centralized in `StoryContracts`; story command names are normalized into typed commands at the parser boundary.
- Wardrobe/choose paths contain exact centralized future placeholders and their empty presentation contracts remain intentionally unfinished.
- AssetBundle building replaces `StreamingAssets/Remote` only after a successful staging build and restores the previous output if the swap fails; it is still a material content operation.
- The Android signing keystore is tracked by Git and should be removed from version control and rotated if the repository has been shared; no secret value was copied here.

## Architecture wave completed on 2026-08-17 (remote Player delivery)

- Editor sessions always construct `StreamingAssetsSource`; every non-Editor Player constructs `HttpContentSource`. The remote root must mirror the publish artifact root and contain `Remote/<platform>/release.json`.
- Player builds use `Tools/build-remote-player.sh`. The script copies the project into a temporary staging workspace, excludes `noveltexts`, `novelsaudio`, `novelsvideos`, and `Remote`, then invokes Unity there. The working project is never stripped or moved during a build.
- Release schema 4 assigns AssetBundles and external files to application, content-shared, or concrete episode groups. Application delivery is prepared before catalog loading; story and episode groups are prepared after episode selection.
- Delivery uses at most three concurrent downloads and reports aggregate byte progress. Cache reservations are synchronized before background pruning.
- `StoryReferenceIndex` is the common extraction pass for compiled/source Ink validation and episode delivery indexing.
- `NovelErrorContext` adds release, content, episode, and delivery-mode identity at the composition boundary.
- Pause and quit use a bounded synchronous save flush; ordinary episode completion retains the asynchronous flush path. EntryPoint and EpisodeRuntime cleanup now complete after partial initialization/disposal failures.
- Large WAV files above the profile threshold fail the content build unless converted to OGG or listed explicitly. The obsolete `tzm_1` exceptions have been removed; newly added large WAVs fail by default.

## Architecture wave completed on 2026-08-17 (story content hierarchy)

- All TZM_1 authoring assets now live under `Assets/RemoteAssets/Content/tzm_1`: `Definition`, story-level `Application`, and episode-level `Episodes/s01e01` subtrees make lifetime ownership visible in the filesystem.
- AssetBundle labels are assigned only at ownership boundaries. `content/tzm_1` produces `novels_content_tzm_1`; `content/tzm_1/episodes/s01e01` overrides its parent with `novels_episode_tzm_1_s01e01`. Descendant folders no longer carry feature-specific labels.
- `NovelDefinition` carries one story bundle name and `EpisodeDefinition` carries one episode bundle name. Setting and localization load from the story bundle; loading, bubble, character, location, and notification assets load from the episode bundle.
- `ContentAddressConvention` is the single source for the new content-rooted addresses. `ContentAddresses`, character sprite resolution, catalog indexing, and Editor reference validation use the same convention and include the episode ID where required.
- The bundle topology was reduced from ten bundles to four: `novels_catalog`, `novels_content_tzm_1`, `novels_episode_tzm_1_s01e01`, and `novels_loading_shared`. External Ink/audio/video delivery groups remain unchanged.
- Unity 6000.3.11f1 completed an isolated import/compile and the full Android `BuildAndValidateContentBatch` without errors. Tests and Play Mode were not run.

## Architecture wave completed on 2026-08-17 (package conventions and grouped delivery)

- `ContentPackageConvention` derives story/episode bundle names, definition paths, content roots, and delivery-group IDs from `contentId` and `episodeId`. Catalog and content assets no longer serialize duplicate bundle/address/prefix fields.
- `ContentAddresses` replaces the disposable `PathGetter` compatibility facade and is created once for the selected episode.
- `ContentProjectIndex` is the common Editor projection consumed by validation, delivery indexing, and bundle construction. `Novels/Content/Create Story` scaffolds a convention-compliant story/episode hierarchy, definition asset, catalog entry, and root AssetBundle labels.
- Release schema 4 groups every bundle and external file. `ContentDeliveryCoordinator` downloads at most three mixed payloads concurrently, reports combined item/byte progress, and includes missing bundle bytes in its free-space check.
- `CatalogFlow` owns release/catalog retry, application-group preparation, and story/episode selection. `ContentDeliveryFlow` owns story/episode delivery; `ApplicationRuntime` remains the session host and error boundary.
- `EpisodeAssetLoader` waits for the episode bundle, loads the five screen prefabs concurrently, and returns an immutable `EpisodeAssetSet` before feature composition.
- `BubblePresentationKind` carries Dialogue, Wardrobe, or Choose routing into QueueProcess. Future trigger literals are resolved once in StoryQueue instead of being compared inside queue execution.
- `ContentRequestRunner` centralizes UnityWebRequest execution, cancellation, progress, and transport error normalization for both HTTP and StreamingAssets sources.
- Android release `4ce17f56cf3a64d1e19eb9597f45e3976651f5bb6eadca83a434f257105d2d3a` contains 119 grouped payloads: 47 episode, 70 story-shared, and 2 application payloads. Unity compilation and complete content build/validation passed without errors or warnings; tests and Play Mode were not run.

## Architecture wave completed on 2026-08-17 (release sessions and build safety)

- Runtime content is pinned to an immutable `ContentReleaseSession`. Every bundle, file, delivery reservation, and episode scope receives the captured session explicitly, so loading a newer manifest cannot redirect work already in progress.
- Bundle records are keyed by release ID, bundle name, and version. Loading another release discards idle records from the previous release while active leases remain valid until their owners dispose them.
- `ContentStoragePlanner` owns one release-aware `RemoteContent` cache for bundles and external files. It reserves mixed payloads against one 512 MiB budget, protects leased paths, checks actual free space after cleanup, and performs ordinary LRU pruning away from the Unity main thread.
- Episode prefabs now have explicit Editor contracts for their required screen component and serialized references. The checks cover Loading, Bubble, Character, Location, and Notification without changing any authored UI dimensions.
- AssetBundle, publish, and Player-seed outputs are produced in one `Library` workspace and committed as a single transaction. Existing destinations are restored on failure; if rollback itself fails, recovery files are retained and both failures are reported.
- Remote Android/iOS Player staging is reusable under `Library/RemotePlayerBuild`, preserves its imported `Library`, validates its inputs, excludes local novel content, and retains build logs and the failed staging project for diagnosis.
- `ApplicationRuntime` expresses catalog loading, story selection, story execution, and return-to-catalog as an explicit state machine while retaining one active novel lifetime slot.
- An isolated Unity 6000.3.11f1 compile/existing-content validation passed. Two consecutive complete Android content build-and-validation runs also passed, exercising replacement of an existing release. Tests, Play Mode, and a device Player build were not run.

## Architecture wave completed on 2026-08-17 (runtime policies and payload materialization)

- `ContentPayloadMaterializer` is the single cache-validation, download, integrity, atomic-commit, touch, and prune path for bundles and external files. Concurrent requests for the same cache path share one materialization operation.
- Temporary downloads live under the separate `ContentStaging` cache root, so ordinary `RemoteContent` LRU pruning cannot race an in-progress commit. Stale staging files are pruned during delivery reservation.
- `ContentDeliveryProgressTracker` owns aggregate item/byte accounting and coalesces ordinary progress notifications to at most one per Unity frame while always publishing completion.
- Camera commands are mapped through one `CameraActionPlan` for both live and immediate/replay execution. Injury and splashes use transient dark/light flashes; Editor story validation checks every statically discoverable camera action against the same capability map.
- Generic `Bundles.Scope` cannot be configured with media after construction. Episode composition receives a `MediaScope` whose immutable resolver is valid from creation and exposes video/audio resolution explicitly.
- `CharacterAssetProfile.Default` is the single code-owned character asset convention used by runtime resolution and Editor validation. `NovelContentAsset` stores only the authored main-character speaker name and no longer exposes misleading empty asset-convention fields in the Inspector.
- `NovelRuntimeSettings` centralizes target frame rate, notification duration, and cut-scene fallback delay. A Resources asset is optional; absent settings retain the previous 30 FPS, 3-second notification, and 3000 ms fallback values.
- Unity 6000.3.11f1 isolated compilation, existing-content validation, and the complete Android content build/validation passed without C# errors or warnings. Tests, Play Mode, and a device Player build were not run; no authored UI dimensions or visual assets were changed.

## Architecture wave completed on 2026-08-17 (delivery policy and focused runtime plans)

- Content progress is an observation-only boundary. `ContentProgressReporter<T>` disables a failing observer after its first exception and logs the failure without invalidating, deleting, retrying, or failing an otherwise valid payload.
- `Cache` is now a pure filesystem assembly with no Unity engine, UniTask, or Disposable dependency. AssetBundle opening belongs to `Bundles.BundlePayloadLoader`; Cache owns only safe relative paths, atomic files, temporary files, and pruning.
- `ContentDeliveryOptions` is the immutable policy for cache size, parallel download count, staging lifetime, and local/remote request behavior. `NovelRuntimeSettings` supplies compatible defaults and the composition root passes the same options to content sources and `Bundles.Entity`.
- `BackgroundPresentationPlan` selects static image, looping video, cut-scene, or cut-scene-with-final-frame behavior. `BackgroundPresentationController` no longer combines recursive calls with boolean mode flags and explicitly requires a poster sprite for video sizing.
- `ContentProjectIndex.Entry` owns one `StoryDependencyManifest` per episode. The shared `StoryDependencyAnalyzer` merges explicit dynamic dependencies with compiled/source Ink discovery, and the same manifest feeds delivery indexing and story-reference validation.
- Editor validation accumulates `ContentValidationIssue` values with stable codes, severity, asset path, content ID, and episode ID. Existing validators that still emit plain messages are adapted to the generic validation code by `ContentValidationReport`.
- Character resolution is split between `CharacterAppearanceStore`, `CharacterAssetAddressResolver`, and `CharacterSpriteSetLoader`; `CharacterSpriteResolver` now only maps a render request to those collaborators.
- Runtime objects follow one lifetime owner: application bootstrap uses its local `using` scope, while `EpisodeRuntime` remains owned by its novel entity instead of also being disposed manually after execution.
- Isolated Unity 6000.3.11f1 compilation/existing-content validation and the complete Android content build/validation passed without C# errors or warnings. The release still contains 4 bundles and 48 referenced external files. Tests, Play Mode, and a device Player build were not run; no scene, prefab, content asset, or UI dimension changed.

## Architecture wave completed on 2026-08-17 (story localization ownership)

- `LocalizationData.asset` is the single authoring source for localized text inside a selected story. `NovelContentAsset` stores only an episode `titleKey`; catalog title/description remain in the application-owned catalog because they are needed before the story bundle is loaded.
- Story definition and localization assets are loaded together from the story bundle. One immutable locale snapshot is created per selected story and reused by episode selection, settings, dialogue headers, character names, and wardrobe labels; localization is no longer loaded again during episode preparation.
- Episode video IDs are derived during content builds from background references discovered in compiled/source Ink, including resolvable declared variables. Matching MP4 files enter `release.json`; runtime recognizes video backgrounds from the active release and the episode/shared delivery groups, so `NovelContentAsset` no longer serializes a manual video list.
- Localization uses normalized string locale codes such as `ru` and `en`, an explicit fallback locale, duplicate-key/duplicate-locale validation, and language-neutral required UI keys. The previous Russian-only enum and hard-coded Russian runtime selection were removed.
- The story scaffolder creates and initializes the localization asset together with the content definition, while `ContentProjectIndex` rejects missing localization assets, missing required UI entries, and missing episode-title entries before content builds.
- Generated C# project compilation completed with 0 errors and 0 warnings. Unity's assembly compilation then exposed missing direct `Novels.Editor` references to the localization and bubble-contract assemblies; both asmdef references were added, Unity regenerated its Bee response file, and `Novels.Editor.dll` compiled without C# errors. Batch content validation could not start concurrently because the project was already open. Tests and Play Mode were not run.

## Evidence Inspected

- `ProjectSettings/{ProjectVersion,ProjectSettings,GraphicsSettings,QualitySettings,EditorBuildSettings}.asset`
- `Packages/manifest.json`, `Packages/packages-lock.json`, local shared-package manifests/asmdefs
- `Assets/Novels/Novels.unity`, all first-party asmdefs, `EntryPoint.cs`, `Entity.cs`, partial factories, `NovelProcess.cs`, `StoryProcessor/Entity.cs`, representative feature/view code
- `Assets/Editor/CreateAssetBundles.cs` and repository inventories

<!-- unity-onboarding:generated:end -->

## Architecture wave completed on 2026-08-18 (versioned multi-platform publication)

- Release schema 5 separates stable logical content addresses from physical payload addresses. Every external payload is published once as `Files/{lowercase-sha256}`; Editor delivery still resolves the logical StreamingAssets path, while HTTP delivery resolves the immutable payload path from the release.
- Runtime and Editor dependency analysis share `StorySpeakerRoleResolver`, including narrator, wardrobe, main-character, and ordinary-character classification.
- Dependency discovery reads authored `.ink` commands and declared variables only. Compiled `.ink.json` remains the runtime story input and a required build input, but its compiler-private JSON strings are no longer scanned as an authoring API.
- The configured build targets are Android and iOS. Bundle file hashing is performed once for shared external files, platform releases are generated independently, and both are merged strictly into one atomic `Build/NovelContent/ServerRoot`.
- `ServerRoot` contains only remotely requested runtime artifacts: `Remote/<platform>/release.json`, referenced versioned bundles, and deduplicated `Files/{sha256}` payloads. Unity manifests, source Ink, and redundant aliases are not published.
- Duplicate shared payload paths must have identical size and SHA-256 metadata across platform releases; a conflict fails publication instead of accepting last-writer-wins behavior.
- `ruby Tools/verify-server-root.rb <https-root> <Android|iOS>` performs a read-only post-upload audit of the release and every referenced bundle/file, including HTTP status, size, and SHA-256.
- Generated C# solution compilation completed with 0 errors and 0 warnings. Tests and Play Mode were not run. The checked-in schema-4 StreamingAssets output must be replaced by the next Unity content build before runtime validation of schema 5.

## Architecture wave completed on 2026-08-18 (authoring, delivery, and deployment transactions)

- Ink authoring validation no longer ignores parser failures. Invalid command lines produce `STORY_COMMAND_INVALID` with the parser code, source file, episode, and one-based line number, preventing a known runtime parse failure from being published.
- One `ContentBuildSnapshot` owns the project index, delivery ownership, external-file metadata, and hashes for a complete build transaction. Android/iOS construction and built-output validation consume that same snapshot rather than independently rescanning Ink and content files.
- `Bundles.ContentReleaseCodec` is the sole JSON boundary for release serialization, deserialization, validation, and immutable snapshot creation. Runtime fallback and Editor publishing/validation use the same decoding rules.
- Deduplicated payload materialization tracks all active consumers. Cancelling one consumer removes only its progress observer; the underlying request is cancelled when the final consumer leaves, while remaining consumers continue sharing one operation.
- Embedded and Hybrid player seeds are composed from the selected platform release. They include only that platform's bundles and the required content-addressed files, never the other platform subtree from the shared `ServerRoot`.
- Publication creates `deployment.json` after immutable payloads and platform release files. It fingerprints every platform release and runtime payload and marks `release.json` files as activation-last boundaries.
- `Tools/verify-server-root.rb` validates the deployment fingerprint, selected release, upload-order flags, sizes, and hashes over HTTP. `Tools/plan-server-root-gc.rb` reports unreachable local server files while retaining current and explicitly supplied previous releases; it never deletes data.
- Generated C# solution compilation completed with 0 errors and 0 warnings. Both Ruby tools passed syntax validation. Tests, Play Mode, Unity content rebuild, server upload, and device builds were not run.

## Release orchestration added on 2026-08-19

- `Tools/release-novel-content.sh` is the single executable entry point for the content-release workflow. With no arguments it builds and validates Android/iOS content locally; upload and Player builds remain explicit opt-ins.
- The script validates the generated `ServerRoot`, extracts its deployment ID, uploads immutable files and bundles before activation manifests, verifies both remote platforms against that exact local deployment, and can then build Android and/or iOS Players.
- `Tools/release-content.env.example` documents machine/server configuration. The personal `Tools/release-content.env` is ignored by Git, and upload remains disabled by default even when a destination is configured.
- The upload adapter is intentionally limited to `rsync`; the destination root must already exist. Other hosting providers should be added as explicit adapters rather than embedding provider-specific credentials in the script.
