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
            internal Func<Content.NovelDefinition, UniTask<Content.EpisodeDefinition>>
                SelectEpisode;
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
            if (ctx.SelectEpisode == null)
                throw new ArgumentNullException(nameof(ctx.SelectEpisode));
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
        }

        internal async UniTask Init()
        {
            var state = new BootstrapState();
            state.Bundles = _ctx.Bundles;
            state.NovelBundles = state.Bundles.CreateScope().AddTo(this);
            _definition = await LoadContent(state.NovelBundles, _ctx.Content);
            _episode = await _ctx.SelectEpisode(_definition);
            ConfigureMedia(state.Bundles);

            var bootstrap = new NovelBootstrapProcess(
                new NovelBootstrapProcess.Ctx
                {
                    SelectStart = () => PrepareApplication(state),
                    ClearSave = () => state.SaveSystem.Clear(),
                    RunEpisode = () => RunEpisode(state),
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);

            await bootstrap.Run();
        }

        internal UniTask FlushSaveAsync()
        {
            return _saveSystem?.FlushAsync() ?? UniTask.CompletedTask;
        }
    }
}
