using System;

namespace Novels.StoryCommands
{
    public sealed class StoryStep
    {
        internal StoryStep(StoryCommand command, StoryContracts.StoryChoice[] choices)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Choices = choices ?? Array.Empty<StoryContracts.StoryChoice>();
        }

        public StoryCommand Command { get; }
        public StoryContracts.StoryChoice[] Choices { get; }
    }

    public readonly struct StoryStepResult
    {
        private StoryStepResult(StoryStep step, StoryParseError error, bool isSuccess)
        {
            Step = step;
            Error = error;
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
        public StoryStep Step { get; }
        public StoryParseError Error { get; }

        internal static StoryStepResult Success(StoryStep step)
        {
            return new StoryStepResult(step, default, true);
        }

        internal static StoryStepResult Failure(StoryParseError error)
        {
            return new StoryStepResult(null, error, false);
        }

        internal static StoryStepResult Failure(string code, string message, string source)
        {
            return Failure(new StoryParseError(code, message, source));
        }
    }
}
