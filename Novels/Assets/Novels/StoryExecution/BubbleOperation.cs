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
            private readonly Func<UniTask> _bubbleShow;
            private readonly Action _bubbleShowImmediate;

            public ShowBubbleQueue(
                UniTaskCompletionSource bubbleDone,
                Func<UniTask> bubbleShow,
                Action bubbleShowImmediate)
            {
                _bubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
                _bubbleShow = bubbleShow ?? throw new ArgumentNullException(nameof(bubbleShow));
                _bubbleShowImmediate = bubbleShowImmediate
                    ?? throw new ArgumentNullException(nameof(bubbleShowImmediate));
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    _bubbleShowImmediate();
                else
                    await _bubbleShow();

                await _bubbleDone.Task.AttachExternalCancellation(context.CancellationToken);
            }
        }
        public readonly struct HideBubbleQueue : IStoryOperation
        {
            private readonly Func<UniTask> _bubbleHide;
            private readonly Action _bubbleHideImmediate;

            public HideBubbleQueue(
                Func<UniTask> bubbleHide,
                Action bubbleHideImmediate)
            {
                _bubbleHide = bubbleHide ?? throw new ArgumentNullException(nameof(bubbleHide));
                _bubbleHideImmediate = bubbleHideImmediate
                    ?? throw new ArgumentNullException(nameof(bubbleHideImmediate));
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    _bubbleHideImmediate();
                else
                    await _bubbleHide();
            }
        }
    }
}
