# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Root: `/Users/iantonishin/Fork/SomeGame/Novels`
- Unity: 6000.3.11f1 (`3000ef702840`)
- Product: single-scene visual-novel player driven by compiled Ink and AssetBundles.
- Last analyzed: 2026-08-15; commit `df4f7828ddb21d00905ba4bb1262df922ca5e00a`.

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
- Feature assemblies: `StoryCommands`, `StoryProcessor`, `StoryQueue`, `QueueProcess`, `Bubble`, `Character`, `Location`, `Notification`, `Audio`, `Waiting`, `Save`, `PathGetter`, with separate View assemblies where applicable. `StoryContracts` owns the shared vocabulary consumed by story-driven features.

## Scenes And Startup

- Only enabled build scene: `Assets/Novels/Novels.unity`.
- Scene `EntryPoint.OnEnable()` initializes UniTask's player loop, caps FPS at 30, constructs `Novels.Entity`, and calls `Init().Forget()`.
- `Entity.Init()` loads the main loading UI, settings, story and feature bundles, then constructs localization, Ink, UI/features, save, audio, and `NovelProcess`.
- Scene data selects prefix `TZM_1`, main character `Salli`, story `s01e01.ink.json`, and seven `novels_*` bundles.
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
- `Save.Entity` receives byte-storage operations through delegates and no longer references the Cache assembly. The root `Entity.SaveSystem` factory owns the Cache adapter. Cache owns filesystem path resolution, atomic byte writes, existence checks, and exact-key deletion; filesystem paths are converted to `file://` URLs only at the Bundles video boundary.
- `StoryProcessor.ReadNext()` exposes typed `Content`, `Choices`, and `Completed` control flow. `NovelProcess` handles completion before parsing and terminates without adding an artificial queue item.
- `EntryPoint` owns the cancellation token for one enabled novel session and cancels it before root disposal. Runtime feature entities destroy the screens and audio objects they instantiate, while session-bound loading, animation, request, wait, and execution operations observe cancellation.
- `Location.VideoPlayback` owns VideoPlayer subscriptions and the active RenderTexture. It prepares video with timeout and session cancellation, releases GPU resources on replacement/disposal, and reports readiness/completion/failure to the unified live/immediate background flow in `Location.Entity`.
- `Character.Entity` owns per-character appearance state keyed by resolved character identity. It resolves a complete sprite set before touching the View, and `ShowCharacterQueue` rebuilds the same character presentation in live and replay modes while varying only the show animation.
- Lifetime ownership uses custom `BaseDisposable` and `.AddTo(this)`; async APIs use UniTask.
- Namespaces follow feature folders. Private fields are `_camelCase`; serialized references are `[SerializeField] private`; braces use Allman style; XML docs and nullable annotations are absent.

## Testing And Tooling

- Unity Test Framework is present transitively, but no EditMode or PlayMode tests were found.
- No project-local CI/test command was found. No build, tests, scene save, reimport, or package mutation was performed.
- No Unity MCP provider/configuration or connected Unity Editor capabilities are available in this session; Editor compile/console state is unverified.

## Risks And Unknowns

- Render-pipeline configuration may be stale or incomplete; validate inside the Editor before graphics work.
- `EntryPoint` starts `Init().Forget()` without an explicit exception handler at the call site.
- Bundle names and path conventions remain implicit string contracts. Shared story speakers and arguments are centralized in `StoryContracts`; story command names are normalized into typed commands at the parser boundary.
- Wardrobe/choose paths contain migration placeholders and appear unfinished.
- AssetBundle building deletes and recreates `StreamingAssets/Remote`; treat the menu command as destructive.
- Repository was already dirty: untracked path `file:/` existed and was not touched.
- Android keystore filename and alias are serialized in settings; no secret value was copied here.

## Evidence Inspected

- `ProjectSettings/{ProjectVersion,ProjectSettings,GraphicsSettings,QualitySettings,EditorBuildSettings}.asset`
- `Packages/manifest.json`, `Packages/packages-lock.json`, local shared-package manifests/asmdefs
- `Assets/Novels/Novels.unity`, all first-party asmdefs, `EntryPoint.cs`, `Entity.cs`, partial factories, `NovelProcess.cs`, `StoryProcessor/Entity.cs`, representative feature/view code
- `Assets/Editor/CreateAssetBundles.cs` and repository inventories

<!-- unity-onboarding:generated:end -->
