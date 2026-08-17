using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Novels
{
    internal partial class Entity : BaseDisposable
    {
        private const string _screenAssetName = "Screen";
        private const string _localizationDataAssetName = "LocalizationData";
        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        internal struct Ctx
        {
            internal Content.NovelContentAsset Content;
            internal CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            internal Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;
        private readonly Content.NovelDefinition _definition;
        private readonly PriorityLoader _priorityLoader;
        private Save.Entity _saveSystem;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            if (ctx.Content == null)
                throw new ArgumentNullException(nameof(ctx.Content));

            _definition = ctx.Content.ToDefinition();
            _priorityLoader = new PriorityLoader(_defaultThreadPriority);
            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var state = new BootstrapState();
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
