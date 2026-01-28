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
            public Func<string, string, UniTask<GameObject>> GetBundledPrefab;
        }

        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly Ctx _ctx;

        private View.Scene _battleScene;
        private View.Screen _battleScreen;
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

        public async UniTask InitBattle()
        {
            var battleScreenPrefab = await _ctx.GetBundledPrefab(_ctx.Data.ScreenBundle.BundleName, _ctx.Data.ScreenBundle.AssetName);
            var battleScreenGO = GameObject.Instantiate(battleScreenPrefab);
            _battleScreen = battleScreenGO.GetComponent<View.Screen>();

            var battleScenePrefab = await _ctx.GetBundledPrefab(_ctx.Data.SceneBundle.BundleName, _ctx.Data.SceneBundle.AssetName);
            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            var characterPrefab = await _ctx.GetBundledPrefab(_ctx.Data.CharacterBundle.BundleName, _ctx.Data.CharacterBundle.BundleName);
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
            await _playerCharacterEntity.Init();

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
                await enemyCharacter.Init();
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
            if (_battleScreen != null) _battleScreen.Release();
            if (_battleScene != null) _battleScene.Release();
        }

        public void ReleaseCharacters()
        {
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