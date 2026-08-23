namespace Novels.Location
{
    internal enum BackgroundPlaybackKind
    {
        StaticImage,
        LoopingVideo,
        CutScene,
        CutSceneWithFinalFrame,
    }

    internal readonly struct BackgroundPresentationPlan
    {
        private BackgroundPresentationPlan(
            string assetName,
            StoryContracts.StoryBackgroundColor backgroundColor,
            BackgroundPlaybackKind playback)
        {
            AssetName = assetName;
            BackgroundColor = backgroundColor;
            Playback = playback;
        }

        internal string AssetName { get; }
        internal StoryContracts.StoryBackgroundColor BackgroundColor { get; }
        internal BackgroundPlaybackKind Playback { get; }

        internal bool UsesVideo => Playback != BackgroundPlaybackKind.StaticImage;
        internal bool IsCutScene =>
            Playback == BackgroundPlaybackKind.CutScene
            || Playback == BackgroundPlaybackKind.CutSceneWithFinalFrame;
        internal bool KeepsFinalVideoFrame =>
            Playback == BackgroundPlaybackKind.CutSceneWithFinalFrame;

        internal static BackgroundPresentationPlan Create(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            bool hasVideo)
        {
            var playback = BackgroundPlaybackKind.StaticImage;
            if (hasVideo)
            {
                playback = presentation.Type == StoryContracts.StoryBackgroundType.CutScene
                    ? presentation.KeepFinalVideoFrame
                        ? BackgroundPlaybackKind.CutSceneWithFinalFrame
                        : BackgroundPlaybackKind.CutScene
                    : BackgroundPlaybackKind.LoopingVideo;
            }
            return new BackgroundPresentationPlan(
                assetName,
                presentation.BackgroundColor,
                playback);
        }
    }
}
