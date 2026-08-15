namespace Novels.StoryCommands
{
    public enum StoryCommandType
    {
        Empty,
        Metadata,
        Keyboard,
        Notification,
        Location,
        CutScene,
        Music,
        Sound,
        Ambient,
        Camera,
        Wait,
        Dialogue,
    }

    public sealed class StoryCommand
    {
        internal StoryCommand(
            StoryCommandType type,
            string source,
            string name,
            string value,
            string[] arguments,
            int waitDuration = 0)
        {
            Type = type;
            Source = source;
            Name = name;
            Value = value;
            Arguments = arguments;
            WaitDuration = waitDuration;
        }

        public StoryCommandType Type { get; }
        public string Source { get; }
        public string Name { get; }
        public string Value { get; }
        public string[] Arguments { get; }
        public int WaitDuration { get; }
    }

    public readonly struct StoryParseError
    {
        internal StoryParseError(string code, string message, string source)
        {
            Code = code;
            Message = message;
            Source = source;
        }

        public string Code { get; }
        public string Message { get; }
        public string Source { get; }
    }

    public readonly struct StoryParseResult
    {
        private StoryParseResult(StoryCommand command, StoryParseError error, bool isSuccess)
        {
            Command = command;
            Error = error;
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
        public StoryCommand Command { get; }
        public StoryParseError Error { get; }

        internal static StoryParseResult Success(StoryCommand command)
        {
            return new StoryParseResult(command, default, true);
        }

        internal static StoryParseResult Failure(string code, string message, string source)
        {
            return new StoryParseResult(null, new StoryParseError(code, message, source), false);
        }
    }
}
