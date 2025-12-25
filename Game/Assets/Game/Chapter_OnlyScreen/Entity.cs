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
            [SerializeField][TextArea(15, 250)] private string _descriptionText;
            [SerializeField] private string _buttonText;
            [SerializeField] private string _backgroundSpriteName;
            
            public readonly string DescriptionText => _descriptionText;
            public readonly string ButtonText => _buttonText;
            public readonly string BackgroundSpriteName => _backgroundSpriteName;
        }

        [SerializeField] private MenuData _menu;
        [SerializeField] private string _bundleName;
        [SerializeField] private string _menuPrefabName;

        internal readonly MenuData Menu => _menu;
        internal readonly string BundleName => _bundleName;
        internal readonly string LoadingPrefabName => _menuPrefabName;
    }
    
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public Func<(string bundleName, string prefabName), UniTask<GameObject>> GetBundledPrefab;
            public Func<(string bundleName, string spriteName), UniTask<Sprite>> GetBundledSprite;
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
            var screenBackgroundSprite = await _ctx.GetBundledSprite((_ctx.Data.BundleName, _ctx.Data.Menu.BackgroundSpriteName));
            var screenPrefabGO = await _ctx.GetBundledPrefab((_ctx.Data.BundleName, _ctx.Data.LoadingPrefabName));
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