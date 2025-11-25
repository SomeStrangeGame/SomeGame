using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.SomeMenu1
{
    [Serializable]
    public class Data
    {
        [SerializeField] private GameObject _someMenu1Prefab;

        public GameObject SomeMenu1Prefab => _someMenu1Prefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private readonly UniTaskCompletionSource<int> _someToken;
        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _someToken = new();
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.SomeMenu1Prefab);
            _screen = go.GetComponent<View.Screen>();
            _screen.Setup(result => _someToken.TrySetResult(result));
        }

        public async UniTask<int> WaitResult() => await _someToken.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}