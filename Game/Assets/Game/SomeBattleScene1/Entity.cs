using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.SomeBattleScene1
{
    [Serializable]
    public sealed class Data
    {
        [SerializeField] private GameObject _someBattleScene1Prefab;
        [SerializeField] private GameObject _someBattleScene1ScreenPrefab;

        internal GameObject SomeBattleScene1Prefab => _someBattleScene1Prefab;
        internal GameObject SomeBattleScene1ScreenPrefab => _someBattleScene1ScreenPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private readonly UniTaskCompletionSource<int> _someToken = new();
        private readonly Ctx _ctx;

        private View.Scene _scene;
        private View.Screen _screen;

        private float _playerInput;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.SomeBattleScene1ScreenPrefab);
            _screen = go.GetComponent<View.Screen>();
            _screen.Setup(new View.Screen.Ctx
            {
                OnSliderValueChanged = result => _playerInput = result
            });

            go = GameObject.Instantiate(_ctx.Data.SomeBattleScene1Prefab);
            _scene = go.GetComponent<View.Scene>();
            _scene.Setup(new View.Scene.Ctx
            {
                GetPlayerInput = () => _playerInput,
                OnComplete = result => _someToken.TrySetResult(result),
            });
        }

        public async UniTask<int> WaitResult() => await _someToken.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
            if (_scene != null) _scene.Release();
        }
    }
}