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
                    return StoryExecution.DelegateStoryOperation.Empty();

                case StoryCommands.NotificationStoryCommand notification:
                    return new StoryExecution.DelegateStoryOperation(context =>
                    {
                        if (context.Mode == StoryExecution.QueueExecutionMode.Live)
                            _ctx.ShowNotification(notification.Data.Text);
                        return Cysharp.Threading.Tasks.UniTask.CompletedTask;
                    });

                case StoryCommands.BackgroundStoryCommand background:
                    return new StoryExecution.BackgroundOperation.SetBackgroundQueue(
                        _ctx.Location.SetImage,
                        background.Data.AssetName,
                        background.Data.Presentation);

                case StoryCommands.AudioStoryCommand audio:
                    var playAudio = audio.Type == StoryCommands.StoryCommandType.Music
                        ? _ctx.Audio.PlayMusic
                        : audio.Type == StoryCommands.StoryCommandType.Sound
                            ? _ctx.Audio.PlaySound
                            : _ctx.Audio.PlayAmbient;
                    return new StoryExecution.DelegateStoryOperation(
                        _ => playAudio(audio.Data.AssetName));

                case StoryCommands.CameraStoryCommand camera:
                    return new StoryExecution.BackgroundOperation.CameraQueue(
                        _ctx.Location.SetCamera,
                        camera.Data.Action);

                case StoryCommands.WaitStoryCommand wait:
                    return new StoryExecution.DelegateStoryOperation(context =>
                        context.Mode == StoryExecution.QueueExecutionMode.Live
                            ? _ctx.Location.Wait(wait.Data.Duration)
                            : Cysharp.Threading.Tasks.UniTask.CompletedTask);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(command),
                        command.GetType().FullName,
                        "The command is not supported by the story command queue builder.");
            }
        }
    }
}
