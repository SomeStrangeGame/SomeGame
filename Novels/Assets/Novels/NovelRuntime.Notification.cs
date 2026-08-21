using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private Notification.NotificationController CreateNotification(
            IBaseDisposable owner,
            GameObject notificationPrefab,
            CancellationToken cancellationToken)
        {
            var notification = new Notification.NotificationController(new Notification.NotificationController.Dependencies
            {
                NotificationPrefab = notificationPrefab,
                CancellationToken = cancellationToken,
                DisplayDuration = _ctx.RuntimeTuning.NotificationDuration,
                OnError = ReportError,
            }).AddTo(owner);
            notification.Init();

            return notification;
        }
    }
}
