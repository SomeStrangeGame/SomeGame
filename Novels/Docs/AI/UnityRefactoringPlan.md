# Unity Refactoring Plan

## Purpose

This document is the working handoff plan for the next refactoring wave in the
`Novels` Unity project. It is intentionally detailed enough to continue the
work without reconstructing prior decisions from chat history.

Project root: `/Users/iantonishin/Fork/SomeGame/Novels`

Last reviewed: 2026-08-15

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

The last full C# compilation completed with 0 errors and 0 warnings. No tests
were created or run.

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

These are not part of the recommended critical path:

1. Split the 343-line `StoryCommands.Entity` internally into tokenization/prefix parsing and typed command mapping. Keep the public parser API unchanged.
2. Replace public mutable QueueProcess fields with constructors and readonly properties to make invalid queue items harder to construct.
3. Pin the UniTask and Ink Git dependencies to explicit tags or commits in `Packages/manifest.json`; the lock file currently records hashes, but regenerating it can follow floating Git references.
4. Correct the assembly typo `Novels.Notication.View` to `Novels.Notification.View` only as an isolated asmdef migration with reference verification.
5. Move hardcoded user-facing Setting/Bubble strings into the existing localization boundary when localization requirements are defined.

## Areas that are currently healthy

- Story responsibilities are divided across parsing, contracts, queue building, and execution.
- StoryContracts has no dependencies.
- No assembly cycles were found in the first-party asmdef graph.
- Root construction follows the project's partial-factory convention.
- Queue ownership and live/replay execution are explicit.
- Save replay no longer leaks or mutates its initial collection.
- The current Git working tree was clean at the time of this review.

## Audit limitations

This plan is based on static repository inspection and the immediately previous
successful C# compilation. Unity Editor Console, Play Mode, profiler captures,
target-device behavior, and an Android player build were not inspected. No test
suite exists in the project, and tests must not be added or run under the
current user constraint.

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
