using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct NotificationQueue : IQueue
    {
        public string NotificationText;
        public Func<string, UniTask> ShowNotification;

        public async readonly UniTask Run()
        {
            ShowNotification(NotificationText).Forget();
        }

        public async readonly UniTask RunImmediate(byte choice)
        {
            
        }
    }
}

