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
        [Serializable]
        public struct MenuData
        {
            [SerializeField] private Sprite _backgroundSprite;
            [SerializeField][TextArea(15, 250)] private string _descriptionText;
            [SerializeField] private string _buttonText;

            internal readonly Sprite BackgroundSprite => _backgroundSprite;
            internal readonly string DescriptionText => _descriptionText;
            internal readonly string ButtonText => _buttonText;
        }

        [Serializable]
        [CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraData")]
        public class CameraDataSO : ScriptableObject
        {
            [SerializeField] private Vector3 _camMoveOffset;
            [SerializeField] private float _camMoveSpeed;
            [SerializeField] private Vector3 _camLookAtOffset;
            [SerializeField] private float _camLookAtSpeed;

            internal Vector3 CamMoveOffset => _camMoveOffset;
            internal float CamMoveSpeed => _camMoveSpeed;
            internal Vector3 CamLookAtOffset => _camLookAtOffset;
            internal float CamLookAtSpeed => _camLookAtSpeed;
        }

        [SerializeField] private string _chapterName;
        [SerializeField] private MenuData _menuStart;
        [SerializeField] private MenuData _menuSuccess;
        [SerializeField] private MenuData _menuFailed;
        [SerializeField] private GameObject _menuPrefab;
        [SerializeField] private CameraDataSO _cameraSO;

        [SerializeField] private GameObject _battleScenePrefab;
        [SerializeField] private GameObject _battleSceneScreenPrefab;

        internal readonly MenuData MenuStart => _menuStart;
        internal readonly MenuData MenuSuccess => _menuSuccess;
        internal readonly MenuData MenuFailed => _menuFailed;
        internal readonly GameObject MenuPrefab => _menuPrefab;

        internal readonly CameraDataSO Camera => _cameraSO;

        internal readonly GameObject BattleScenePrefab => _battleScenePrefab;
        internal readonly GameObject BattleSceneScreenPrefab => _battleSceneScreenPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
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

        private ChapterScreen.View.Screen _screen;
        private ChapterScreen.View.Screen Screen
        {
            get
            {
                if (_screen == null)
                {
                    var go = GameObject.Instantiate(_ctx.Data.MenuPrefab);
                    _screen = go.GetComponent<ChapterScreen.View.Screen>();
                }
                return _screen;
            }
        }

        public Entity(Ctx ctx)
        {
            _startScreenToken = new();
            _battleToken = new();
            _token = new();
            _ctx = ctx;
        }

        public async UniTask InitStartScreen()
        {
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuStart.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuStart.DescriptionText,
                ButtonText = _ctx.Data.MenuStart.ButtonText,
                OnComplete = () => _startScreenToken.TrySetResult(),
            });
        }

        public void ReleaseStartScreen() => ReleaseScreen();

        public async UniTask WaitStartScreenResult() => await _startScreenToken.Task;

        public async UniTask InitBattle()
        {
            var go = GameObject.Instantiate(_ctx.Data.BattleSceneScreenPrefab);
            _battleScreen = go.GetComponent<View.Screen>();

            go = GameObject.Instantiate(_ctx.Data.BattleScenePrefab);
            _battleScene = go.GetComponent<View.Scene>();

            var camera = new Camera.Entity(new Camera.Entity.Ctx
            {
                MoveOffset = _ctx.Data.Camera.CamMoveOffset,
                MoveSpeed = _ctx.Data.Camera.CamMoveSpeed,
                LookAtOffset = _ctx.Data.Camera.CamLookAtOffset,
                LookAtSpeed = _ctx.Data.Camera.CamLookAtSpeed,

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
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuSuccess.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuSuccess.DescriptionText,
                ButtonText = _ctx.Data.MenuSuccess.ButtonText,
                OnComplete = () => _token.TrySetResult(1),
            });
        }

        public async UniTask InitFailedScreen()
        {
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuFailed.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuFailed.DescriptionText,
                ButtonText = _ctx.Data.MenuFailed.ButtonText,
                OnComplete = () => _token.TrySetResult(0),
            });
        }

        public async UniTask<int> WaitResult() => await _token.Task;

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