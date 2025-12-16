using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private SomeBattleScene1.Data _someBattleScene1;
        [SerializeField] private Chapter_0.Data _chapter_0;
        [SerializeField] private Chapter_1.Data _chapter_1;

        internal readonly Loading.Data LoadingData => _loadingData;
        internal readonly SomeBattleScene1.Data SomeBattleScene1 => _someBattleScene1;
        internal readonly Chapter_0.Data Chapter_0 => _chapter_0;
        internal readonly Chapter_1.Data Chapter_1 => _chapter_1;
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

            Chapter_0Process().Forget();
        }
    }
}