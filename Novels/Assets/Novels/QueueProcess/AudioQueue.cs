using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct AudioQueue : IQueue
    {
        public Func<string, UniTask> PlayAudio;
        public string AssetName;

        public async readonly UniTask Run()
        {
            await PlayAudio(AssetName);
        }

        public async readonly UniTask RunImmediate(byte choice)
        {
            await PlayAudio(AssetName);
        }
    }
}
