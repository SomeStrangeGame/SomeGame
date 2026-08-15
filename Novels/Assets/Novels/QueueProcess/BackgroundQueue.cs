using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class BackgroundQueue
    {
        public struct SetBackgroundQueue : IQueue
        {
            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImage;
            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImageImmediate;
            public string AssetName;
            public StoryContracts.StoryBackgroundPresentation Presentation;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                    await SetImageImmediate(AssetName, Presentation);
                else
                    await SetImage(AssetName, Presentation);
            }
        }
        public struct CameraQueue : IQueue
        {
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCamera;
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCameraImmediate;
            public StoryContracts.StoryCameraAction Action;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                    await SetCameraImmediate(Action);
                else
                    await SetCamera(Action);
            }
        }
    }
}
