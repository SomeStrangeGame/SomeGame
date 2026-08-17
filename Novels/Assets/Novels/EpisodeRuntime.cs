using System;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal sealed class EpisodeRuntime : BaseDisposable
    {
        private Func<UniTask> _run;
        private Func<UniTask> _flushSave;

        internal EpisodeRuntime()
        {
            Scope = new EpisodeScope().AddTo(this);
        }

        internal EpisodeScope Scope { get; }

        internal void Configure(Func<UniTask> run, Func<UniTask> flushSave)
        {
            if (_run != null)
                throw new InvalidOperationException("Episode runtime is already configured.");
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _flushSave = flushSave ?? throw new ArgumentNullException(nameof(flushSave));
        }

        internal async UniTask Run()
        {
            if (_run == null)
                throw new InvalidOperationException("Episode runtime is not configured.");

            try
            {
                await _run();
            }
            finally
            {
                await _flushSave();
            }
        }
    }
}
