# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Root: `/Users/iantonishin/Fork/SomeGame/Novels`
- Unity: 6000.3.11f1 (`3000ef702840`)
- Product: single-scene visual-novel player driven by compiled Ink and AssetBundles.
- Last analyzed: 2026-08-17; baseline commit `2002ab14f2c42a180eb1b1ae306f46003285f7a0`.

## Confirmed Environment

- Render pipeline: unresolved. Graphics Settings reference an SRP asset and `URPProjectSettings.asset` exists, but no URP package or matching tracked pipeline asset was found.
- Input: legacy Input Manager (`activeInputHandler: 0`).
- Primary target: Android; the bundle builder currently only invokes `BuildTarget.Android`.
- Important dependencies: Ink, UniTask, uGUI, plus local `somegame.*` packages for bundles/cache, disposal, loading, localization, settings, logs, and ScriptableObject data.

## Structure And Assemblies

- `Assets/Novels`: runtime composition, feature/domain assemblies, views, and the only scene.
- `Assets/Novels/StoryCommands`: typed story-command model and legacy Ink-line parser.
- `Assets/RemoteAssets`: bundled UI/images/localization/settings/loading authoring content.
- `Assets/StreamingAssets`: Ink JSON, audio/video, and built remote bundle payloads.
- `Assets/Editor/CreateAssetBundles.cs`: manual Android AssetBundle builder/cache clearer.
- `../../Packages/*`: shared local packages; moving `Novels` alone breaks the relative `file:` dependencies.
- `Novels`: broad composition-root assembly referencing feature assemblies and shared packages.
- `Novels.Content`: immutable `NovelDefinition` and `EpisodeDefinition` configuration independent of scene serialization.
- `Novels.Diagnostics`: neutral error code, severity, source, and exception contracts.
- `Novels.Editor`: editor-only validation of loaded novel configuration and Android bundle output.
- Feature assemblies: `StoryCommands`, `StoryProcessor`, `StoryQueue`, `QueueProcess`, `Bubble`, `Character`, `Location`, `Notification`, `Audio`, `Waiting`, `Save`, `PathGetter`, with separate View assemblies where applicable. `StoryContracts` owns the shared vocabulary consumed by story-driven features.

## Scenes And Startup

- Only enabled build scene: `Assets/Novels/Novels.unity`.
- Scene `EntryPoint.OnEnable()` initializes UniTask's player loop, caps FPS at 30, creates the concrete `IContentSource`, captures `Application.persistentDataPath` on the main thread, constructs `ApplicationRuntime`, and starts an exception-observing session wrapper.
- `ApplicationRuntime` owns the shared bundle service, local bootstrap/retry UI, remote catalog, story selection, and the currently active novel runtime. `Entity.Init()` owns one selected story and creates `NovelBootstrapProcess`, which coordinates application preparation, New Game/Continue selection, episode preparation, and episode execution through delegates.
- The scene contains no concrete story ID. Startup loads `NovelCatalog.asset` and its selection screen from `novels_catalog`; `Catalog.Entity/View` presents localized cards and returns a selected catalog entry. The entry supplies explicit content bundle and asset addresses such as `novels_content_tzm_1` and `Assets/RemoteAssets/Content/TZM_1/TZM_1.asset`. The loaded definition then presents an explicit episode selection and supplies per-story feature bundle names.
- `StoryCommandParser` converts legacy colon-delimited Ink lines into typed commands; `NovelProcess` maps them to queued actions for location/cut-scene, audio, camera, waits, notifications, character presentation, dialogue, and choices.

## Architecture And Conventions

- Confirmed manual composition root: a partial `Novels.Entity` wires feature entities using nested `Ctx` structs and delegates; no DI container.
- Confirmed Entity/View split: disposable plain-C# feature entities drive uGUI `Screen` MonoBehaviours.
- Confirmed command queue with async and immediate/replay modes; saved choices are persisted as bytes.
- Story syntax parsing is isolated in `Novels.StoryCommands`; each command exposes only its specialized payload (`Dialogue`, `Background`, `Audio`, `Notification`, `Camera`, or `Wait`). Dialogue presentation, choice actions, character control arguments, background options, and camera actions are converted from authored strings to typed `StoryContracts` values at this boundary. Free-form character asset candidates remain strings because their resource category is resolved during sprite lookup. `StoryProcessor` converts Ink choices to the neutral `StoryChoice` contract. `StoryQueue` accumulates commands and builds executable queue batches, while `NovelProcess` only receives steps and executes ready batches through delegates.
- A dialogue may have empty speaker/text when Ink exposes choices without a prompt; character presentation is updated only when the dialogue contains speaker or text content.
- `StoryQueue.StoryCommandQueueBuilder` maps each non-dialogue story command to one executable queue item. `StoryQueue.DialogueQueueBuilder` owns the last-character state and returns explicit queue items that belong before and after the accumulated commands. It resolves the configured main-character name and typed dialogue presentation into a neutral `StorySpeakerRole` and `StoryCharacterPosition`; `StoryQueue.Entity` is the sole owner of command accumulation and final batch composition. Character, Bubble, and QueueProcess consume the resolved role instead of comparing authored speaker strings.
- Dialogue background alignment remains a neutral `StoryDialogueAlignment` through StoryQueue and QueueProcess; `Location.Entity` owns the conversion to Unity's `TextAlignment` used by its View.
- `QueueProcess.Executor` owns sequential execution and draining of completed queue batches. It converts the optional saved choice into a typed `QueueExecutionContext`; every `IQueue` has one `Run(context)` entry point and selects live or replay behavior through `QueueExecutionMode`. The executor is created through the root `Entity.QueueProcess` partial factory; `NovelProcess` receives only its execution delegate and remains responsible for story progression. `Save.Entity` owns the immutable initial-choice snapshot and its replay cursor, exposing the next saved choice through a delegate without leaking its collection.
- `QueueExecutionContext` carries the novel-session cancellation token. Queue commands are immutable after construction, validate required delegates in their constructors, and cancel user-input waits without leaving the executor suspended.
- Notifications preserve their non-blocking story behavior through a Notification-owned FIFO dispatcher. The dispatcher serializes presentation, observes cancellation and exceptions, and replaces queue-level fire-and-forget work.
- `Save.Entity` receives byte-storage operations through delegates and no longer references the Cache assembly. The root `Entity.SaveSystem` factory owns the Cache adapter. Cache owns filesystem path resolution, atomic byte writes, existence checks, and exact-key deletion; filesystem paths are converted to `file://` URLs only at the Bundles video boundary.
- Save storage writes and accepts only a versioned binary envelope containing content identity, content version, and the choice payload. Saves in obsolete formats or belonging to another episode/version are not replayed.
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

## Architecture wave completed on 2026-08-17

- Two unresolved merge-conflict blocks were removed from folder metadata while preserving the existing folder GUIDs.
- `EpisodeScope` now owns episode-only screens, processors, audio, waits, notifications, and the bundle scope; application services remain owned by the root entity.
- `PriorityLoader` centralizes temporary background-loading priority changes used during bootstrap.
- `EpisodeMediaDefinition` declares available video IDs. Videos are resolved and cached only when first shown.
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
- `NovelContentAsset` is the authoring source for one novel's identity, application bundles, episodes, media, AudioMixer, and content versions. Every story has its own content and feature bundle names (`*_tzm_1` for the current story); only the main loading screen is in `novels_loading_shared`.
- `Bundles.Entity` can load bundles before a novel prefix is known. Its media resolver is configured later with the prefix and episode media manifest, after `NovelContentAsset` has loaded. This keeps the local `StreamingAssets` source compatible with its future replacement by a remote server/CDN.
- `NovelDefinition`, episode collections, video IDs, audio overrides, and silent-audio IDs expose defensive read-only collections.
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

- Unity Test Framework is present transitively, but no EditMode or PlayMode tests were found.
- No project-local CI/test command was found. Tests were not created or run, and no scene/prefab was saved.
- Unity regenerated response/project files for the architecture wave. Every changed asmdef compiled through Unity Roslyn, and the complete generated solution compiled with 0 errors and 0 warnings. Tests were not created or run. A separate Unity batch-mode import remained unavailable because the project was already open in another Editor instance; runtime behavior remains unverified.

## Risks And Unknowns

- Render-pipeline configuration may be stale or incomplete; validate inside the Editor before graphics work.
- Bundle names remain authored string contracts, but loaded asset addresses are resolved against an exact per-bundle catalog. Shared story speakers and arguments are centralized in `StoryContracts`; story command names are normalized into typed commands at the parser boundary.
- Wardrobe/choose paths contain exact centralized future placeholders and their empty presentation contracts remain intentionally unfinished.
- AssetBundle building replaces `StreamingAssets/Remote` only after a successful staging build and restores the previous output if the swap fails; it is still a material content operation.
- The Android signing keystore is tracked by Git and should be removed from version control and rotated if the repository has been shared; no secret value was copied here.

## Evidence Inspected

- `ProjectSettings/{ProjectVersion,ProjectSettings,GraphicsSettings,QualitySettings,EditorBuildSettings}.asset`
- `Packages/manifest.json`, `Packages/packages-lock.json`, local shared-package manifests/asmdefs
- `Assets/Novels/Novels.unity`, all first-party asmdefs, `EntryPoint.cs`, `Entity.cs`, partial factories, `NovelProcess.cs`, `StoryProcessor/Entity.cs`, representative feature/view code
- `Assets/Editor/CreateAssetBundles.cs` and repository inventories

<!-- unity-onboarding:generated:end -->
