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
        public struct MenuScreenData
        {
            [SerializeField] private string _bundleMenuName;
            [SerializeField] private string _prefabMenuName;

            public readonly string BundleMenuName => _bundleMenuName;
            public readonly string PrefabMenuName => _prefabMenuName;
        }

        [Serializable]
        public struct MenuData
        {
            [SerializeField][TextArea(15, 250)] private string _descriptionText;
            [SerializeField] private string _buttonText;
            [SerializeField] private string _backgroundBundleName;
            [SerializeField] private string _backgroundSpriteName;
            
            public readonly string DescriptionText => _descriptionText;
            public readonly string ButtonText => _buttonText;
            public readonly string BackgroundBundleName => _backgroundBundleName;
            public readonly string BackgroundSpriteName => _backgroundSpriteName;
        }

        [SerializeField] private MenuScreenData _menuScreen;
        [SerializeField] private MenuData _menu;

        internal readonly MenuScreenData MenuScreen => _menuScreen;
        internal readonly MenuData Menu => _menu;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public Func<Data.MenuScreenData, UniTask<GameObject>> GetBundledPrefab;
            public Func<Data.MenuData, UniTask<Sprite>> GetBundledSprite;
        }

        private readonly UniTaskCompletionSource _token;
        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _token = new();
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var screenBackgroundSprite = await _ctx.GetBundledSprite(_ctx.Data.Menu);
            var screenPrefabGO = await _ctx.GetBundledPrefab(_ctx.Data.MenuScreen);
            var screenGO = GameObject.Instantiate(screenPrefabGO);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.Setup(new View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
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