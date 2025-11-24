using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

[Serializable]
public class Data
{
    [SerializeField] private Game.Loading.Data _loadingData;
    [SerializeField] private Game.SomeMenu1.Data _someMenu1Data;
    [SerializeField] private Game.SomeBattleScene1.Data _someBattleScene1;

    public Game.Loading.Data LoadingData => _loadingData;
    public Game.SomeMenu1.Data SomeMenu1Data => _someMenu1Data;
    public Game.SomeBattleScene1.Data SomeBattleScene1 => _someBattleScene1;
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
                SomeBattleScene1Process().Forget();
                break;
        }
    }

    private async UniTask SomeBattleScene1Process()
    {
        var result = 0;
        var ctx = new Game.SomeBattleScene1.Entity.Ctx
        {
            Data = _ctx.Data.SomeBattleScene1,
        };
        using (var someBattle1 = new Game.SomeBattleScene1.Entity(ctx))
        {
            await someBattle1.Init();

            await _loading.Hide();

            result = await someBattle1.WaitResult(); //wait some process...

            await _loading.Show();
        }

        switch (result)
        {
            case 2:
                SomeMenu1Process().Forget();
                break;
        }
    }
}
