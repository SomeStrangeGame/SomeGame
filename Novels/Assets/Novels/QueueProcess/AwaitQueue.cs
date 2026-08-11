using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct AwaitQueue : IQueue
    {
        public Func<bool> IsLoadingInProcess;
        public Func<float, UniTask> Wait;
        public float Timer;

        public async readonly UniTask Run()
        {
            if (!IsLoadingInProcess())
                await Wait(Timer);
        }
    }
}

