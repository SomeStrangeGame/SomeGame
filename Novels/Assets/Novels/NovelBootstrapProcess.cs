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
            internal Func<UniTask<NovelStartSession>> Prepare;
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
            var session = await _ctx.Prepare();
            if (session.Selection == SettingSelection.NewGame)
                session.ClearSave();

            _ctx.CancellationToken.ThrowIfCancellationRequested();
            await session.RunEpisode();
        }
    }

    internal readonly struct NovelStartSession
    {
        internal NovelStartSession(
            SettingSelection selection,
            Action clearSave,
            Func<UniTask> runEpisode)
        {
            Selection = selection;
            ClearSave = clearSave ?? throw new ArgumentNullException(nameof(clearSave));
            RunEpisode = runEpisode ?? throw new ArgumentNullException(nameof(runEpisode));
        }

        internal SettingSelection Selection { get; }
        internal Action ClearSave { get; }
        internal Func<UniTask> RunEpisode { get; }
    }
}
