using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Loading.Entity> CreateMainLoading(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetMainLoadingPrefabAssetName("Screen")),
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            await loading.Init();

            return loading;
        }

        private async UniTask<Loading.Entity> CreateLoading(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetLoadingPrefabAssetName("Screen")),
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            await loading.Init();

            return loading;
        }
    }
}

