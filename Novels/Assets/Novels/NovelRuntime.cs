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
            internal Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
            internal Bundles.Entity Bundles;
            internal Catalog.NovelCatalogEntry Content;
            internal string PersistentDataPath;
            internal Camera TargetCamera;
            internal FallbackAssets FallbackAssets;
            internal NovelRuntimeTuning RuntimeTuning;
            internal Func<Content.NovelDefinition, UniTask<Content.EpisodeDefinition>>
                SelectEpisode;
            internal Func<string, UniTask<Bundles.ContentDeliveryLease>>
                PrepareNovelContent;
            internal Func<string, UniTask<Bundles.ContentDeliveryLease>>
                PrepareFullNovelContent;
            internal Action HidePreparationScreen;
        }

        private readonly Dependencies _ctx;
        private readonly PriorityLoader _priorityLoader;
        private Content.NovelDefinition _definition;
        private Content.EpisodeDefinition _episode;
        private AudioMixer _audioMixer;
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
            if (ctx.PrepareFullNovelContent == null)
                throw new ArgumentNullException(nameof(ctx.PrepareFullNovelContent));
            if (ctx.HidePreparationScreen == null)
                throw new ArgumentNullException(nameof(ctx.HidePreparationScreen));
            if (ctx.TargetCamera == null)
                throw new ArgumentNullException(nameof(ctx.TargetCamera));
            if (ctx.FallbackAssets == null)
                throw new ArgumentNullException(nameof(ctx.FallbackAssets));
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
        }

        internal async UniTask<EpisodeRunResult> Init()
        {
            var storyAssets = _ctx.Bundles.CreateScope().AddTo(this);
            (await _ctx.PrepareNovelContent(_ctx.Content.ContentId))?.AddTo(this);
            var previewBundle = ContentAddressing.ContentPackageConvention
                .PreviewBundle(_ctx.Content.ContentId);
            var hasPreview = _ctx.Bundles.HasBundle(previewBundle);
            _assetBundleName = hasPreview
                ? previewBundle
                : _ctx.Content.ContentBundleName;
            StreamingExperimentDiagnostics.SetQuality(
                hasPreview ? "Preview" : "Full");
            _definition = await LoadContent(
                storyAssets,
                _ctx.Content,
                _assetBundleName);
            if (hasPreview && _ctx.Bundles.StreamingPlan != null)
            {
                _streaming = new StoryStreamingController(
                    _ctx.Bundles,
                    storyAssets,
                    _ctx.Bundles.StreamingPlan,
                    _ctx.CancellationToken,
                    _ctx.OnLog,
                    OnChunkReady).AddTo(this);
                _streaming.Start();
            }
            else if (hasPreview)
                ActivateFullContent(storyAssets).Forget();
            _ctx.HidePreparationScreen();
            _progress = new NovelProgress(
                _definition,
                _ctx.PersistentDataPath,
                _ctx.OnLog);
            var playableDefinition = new Content.NovelDefinition(
                _definition.Id,
                _definition.MainCharacter,
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
                _progress.Complete(_episode, result.ContinuationState);
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
            StreamingExperimentDiagnostics.SetQuality($"Chunk {index}");
        }

        private async UniTaskVoid ActivateFullContent(Bundles.Scope storyAssets)
        {
            try
            {
                var lease = await _ctx.PrepareFullNovelContent(_ctx.Content.ContentId);
                lease?.AddTo(this);
                await storyAssets.GetAssetBundle(_definition.BundleName);
                _assetBundleName = _definition.BundleName;
                if (_activeLocation != null)
                    await _activeLocation.EnableFullQuality();
                StreamingExperimentDiagnostics.SetQuality("Full");
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"Full-quality content prefetch failed: {exception.Message}"));
            }
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
