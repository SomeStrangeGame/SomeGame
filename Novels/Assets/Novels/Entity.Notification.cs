using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Notification.Entity CreateNotification(
            IBaseDisposable owner,
            GameObject notificationPrefab,
            CancellationToken cancellationToken)
        {
            var notification = new Notification.Entity(new Notification.Entity.Ctx
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
