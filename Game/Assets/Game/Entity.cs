using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game
{
    [Serializable]
    internal sealed class Data
    {
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private SomeMenu1.Data _someMenu1Data;
        [SerializeField] private SomeBattleScene1.Data _someBattleScene1;

        internal Loading.Data LoadingData => _loadingData;
        internal SomeMenu1.Data SomeMenu1Data => _someMenu1Data;
        internal SomeBattleScene1.Data SomeBattleScene1 => _someBattleScene1;
    }

    internal sealed class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private Game.Loading.Entity _loading;

        private readonly Ctx _ctx;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask Init()
        {
            _loading = new Loading.Entity(new Game.Loading.Entity.Ctx
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
            var ctx = new SomeMenu1.Entity.Ctx
            {
                Data = _ctx.Data.SomeMenu1Data,
            };
            using (var someMenu1 = new SomeMenu1.Entity(ctx))
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
            var ctx = new SomeBattleScene1.Entity.Ctx
            {
                Data = _ctx.Data.SomeBattleScene1,
            };
            using (var someBattle1 = new SomeBattleScene1.Entity(ctx))
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
}