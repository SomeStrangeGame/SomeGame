using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public class BackgroundOperation
    {
        public readonly struct SetBackgroundQueue : IStoryOperation
        {
            private readonly Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> _setImage;
            private readonly Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> _setImageImmediate;
            private readonly string _assetName;
            private readonly StoryContracts.StoryBackgroundPresentation _presentation;

            public SetBackgroundQueue(
                Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> setImage,
                Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> setImageImmediate,
                string assetName,
                StoryContracts.StoryBackgroundPresentation presentation)
            {
                _setImage = setImage ?? throw new ArgumentNullException(nameof(setImage));
                _setImageImmediate = setImageImmediate
                    ?? throw new ArgumentNullException(nameof(setImageImmediate));
                _assetName = assetName ?? string.Empty;
                _presentation = presentation;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    await _setImageImmediate(_assetName, _presentation);
                else
                    await _setImage(_assetName, _presentation);
            }
        }
        public readonly struct CameraQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.StoryCameraAction, UniTask> _setCamera;
            private readonly Func<StoryContracts.StoryCameraAction, UniTask> _setCameraImmediate;
            private readonly StoryContracts.StoryCameraAction _action;

            public CameraQueue(
                Func<StoryContracts.StoryCameraAction, UniTask> setCamera,
                Func<StoryContracts.StoryCameraAction, UniTask> setCameraImmediate,
                StoryContracts.StoryCameraAction action)
            {
                _setCamera = setCamera ?? throw new ArgumentNullException(nameof(setCamera));
                _setCameraImmediate = setCameraImmediate
                    ?? throw new ArgumentNullException(nameof(setCameraImmediate));
                _action = action;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    await _setCameraImmediate(_action);
                else
                    await _setCamera(_action);
            }
        }
    }
}
