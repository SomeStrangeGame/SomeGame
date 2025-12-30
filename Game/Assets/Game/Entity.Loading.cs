using Cysharp.Threading.Tasks;
using Game.Disposable;

namespace Game
{
    internal sealed partial class Entity
    {
        private Loading.Entity _loading;

        private async UniTask LoadingProcess()
        {
             _loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,
                GetBundledPrefab = data => GetBundledPrefab(data.bundleName, data.prefabName),
            }).AddTo(this);
            await _loading.Init();
            _loading.ShowImmediate();
        }
    }
}
