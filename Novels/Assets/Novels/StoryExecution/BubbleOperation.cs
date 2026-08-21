using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public class BubbleOperation
    {
        public class SetBubbleQueue : IStoryOperation
        {
            private readonly BubbleOperationRequest _request;
            private readonly ChoiceSelectionHandler _choices;
            private readonly BubblePresentationRouter _router;

            public SetBubbleQueue(BubbleOperationRequest request)
            {
                _request = request ?? throw new ArgumentNullException(nameof(request));
                _choices = new ChoiceSelectionHandler(request);
                _router = new BubblePresentationRouter(request, _choices);
            }

            public UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _choices.ApplySaved(context.SavedDecision);
                    _request.BubbleDone.TrySetResult();
                    return UniTask.CompletedTask;
                }
                _router.Present();
                return UniTask.CompletedTask;
            }
        }

        public readonly struct ShowBubbleQueue : IStoryOperation
        {
            private readonly UniTaskCompletionSource _bubbleDone;
            private readonly Func<StoryContracts.PresentationMode, UniTask> _show;

            public ShowBubbleQueue(
                UniTaskCompletionSource bubbleDone,
                Func<StoryContracts.PresentationMode, UniTask> show)
            {
                _bubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
                _show = show ?? throw new ArgumentNullException(nameof(show));
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _show(context.PresentationMode);

                await _bubbleDone.Task.AttachExternalCancellation(context.CancellationToken);
            }
        }
        public readonly struct HideBubbleQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.PresentationMode, UniTask> _hide;

            public HideBubbleQueue(
                Func<StoryContracts.PresentationMode, UniTask> hide)
            {
                _hide = hide ?? throw new ArgumentNullException(nameof(hide));
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _hide(context.PresentationMode);
            }
        }
    }
}
