using System.Collections.Generic;
using Disposable;
using Ink.Runtime;

namespace Novels.StoryProcessor
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string StoryText;
        }

        private readonly Ctx _ctx;

        private readonly Story _story;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _story = new Story(_ctx.StoryText);
        }

        public string GetNextText()
        {
            if (!_story.canContinue) return string.Empty;
            
            return _story.Continue().Trim();
        }

        public List<Choice> GetChoices()
        {
            return _story.currentChoices;
        }

        public void SetChoice(int index)
        {
            _story.ChooseChoiceIndex (index);
        }
    }
}

