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
                OnSliderValueChanged = result => Debug.Log("")
            });

            go = GameObject.Instantiate(_ctx.Data.SomeBattleScene1Prefab);
            _scene = go.GetComponent<View.Scene>();

            var playerCharacter = new Character.Entity(new Character.Entity.Ctx
            {
                CharacterView = _scene.PlayerCharacter,
                Speed = 1.5f,

                GetTargetPosition = () => GetTargetPosition(true),
                GetLookAtTargetPosition = () => GetLookAtTargetPosition(true),
                GetAttackInput = () => GetAttackInput(true),
                GetDodgeInput = () => GetDodgeInput(true),

            }).AddTo(this);
            await playerCharacter.Init();

            _scene.Setup(new View.Scene.Ctx
            {
                OnComplete = result => _someToken.TrySetResult(result),
            });
        }

        private Vector3 GetTargetPosition(bool isPlayer)
        {
            if (isPlayer)
            {
                var targetPosition = _scene.PlayerCharacter.transform.position;
                targetPosition += Vector3.right * Input.GetAxis("Horizontal");
                targetPosition += Vector3.forward * Input.GetAxis("Vertical");

                return targetPosition;
            }
            else
            {
                return Vector3.zero;
            }
        }

        private Vector3 GetLookAtTargetPosition(bool isPlayer)
        {
            return _scene.TargetObject.transform.position;
        }

        private bool GetAttackInput(bool isPlayer)
        {
            if (isPlayer)
            {
                return Input.GetKeyUp(KeyCode.Space);
            }
            else
            {
                return false;
            }
        }

        private bool GetDodgeInput(bool isPlayer)
        {
            if (isPlayer)
            {
                return Input.GetKeyUp(KeyCode.E);
            }
            else
            {
                return false;
            }
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