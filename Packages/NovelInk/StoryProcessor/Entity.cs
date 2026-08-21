using System;
using Disposable;
using Ink.Runtime;

namespace Novels.StoryProcessor
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string StoryText;
            public string InitialState;
            public string SourceMapText;
        }

        private readonly Ctx _ctx;

        private readonly Story _story;
        private readonly StorySourceMapResolver _sourceMap;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _story = new Story(_ctx.StoryText);
            _sourceMap = new StorySourceMapResolver(_ctx.SourceMapText);
            if (!string.IsNullOrWhiteSpace(_ctx.InitialState))
                _story.state.LoadJson(_ctx.InitialState);
        }

        public string ExportState() => _story.state.ToJson();

        public StoryReadResult ReadNext()
        {
            var hasContent = _story.canContinue;
            var source = hasContent
                ? _story.Continue().Trim()
                : string.Empty;
            var choices = GetChoices();
            var sourceLocation = GetSourceLocation();

            if (choices.Length > 0)
                return new StoryReadResult(
                    StoryReadStatus.Choices,
                    source,
                    choices,
                    sourceLocation);

            if (!hasContent)
                return new StoryReadResult(
                    StoryReadStatus.Completed,
                    source,
                    choices,
                    sourceLocation);

            return new StoryReadResult(
                StoryReadStatus.Content,
                source,
                choices,
                sourceLocation);
        }

        private StorySourceLocation GetSourceLocation()
        {
            var output = _story.state.outputStream;
            for (var index = output.Count - 1; index >= 0; index--)
            {
                var location = _sourceMap.Resolve(output[index].path?.ToString());
                if (location.IsValid)
                    return location;
            }

            var choices = _story.currentChoices;
            for (var index = 0; index < choices.Count; index++)
            {
                var location = _sourceMap.Resolve(choices[index].sourcePath);
                if (location.IsValid)
                    return location;
            }

            return _sourceMap.Resolve(_story.state.currentPointer.path?.ToString());
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
