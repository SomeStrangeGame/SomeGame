using Cysharp.Threading.Tasks;
using Game;
using Game.Disposable;

public class Entity: BaseDisposable
{
    public struct Ctx
    {
        public Data Data;
    }

    private readonly Ctx _ctx;

    public Entity(Ctx ctx)
    {
        _ctx = ctx;

        AsyncProcess();
    }

    private async void AsyncProcess()
    {
        var loadingDone = false;
        var loading = new Game.Loading.Entity(new Game.Loading.Entity.Ctx
        {
            Data = _ctx.Data.LoadingData,

            InitDone = () => loadingDone = true,
        }).AddTo(this);

        while (!loadingDone) await UniTask.Yield();

        loading.ShowImmediate();
    }
}
