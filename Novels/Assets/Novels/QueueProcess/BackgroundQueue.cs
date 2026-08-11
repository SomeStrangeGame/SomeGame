using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class BackgroundQueue
    {
        public struct LocationQueue : IQueue
        {
            public Func<bool, string, bool, bool, string[], UniTask> SetImage;
            public Func<bool> IsLoadingInProcess;
            public string AssetName;
            public string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(IsLoadingInProcess(), AssetName, false, false, Args);
            }
        }
        public struct CutSceneQueue : IQueue
        {
            public Func<bool, string, bool, bool, string[], UniTask> SetImage;
            public Func<bool> IsLoadingInProcess;
            public string AssetName;
            public string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(IsLoadingInProcess(), AssetName, true, false, Args);
            }
        }
        public struct CameraQueue : IQueue
        {
            public Func<bool> IsLoadingInProcess;
            public Func<bool, string, UniTask> SetCamera;
            public string Value;

            public async readonly UniTask Run()
            {
                await SetCamera(IsLoadingInProcess(), Value);
            }
        }
    }
}

