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

        public StoryContracts.StoryChoice[] GetChoices()
        {
            var choices = _story.currentChoices;
            var result = new StoryContracts.StoryChoice[choices.Count];

            for (var index = 0; index < choices.Count; index++)
            {
                var choice = choices[index];
                result[index] = new StoryContracts.StoryChoice(choice.index, choice.text);
            }

            return result;
        }

        public void SetChoice(int index)
        {
            _story.ChooseChoiceIndex (index);
        }
    }
}
