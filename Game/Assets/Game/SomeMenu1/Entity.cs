using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using Game.SomeMenu1.View;
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

        private ISomeScreen1 _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.SomeMenu1Prefab);
            _screen = go.GetComponent<ISomeScreen1>();
        }

        public async UniTask<int> WaitResult() 
        {
            return await _screen.GetProcess();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _screen?.Release();
        }
    }
}