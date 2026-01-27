using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Chapter_ScreenAndBattle
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private string _chapterName;
        [SerializeField] private Chapter_OnlyScreen.Data.MenuData _menuStart;
        [SerializeField] private Chapter_OnlyScreen.Data.MenuData _menuSuccess;
        [SerializeField] private Chapter_OnlyScreen.Data.MenuData _menuFailed;

        [SerializeField] private string _battleBundleName;
        [SerializeField] private string _battleScenePrefabName;
        [SerializeField] private string _battleScreenPrefabName;
        [SerializeField] private string _battleCameraDataName;

        public readonly Chapter_OnlyScreen.Data.MenuData MenuStart => _menuStart;
        public readonly Chapter_OnlyScreen.Data.MenuData MenuSuccess => _menuSuccess;
        public readonly Chapter_OnlyScreen.Data.MenuData MenuFailed => _menuFailed;

        internal readonly string BattleBundleName => _battleBundleName;
        internal readonly string BattleScenePrefabName => _battleScenePrefabName;
        internal readonly string BattleScreenPrefabName => _battleScreenPrefabName;
        internal readonly string BattleCameraDataName => _battleCameraDataName;
    }

    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            public Func<string, string, UniTask<CameraDataSO>> GetBundledCameraData;
        }

        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly Ctx _ctx;

        private View.Scene _battleScene;
        private View.Screen _battleScreen;

        private Character.Entity _playerCharacterEntity;
        private List<Character.Entity> _enemyCharacterEntites;
        private Character.Behaviour _behaviour;

        public Entity(Ctx ctx)
        {
            _battleToken = new();
            _ctx = ctx;
        }

        public async UniTask InitBattle()
        {
            var battleScreenPrefab = await _ctx.GetBundledPrefab(_ctx.Data.BattleBundleName, _ctx.Data.BattleScreenPrefabName);
            var battleScreenGO = GameObject.Instantiate(battleScreenPrefab);
            _battleScreen = battleScreenGO.GetComponent<View.Screen>();

            var battleScenePrefab = await _ctx.GetBundledPrefab(_ctx.Data.BattleBundleName, _ctx.Data.BattleScenePrefabName);
            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            var cameraData = await _ctx.GetBundledCameraData(_ctx.Data.BattleBundleName, _ctx.Data.BattleCameraDataName);
            var camera = new Camera.Entity(new Camera.Entity.Ctx
            {
                MoveOffset = cameraData.CamMoveOffset,
                MoveSpeed = cameraData.CamMoveSpeed,
                LookAtOffset = cameraData.CamLookAtOffset,
                LookAtSpeed = cameraData.CamLookAtSpeed,

                GetCameraTargetPosition = () => _battleScene.PlayerCharacter.transform.position,
            }).AddTo(this);
            await camera.Init();

            _playerCharacterEntity = new Character.Entity(new Character.Entity.Ctx
            {
                CharacterView = _battleScene.PlayerCharacter,
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
            foreach (var enemyCharacterView in _battleScene.EnemyCharacters)
            {
                var enemyCharacter = new Character.Entity(new Character.Entity.Ctx
                {
                    CharacterView = enemyCharacterView,
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

        public async UniTask<int> WaitBattleResult() => await _battleToken.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            ReleaseBattle();
        }
    }
}