using System;

namespace Novels.StoryQueue
{
    internal sealed class StoryCommandQueueBuilder
    {
        private readonly Entity.CommandCtx _ctx;

        internal StoryCommandQueueBuilder(Entity.CommandCtx ctx)
        {
            _ctx = ctx;
        }

        internal QueueProcess.IQueue Build(StoryCommands.StoryCommand command)
        {
            switch (command.Type)
            {
                case StoryCommands.StoryCommandType.Empty:
                case StoryCommands.StoryCommandType.Metadata:
                case StoryCommands.StoryCommandType.Keyboard:
                    return new QueueProcess.EmptyQueue();

                case StoryCommands.StoryCommandType.Notification:
                    return new QueueProcess.NotificationQueue(
                        _ctx.ShowNotification,
                        command.Notification.Text);

                case StoryCommands.StoryCommandType.Location:
                case StoryCommands.StoryCommandType.CutScene:
                    return new QueueProcess.BackgroundQueue.SetBackgroundQueue(
                        _ctx.SetImage,
                        _ctx.SetImageImmediate,
                        command.Background.AssetName,
                        command.Background.Presentation);

                case StoryCommands.StoryCommandType.Music:
                case StoryCommands.StoryCommandType.Sound:
                case StoryCommands.StoryCommandType.Ambient:
                    var playAudio = command.Type == StoryCommands.StoryCommandType.Music
                        ? _ctx.PlayMusic
                        : command.Type == StoryCommands.StoryCommandType.Sound
                            ? _ctx.PlaySound
                            : _ctx.PlayAmbient;
                    return new QueueProcess.AudioQueue(
                        playAudio,
                        command.Audio.AssetName);

                case StoryCommands.StoryCommandType.Camera:
                    return new QueueProcess.BackgroundQueue.CameraQueue(
                        _ctx.SetCamera,
                        _ctx.SetCameraImmediate,
                        command.Camera.Action);

                case StoryCommands.StoryCommandType.Wait:
                    return new QueueProcess.AwaitQueue(
                        _ctx.Wait,
                        command.Wait.Duration);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(command.Type),
                        command.Type,
                        "The command is not supported by the story command queue builder.");
            }
        }
    }
}
