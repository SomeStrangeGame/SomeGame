using System;

namespace Novels.StoryContracts
{
    public static class StoryBackgroundAssets
    {
        public const string Darkness = "Темнота";
        public const string BlackScreen = "Чёрный экран";

        public static bool IsSolidBlack(string assetName) =>
            string.Equals(assetName?.Trim(), Darkness, StringComparison.OrdinalIgnoreCase)
            || string.Equals(assetName?.Trim(), BlackScreen, StringComparison.OrdinalIgnoreCase);
    }

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
