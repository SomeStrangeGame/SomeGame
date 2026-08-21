using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public sealed class StoryOperationExecutor
    {
        public async UniTask Run(
            Queue<IStoryOperation> queue,
            StoryContracts.StoryDecision? savedDecision,
            CancellationToken cancellationToken)
        {
            var context = savedDecision.HasValue
                ? StoryExecutionContext.Replay(savedDecision.Value, cancellationToken)
                : StoryExecutionContext.Live(cancellationToken);

            while (queue.TryDequeue(out var element))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await element.Run(context);
            }
        }
    }
}
