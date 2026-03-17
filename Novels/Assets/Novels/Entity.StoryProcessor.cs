using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private StoryProcessor.Entity CreateStoryProcessor(string storyText)
        {
            return new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
            }).AddTo(this);
        }
    }
}

