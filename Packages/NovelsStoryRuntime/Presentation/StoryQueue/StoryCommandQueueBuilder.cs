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

                case StoryCommands.AnalyticsEndingStoryCommand ending:
                    return new StoryExecution.DelegateStoryOperation(context =>
                    {
                        if (context.Mode == StoryExecution.QueueExecutionMode.Live)
                            _dependencies.OnEndingReached?.Invoke(ending.Data.EndingId);
                        return Cysharp.Threading.Tasks.UniTask.CompletedTask;
                    });

                case StoryCommands.BackgroundStoryCommand background:
                    return new StoryExecution.DelegateStoryOperation(context =>
                        _dependencies.Location.SetImage(
                            background.Data.AssetName,
                            background.Data.Presentation,
                            context.PresentationMode));

                case StoryCommands.AudioStoryCommand audio:
                    return new StoryExecution.DelegateStoryOperation(
                        _ => _dependencies.PlayAudio?.Invoke(audio.Data.AssetName, audio.Type)
                            ?? Cysharp.Threading.Tasks.UniTask.CompletedTask);

                case StoryCommands.CameraStoryCommand camera:
                    return new StoryExecution.DelegateStoryOperation(context =>
                        _dependencies.Location.SetCamera(
                            camera.Data.Action,
                            context.PresentationMode));

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
