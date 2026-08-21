using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public readonly struct WaitOperation : IStoryOperation
    {
        private readonly Func<float, UniTask> _wait;
        private readonly float _timer;

        public WaitOperation(Func<float, UniTask> wait, float timer)
        {
            _wait = wait ?? throw new ArgumentNullException(nameof(wait));
            _timer = timer;
        }

        public async UniTask Run(StoryExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.Mode == QueueExecutionMode.Live)
                await _wait(_timer);
        }
    }
}
