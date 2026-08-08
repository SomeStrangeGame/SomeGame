using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Notification.Entity CreateNotification(GameObject notificationPrefab)
        {
            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                NotificationPrefab = notificationPrefab,
            }).AddTo(this);
            notification.Init();

            return notification;
        }
    }
}

