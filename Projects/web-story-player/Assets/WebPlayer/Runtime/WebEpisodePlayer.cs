using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Novels.WebPlayer
{
    // Browser composition only; all interpretation and presentation use the shared runtime.
    internal sealed class WebEpisodePlayer : BaseDisposable
    {
        private readonly WebStoryRuntimeSession _session;
        private readonly Action<string, string> _emit;
        private readonly Dictionary<string, string> _assetBundles = new(StringComparer.OrdinalIgnoreCase);
        private WebDecisionSaveSession _save;
        private string _initialBundle;
        private EventSystem _input;
        private WebNovelProgress _progress;
        internal string EpisodeId { get; private set; }
        internal bool HasNextEpisode { get; private set; }

        internal void StopInput()
        {
            if (_input != null) _input.enabled = false;
        }

        internal WebEpisodePlayer(WebStoryRuntimeSession session, Action<string, string> emit)
        {
            _session = session;
            _emit = emit;
        }

        internal async UniTask FlushForStop()
        {
            if (_save != null) await _save.RetryFlushAsync();
            if (_progress != null) await _progress.FlushAsync();
        }

        internal async UniTask<EpisodeRunResult> Run(WebStorySaveStore store)
        {
            var token = _session.CancellationToken;
            var chunks = _session.Content.StreamingPlan.chunks;
            _initialBundle = chunks[0].bundle;
            foreach (var chunk in chunks)
                foreach (var asset in chunk.assets ?? Array.Empty<string>())
                    _assetBundles[Canonicalize(asset)] = chunk.bundle;
            var content = await _session.Assets.GetBundledSO<Content.NovelContentAsset>(
                new Bundles.BundleAssetAddress(_initialBundle,
                    ContentAddressing.ContentPackageConvention.DefinitionAsset(_session.Configuration.storyId)));
            token.ThrowIfCancellationRequested();
            if (content == null) throw new InvalidOperationException("Story definition is missing.");
            var definition = content.ToDefinition();
            if (!string.Equals(definition.Id, _session.Configuration.storyId, StringComparison.Ordinal))
                throw new InvalidOperationException("Story definition does not match launch.");
            var addresses = new ContentAddressing.ContentAddresses(definition.Id, definition.ResolveArtAddress);
            var text = await _session.Content.GetText(addresses.NovelText(definition.StoryPath));
            token.ThrowIfCancellationRequested();
            _progress = await WebNovelProgress.Open(store, definition, _session.Configuration.storyVersion);
            token.ThrowIfCancellationRequested();
            var playable = _progress.Progress.PlayableEpisodes;
            var episode = playable[playable.Count - 1];
            EpisodeId = episode.Id;
            var initialState = _progress.Progress.GetEntryState(episode);
            // A completed final/early-ending episode stays finished on reload.
            if (_progress.Progress.CompletedEpisodeIds.Contains(episode.Id))
                return EpisodeRunResult.Completed();
            _save = (await WebDecisionSaveSession.Open(store, definition.Id,
                _session.Configuration.storyVersion, definition.ContentVersion, episode.Id,
                error => Debug.LogError(error.Message))).AddTo(this);
            token.ThrowIfCancellationRequested();
            var savedDecisions = _save.Decisions.GetInitialDecisionsSnapshot();
            if (savedDecisions.Length > 0 && !ReplayValidator.IsCompatible(text, initialState, savedDecisions, out var reason))
                throw new InvalidOperationException("Save cannot be replayed; it was preserved: " + reason);
            _emit("resume_loaded", savedDecisions.Length.ToString());
            var story = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
                { StoryText = text, InitialState = initialState }).AddTo(this);
            var screen = ContentAddressing.ContentAssetNames.EpisodeScreen;
            var bubblePrefab = await Prefab(addresses.BubblePrefab(screen));
            var characterPrefab = await Prefab(addresses.CharacterPrefab(screen));
            var locationPrefab = await Prefab(addresses.LocationPrefab(screen), "location");
            var notificationPrefab = await Prefab(addresses.NotificationPrefab(screen), "notification");
            var choosePrefab = await _session.Assets.TryGetBundledPrefab(
                new Bundles.BundleAssetAddress(_initialBundle, addresses.ChoosePrefab(screen)));
            token.ThrowIfCancellationRequested();
            var cameraObject = new GameObject("StoryCamera", typeof(Camera));
            cameraObject.transform.SetParent(_session.RuntimeRoot, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            if (EventSystem.current == null)
            {
                var input = new GameObject("StoryInput", typeof(EventSystem), typeof(StandaloneInputModule));
                _input = input.GetComponent<EventSystem>();
                input.transform.SetParent(_session.RuntimeRoot, false);
            }
            var bubble = new Bubble.BubbleController(new Bubble.BubbleController.Dependencies
            {
                BubblePrefab = bubblePrefab, CancellationToken = token,
                CanOpenWardrobe = () => false,
            }).AddTo(this);
            bubble.Init();
            var character = new Character.CharacterController(new Character.CharacterController.Dependencies
            {
                ScreenPrefab = characterPrefab, ContentPrefix = definition.Prefix,
                ResolveArtAddress = definition.ResolveArtAddress, AssetProfile = definition.CharacterAssets,
                GetSprite = Sprite, GetFullQualitySprite = Sprite,
                GetSpriteTrimManifest = () => UniTask.FromResult<Character.CharacterSpriteTrimManifest>(null),
                MissingCharacter = RequiredResource<Sprite>("missing-character"), CancellationToken = token,
            }).AddTo(this);
            character.Init();
            var location = new Location.LocationController(new Location.LocationController.Dependencies
            {
                ScreenPrefab = locationPrefab, TargetCamera = camera,
                GetSprite = name => Sprite(addresses.LocationImage(name)),
                GetFullQualitySprite = name => Sprite(addresses.LocationImage(name)),
                MissingBackground = RequiredResource<Sprite>("missing-background"),
                CancellationToken = token, CutSceneFallbackDelayMilliseconds = 1,
                OnError = error => Debug.LogError(error.Message),
            }).AddTo(this);
            location.Init();
            var choose = new Choose.ChooseController(token, choosePrefab).AddTo(this);
            choose.Init();
            var notification = new Notification.NotificationController(new Notification.NotificationController.Dependencies
            {
                NotificationPrefab = notificationPrefab, CancellationToken = token,
                DisplayDuration = TimeSpan.FromSeconds(3), OnError = error => Debug.LogError(error.Message),
            }).AddTo(this);
            notification.Init();
            var parser = new StoryCommands.Entity();
            var queue = new StoryQueue.StoryQueueBuilder(new StoryQueue.StoryQueueBuilder.Dependencies
            {
                MainCharacter = definition.MainCharacter, Story = story, Save = _save.Decisions,
                Bubble = bubble, Character = character, Location = location, Choose = choose,
                Notification = notification, // Audio callback and Wardrobe intentionally absent.
                LoadChooseThumbnail = name => Sprite(addresses.ChooseItem(name)),
                LoadBubbleChoiceIcon = name => string.IsNullOrWhiteSpace(name)
                    ? UniTask.FromResult<Sprite>(null) : Sprite(addresses.ChooseItem(name)),
                Wait = seconds => UniTask.Delay(TimeSpan.FromSeconds(Math.Max(0, seconds)), cancellationToken: token),
                FlushCheckpoint = async () =>
                {
                    await _save.Decisions.FlushAsync();
                    token.ThrowIfCancellationRequested();
                    _emit("save_committed", episode.Id);
                },
                OnDialogueReady = (kind, count) => _emit("dialogue_ready", kind + ":" + count),
                OnChoiceSelected = id => _emit("choice_selected", id.ToString()),
                OnEndingReached = id => _emit("ending_reached", id),
            });
            var executor = new StoryExecution.StoryOperationExecutor();
            var process = new NovelProcess(new NovelProcess.Dependencies
            {
                ReadNext = story.ReadNext, ExportStoryState = story.ExportState,
                ParseStep = parser.ParseStep, BuildQueue = queue.TryBuild, CompleteQueue = queue.TryComplete,
                ExecuteQueue = executor.Run, GetNextSavedDecision = _save.Decisions.GetNextSavedDecision,
                HideLoading = () => UniTask.CompletedTask,
                OnReady = () => _emit("reading_started", episode.Id),
                CancellationToken = token, OnError = error => Debug.LogError(error.Message),
                IsEpisodeEnd = source => !string.IsNullOrWhiteSpace(definition.EndMarker)
                    && (source ?? "").TrimStart().StartsWith(definition.EndMarker, StringComparison.OrdinalIgnoreCase),
            }).AddTo(this);
            _session.Episode.Configure(process.Run, _save.Decisions.FlushAsync);
            var result = await _session.Episode.Run();
            token.ThrowIfCancellationRequested();
            if (result.Status == EpisodeRunStatus.Completed)
            {
                StopInput();
                _progress.Progress.Complete(episode, result.ContinuationState);
                // Decisions commit first; completion + next entry state commit together.
                await _progress.FlushAsync();
                token.ThrowIfCancellationRequested();
                HasNextEpisode = _progress.Progress.PlayableEpisodes.Last().Id != episode.Id;
            }
            return result;
        }

        private async UniTask<GameObject> Prefab(string address, string fallback = null)
        {
            var prefab = await _session.Assets.TryGetBundledPrefab(
                new Bundles.BundleAssetAddress(_initialBundle, address));
            _session.CancellationToken.ThrowIfCancellationRequested();
            if (prefab == null && fallback != null) prefab = RequiredResource<GameObject>(fallback);
            return prefab != null ? prefab : throw new InvalidOperationException("Required story prefab missing: " + address);
        }

        private async UniTask<Sprite> Sprite(string address)
        {
            _session.CancellationToken.ThrowIfCancellationRequested();
            var bundle = _assetBundles.TryGetValue(Canonicalize(address), out var found) ? found : _initialBundle;
            // Existing bundle scope deduplicates downloads; only demand-loaded chunks are retained.
            // TryGetBundledSprite requires ownership; it does not acquire a new chunk.
            await _session.Assets.GetAssetBundle(bundle);
            _session.CancellationToken.ThrowIfCancellationRequested();
            return await _session.Assets.TryGetBundledSprite(new Bundles.BundleAssetAddress(bundle, address));
        }

        private static string Canonicalize(string value) =>
            (value ?? "").Replace('\\', '/').Normalize(NormalizationForm.FormC).Trim();

        private static T RequiredResource<T>(string name) where T : UnityEngine.Object =>
            Resources.Load<T>("WebFallbacks/" + name)
                ?? throw new InvalidOperationException("Build fallback is missing: " + name);
    }
}
