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
            internal Func<
                Content.NovelDefinition,
                Content.EpisodeDefinition,
                UniTask<Bundles.ContentDeliveryLease>> PrepareEpisodeContent;
        }

        private readonly Dependencies _ctx;
        private readonly PriorityLoader _priorityLoader;
        private Content.NovelDefinition _definition;
        private Content.EpisodeDefinition _episode;
        private AudioMixer _audioMixer;
        private Save.SaveSystem _saveSystem;
        private NovelProgress _progress;

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
            if (ctx.PrepareEpisodeContent == null)
                throw new ArgumentNullException(nameof(ctx.PrepareEpisodeContent));
            if (ctx.PrepareNovelContent == null)
                throw new ArgumentNullException(nameof(ctx.PrepareNovelContent));
            if (ctx.TargetCamera == null)
                throw new ArgumentNullException(nameof(ctx.TargetCamera));
            if (ctx.FallbackAssets == null)
                throw new ArgumentNullException(nameof(ctx.FallbackAssets));
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
        }

        internal async UniTask<EpisodeRunResult> Init()
        {
            var novelSession = new NovelSession(_ctx.Bundles.CreateScope()).AddTo(this);
            novelSession.AttachDelivery(await _ctx.PrepareNovelContent(
                _ctx.Content.ContentId));
            _definition = await LoadContent(novelSession.Bundles, _ctx.Content);
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
            episodeRuntime.AttachDelivery(await _ctx.PrepareEpisodeContent(
                _definition,
                _episode));

            var bootstrap = new NovelBootstrapProcess(
                new NovelBootstrapProcess.Dependencies
                {
                    Prepare = () => PrepareApplication(
                        novelSession.Bundles,
                        episodeRuntime),
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);

            var result = await bootstrap.Run();
            if (result.Status == EpisodeRunStatus.Completed)
                _progress.Complete(_episode, result.ContinuationState);
            return result.Status == EpisodeRunStatus.Failed && result.Error.HasValue
                ? EpisodeRunResult.Failed(WithContext(result.Error.Value))
                : result;
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
