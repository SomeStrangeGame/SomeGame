using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal sealed class NovelBootstrapProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal Func<UniTask<SettingSelection>> SelectStart;
            internal Action ClearSave;
            internal Func<UniTask> RunEpisode;
            internal CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;

        internal NovelBootstrapProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask Run()
        {
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            var selection = await _ctx.SelectStart();
            if (selection == SettingSelection.NewGame)
                _ctx.ClearSave();

            _ctx.CancellationToken.ThrowIfCancellationRequested();
            await _ctx.RunEpisode();
        }
    }
}
