using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    internal static class BubbleOperation
    {
        internal sealed class SetBubbleQueue : IStoryOperation
        {
            private readonly BubbleOperationRequest _request;
            private readonly ChoiceSelectionHandler _choices;
            private readonly BubblePresentationRouter _router;

            internal SetBubbleQueue(BubbleOperationRequest request)
            {
                _request = request ?? throw new ArgumentNullException(nameof(request));
                _choices = new ChoiceSelectionHandler(request);
                _router = new BubblePresentationRouter(request, _choices);
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _choices.ApplySaved(context.SavedDecision);
                    _request.Completed.TrySetResult();
                    return;
                }
                if (_choices.TryApplyQueuedWardrobeChoice())
                    return;
                await _router.Present();
            }
        }

    }
}
