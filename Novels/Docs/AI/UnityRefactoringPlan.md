# Unity Refactoring Plan

## Purpose

This document is the working handoff plan for the next refactoring wave in the
`Novels` Unity project. It is intentionally detailed enough to continue the
work without reconstructing prior decisions from chat history.

Project root: `/Users/iantonishin/Fork/SomeGame/Novels`

Last reviewed: 2026-08-17

## Architecture wave completed on 2026-08-17 (catalog-scale runtime)

The ten follow-up findings are complete:

1. Save storage is namespaced per content and episode. The former global `SaveChoice` is intentionally not migrated.
2. A reusable `DisposableSlot<T>` owns exactly one replaceable novel runtime without retaining completed sessions.
3. Catalog retry is limited to typed transport/integrity failures; compatibility and configuration failures remain fatal.
4. A platform `release.json` pins the complete content snapshot and carries minimum-client and schema compatibility.
5. Bundle records deduplicate in-flight loads and use reference-counted scope leases.
6. Bundles download to temporary files, receive streaming integrity checks, and load through `AssetBundle.LoadFromFileAsync`.
7. Ink, audio, and video share a release-versioned, integrity-checked 512 MiB cache with LRU pruning.
8. Novel preparation produces immutable prepared resources rather than a partially initialized mutable bootstrap bag.
9. Application UI localization is centralized; episode titles are localized; the local bootstrap is an editable built-in prefab with a code fallback.
10. Catalog cards support status/enabled state and live inside a scrollable viewport; the build pipeline emits and validates the atomic release snapshot.

The Android content output was rebuilt and contains ten pinned bundles and 116 external content files. Unity compilation, the existing authoring validator, release validation, and generated integrity metadata completed successfully. Tests were not created or run. The tracked Android keystore was intentionally left unchanged because removal and credential rotation remain a separately authorized release-security operation.

## Architecture wave completed on 2026-08-17 (catalog and delivery)

The current ten-item refactoring wave is complete:

1. Bundle transport is represented by injected `IContentSource`; `EntryPoint` is the composition root for the current `StreamingAssetsSource`.
2. Bundle manifests/version pointers and downloaded Ink text retain a last-known-good cache fallback.
3. Catalog entries carry explicit bundle and asset addresses, and every `TZM_1` content/feature bundle is story-specific; only the reusable main loading screen remains shared.
4. `ApplicationRuntime` owns application-wide catalog and bundle services, while `Entity` owns one selected novel and its disposable bundle scope.
5. A concrete `EpisodeDefinition` is selected before novel bootstrap; runtime code no longer silently reads the first episode through `NovelDefinition.Episode`.
6. A code-created local bootstrap screen appears before remote assets exist and offers retry when the catalog cannot be loaded.
7. Story and episode cards use dedicated `Catalog.Entity`, `Catalog.View.Screen`, and `Catalog.View.Card` assemblies instead of reusing Setting.
8. Catalog and story-card text is localized in `NovelCatalogAsset`, with current-UI-culture selection and deterministic fallback.
9. Catalog addresses and common bootstrap asset names are centralized in `CatalogAddresses` and `BootstrapAddresses`.
10. Each built bundle now receives `manifest.json` containing version, byte length, SHA-256, and CRC. Runtime checks size and SHA-256 before accepting downloaded or cached bytes and remains compatible with legacy `version.txt` delivery.

The Android bundles were rebuilt through the staged build pipeline and the existing content validator completed successfully. Unity compiled the changed assemblies without compiler errors or warnings. Tests were not created or run.

## Architecture wave completed on 2026-08-17 (third review)

The nine items from the third project review are complete: authoring content is
stored in `NovelContentAsset`; episode lifetime and final save flush are owned by
`EpisodeRuntime`; SaveWriter is observable; Sound uses a bounded voice pool;
Bubble choices reuse buttons; loaded bundle names form an address catalog;
bundle builds use staging and rollback; compiled Ink/media receive Editor
validation; infrastructure failures cross package boundaries as neutral typed
failures and are adapted to `NovelError` at composition.

The scene stores no concrete content ID. Startup loads the versioned
`novels_catalog` bundle, presents its authored list of stories, and uses the
selected ID to load `Assets/RemoteAssets/Content/{contentId}.asset` from
`novels_content`. Both the catalog and complete novel definitions can therefore
be replaced through the same remote delivery path as other content. The
redundant legacy scene `Data` fields have been removed, and the versioned save
envelope remains compatible. An isolated Unity
6000.3.11f1 compile and batch content validation completed successfully. Tests
were not created or run, and the destructive AssetBundle build command itself
was not executed during validation.

## Architecture wave completed on 2026-08-17

The second review wave is complete: damaged metadata was repaired; explicit
lazy media resolution, full episode lifetime ownership, authorable content
identity/version, coalesced save writes, extracted bundle transport/media
responsibilities, reusable/streaming audio, grouped StoryQueue capability
ports, centralized loading priority, and obsolete bundle-cache pruning were
implemented. Runtime audio and unsupported-camera failures also report through
`NovelError`; reusable shared packages retain their package-neutral logs.

An isolated Unity 6000.3.11f1 import and C# compilation completed without
compiler errors or warnings. Per the standing decision, tests were not created
or run; Play Mode and device behavior remain unverified.

## Architecture wave completed on 2026-08-15

The eight follow-up architecture items are complete:

1. `StoryQueue.TryComplete()` flushes commands after the final Dialogue when Ink completes.
2. Save data uses a versioned content-aware envelope. Support for the obsolete raw-byte format was subsequently removed together with save migration.
3. `Novels.Content` defines immutable `NovelDefinition` and `EpisodeDefinition` models; the scene now references the authoring asset directly (the temporary legacy `Data` adapter was removed in the later authoring wave).
4. `NovelBootstrapProcess` owns application/start-selection/episode workflow while root partial factories still create concrete dependencies.
5. `Novels.Editor` validates loaded content configuration and protects AssetBundle building with the same validation.
6. `Bundles.Scope` gives episode bundles explicit lifetime ownership and removes bundle-qualified cached references when released.
7. Story commands are a closed class hierarchy with payloads that cannot be combined into invalid states.
8. `Novels.Diagnostics` supplies typed Warning/Recoverable/Fatal errors for parsing, persistence, initialization, and queue execution.

Additional integration corrections made while validating this wave:

- Choices accompanying metadata/non-dialogue Ink output are attached to an empty `Dialogue` instead of being rejected.
- Video cleanup tolerates the scene destroying its VideoPlayer before root disposal.
- Full generated-solution compilation completed with 0 errors and 0 warnings; tests were neither created nor run.

## Current architectural baseline

The story pipeline refactoring is complete:

- `Novels.StoryContracts` owns shared typed story vocabulary and has no assembly dependencies.
- `Novels.StoryCommands` parses authored Ink lines into typed commands.
- `Novels.StoryProcessor` adapts Ink content and choices.
- `Novels.StoryQueue` accumulates commands and composes executable batches.
- `StoryCommandQueueBuilder` builds non-dialogue queue items.
- `DialogueQueueBuilder` builds explicit `BeforeCommands` and `AfterCommands` sections and owns last-character state.
- `StoryQueue.Entity` is the sole owner of pending-command accumulation and final batch composition.
- `Novels.QueueProcess` owns executable queue items and `QueueProcess.Executor`.
- Every `IQueue` has one `Run(QueueExecutionContext)` entry point with typed `Live` and `Replay` modes.
- `NovelProcess` coordinates story progression through delegates and does not own parsing, queue construction, execution, or save collections.
- `Save.Entity` owns the initial saved-choice snapshot and replay cursor.
- Root composition uses partial factory files such as `Entity.StoryQueue.cs` and `Entity.QueueProcess.cs`.

The latest generated-solution C# compilation completed with 0 errors and 0
warnings. No tests were created or run. A separate Unity batch-mode import was
blocked because the project was already open in another Editor instance.

## Standing constraints and conventions

Keep these constraints unless the user explicitly changes them:

1. Work one step at a time. Explain the next step before implementing it.
2. Do not create or run tests.
3. After implementation, perform static checks and a full C# compilation.
4. Each feature should use its own asmdef when that matches the existing project structure.
5. Root `Entity` creates concrete feature objects through partial factory files.
6. Process classes receive integrations through delegates rather than concrete feature references.
7. Preserve authored story syntax at the parser boundary; downstream systems should consume typed contracts.
8. Do not change runtime behavior during an architectural extraction unless the behavior change is explicitly part of the approved step.
9. Preserve these future placeholder strings exactly until their features are implemented:

   - `"some wardrobe trigger"`
   - `"some choose trigger"`

10. Preserve the save-file byte format unless a separately approved migration step is introduced.
11. Do not rename `Dialogue`.
12. A choice set may belong to an empty `Dialogue`; empty speaker and text are valid when choices exist.

## Recommended order

### Step 1 — Extract save storage and fix deletion/path handling

Priority: High

Confidence: Confirmed

Estimated size: Medium

Status: Completed on 2026-08-15

Implemented result:

- `Save.Entity` receives `ReadBytes`, `WriteBytes`, and `Delete` through delegates and no longer references Cache directly.
- The root `Entity.SaveSystem` partial factory creates and owns the Cache adapter.
- Cache keeps filesystem paths as filesystem paths, uses `Path.Combine`, writes through a temporary file, and deletes exact keys.
- Bundles converts cached video filesystem paths to `file://` URLs at the video boundary.
- The machine-specific Location URL prefix and the Editor cache-clear URI misuse were removed.
- The existing raw byte save format was preserved.

#### Evidence

- `Assets/Novels/Save/Entity.cs` writes the save through `Cache.Entity` using the key `SaveChoice`.
- `Save.Entity.Clear()` deletes `Application.persistentDataPath/CachedFiles/Remote`, not the actual `CachedFiles/SaveChoice` file.
- `../Packages/Cache/Entity.cs` prepends `file://` to a path that is later passed to `System.IO` under `UNITY_EDITOR_OSX`.
- A physical `Novels/file:/Users/...` directory tree has already been created inside the project, confirming that a URI is being treated as a filesystem path.
- `Assets/Editor/CreateAssetBundles.cs` repeats the same `file://` plus `System.IO` pattern.

#### Goal

Make the Cache package the single owner of local filesystem path resolution and
give Save a key-based storage API that can read, write, and delete exactly its
own file.

#### Proposed design

- Add explicit Cache operations such as:

  - `ReadBytes(string key)`
  - `WriteBytes(string key, byte[] data)`
  - `Delete(string key)`
  - optionally `Exists(string key)`

- Keep local filesystem paths as filesystem paths. Add `file://` only at a boundary that explicitly requires a URL, such as `UnityWebRequest` or `VideoPlayer`.
- Replace string concatenation with `Path.Combine` inside Cache.
- Make Save call `cache.Delete(_ctx.SaveChoiceFileName)` instead of deleting a broad directory.
- Prefer an atomic write strategy: write a temporary file and replace/move it to the final path.
- Distinguish a missing save from a real read/corruption error instead of swallowing every exception as `"No save file"`.
- Keep the current raw byte sequence format for backward compatibility.
- Review `Assets/Editor/CreateAssetBundles.cs` and `Packages/Bundles` for the same path/URL distinction, but avoid unrelated package rewrites.

#### Behavior that must remain unchanged

- Choice IDs remain bytes.
- `byte.MaxValue` continues to mean a saved dialogue advance without a choice.
- New choices are appended to the in-memory current save.
- The initial replay snapshot does not grow when new choices are made during the same session.

#### Validation

- Confirm the save resolves under the actual `Application.persistentDataPath/CachedFiles` directory.
- Confirm New Game deletes the exact save file.
- Confirm `file:/` is no longer created by the new code path.
- Confirm an existing byte save can still be loaded.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

### Step 2 — Add an explicit end-of-story result

Priority: High

Confidence: Confirmed from the code path

Estimated size: Small to medium

Status: Completed on 2026-08-15

Implemented result:

- `StoryProcessor.ReadNext()` returns a typed `StoryReadResult` with `Content`, `Choices`, or `Completed` status.
- An authored empty line remains `Content`; only the absence of both continuable Ink content and choices becomes `Completed`.
- `NovelProcess` handles completion before parsing and exits its loop without producing an `EmptyQueue`.
- Parsing remains behind a delegate and receives only non-completed source and choices.

#### Evidence

- `StoryProcessor.Entity.GetNextText()` returns `string.Empty` when Ink `canContinue` is false.
- `StoryCommands.Entity.Parse()` converts an empty source without choices into `StoryCommandType.Empty`.
- `StoryQueue.Entity` accumulates non-dialogue commands until a Dialogue arrives.
- At the end of the story no later Dialogue exists, so `NovelProcess` can continue yielding and adding `EmptyQueue` items indefinitely.

#### Goal

Represent completion as control flow rather than as story text.

#### Proposed design

- Introduce a typed `StoryReadResult` in the StoryProcessor boundary with statuses such as:

  - `Content`
  - `Choices`
  - `Completed`
  - optionally `Error` if Ink errors are surfaced here

- Do not pass `Completed` to `StoryCommands.ParseStep`.
- Extend the `NovelProcess` delegate boundary so it can distinguish a produced step from story completion.
- Exit the `NovelProcess` loop cleanly on `Completed`.
- Decide explicitly what UI should remain visible at completion; do not infer a new ending screen without user direction.
- Keep an authored empty line distinct from the end of the Ink stream.

#### Validation

- Confirm authored empty lines retain their current meaning.
- Confirm a completed Ink story stops requesting steps.
- Confirm `_pendingQueue` cannot grow after completion.
- Confirm live and replay modes both terminate.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

### Step 3 — Establish lifecycle ownership and async cancellation

Priority: Medium to high

Confidence: Confirmed ownership gap; runtime impact not reproduced in Play Mode

Estimated size: Medium

Status: Completed on 2026-08-15

Implemented result:

- `EntryPoint` owns a cancellation token for each enabled novel session and cancels it before disposing the root entity.
- Top-level initialization is wrapped with explicit cancellation and exception handling.
- Root loading and story execution awaits observe the session token.
- Bubble, Character, Location, Notification, Waiting, Audio, and Loading operations observe cancellation at their frame/request boundaries.
- Bubble, Character, Location, and Notification destroy their instantiated screens during disposal; Audio destroys and clears every owned audio object.

#### Evidence

- Bubble, Character, Location, and Notification entities instantiate screen GameObjects but do not override `OnDispose` to destroy them.
- Audio owns runtime-created GameObjects but does not clear all of them on disposal.
- `EntryPoint.OnEnable()` calls `_entity.Init().Forget()`.
- `EntryPoint.OnDisable()` disposes the entity, but outstanding initialization, animation, loading, and video awaits have no shared cancellation token.
- Package Loading and Setting entities already demonstrate screen destruction in `OnDispose` and can be used as the local convention.

#### Goal

Make every runtime object and asynchronous operation have an explicit owner and
termination path.

#### Proposed design

- Add `OnDispose` cleanup to screen-owning feature entities.
- Add Audio cleanup that destroys all owned audio GameObjects and clears the dictionary.
- Introduce a cancellation token owned by `EntryPoint` or root `Entity`.
- Pass cancellation only through operations whose lifetime is tied to the novel session.
- Check cancellation in loading, animation, and video wait loops.
- Replace top-level fire-and-forget initialization with an error-reporting wrapper or an explicitly observed UniTask.
- Avoid adding a global cancellation service or new framework.

#### Validation

- Confirm disabling and re-enabling EntryPoint does not duplicate screens or audio objects.
- Confirm disposal stops further initialization work.
- Confirm no view is accessed after destruction.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

### Step 4 — Extract Location video playback ownership

Priority: High for long-running sessions

Confidence: Confirmed code defects; memory impact needs profiler/runtime validation

Estimated size: Medium to large

Status: Completed on 2026-08-15

Implemented result:

- `Novels.Location.VideoPlayback` is the single owner of VideoPlayer event subscriptions, the active RenderTexture, video preparation, completion, failure handling, and cleanup.
- Video events are subscribed once and removed symmetrically during disposal.
- Replacing, rejecting, timing out, or disposing playback stops the player and releases and destroys the active RenderTexture.
- Video preparation has a 10-second timeout and observes the existing novel-session cancellation token.
- `Location.Entity` uses one shared background path with a local `Live`/`Immediate` mode while preserving replay speed and cut-scene fallback behavior.
- `Location.View.Screen` retains the serialized VideoPlayer but no longer owns callback state.
- Camera lookup remains unchanged and can be addressed separately; no scene or prefab serialization changed.

#### Evidence

- `Location.Entity` creates a new `RenderTexture` for video playback in both live and replay paths.
- No matching `Release` or `Destroy` exists.
- `Location.View.Screen.OnVideoReady()` subscribes through `prepareCompleted` but unsubscribes from `loopPointReached`.
- Repeated `SetVideo` calls can accumulate handlers.
- The Editor macOS URL is prefixed with the developer-specific absolute path `/Users/iantonishin/SomeGame/Novels/`.
- Live and immediate background/video flows are largely duplicated.
- Video readiness and completion loops have no cancellation or timeout.

#### Goal

Create one owner for `VideoPlayer`, event subscriptions, render texture, URL
normalization, playback completion, and cleanup.

#### Proposed design

- Add an internal `VideoPlayback` or `BackgroundMediaPlayback` class inside `Novels.Location`.
- Let it own the active RenderTexture and release/destroy the previous texture before replacement.
- Subscribe once or always unsubscribe symmetrically before resubscribing.
- Remove all video handlers during disposal.
- Resolve local paths through an injected URL/path adapter, not a developer-specific prefix.
- Represent live versus replay through a Location-specific playback mode; do not introduce a dependency from Location to QueueProcess.
- Consolidate duplicated `SetImage` and `SetImageImmediate` orchestration while preserving animation differences.
- Add cancellation and a deliberate timeout/error result for video preparation.
- Inject the target Camera or background-color delegate instead of using `Camera.allCameras[0]`.

#### Additional small correction

`Location.Entity.SetCamera(FadeIn)` currently starts `SetEffect(...).Forget()` while
other camera actions are awaited. Unless intentionally concurrent, make FadeIn
follow the same sequential queue contract.

#### Validation

- Confirm event counts do not grow across repeated videos.
- Confirm old RenderTextures are released.
- Confirm video paths work without machine-specific prefixes.
- Confirm live and replay retain their intended timing differences.
- Confirm a failed or cancelled video cannot hang the queue.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

### Step 5 — Make character appearance state explicit and addressable

Priority: Medium

Confidence: Likely correctness risk

Estimated size: Medium

Status: Completed on 2026-08-15

Implemented result:

- `Character.Entity` stores clothes, hair, and accessory selection in a `CharacterAppearanceState` keyed by resolved character identity for the lifetime of the novel session.
- Main-character and Wardrobe requests share the explicit `MainCharacter` identity, while other speakers retain independent state by authored name.
- Main-character choice setters invalidate only the corresponding main-character resolved state.
- Sprite lookup produces a complete `CharacterSpriteSet`; the View is updated only after all sprite groups finish resolving.
- `ShowCharacterQueue` resolves and applies the same sprite state in live and replay modes; only animated versus immediate presentation differs.
- Scene, prefab, story syntax, asset path construction, and assembly dependencies remain unchanged.

#### Evidence

- `Character.Entity` stores one `_currentCharacterClothes`, `_currentCharacterHair`, and `_currentCharacterAccessories` value for all non-main speakers.
- These values are not keyed or reset by resolved character ID.
- A newly displayed character can therefore attempt to reuse an asset candidate selected for the previous character.
- `ShowCharacterQueue` replay mode calls `CharacterShowImmediate` without calling `CharacterSetImage`, so replay may not rebuild the visible sprite state.

#### Goal

Separate character appearance state, asset resolution, and view application.

#### Proposed design

- Introduce a `CharacterAppearanceState` keyed by the resolved character identity.
- Keep main-character selections explicit rather than mixing them with the currently rendered secondary character.
- Add an asset-resolution result that contains the complete sprite set to apply to the view.
- Apply the same resolved sprite state in live and replay modes; vary only the animation.
- Decide whether appearance state should persist per character or reset on every speaker transition, based on authored story expectations.
- Keep asset candidate strings as strings because their resource category is resolved during lookup.

#### Validation

- Alternate between two characters with different clothes, hair, and accessories.
- Confirm state does not bleed between them.
- Confirm main-character choices remain persistent.
- Confirm replay reconstructs the final character presentation.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

### Step 6 — Reduce Bubble presentation DTO duplication

Priority: Medium for maintainability

Confidence: Confirmed duplication, not a current functional defect

Estimated size: Medium

Status: Completed on 2026-08-15

Implemented result:

- Added the independent `Novels.Bubble.Contracts` assembly with neutral `BubblePresentation`, `BubbleText`, and `BubbleChoice` types; it depends only on `Novels.StoryContracts` and contains no Unity types.
- QueueProcess, StoryQueue, Bubble, and the composition root reference the shared contract explicitly.
- `SetBubbleQueue` creates one presentation object while retaining ownership of save/replay choice execution and completion callbacks.
- The root composition passes `bubble.SetBubbleScreen` directly and no longer maps equivalent DTOs.
- `Bubble.Entity` performs the single meaningful conversion from neutral presentation to the existing UI-specific `View.Screen.BubbleCtx`.
- Wardrobe and Choose now follow the same shared-contract path through `WardrobePresentation` and `ChoosePresentation`; their marker contracts remain intentionally empty until the features are implemented.
- Their exact future trigger strings are centralized in `BubbleTriggers` without changing either literal.
- The six-step primary refactoring plan is complete.

#### Evidence

Similar Bubble DTOs currently exist in:

- `QueueProcess.BubbleQueue.SetBubbleQueue`
- `Bubble.Entity`
- `Bubble.View.Screen`

The root `Entity.StoryQueue` composition maps between these representations.

#### Goal

Define one stable presentation boundary without coupling StoryQueue or
QueueProcess directly to a concrete Unity View.

#### Proposed design

- Delay this step until the intended Wardrobe and Choose behavior is understood.
- Consider a `Novels.Bubble.Contracts` asmdef if multiple assemblies need neutral Bubble models.
- Keep Unity objects and `UnityEngine.UI` types out of the contracts assembly.
- Prefer IDs/text plus a narrow selection callback or completion contract.
- Remove mechanical DTO-to-DTO conversions only after the final presentation contract is known.

#### Validation

- Confirm ordinary dialogue, narrator, thoughts, hints, disclaimers, and choices render identically.
- Preserve both future trigger placeholder strings exactly.
- Confirm assembly dependencies remain acyclic.
- Confirm full compilation has 0 errors and 0 warnings.
- Do not run tests.

## Optional lower-priority cleanup

The following second-wave items were completed on 2026-08-15:

1. Queue execution now carries session cancellation; Bubble choice waits terminate with the session.
2. Notification owns a non-blocking FIFO dispatcher instead of QueueProcess starting an unobserved task.
3. Setting returns a typed, cancellation-aware selection and consumes localized text.
4. Bundles validates and cancels requests, uses versioned cache keys, and removes its hard-coded Android video lookup.
5. Shared BaseDisposable is idempotent and completes owned cleanup after disposal failures.
6. QueueProcess commands are constructor-built and immutable.
7. StoryCommands is split internally into prefix parsing and typed command mapping while preserving its public API.
8. Location receives Camera through its context, Bubble headers use localization, the Notification View asmdef typo is fixed with its GUID preserved, and Git dependencies are pinned to the existing lock hashes.

Remaining work is intentionally separate from this refactoring wave:

1. Decide whether the missing render-pipeline assets should be restored with URP or the stale SRP references removed.
2. Design a release content strategy for approximately 1.2 GB under StreamingAssets, primarily WAV audio and MP4 video.
3. Remove the tracked Android signing keystore from version control and rotate it if the repository has been shared.

## Areas that are currently healthy

- Story responsibilities are divided across parsing, contracts, queue building, and execution.
- StoryContracts has no dependencies.
- No assembly cycles were found in the first-party asmdef graph.
- Root construction follows the project's partial-factory convention.
- Queue ownership and live/replay execution are explicit.
- Save replay no longer leaks or mutates its initial collection.
- The current Git working tree was clean at the time of this review.

## Audit limitations

This plan is based on static repository inspection and a successful generated-
solution C# compilation. The final Unity Editor import/Console state, Play Mode,
profiler captures, target-device behavior, and an Android player build were not
inspected. No test suite exists in the project, and tests must not be added or
run under the current user constraint.

## Instructions for the next agent/model

Before changing code:

1. Read this file completely.
2. Read `Docs/AI/UnityProjectContext.md`.
3. Inspect the current implementation and usages for the selected step; this plan may become stale after later edits.
4. Explain the detailed implementation plan to the user and wait for approval when the user asks to proceed step by step.
5. Implement only the approved step.
6. Preserve unrelated user changes in a dirty worktree.
7. Add Unity `.meta` files for new assets/scripts.
8. Run static searches and a full C# compilation, but do not create or run tests.
9. Verify that `Novels.slnx` contains no incidental generated diff.
10. Update this plan and `UnityProjectContext.md` when an architectural decision changes the documented baseline.
