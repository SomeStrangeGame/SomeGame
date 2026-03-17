using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Location.Entity> CreateLocation(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var location = new Location.Entity(new Location.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationImagePath(assetName)),
                GetVideoURL = pathGetter.GetVideoPath,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await location.Init();

            return location;
        }
    }
}

