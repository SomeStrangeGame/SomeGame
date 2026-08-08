using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Loading.Entity CreateMainLoading(GameObject bundledPrefab)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                BundledPrefab = bundledPrefab,
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            loading.Init();

            return loading;
        }

        private Loading.Entity CreateLoading(GameObject bundledPrefab)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                BundledPrefab = bundledPrefab,
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            loading.Init();

            return loading;
        }
    }
}

