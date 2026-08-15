using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public sealed class Executor
    {
        public async UniTask Run(
            Queue<IQueue> queue,
            byte? savedChoice,
            CancellationToken cancellationToken)
        {
            var context = savedChoice.HasValue
                ? QueueExecutionContext.Replay(savedChoice.Value, cancellationToken)
                : QueueExecutionContext.Live(cancellationToken);

            while (queue.TryDequeue(out var element))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await element.Run(context);
            }
        }
    }
}
