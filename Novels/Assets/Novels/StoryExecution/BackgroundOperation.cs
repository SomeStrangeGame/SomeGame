using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public class BackgroundOperation
    {
        public readonly struct SetBackgroundQueue : IStoryOperation
        {
            private readonly Func<string, StoryContracts.StoryBackgroundPresentation,
                StoryContracts.PresentationMode, UniTask> _setImage;
            private readonly string _assetName;
            private readonly StoryContracts.StoryBackgroundPresentation _presentation;

            public SetBackgroundQueue(
                Func<string, StoryContracts.StoryBackgroundPresentation,
                    StoryContracts.PresentationMode, UniTask> setImage,
                string assetName,
                StoryContracts.StoryBackgroundPresentation presentation)
            {
                _setImage = setImage ?? throw new ArgumentNullException(nameof(setImage));
                _assetName = assetName ?? string.Empty;
                _presentation = presentation;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _setImage(
                    _assetName,
                    _presentation,
                    context.PresentationMode);
            }
        }
        public readonly struct CameraQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.StoryCameraAction,
                StoryContracts.PresentationMode, UniTask> _setCamera;
            private readonly StoryContracts.StoryCameraAction _action;

            public CameraQueue(
                Func<StoryContracts.StoryCameraAction,
                    StoryContracts.PresentationMode, UniTask> setCamera,
                StoryContracts.StoryCameraAction action)
            {
                _setCamera = setCamera ?? throw new ArgumentNullException(nameof(setCamera));
                _action = action;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _setCamera(_action, context.PresentationMode);
            }
        }
    }
}
