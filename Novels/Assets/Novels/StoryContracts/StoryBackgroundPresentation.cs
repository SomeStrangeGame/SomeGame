namespace Novels.StoryContracts
{
    public enum StoryBackgroundType
    {
        Location,
        CutScene,
    }

    public enum StoryBackgroundColor
    {
        Black,
        White,
    }

    public sealed class StoryBackgroundPresentation
    {
        public StoryBackgroundPresentation(
            StoryBackgroundType type,
            StoryBackgroundColor backgroundColor,
            bool keepFinalVideoFrame)
        {
            Type = type;
            BackgroundColor = backgroundColor;
            KeepFinalVideoFrame = keepFinalVideoFrame;
        }

        public StoryBackgroundType Type { get; }
        public StoryBackgroundColor BackgroundColor { get; }
        public bool KeepFinalVideoFrame { get; }
    }
}
