using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public sealed class Executor
    {
        public async UniTask Run(
            Queue<IQueue> queue,
            StoryContracts.StoryDecision? savedDecision,
            CancellationToken cancellationToken)
        {
            var context = savedDecision.HasValue
                ? QueueExecutionContext.Replay(savedDecision.Value, cancellationToken)
                : QueueExecutionContext.Live(cancellationToken);

            while (queue.TryDequeue(out var element))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await element.Run(context);
            }
        }
    }
}
