namespace Novels.StoryCommands
{
    public sealed class DialogueCommandData
    {
        internal DialogueCommandData(
            string speaker,
            string text,
            StoryContracts.DialoguePresentation presentation,
            StoryContracts.StoryChoiceAction choiceActions,
            StoryContracts.CharacterPresentation character)
        {
            Speaker = speaker ?? string.Empty;
            Text = text ?? string.Empty;
            Presentation = presentation;
            ChoiceActions = choiceActions;
            Character = character;
        }

        public string Speaker { get; }
        public string Text { get; }
        public StoryContracts.DialoguePresentation Presentation { get; }
        public StoryContracts.StoryChoiceAction ChoiceActions { get; }
        public StoryContracts.CharacterPresentation Character { get; }
    }

    public sealed class BackgroundCommandData
    {
        internal BackgroundCommandData(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            AssetName = assetName ?? string.Empty;
            Presentation = presentation;
        }

        public string AssetName { get; }
        public StoryContracts.StoryBackgroundPresentation Presentation { get; }
    }

    public sealed class AudioCommandData
    {
        internal AudioCommandData(string assetName)
        {
            AssetName = assetName ?? string.Empty;
        }

        public string AssetName { get; }
    }

    public sealed class NotificationCommandData
    {
        internal NotificationCommandData(string text)
        {
            Text = text ?? string.Empty;
        }

        public string Text { get; }
    }

    public sealed class CameraCommandData
    {
        internal CameraCommandData(StoryContracts.StoryCameraAction action)
        {
            Action = action;
        }

        public StoryContracts.StoryCameraAction Action { get; }
    }

    public sealed class WaitCommandData
    {
        internal WaitCommandData(int duration)
        {
            Duration = duration;
        }

        public int Duration { get; }
    }
}
