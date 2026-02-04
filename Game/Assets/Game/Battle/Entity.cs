using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.SOData;
using Game.Disposable;
using UnityEngine;

namespace Game.Battle
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public CameraData CameraData;
            public Func<UniTask<GameObject>> GetCharacterInputScreenPrefab;
            public Func<UniTask<GameObject>> GetCharacterPrefab;
            public Func<UniTask<GameObject>> GetBattleScenePrefab;
            public Func<UniTask<GameObject>> GetBattleScreenPrefab;
        }

        public sealed class Preload : BaseDisposable
        {
            public struct Ctx
            {
                public Func<List<UniTask>> GetAssets;
            }

            private Ctx _ctx;

            public Preload(Ctx ctx)
            {
                _ctx = ctx;
            }

            public async UniTask Process()
            {
                await UniTask.WhenAll(_ctx.GetAssets());
            }
        }

        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly Ctx _ctx;

        private View.Scene _battleScene;
        private GameObject _battleScreenGO;
        private GameObject _characterInputScreenGO;
        private GameObject _playerCharacterGO;
        private readonly List<GameObject> _enemyCharactersGO;

        private Character.Entity _playerCharacterEntity;
        private List<Character.Entity> _enemyCharacterEntites;
        private Character.Behaviour _behaviour;

        public Entity(Ctx ctx)
        {
            _battleToken = new();
            _enemyCharactersGO = new();
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var (characterInputScreenPrefab, battleScenePrefab, characterPrefab, battleSceneScreenPrefab) = await UniTask.WhenAll(
                _ctx.GetCharacterInputScreenPrefab(),
                _ctx.GetBattleScenePrefab(),
                _ctx.GetCharacterPrefab(),
                _ctx.GetBattleScreenPrefab()
            );

            _characterInputScreenGO = GameObject.Instantiate(characterInputScreenPrefab);

            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            _battleScreenGO = GameObject.Instantiate(battleSceneScreenPrefab);

            _playerCharacterGO = GameObject.Instantiate(characterPrefab);
            var characterPoint = _battleScene.PlayerCharacterPoint.Point;
            _playerCharacterGO.transform.SetPositionAndRotation(characterPoint.position, characterPoint.rotation);

            var camera = new Camera.Entity(new Camera.Entity.Ctx
            {
                Data = _ctx.CameraData,
                GetCameraTargetPosition = () => _playerCharacterGO.transform.position,
            }).AddTo(this);
            camera.Init();

            _playerCharacterEntity = new Character.Entity(new Character.Entity.Ctx
            {
                CharacterView = _playerCharacterGO,
                Health = _battleScene.PlayerCharacterPoint.Health,

                GetTargetPosition = characterEntity => _behaviour.GetTargetPosition(characterEntity, true),
                GetLookAtTargetPosition = characterEntity => _behaviour.GetLookAtTargetPosition(characterEntity, true),
                GetAttackInput = characterEntity => _behaviour.GetAttackInput(characterEntity, true),
                GetDodgeInput = characterEntity => _behaviour.GetDodgeInput(characterEntity, true),
            }).AddTo(this);
            _playerCharacterEntity.Init();

            _enemyCharacterEntites = new();
            foreach (var enemyPoint in _battleScene.EnemyCharacterPoints)
            {
                var enemyCharacterGO = GameObject.Instantiate(characterPrefab);
                enemyCharacterGO.transform.SetPositionAndRotation(enemyPoint.Point.position, enemyPoint.Point.rotation);
                _enemyCharactersGO.Add(enemyCharacterGO);

                var enemyCharacter = new Character.Entity(new Character.Entity.Ctx
                {
                    CharacterView = enemyCharacterGO,
                    Health = enemyPoint.Health,

                    GetTargetPosition = characterEntity => _behaviour.GetTargetPosition(characterEntity, false),
                    GetLookAtTargetPosition = characterEntity => _behaviour.GetLookAtTargetPosition(characterEntity, false),
                    GetAttackInput = characterEntity => _behaviour.GetAttackInput(characterEntity, false),
                    GetDodgeInput = characterEntity => _behaviour.GetDodgeInput(characterEntity, false),
                }).AddTo(this);
                enemyCharacter.Init();
                _enemyCharacterEntites.Add(enemyCharacter);
            }

            _behaviour = new Character.Behaviour(new Character.Behaviour.Ctx
            {
                PlayerCharacterEntity = _playerCharacterEntity,
                EnemyCharacterEntites = _enemyCharacterEntites,
            }).AddTo(this);

            _battleScene.Setup(new View.Scene.Ctx
            {
                PlayerCharacterGO = _playerCharacterGO,
                EnemyCharactersGO = _enemyCharactersGO,
                OnUpdate = deltaTime =>
                {
                    camera.UpdatePos(deltaTime);
                },
                OnComplete = result => _battleToken.TrySetResult(result),
            });
        }

        public void ReleaseBattle()
        {
            if (_battleScreenGO != null) GameObject.Destroy(_battleScreenGO);
            if (_battleScene != null) GameObject.Destroy(_battleScene.gameObject);
        }

        public void ReleaseCharacters()
        {
            if (_characterInputScreenGO != null) GameObject.Destroy(_characterInputScreenGO);
            GameObject.Destroy(_playerCharacterGO);
            foreach(var character in _enemyCharactersGO)
                GameObject.Destroy(character);
            _enemyCharactersGO.Clear();
        }

        public async UniTask<int> WaitBattleResult() => await _battleToken.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            ReleaseCharacters();
            ReleaseBattle();
        }
    }
}