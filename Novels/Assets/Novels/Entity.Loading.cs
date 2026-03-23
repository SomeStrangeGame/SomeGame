using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Loading.Entity CreateMainLoading(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetMainLoadingPrefabAssetName("Screen")),
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            loading.Init();

            return loading;
        }

        private Loading.Entity CreateLoading(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetLoadingPrefabAssetName("Screen")),
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            loading.Init();

            return loading;
        }
    }
}

