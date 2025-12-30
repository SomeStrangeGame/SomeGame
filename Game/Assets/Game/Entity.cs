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
        [SerializeField] private Chapter_OnlyScreen.Data _chapter_intro;
        [SerializeField] private Chapter_ScreenAndBattle.Data[] _chapters;

        internal readonly Loading.Data LoadingData => _loadingData;
        internal readonly Chapter_OnlyScreen.Data Chapter_intro => _chapter_intro;
        internal readonly Chapter_ScreenAndBattle.Data[] Chapters => _chapters;
    }

    internal sealed partial class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private readonly Ctx _ctx;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask Init()
        {
            await LoadingProcess();

            ChapterIntroProcess().Forget();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ClearBundles();
        }
    }
}