using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Location.Entity CreateLocation(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var location = new Location.Entity(new Location.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationImagePath(assetName)),
                GetVideoURL = pathGetter.GetVideoPath,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
            location.Init();

            return location;
        }
    }
}

