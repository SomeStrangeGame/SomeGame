using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private static StoryProcessor.Entity CreateStoryProcessor(
            IBaseDisposable owner,
            string storyText,
            string initialState)
        {
            return new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
                InitialState = initialState,
            }).AddTo(owner);
        }

        private StoryQueue.StoryQueueBuilder CreateStoryQueue(
            StoryProcessor.Entity storyProcessor,
            EpisodePresentation presentation,
            CancellationToken cancellationToken,
            System.Func<string, Cysharp.Threading.Tasks.UniTask<UnityEngine.Sprite>> loadChooseThumbnail,
            Save.SaveSystem save)
        {
            return new StoryQueue.StoryQueueBuilder(
                new StoryQueue.StoryQueueBuilder.Dependencies
                {
                    MainCharacter = _definition.MainCharacter,
                    Notification = presentation.Notification,
                    Location = presentation.Location,
                    Audio = presentation.Audio,
                    Bubble = presentation.Bubble,
                    Wardrobe = presentation.Wardrobe,
                    Choose = presentation.Choose,
                    Character = presentation.Character,
                    Save = save,
                    Story = storyProcessor,
                    Wait = seconds => Wait(seconds, cancellationToken),
                    LoadChooseThumbnail = loadChooseThumbnail,
                    OnDialogueReady = (presentationKind, choiceCount) =>
                        _ctx.SmokeTelemetry?.Emit(
                            "dialogue.ready",
                            ("contentId", _definition.Id),
                            ("episodeId", _episode.Id),
                            ("presentation", presentationKind),
                            ("choiceCount", choiceCount.ToString())),
                    OnChoiceSelected = choiceId => _ctx.SmokeTelemetry?.Emit(
                        "choice.selected",
                        ("contentId", _definition.Id),
                        ("episodeId", _episode.Id),
                        ("choiceId", choiceId.ToString())),
                });
        }

        private static async UniTask Wait(
            float seconds,
            CancellationToken cancellationToken)
        {
            while (seconds > 0f)
            {
                await UniTask.Yield(cancellationToken);
                seconds -= Time.deltaTime;
            }
        }
    }
}
