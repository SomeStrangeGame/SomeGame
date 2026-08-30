using System;

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

    public abstract class StoryCommand
    {
        protected StoryCommand(StoryCommandType type, string source)
        {
            Type = type;
            Source = source ?? string.Empty;
        }

        public StoryCommandType Type { get; }
        public string Source { get; }

        internal static StoryCommand CreateEmpty(string source)
        {
            return new EmptyStoryCommand(source);
        }

        internal static StoryCommand CreateMetadata(string source)
        {
            return new MetadataStoryCommand(source);
        }

        internal static StoryCommand CreateKeyboard(string source)
        {
            return new KeyboardStoryCommand(source);
        }

        internal static StoryCommand CreateDialogue(
            string source,
            string speaker,
            string text,
            StoryContracts.DialoguePresentation presentation,
            StoryContracts.StoryChoiceAction choiceActions,
            string choiceConfirmationText,
            StoryContracts.CharacterPresentation character)
        {
            return new DialogueStoryCommand(
                source,
                new DialogueCommandData(
                    speaker,
                    text,
                    presentation,
                    choiceActions,
                    choiceConfirmationText,
                    character,
                    StoryContracts.StorySpeakers.IsWardrobe(speaker, out var target)
                        ? target
                        : string.Empty));
        }

        internal static StoryCommand CreateBackground(
            StoryCommandType type,
            string source,
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation)
        {
            if (type != StoryCommandType.Location
                && type != StoryCommandType.CutScene)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "The command type does not use background data.");
            }

            return new BackgroundStoryCommand(
                type,
                source,
                new BackgroundCommandData(assetName, presentation));
        }

        internal static StoryCommand CreateAudio(
            StoryCommandType type,
            string source,
            string assetName)
        {
            if (type != StoryCommandType.Music
                && type != StoryCommandType.Sound
                && type != StoryCommandType.Ambient)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "The command type does not use audio data.");
            }

            return new AudioStoryCommand(
                type,
                source,
                new AudioCommandData(assetName));
        }

        internal static StoryCommand CreateNotification(string source, string text)
        {
            return new NotificationStoryCommand(
                source,
                new NotificationCommandData(text));
        }

        internal static StoryCommand CreateCamera(
            string source,
            StoryContracts.StoryCameraAction action)
        {
            return new CameraStoryCommand(
                source,
                new CameraCommandData(action));
        }

        internal static StoryCommand CreateWait(string source, int duration)
        {
            return new WaitStoryCommand(source, new WaitCommandData(duration));
        }
    }

    public sealed class EmptyStoryCommand : StoryCommand
    {
        internal EmptyStoryCommand(string source)
            : base(StoryCommandType.Empty, source)
        {
        }
    }

    public sealed class MetadataStoryCommand : StoryCommand
    {
        internal MetadataStoryCommand(string source)
            : base(StoryCommandType.Metadata, source)
        {
        }
    }

    public sealed class KeyboardStoryCommand : StoryCommand
    {
        internal KeyboardStoryCommand(string source)
            : base(StoryCommandType.Keyboard, source)
        {
        }
    }

    public sealed class DialogueStoryCommand : StoryCommand
    {
        internal DialogueStoryCommand(string source, DialogueCommandData data)
            : base(StoryCommandType.Dialogue, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public DialogueCommandData Data { get; }
    }

    public sealed class BackgroundStoryCommand : StoryCommand
    {
        internal BackgroundStoryCommand(
            StoryCommandType type,
            string source,
            BackgroundCommandData data)
            : base(type, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public BackgroundCommandData Data { get; }
    }

    public sealed class AudioStoryCommand : StoryCommand
    {
        internal AudioStoryCommand(
            StoryCommandType type,
            string source,
            AudioCommandData data)
            : base(type, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public AudioCommandData Data { get; }
    }

    public sealed class NotificationStoryCommand : StoryCommand
    {
        internal NotificationStoryCommand(string source, NotificationCommandData data)
            : base(StoryCommandType.Notification, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public NotificationCommandData Data { get; }
    }

    public sealed class CameraStoryCommand : StoryCommand
    {
        internal CameraStoryCommand(string source, CameraCommandData data)
            : base(StoryCommandType.Camera, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public CameraCommandData Data { get; }
    }

    public sealed class WaitStoryCommand : StoryCommand
    {
        internal WaitStoryCommand(string source, WaitCommandData data)
            : base(StoryCommandType.Wait, source)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public WaitCommandData Data { get; }
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
        private StoryParseResult(
            StoryCommand command,
            StoryParseError error,
            bool isSuccess)
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

        internal static StoryParseResult Failure(
            string code,
            string message,
            string source)
        {
            return new StoryParseResult(
                null,
                new StoryParseError(code, message, source),
                false);
        }
    }
}
