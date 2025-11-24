using Cysharp.Threading.Tasks;
using Game;
using Game.Disposable;
using UnityEngine;

public class Entity: BaseDisposable
{
    public struct Ctx
    {
        public Data Data;
    }

    private Game.Loading.Entity _loading;

    private readonly Ctx _ctx;

    public Entity(Ctx ctx)
    {
        _ctx = ctx;
    }

    public async UniTask Init()
    {
        _loading = new Game.Loading.Entity(new Game.Loading.Entity.Ctx
        {
            Data = _ctx.Data.LoadingData,
        }).AddTo(this);
        await _loading.Init();
        _loading.ShowImmediate();

        SomeMenu1Process().Forget();
    }

    private async UniTask SomeMenu1Process()
    {
        var someMenu1 = new Game.SomeMenu1.Entity(new Game.SomeMenu1.Entity.Ctx
        {
            Data = _ctx.Data.SomeMenu1Data,
        });
        await someMenu1.Init();

        await _loading.Hide();

        var result = await someMenu1.WaitResult();

        await _loading.Show();

        someMenu1.Dispose();

        switch (result)
        {
            case 1:
                SomeMenu1Process().Forget();
                break;
        }
    }
}
