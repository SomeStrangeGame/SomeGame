using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Notification.Entity CreateNotification(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                GetNotificationPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsNotificationBundleName, pathGetter.GetNotificationPrefabAssetName("Screen")),
            }).AddTo(this);
            notification.Init();

            return notification;
        }
    }
}

