using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal sealed class EpisodeRuntime : BaseDisposable
    {
        private readonly CancellationTokenSource _lifetimeCancellation;
        private Func<UniTask<EpisodeRunResult>> _run;
        private Func<UniTask> _flushSave;

        internal EpisodeRuntime(CancellationToken applicationCancellationToken)
        {
            _lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                applicationCancellationToken);
            Scope = new EpisodeScope();
        }

        internal EpisodeScope Scope { get; }
        internal CancellationToken CancellationToken => _lifetimeCancellation.Token;

        internal void Configure(
            Func<UniTask<EpisodeRunResult>> run,
            Func<UniTask> flushSave)
        {
            if (_run != null)
                throw new InvalidOperationException("Episode runtime is already configured.");
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _flushSave = flushSave ?? throw new ArgumentNullException(nameof(flushSave));
        }

        internal async UniTask<EpisodeRunResult> Run()
        {
            if (_run == null)
                throw new InvalidOperationException("Episode runtime is not configured.");

            try
            {
                return await _run();
            }
            finally
            {
                await _flushSave();
            }
        }

        protected override void OnDispose()
        {
            _lifetimeCancellation.Cancel();
            Scope.Dispose();
            _lifetimeCancellation.Dispose();
            base.OnDispose();
        }
    }
}
