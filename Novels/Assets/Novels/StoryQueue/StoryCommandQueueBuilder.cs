using System;

namespace Novels.StoryQueue
{
    internal sealed class StoryCommandQueueBuilder
    {
        private readonly StoryQueueBuilder.Dependencies _dependencies;

        internal StoryCommandQueueBuilder(StoryQueueBuilder.Dependencies dependencies)
        {
            _dependencies = dependencies;
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
                            _dependencies.Notification.Enqueue(notification.Data.Text);
                        return Cysharp.Threading.Tasks.UniTask.CompletedTask;
                    });

                case StoryCommands.BackgroundStoryCommand background:
                    return new StoryExecution.BackgroundOperation.SetBackgroundQueue(
                        _dependencies.Location.SetImage,
                        background.Data.AssetName,
                        background.Data.Presentation);

                case StoryCommands.AudioStoryCommand audio:
                    var audioType = audio.Type == StoryCommands.StoryCommandType.Music
                        ? Audio.AudioController.Audio.Music
                        : audio.Type == StoryCommands.StoryCommandType.Sound
                            ? Audio.AudioController.Audio.Sound
                            : Audio.AudioController.Audio.Ambient;
                    return new StoryExecution.DelegateStoryOperation(
                        _ => _dependencies.Audio.PlayAudio(audio.Data.AssetName, audioType));

                case StoryCommands.CameraStoryCommand camera:
                    return new StoryExecution.BackgroundOperation.CameraQueue(
                        _dependencies.Location.SetCamera,
                        camera.Data.Action);

                case StoryCommands.WaitStoryCommand wait:
                    return new StoryExecution.DelegateStoryOperation(context =>
                        context.Mode == StoryExecution.QueueExecutionMode.Live
                            ? _dependencies.Wait(wait.Data.Duration)
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
