namespace Novels.StoryProcessor
{
    public enum StoryReadStatus
    {
        Content,
        Choices,
        Completed,
    }

    public readonly struct StoryReadResult
    {
        public readonly StoryReadStatus Status;
        public readonly string Source;
        public readonly StoryContracts.StoryChoice[] Choices;
        public readonly StorySourceLocation SourceLocation;

        public StoryReadResult(
            StoryReadStatus status,
            string source,
            StoryContracts.StoryChoice[] choices,
            StorySourceLocation sourceLocation = default)
        {
            Status = status;
            Source = source;
            Choices = choices;
            SourceLocation = sourceLocation;
        }
    }
}
