using Cysharp.Threading.Tasks;
using Disposable;

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
                GetVideoURL = assetName => bundles.GetVideoURL(assetName),

                OnLog = _ctx.OnLog,
            }).AddTo(this);
            await location.Init();

            return location;
        }
    }
}

