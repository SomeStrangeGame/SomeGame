using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public interface IStoryOperation
    {
        public UniTask Run(StoryExecutionContext context);
    }
}
