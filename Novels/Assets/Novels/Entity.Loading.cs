using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
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
                CancellationToken = _ctx.CancellationToken,
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            loading.Init();

            return loading;
        }

        private Loading.Entity CreateLoading(
            IBaseDisposable owner,
            GameObject bundledPrefab,
            CancellationToken cancellationToken)
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                BundledPrefab = bundledPrefab,
                CancellationToken = cancellationToken,
            };
            var loading = new Loading.Entity(loadingCtx).AddTo(owner);
            loading.Init();

            return loading;
        }
    }
}
