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

        public StoryReadResult ReadNext()
        {
            var hasContent = _story.canContinue;
            var source = hasContent
                ? _story.Continue().Trim()
                : string.Empty;
            var choices = GetChoices();

            if (choices.Length > 0)
                return new StoryReadResult(StoryReadStatus.Choices, source, choices);

            if (!hasContent)
                return new StoryReadResult(StoryReadStatus.Completed, source, choices);

            return new StoryReadResult(StoryReadStatus.Content, source, choices);
        }

        private StoryContracts.StoryChoice[] GetChoices()
        {
            var currentChoices = _story.currentChoices;
            var result = new StoryContracts.StoryChoice[currentChoices.Count];

            for (var index = 0; index < currentChoices.Count; index++)
            {
                var choice = currentChoices[index];
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
