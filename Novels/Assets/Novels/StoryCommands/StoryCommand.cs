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

    public sealed class StoryCommand
    {
        private StoryCommand(
            StoryCommandType type,
            string source,
            DialogueCommandData dialogue = null,
            BackgroundCommandData background = null,
            AudioCommandData audio = null,
            NotificationCommandData notification = null,
            CameraCommandData camera = null,
            WaitCommandData wait = null)
        {
            Type = type;
            Source = source ?? string.Empty;
            Dialogue = dialogue;
            Background = background;
            Audio = audio;
            Notification = notification;
            Camera = camera;
            Wait = wait;
        }

        public StoryCommandType Type { get; }
        public string Source { get; }
        public DialogueCommandData Dialogue { get; }
        public BackgroundCommandData Background { get; }
        public AudioCommandData Audio { get; }
        public NotificationCommandData Notification { get; }
        public CameraCommandData Camera { get; }
        public WaitCommandData Wait { get; }

        internal static StoryCommand CreateEmpty(string source)
        {
            return new StoryCommand(StoryCommandType.Empty, source);
        }

        internal static StoryCommand CreateMetadata(string source)
        {
            return new StoryCommand(StoryCommandType.Metadata, source);
        }

        internal static StoryCommand CreateKeyboard(string source)
        {
            return new StoryCommand(StoryCommandType.Keyboard, source);
        }

        internal static StoryCommand CreateDialogue(
            string source,
            string speaker,
            string text,
            StoryContracts.DialoguePresentation presentation,
            StoryContracts.StoryChoiceAction choiceActions,
            StoryContracts.CharacterPresentation character)
        {
            return new StoryCommand(
                StoryCommandType.Dialogue,
                source,
                dialogue: new DialogueCommandData(
                    speaker,
                    text,
                    presentation,
                    choiceActions,
                    character));
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
                throw new ArgumentOutOfRangeException(nameof(type), type, "The command type does not use background data.");
            }

            return new StoryCommand(
                type,
                source,
                background: new BackgroundCommandData(assetName, presentation));
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
                throw new ArgumentOutOfRangeException(nameof(type), type, "The command type does not use audio data.");
            }

            return new StoryCommand(type, source, audio: new AudioCommandData(assetName));
        }

        internal static StoryCommand CreateNotification(string source, string text)
        {
            return new StoryCommand(
                StoryCommandType.Notification,
                source,
                notification: new NotificationCommandData(text));
        }

        internal static StoryCommand CreateCamera(
            string source,
            StoryContracts.StoryCameraAction action)
        {
            return new StoryCommand(
                StoryCommandType.Camera,
                source,
                camera: new CameraCommandData(action));
        }

        internal static StoryCommand CreateWait(string source, int duration)
        {
            return new StoryCommand(
                StoryCommandType.Wait,
                source,
                wait: new WaitCommandData(duration));
        }
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
