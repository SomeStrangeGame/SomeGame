using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Chapter_1
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

        [SerializeField] private MenuData _menuStart;
        [SerializeField] private MenuData _menuSuccess;
        [SerializeField] private MenuData _menuFailed;
        [SerializeField] private GameObject _menuPrefab;

        internal readonly MenuData MenuStart => _menuStart;
        internal readonly MenuData MenuSuccess => _menuSuccess;
        internal readonly MenuData MenuFailed => _menuFailed;

        internal readonly GameObject MenuPrefab => _menuPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private readonly UniTaskCompletionSource<int> _firstToken;
        private readonly UniTaskCompletionSource<int> _battleToken;
        private readonly UniTaskCompletionSource<int> _token;
        private readonly Ctx _ctx;

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
            _firstToken = new();
            _battleToken = new();
            _token = new();
            _ctx = ctx;
        }

        public async UniTask InitStart()
        {
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuStart.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuStart.DescriptionText,
                ButtonText = _ctx.Data.MenuStart.ButtonText,
                OnComplete = result => _firstToken.TrySetResult(result),
            });
        }

        public async UniTask<int> WaitFirstResult() => await _firstToken.Task;

        public async UniTask InitSuccess()
        {
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuSuccess.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuSuccess.DescriptionText,
                ButtonText = _ctx.Data.MenuSuccess.ButtonText,
                OnComplete = _ => _token.TrySetResult(1),
            });
        }

        public async UniTask InitFailed()
        {
            Screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.MenuFailed.BackgroundSprite,
                DescriptionText = _ctx.Data.MenuFailed.DescriptionText,
                ButtonText = _ctx.Data.MenuFailed.ButtonText,
                OnComplete = _ => _token.TrySetResult(2),
            });
        }

        public async UniTask<int> WaitResult() => await _token.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}