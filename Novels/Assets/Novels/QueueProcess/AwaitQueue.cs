using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct AwaitQueue : IQueue
    {
        public Func<float, UniTask> Wait;
        public float Timer;

        public async readonly UniTask Run()
        {
            await Wait(Timer);
        }

        public async readonly UniTask RunImmediate(byte choice)
        {
            
        }
    }
}

