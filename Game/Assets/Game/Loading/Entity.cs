using Cysharp.Threading.Tasks;
using Game.Disposable;
using System;
using UnityEngine;

namespace Game.Loading
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private string _bundleName;
        [SerializeField] private string _loadingPrefabName;

        internal readonly string BundleName => _bundleName;
        internal readonly string LoadingPrefabName => _loadingPrefabName;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public Func<(string bundleName, string prefabName), UniTask<GameObject>> GetBundledPrefab;
        }

        private View.Screen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init() 
        {
            var screenPrefabGO = await _ctx.GetBundledPrefab((_ctx.Data.BundleName, _ctx.Data.LoadingPrefabName));
            var screenGO = GameObject.Instantiate(screenPrefabGO);

            _screen = screenGO.GetComponent<View.Screen>();
        }

        public void ShowImmediate() => _screen.ShowImmediate();
        public void HideImmediate() => _screen.HideImmediate();

        public async UniTask Show() => await _screen.Show();
        public async UniTask Hide() => await _screen.Hide();

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}