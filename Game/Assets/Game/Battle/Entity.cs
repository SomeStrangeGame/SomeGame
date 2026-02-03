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
            public BattleData Data;
            public Func<BundleData, UniTask<GameObject>> GetBundledPrefab;
        }

        public sealed class Preload : BaseDisposable
        {
            public struct Ctx
            {
                public BattleData Data;
                public Func<string, UniTask<AssetBundle>> GetAssets;
            }

            private Ctx _ctx;

            public Preload(Ctx ctx)
            {
                _ctx = ctx;
            }

            public async UniTask Process()
            {
                var battleScreenPrefabLoading = _ctx.GetAssets(_ctx.Data.ScreenBundle.BundleName);
                var battleScenePrefabLoading = _ctx.GetAssets(_ctx.Data.SceneBundle.BundleName);
                var characterScreenPrefabLoading = _ctx.GetAssets(_ctx.Data.CharacterScreenBundle.BundleName);
                var characterPrefabLoading = _ctx.GetAssets(_ctx.Data.CharacterBundle.BundleName);
            
                await UniTask.WhenAll(
                    battleScreenPrefabLoading,
                    battleScenePrefabLoading,
                    characterScreenPrefabLoading,
                    characterPrefabLoading
                );
            }
        }

        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly Ctx _ctx;

        private View.Scene _battleScene;
        private GameObject _battleScreenGO;
        private Character.View.Screen _characterInputScreen;
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
            var characterInputScreenPrefabLoading = _ctx.GetBundledPrefab(_ctx.Data.CharacterScreenBundle);
            var battleScenePrefabLoading = _ctx.GetBundledPrefab(_ctx.Data.SceneBundle);
            var characterPrefabLoading = _ctx.GetBundledPrefab(_ctx.Data.CharacterBundle);
            var battleSceneScreenPrefabLoading = _ctx.GetBundledPrefab(_ctx.Data.ScreenBundle);

            var (characterInputScreenPrefab, battleScenePrefab, characterPrefab, battleSceneScreenPrefab) = await UniTask.WhenAll(
                characterInputScreenPrefabLoading,
                battleScenePrefabLoading,
                characterPrefabLoading,
                battleSceneScreenPrefabLoading
            );

            var characterInputScreenGO = GameObject.Instantiate(characterInputScreenPrefab);
            _characterInputScreen = characterInputScreenGO.GetComponent<Character.View.Screen>();

            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            _battleScreenGO = GameObject.Instantiate(battleSceneScreenPrefab);

            _playerCharacterGO = GameObject.Instantiate(characterPrefab);
            var characterPoint = _battleScene.PlayerCharacterPoint.transform;
            _playerCharacterGO.transform.SetPositionAndRotation(characterPoint.position, characterPoint.rotation);

            var camera = new Camera.Entity(new Camera.Entity.Ctx
            {
                Data = _ctx.Data.Camera,
                GetCameraTargetPosition = () => _playerCharacterGO.transform.position,
            }).AddTo(this);
            camera.Init();

            _playerCharacterEntity = new Character.Entity(new Character.Entity.Ctx
            {
                CharacterView = _playerCharacterGO,
                Health = 10,
                Speed = 2.5f,
                AttackDistance = 2f,

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
                enemyCharacterGO.transform.SetPositionAndRotation(enemyPoint.transform.position, enemyPoint.transform.rotation);
                _enemyCharactersGO.Add(enemyCharacterGO);

                var enemyCharacter = new Character.Entity(new Character.Entity.Ctx
                {
                    CharacterView = enemyCharacterGO,
                    Health = 3,
                    Speed = 2.5f,
                    AttackDistance = 2f,

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
            if (_battleScene != null) _battleScene.Release();
        }

        public void ReleaseCharacters()
        {
            if (_characterInputScreen != null) _characterInputScreen.Release();
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