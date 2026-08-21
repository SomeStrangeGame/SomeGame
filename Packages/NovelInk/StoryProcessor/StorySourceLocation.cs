namespace Novels.StoryProcessor
{
    public readonly struct StorySourceLocation
    {
        public StorySourceLocation(string fileName, int lineNumber)
        {
            FileName = fileName ?? string.Empty;
            LineNumber = lineNumber;
        }

        public string FileName { get; }
        public int LineNumber { get; }
        public bool IsValid => LineNumber > 0;

        public override string ToString() => IsValid
            ? $"{FileName}:{LineNumber}"
            : string.Empty;
    }
}
