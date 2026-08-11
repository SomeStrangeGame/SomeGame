using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct NotificationQueue : IQueue
    {
        public Func<bool> IsLoadingInProcess;
        public string NotificationText;
        public Func<string, UniTask> ShowNotification;

        public async readonly UniTask Run()
        {
            if (!IsLoadingInProcess())
                ShowNotification(NotificationText).Forget();
        }

        public async readonly UniTask RunImmediate(byte choice)
        {
            
        }
    }
}

