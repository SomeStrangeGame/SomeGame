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

        private Story _story;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _story = new Story(_ctx.StoryText);
        }

        public bool TryGetNextText(out string text)
        {
            text = string.Empty;
            if (!_story.canContinue) return false;
            
            text = _story.Continue().Trim();
            return true;
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

