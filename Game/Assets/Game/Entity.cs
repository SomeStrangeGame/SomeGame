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

    internal sealed partial class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private Loading.Entity _loading;

        private readonly Ctx _ctx;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask Init()
        {
            _loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,
            }).AddTo(this);
            await _loading.Init();
            _loading.ShowImmediate();

            SomeMenu1Process().Forget();
        }
    }
}