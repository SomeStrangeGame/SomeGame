using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct NotificationQueue : IQueue
    {
        public string NotificationText;
        public Func<string, UniTask> ShowNotification;

        public async readonly UniTask Run(QueueExecutionContext context)
        {
            if (context.Mode == QueueExecutionMode.Live)
                ShowNotification(NotificationText).Forget();
        }
    }
}
