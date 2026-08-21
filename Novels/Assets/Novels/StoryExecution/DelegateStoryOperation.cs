using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public readonly struct DelegateStoryOperation : IStoryOperation
    {
        private readonly Func<StoryExecutionContext, UniTask> _run;

        public DelegateStoryOperation(Func<StoryExecutionContext, UniTask> run)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
        }

        public UniTask Run(StoryExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            return _run(context);
        }

        public static DelegateStoryOperation Empty() =>
            new(_ => UniTask.CompletedTask);
    }
}
