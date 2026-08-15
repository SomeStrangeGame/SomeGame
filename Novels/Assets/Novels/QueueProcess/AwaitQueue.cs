using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct AwaitQueue : IQueue
    {
        public Func<float, UniTask> Wait;
        public float Timer;

        public async readonly UniTask Run(QueueExecutionContext context)
        {
            if (context.Mode == QueueExecutionMode.Live)
                await Wait(Timer);
        }
    }
}
