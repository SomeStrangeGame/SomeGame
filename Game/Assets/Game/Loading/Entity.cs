using Cysharp.Threading.Tasks;
using Game.Disposable;
using Game.Loading;
using Game.Loading.View;
using System;
using UnityEngine;

namespace Game.Loading
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public Action InitDone;
        }

        private IScreen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            Init();
        }

        private async void Init() 
        {
            var go = GameObject.Instantiate(_ctx.Data.LoadingPrefab);
            _screen = go.GetComponent<IScreen>();

            _ctx.InitDone.Invoke();
        }

        public void ShowImmediate() => _screen.ShowImmediate();
        public void HideImmediate() => _screen.HideImmediate();

        public async UniTask Show() => await _screen.Show();
        public async UniTask Hide() => await _screen.Hide();

        protected override void OnDispose()
        {
            base.OnDispose();
            _screen?.Release();
        }
    }
}