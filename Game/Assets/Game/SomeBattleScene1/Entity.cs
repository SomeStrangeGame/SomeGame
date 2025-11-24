using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using Game.SomeBattleScene1.View;
using UnityEngine;

namespace Game.SomeBattleScene1
{
    [Serializable]
    public class Data
    {
        [SerializeField] private GameObject _someBattleScene1Prefab;
        [SerializeField] private GameObject _someBattleScene1ScreenPrefab;

        public GameObject SomeBattleScene1Prefab => _someBattleScene1Prefab;
        public GameObject SomeBattleScene1ScreenPrefab => _someBattleScene1ScreenPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private ISomeBattleScene1 _scene;
        private ISomeBattleScene1Screen _screen;

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.SomeBattleScene1ScreenPrefab);
            _screen = go.GetComponent<ISomeBattleScene1Screen>();

            go = GameObject.Instantiate(_ctx.Data.SomeBattleScene1Prefab);
            _scene = go.GetComponent<ISomeBattleScene1>();
            _scene.InitScreen(_screen);
        }

        public async UniTask<int> WaitResult() 
        {
            return await _scene.GetProcess();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _screen?.Release();
            _scene?.Release();
        }
    }
}