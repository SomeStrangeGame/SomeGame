using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public readonly struct EmptyQueue : IQueue
    {
        public UniTask Run(QueueExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            return UniTask.CompletedTask;
        }
    }
}
