using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BattleStory.SOData;
using Disposable;
using UnityEngine;

namespace BattleStory.Battle
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public CameraData CameraData;
            public Func<UniTask<GameObject>> GetMeleeCharacterInputScreenPrefab;
            public Func<UniTask<GameObject>> GetMeleeCharacterPrefab;
            public Func<UniTask<GameObject>> GetDistanceCharacterInputScreenPrefab;
            public Func<UniTask<GameObject>> GetDistanceCharacterPrefab;
            public Func<UniTask<GameObject>> GetBattleScenePrefab;
            public Func<UniTask<GameObject>> GetBattleScreenPrefab;
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
            var (meleeCharacterInputScreenPrefab, distanceCharacterInputScreenPrefab, battleScenePrefab, meleeCharacterPrefab, distanceCharacterPrefab, battleSceneScreenPrefab) = await UniTask.WhenAll(
                _ctx.GetMeleeCharacterInputScreenPrefab(),
                _ctx.GetDistanceCharacterInputScreenPrefab(),
                _ctx.GetBattleScenePrefab(),
                _ctx.GetMeleeCharacterPrefab(),
                _ctx.GetDistanceCharacterPrefab(),
                _ctx.GetBattleScreenPrefab()
            );

            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            _characterInputScreenGO = GameObject.Instantiate(_battleScene.PlayerCharacterPoint.IsDistance ? distanceCharacterInputScreenPrefab : meleeCharacterInputScreenPrefab);

            _battleScreenGO = GameObject.Instantiate(battleSceneScreenPrefab);

            _playerCharacterGO = GameObject.Instantiate(_battleScene.PlayerCharacterPoint.IsDistance ? distanceCharacterPrefab : meleeCharacterPrefab);
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
                GetLookAtTargetPosition = (characterEntity, isDistance) => _behaviour.GetLookAtTargetPosition(characterEntity, isDistance, true),
                GetAttackInput = (characterEntity, isDistance) => _behaviour.GetAttackInput(characterEntity, isDistance, true),
                GetDodgeInput = characterEntity => _behaviour.GetDodgeInput(characterEntity, true),
            }).AddTo(this);
            _playerCharacterEntity.Init();

            _enemyCharacterEntites = new();
            foreach (var enemyPoint in _battleScene.EnemyCharacterPoints)
            {
                var enemyCharacterGO = GameObject.Instantiate(enemyPoint.IsDistance ? distanceCharacterPrefab : meleeCharacterPrefab);
                enemyCharacterGO.transform.SetPositionAndRotation(enemyPoint.Point.position, enemyPoint.Point.rotation);
                _enemyCharactersGO.Add(enemyCharacterGO);

                var enemyCharacter = new Character.Entity(new Character.Entity.Ctx
                {
                    CharacterView = enemyCharacterGO,
                    Health = enemyPoint.Health,

                    GetTargetPosition = characterEntity => _behaviour.GetTargetPosition(characterEntity, false),
                    GetLookAtTargetPosition = (characterEntity, isDistance) => _behaviour.GetLookAtTargetPosition(characterEntity, isDistance, false),
                    GetAttackInput = (characterEntity, isDistance) => _behaviour.GetAttackInput(characterEntity, isDistance, false),
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