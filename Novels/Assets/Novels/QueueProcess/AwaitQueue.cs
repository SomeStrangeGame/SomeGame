using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public readonly struct AwaitQueue : IQueue
    {
        private readonly Func<float, UniTask> _wait;
        private readonly float _timer;

        public AwaitQueue(Func<float, UniTask> wait, float timer)
        {
            _wait = wait ?? throw new ArgumentNullException(nameof(wait));
            _timer = timer;
        }

        public async UniTask Run(QueueExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.Mode == QueueExecutionMode.Live)
                await _wait(_timer);
        }
    }
}
