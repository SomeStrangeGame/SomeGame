using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public sealed class Executor
    {
        public async UniTask Run(
            Queue<IQueue> queue,
            byte? savedChoice)
        {
            var context = savedChoice.HasValue
                ? QueueExecutionContext.Replay(savedChoice.Value)
                : QueueExecutionContext.Live();

            while (queue.TryDequeue(out var element))
                await element.Run(context);
        }
    }
}
