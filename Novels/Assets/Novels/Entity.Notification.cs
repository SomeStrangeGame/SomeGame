using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Notification.Entity> CreateNotification(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                GetNotificationPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsNotificationBundleName, pathGetter.GetNotificationPrefabAssetName("Screen")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await notification.Init();

            return notification;
        }
    }
}

