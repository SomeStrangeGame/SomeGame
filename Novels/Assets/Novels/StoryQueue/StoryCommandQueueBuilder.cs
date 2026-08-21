using System;

namespace Novels.StoryQueue
{
    internal sealed class StoryCommandQueueBuilder
    {
        private readonly StoryQueueBuilder.CommandCtx _ctx;

        internal StoryCommandQueueBuilder(StoryQueueBuilder.CommandCtx ctx)
        {
            _ctx = ctx;
        }

        internal StoryExecution.IStoryOperation Build(StoryCommands.StoryCommand command)
        {
            switch (command)
            {
                case StoryCommands.EmptyStoryCommand:
                case StoryCommands.MetadataStoryCommand:
                case StoryCommands.KeyboardStoryCommand:
                    return new StoryExecution.NoOpOperation();

                case StoryCommands.NotificationStoryCommand notification:
                    return new StoryExecution.NotificationOperation(
                        _ctx.ShowNotification,
                        notification.Data.Text);

                case StoryCommands.BackgroundStoryCommand background:
                    return new StoryExecution.BackgroundOperation.SetBackgroundQueue(
                        _ctx.Location.SetImage,
                        _ctx.Location.SetImageImmediate,
                        background.Data.AssetName,
                        background.Data.Presentation);

                case StoryCommands.AudioStoryCommand audio:
                    var playAudio = audio.Type == StoryCommands.StoryCommandType.Music
                        ? _ctx.Audio.PlayMusic
                        : audio.Type == StoryCommands.StoryCommandType.Sound
                            ? _ctx.Audio.PlaySound
                            : _ctx.Audio.PlayAmbient;
                    return new StoryExecution.PlayAudioOperation(
                        playAudio,
                        audio.Data.AssetName);

                case StoryCommands.CameraStoryCommand camera:
                    return new StoryExecution.BackgroundOperation.CameraQueue(
                        _ctx.Location.SetCamera,
                        _ctx.Location.SetCameraImmediate,
                        camera.Data.Action);

                case StoryCommands.WaitStoryCommand wait:
                    return new StoryExecution.WaitOperation(
                        _ctx.Location.Wait,
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
