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
            switch (command)
            {
                case StoryCommands.EmptyStoryCommand:
                case StoryCommands.MetadataStoryCommand:
                case StoryCommands.KeyboardStoryCommand:
                    return new QueueProcess.EmptyQueue();

                case StoryCommands.NotificationStoryCommand notification:
                    return new QueueProcess.NotificationQueue(
                        _ctx.ShowNotification,
                        notification.Data.Text);

                case StoryCommands.BackgroundStoryCommand background:
                    return new QueueProcess.BackgroundQueue.SetBackgroundQueue(
                        _ctx.SetImage,
                        _ctx.SetImageImmediate,
                        background.Data.AssetName,
                        background.Data.Presentation);

                case StoryCommands.AudioStoryCommand audio:
                    var playAudio = audio.Type == StoryCommands.StoryCommandType.Music
                        ? _ctx.PlayMusic
                        : audio.Type == StoryCommands.StoryCommandType.Sound
                            ? _ctx.PlaySound
                            : _ctx.PlayAmbient;
                    return new QueueProcess.AudioQueue(
                        playAudio,
                        audio.Data.AssetName);

                case StoryCommands.CameraStoryCommand camera:
                    return new QueueProcess.BackgroundQueue.CameraQueue(
                        _ctx.SetCamera,
                        _ctx.SetCameraImmediate,
                        camera.Data.Action);

                case StoryCommands.WaitStoryCommand wait:
                    return new QueueProcess.AwaitQueue(
                        _ctx.Wait,
                        wait.Data.Duration);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(command),
                        command.GetType().FullName,
                        "The command is not supported by the story command queue builder.");
            }
        }
    }
}
