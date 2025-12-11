using System;
using System.Collections.Generic;
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

        [SerializeField] private Vector3 _camMoveOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        internal GameObject SomeBattleScene1Prefab => _someBattleScene1Prefab;
        internal GameObject SomeBattleScene1ScreenPrefab => _someBattleScene1ScreenPrefab;

        internal Vector3 CamMoveOffset => _camMoveOffset;
        internal float CamMoveSpeed => _camMoveSpeed;
        internal Vector3 CamLookAtOffset => _camLookAtOffset;
        internal float CamLookAtSpeed => _camLookAtSpeed;
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

        private Character.Entity _playerCharacterEntity;
        private List<Character.Entity> _enemyCharacterEntites;

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

            var camera = new Camera.Entity(new Camera.Entity.Ctx
            {
                MoveOffset = _ctx.Data.CamMoveOffset,
                MoveSpeed = _ctx.Data.CamMoveSpeed,
                LookAtOffset = _ctx.Data.CamLookAtOffset,
                LookAtSpeed = _ctx.Data.CamLookAtSpeed,

                GetCameraTargetPosition = () => _scene.PlayerCharacter.transform.position,
            }).AddTo(this);
            await camera.Init();

            _playerCharacterEntity = new Character.Entity(new Character.Entity.Ctx
            {
                CharacterView = _scene.PlayerCharacter,
                Speed = 1.5f,

                GetTargetPosition = characterEntity => GetTargetPosition(characterEntity, true),
                GetLookAtTargetPosition = characterEntity => GetLookAtTargetPosition(characterEntity, true),
                GetAttackInput = characterEntity => GetAttackInput(true),
                GetDodgeInput = characterEntity => GetDodgeInput(true),
            }).AddTo(this);
            await _playerCharacterEntity.Init();

            _enemyCharacterEntites = new();
            foreach (var enemyCharacterView in _scene.EnemyCharacters)
            {
                var enemyCharacter = new Character.Entity(new Character.Entity.Ctx
                {
                    CharacterView = enemyCharacterView,
                    Speed = 1.5f,

                    GetTargetPosition = characterEntity => GetTargetPosition(characterEntity, false),
                    GetLookAtTargetPosition = characterEntity => GetLookAtTargetPosition(characterEntity, false),
                    GetAttackInput = characterEntity => GetAttackInput(false),
                    GetDodgeInput = characterEntity => GetDodgeInput(false),
                }).AddTo(this);
                await enemyCharacter.Init();
                _enemyCharacterEntites.Add(enemyCharacter);
            }

            _scene.Setup(new View.Scene.Ctx
            {
                OnUpdate = deltaTime =>
                {
                    camera.UpdatePos(deltaTime);
                },
                OnComplete = result => _someToken.TrySetResult(result),
            });
        }

        private Vector3 GetTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            if (isPlayer)
            {
                var targetPos = characterEntity.Anim.rootPosition;
                targetPos += Vector3.right * Input.GetAxis("Horizontal");
                targetPos += Vector3.forward * Input.GetAxis("Vertical");

                return targetPos;
            }
            else
            {
                var heading = _playerCharacterEntity.Anim.rootPosition - characterEntity.Anim.rootPosition;
                var distance = heading.magnitude;
                var direction = heading / distance;
                var targetPos = _playerCharacterEntity.Anim.rootPosition - direction * 1.5f;
                return targetPos;
            }
        }

        private Vector3 GetLookAtTargetPosition(Character.Entity characterEntity, bool isPlayer)
        {
            if (isPlayer)
            {
                var lookAtPoint = characterEntity.Anim.rootPosition + characterEntity.Anim.GetBoneTransform(HumanBodyBones.Chest).forward * 5f;
                var minDistance = float.MaxValue;
                foreach (var enemyEntity in _enemyCharacterEntites)
                {
                    if (!enemyEntity.Anim.enabled) continue;

                    var distance = Vector3.Distance(enemyEntity.Anim.rootPosition, characterEntity.Anim.rootPosition);
                    if (distance > minDistance) continue;

                    minDistance = distance;
                    lookAtPoint = enemyEntity.Anim.GetBoneTransform(HumanBodyBones.Chest).position;
                }
                return lookAtPoint;
            }
            else
            {
                return _playerCharacterEntity.Anim.GetBoneTransform(HumanBodyBones.Chest).position;
            }
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