using Cysharp.Threading.Tasks;
using Disposable;
using System;
using UnityEngine;

namespace Loading
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject BundledPrefab;
        }

        private View.Screen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenPrefabGO = _ctx.BundledPrefab;
            var screenGO = GameObject.Instantiate(screenPrefabGO);

            _screen = screenGO.GetComponent<View.Screen>();
        }

        public async UniTask Show() => await _screen.Show();
        public async UniTask Hide() => await _screen.Hide();

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) GameObject.Destroy(_screen.gameObject);
        }
    }
}