using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

[Serializable]
public class Data
{
    [SerializeField] private Game.Loading.Data _loadingData;
    [SerializeField] private Game.SomeMenu1.Data _someMenu1Data;

    public Game.Loading.Data LoadingData => _loadingData;
    public Game.SomeMenu1.Data SomeMenu1Data => _someMenu1Data;
}

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
        var result = 0;
        var ctx = new Game.SomeMenu1.Entity.Ctx
        {
            Data = _ctx.Data.SomeMenu1Data,
        };
        using (var someMenu1 = new Game.SomeMenu1.Entity(ctx))
        {
            await someMenu1.Init();

            await _loading.Hide();

            result = await someMenu1.WaitResult(); //wait some process...

            await _loading.Show();
        }

        switch (result)
        {
            case 1:
                SomeMenu1Process().Forget();
                break;
        }
    }
}
