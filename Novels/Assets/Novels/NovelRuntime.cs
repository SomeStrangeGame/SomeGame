using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Audio;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Novels
{
    internal partial class NovelRuntime : BaseDisposable
    {
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        internal struct Dependencies
        {
            internal CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
            internal Diagnostics.SmokeTelemetry SmokeTelemetry;
            internal Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
            internal Bundles.Entity Bundles;
            internal Catalog.NovelCatalogEntry Content;
            internal string PersistentDataPath;
            internal Camera TargetCamera;
            internal AudioMixer AudioMixer;
            internal FallbackAssets FallbackAssets;
            internal NovelRuntimeTuning RuntimeTuning;
            internal Func<
                Content.NovelDefinition,
                UniTask<Content.EpisodeDefinition>>
                SelectEpisode;
            internal Func<string, UniTask<Bundles.ContentDeliveryLease>>
                PrepareNovelContent;
            internal Action HidePreparationScreen;
        }

        private readonly Dependencies _ctx;
        private readonly PriorityLoader _priorityLoader;
        private Content.NovelDefinition _definition;
        private Content.EpisodeDefinition _episode;
        private string _assetBundleName;
        private Save.SaveSystem _saveSystem;
        private NovelProgress _progress;
        private Location.LocationController _activeLocation;
        private Character.CharacterController _activeCharacter;
        private StoryStreamingController _streaming;

        internal NovelRuntime(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.Content == null)
                throw new ArgumentNullException(nameof(ctx.Content));
            if (string.IsNullOrWhiteSpace(ctx.PersistentDataPath))
                throw new ArgumentException(
                    "Persistent data path must not be empty.",
                    nameof(ctx.PersistentDataPath));
            if (ctx.SelectEpisode == null)
                throw new ArgumentNullException(nameof(ctx.SelectEpisode));
            if (ctx.PrepareNovelContent == null)
                throw new ArgumentNullException(nameof(ctx.PrepareNovelContent));
            if (ctx.HidePreparationScreen == null)
                throw new ArgumentNullException(nameof(ctx.HidePreparationScreen));
            if (ctx.TargetCamera == null)
                throw new ArgumentNullException(nameof(ctx.TargetCamera));
            if (ctx.AudioMixer == null)
                throw new ArgumentNullException(nameof(ctx.AudioMixer));
            if (ctx.FallbackAssets == null)
                throw new ArgumentNullException(nameof(ctx.FallbackAssets));
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
        }

        internal async UniTask<EpisodeRunResult> Init()
        {
            var storyAssets = _ctx.Bundles.CreateScope().AddTo(this);
            (await _ctx.PrepareNovelContent(_ctx.Content.ContentId))?.AddTo(this);
            var streamingPlan = _ctx.Bundles.StreamingPlan;
            var chunks = streamingPlan?.chunks;
            var hasStreamingPlan = chunks != null && chunks.Length > 0;
            _assetBundleName = hasStreamingPlan
                ? chunks[0].bundle
                : _ctx.Content.ContentBundleName;
            _definition = await LoadContent(
                storyAssets,
                _ctx.Content,
                _assetBundleName);
            if (hasStreamingPlan)
            {
                _streaming = new StoryStreamingController(
                    _ctx.Bundles,
                    storyAssets,
                    streamingPlan,
                    _ctx.CancellationToken,
                    _ctx.OnLog,
                    OnChunkReady).AddTo(this);
                _streaming.Start();
            }
            _ctx.HidePreparationScreen();
            _progress = new NovelProgress(
                _definition,
                _ctx.PersistentDataPath,
                _ctx.OnLog);
            var playableDefinition = new Content.NovelDefinition(
                _definition.Id,
                _definition.MainCharacter,
                _definition.ContentVersion,
                _definition.EndMarker,
                _definition.SilentAudioIds,
                _progress.PlayableEpisodes);
            _episode = await _ctx.SelectEpisode(playableDefinition);
            _progress.Begin(_episode);
            var episodeRuntime = new EpisodeRuntime(_ctx.CancellationToken).AddTo(this);

            EpisodeRunResult result;
            try
            {
                _ctx.CancellationToken.ThrowIfCancellationRequested();
                var prepared = await PrepareApplication(storyAssets, episodeRuntime);
                if (prepared.selection == SettingSelection.NewGame)
                    prepared.episode.SaveSystem.Clear();
                _ctx.CancellationToken.ThrowIfCancellationRequested();
                result = await RunEpisode(prepared.episode);
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
            {
                result = EpisodeRunResult.Cancelled();
            }
            if (result.Status == EpisodeRunStatus.Completed)
            {
                _progress.Complete(_episode, result.ContinuationState);
                _ctx.SmokeTelemetry?.Emit(
                    "episode.completed",
                    ("contentId", _definition.Id),
                    ("episodeId", _episode.Id));
            }
            return result.Status == EpisodeRunStatus.Failed && result.Error.HasValue
                ? EpisodeRunResult.Failed(WithContext(result.Error.Value))
                : result;
        }

        private void OnChunkReady(int index)
        {
            if (_activeLocation != null)
                _activeLocation.EnableFullQuality().Forget();
            if (_activeCharacter != null)
                _activeCharacter.EnableFullQuality().Forget();
        }

        internal UniTask FlushSaveAsync()
        {
            return _saveSystem?.FlushAsync() ?? UniTask.CompletedTask;
        }

        internal void FlushSaveSynchronously()
        {
            _saveSystem?.FlushSynchronously();
        }

        private void ReportError(Diagnostics.NovelError error)
        {
            _ctx.OnError?.Invoke(WithContext(error));
        }

        private Diagnostics.NovelError WithContext(Diagnostics.NovelError error)
        {
            var context = new Diagnostics.NovelErrorContext(
                _ctx.Bundles.ReleaseId,
                _definition?.Id ?? _ctx.Content.ContentId,
                _episode?.Id ?? string.Empty,
                _ctx.Bundles.DeliveryMode.ToString());
            return error.WithContext(context);
        }
    }
}
