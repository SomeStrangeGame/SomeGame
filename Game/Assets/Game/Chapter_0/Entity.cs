using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Chapter_0
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField][TextArea(15, 250)] private string _descriptionText;
        [SerializeField] private string _buttonText;
        [SerializeField] private GameObject _menuPrefab;

        internal readonly Sprite BackgroundSprite => _backgroundSprite;
        internal readonly string DescriptionText => _descriptionText;
        internal readonly string ButtonText => _buttonText;
        internal readonly GameObject MenuPrefab => _menuPrefab;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private readonly UniTaskCompletionSource<int> _someToken;
        private readonly Ctx _ctx;

        private ChapterScreen.View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _someToken = new();
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var go = GameObject.Instantiate(_ctx.Data.MenuPrefab);
            _screen = go.GetComponent<ChapterScreen.View.Screen>();
            _screen.Setup(new ChapterScreen.View.Screen.Ctx
            {
                BackgroundSprite = _ctx.Data.BackgroundSprite,
                DescriptionText = _ctx.Data.DescriptionText,
                ButtonText = _ctx.Data.ButtonText,
                OnComplete = result => _someToken.TrySetResult(result),
            });
        }

        public async UniTask<int> WaitResult() => await _someToken.Task;

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}