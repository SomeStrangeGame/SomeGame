using Cysharp.Threading.Tasks;
using Game.Disposable;
using System;
using UnityEngine;

namespace Game.Loading
{
    [Serializable]
    public class Data
    {
        [SerializeField] private GameObject _loadingPrefab;

        public GameObject LoadingPrefab => _loadingPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private View.Screen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init() 
        {
            var go = GameObject.Instantiate(_ctx.Data.LoadingPrefab);
            _screen = go.GetComponent<View.Screen>();
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