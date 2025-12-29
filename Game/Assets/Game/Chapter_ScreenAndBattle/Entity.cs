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
        [SerializeField] private string _menuBundleName;
        [SerializeField] private string _menuPrefabName;

        [SerializeField] private string _battleBundleName;
        [SerializeField] private string _battleScenePrefabName;
        [SerializeField] private string _battleScreenPrefabName;
        [SerializeField] private string _battleCameraDataName;

        internal readonly Chapter_OnlyScreen.Data.MenuData MenuStart => _menuStart;
        internal readonly Chapter_OnlyScreen.Data.MenuData MenuSuccess => _menuSuccess;
        internal readonly Chapter_OnlyScreen.Data.MenuData MenuFailed => _menuFailed;
        internal readonly string MenuBundleName => _menuBundleName;
        internal readonly string MenuPrefabName => _menuPrefabName;

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
            public Func<(string bundleName, string prefabName), UniTask<GameObject>> GetBundledPrefab;
            public Func<(string bundleName, string spriteName), UniTask<Sprite>> GetBundledSprite;
            public Func<(string bundleName, string soName), UniTask<CameraDataSO>> GetBundledCameraData;
        }

        private readonly UniTaskCompletionSource _startScreenToken;
        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly UniTaskCompletionSource<int> _token;
        private readonly Ctx _ctx;

        private View.Scene _battleScene;
        private View.Screen _battleScreen;

        private Character.Entity _playerCharacterEntity;
        private List<Character.Entity> _enemyCharacterEntites;
        private Character.Behaviour _behaviour;

        private Chapter_OnlyScreen.View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _startScreenToken = new();
            _battleToken = new();
            _token = new();
            _ctx = ctx;
        }

        public async UniTask InitStartScreen()
        {
            var screenBackgroundSprite = await _ctx.GetBundledSprite((_ctx.Data.MenuBundleName, _ctx.Data.MenuStart.BackgroundSpriteName));
            (await GetScreen()).Setup(new Chapter_OnlyScreen.View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                DescriptionText = _ctx.Data.MenuStart.DescriptionText,
                ButtonText = _ctx.Data.MenuStart.ButtonText,
                OnComplete = () => _startScreenToken.TrySetResult(),
            });
        }

        public void ReleaseStartScreen() => ReleaseScreen();

        public async UniTask WaitStartScreenResult() => await _startScreenToken.Task;

        public async UniTask InitBattle()
        {
            var battleScreenPrefab = await _ctx.GetBundledPrefab((_ctx.Data.BattleBundleName, _ctx.Data.BattleScreenPrefabName));
            var battleScreenGO = GameObject.Instantiate(battleScreenPrefab);
            _battleScreen = battleScreenGO.GetComponent<View.Screen>();

            var battleScenePrefab = await _ctx.GetBundledPrefab((_ctx.Data.BattleBundleName, _ctx.Data.BattleScenePrefabName));
            var battleSceneGO = GameObject.Instantiate(battleScenePrefab);
            _battleScene = battleSceneGO.GetComponent<View.Scene>();

            var cameraData = await _ctx.GetBundledCameraData((_ctx.Data.BattleBundleName, _ctx.Data.BattleCameraDataName));
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

        public async UniTask InitSuccessScreen()
        {
            var screenBackgroundSprite = await _ctx.GetBundledSprite((_ctx.Data.MenuBundleName, _ctx.Data.MenuSuccess.BackgroundSpriteName));
            (await GetScreen()).Setup(new Chapter_OnlyScreen.View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                DescriptionText = _ctx.Data.MenuSuccess.DescriptionText,
                ButtonText = _ctx.Data.MenuSuccess.ButtonText,
                OnComplete = () => _token.TrySetResult(1),
            });
        }

        public async UniTask InitFailedScreen()
        {
            var screenBackgroundSprite = await _ctx.GetBundledSprite((_ctx.Data.MenuBundleName, _ctx.Data.MenuFailed.BackgroundSpriteName));
            (await GetScreen()).Setup(new Chapter_OnlyScreen.View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                DescriptionText = _ctx.Data.MenuFailed.DescriptionText,
                ButtonText = _ctx.Data.MenuFailed.ButtonText,
                OnComplete = () => _token.TrySetResult(0),
            });
        }

        public async UniTask<int> WaitResult() => await _token.Task;

        private async UniTask<Chapter_OnlyScreen.View.Screen> GetScreen()
        {
            if (_screen == null)
            {
                var screenPrefabGO = await _ctx.GetBundledPrefab((_ctx.Data.MenuBundleName, _ctx.Data.MenuPrefabName));
                var go = GameObject.Instantiate(screenPrefabGO);
                _screen = go.GetComponent<Chapter_OnlyScreen.View.Screen>();
            }
            return _screen;
        }

        private void ReleaseScreen()
        {
            if (_screen != null) _screen.Release();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ReleaseScreen();
            ReleaseBattle();
        }
    }
}