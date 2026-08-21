using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private StoryProcessor.Entity CreateStoryProcessor(
            IBaseDisposable owner,
            string storyText,
            string initialState,
            string sourceMapText)
        {
            return new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
                InitialState = initialState,
                SourceMapText = sourceMapText,
            }).AddTo(owner);
        }
    }
}
