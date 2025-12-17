using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Chapter_OnlyScreen
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

        [SerializeField] private MenuData _menu;
        [SerializeField] private GameObject _menuPrefab;

        internal readonly MenuData Menu => _menu;
        internal readonly GameObject MenuPrefab => _menuPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private readonly UniTaskCompletionSource _token;
        private readonly Ctx _ctx;

        private ChapterScreen.View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _token = new();
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.MenuPrefab);
            _screen = go.GetComponent<ChapterScreen.View.Screen>();
            _screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.Menu.BackgroundSprite,
                DescriptionText = _ctx.Data.Menu.DescriptionText,
                ButtonText = _ctx.Data.Menu.ButtonText,
                OnComplete = () => _token.TrySetResult(),
            });
        }

        public async UniTask WaitResult() => await _token.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}