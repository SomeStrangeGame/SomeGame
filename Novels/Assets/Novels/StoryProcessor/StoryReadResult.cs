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

        public StoryReadResult(
            StoryReadStatus status,
            string source,
            StoryContracts.StoryChoice[] choices)
        {
            Status = status;
            Source = source;
            Choices = choices;
        }
    }
}
