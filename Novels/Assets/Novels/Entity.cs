using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Audio;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Novels
{
    internal partial class Entity : BaseDisposable
    {
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        internal struct Ctx
        {
            internal CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
            internal Bundles.Entity Bundles;
            internal Catalog.NovelCatalogEntry Content;
            internal string Locale;
            internal string PersistentDataPath;
            internal Camera TargetCamera;
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

        private readonly Ctx _ctx;
        private readonly PriorityLoader _priorityLoader;
        private Content.NovelDefinition _definition;
        private Content.EpisodeDefinition _episode;
        private AudioMixer _audioMixer;
        private Save.Entity _saveSystem;

        internal Entity(Ctx ctx)
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
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
        }

        internal async UniTask<EpisodeRunResult> Init()
        {
            var novelSession = new NovelSession(_ctx.Bundles.CreateScope()).AddTo(this);
            novelSession.AttachDelivery(await _ctx.PrepareNovelContent(
                _ctx.Content.ContentId));
            _definition = await LoadContent(novelSession.Bundles, _ctx.Content);
            _episode = await _ctx.SelectEpisode(_definition);
            var episodeRuntime = CreateEpisodeRuntime().AddTo(this);
            episodeRuntime.AttachDelivery(await _ctx.PrepareEpisodeContent(
                _definition,
                _episode));

            var bootstrap = new NovelBootstrapProcess(
                new NovelBootstrapProcess.Ctx
                {
                    Prepare = () => PrepareApplication(
                        novelSession.Bundles,
                        episodeRuntime),
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);

            var result = await bootstrap.Run();
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
