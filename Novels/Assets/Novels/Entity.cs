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
        private const string _screenAssetName = "Screen";
        private const string _localizationDataAssetName = "LocalizationData";
        private const string _contentBundleName = "novels_content";
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        internal struct Ctx
        {
            internal string ContentId;
            internal CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;
        private readonly PriorityLoader _priorityLoader;
        private Content.NovelDefinition _definition;
        private AudioMixer _audioMixer;
        private Save.Entity _saveSystem;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            if (string.IsNullOrWhiteSpace(ctx.ContentId))
                throw new ArgumentException("Content ID is required.", nameof(ctx.ContentId));
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var state = new BootstrapState();
            state.Bundles = CreateBundles();
            _definition = await LoadContent(state.Bundles);
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
