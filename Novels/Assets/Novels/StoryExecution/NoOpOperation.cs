using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public readonly struct NoOpOperation : IStoryOperation
    {
        public UniTask Run(StoryExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            return UniTask.CompletedTask;
        }
    }
}
