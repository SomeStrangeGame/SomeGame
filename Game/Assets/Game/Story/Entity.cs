using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using Game.SOData;
using UnityEngine;

namespace Game.Story
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public ScreenData MenuData;
            public Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            public Func<string, string, UniTask<Sprite>> GetBundledSprite;
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
            var screenBackgroundSpriteLoading = _ctx.GetBundledSprite(_ctx.MenuData.BackgroundBundle.BundleName, _ctx.MenuData.BackgroundBundle.AssetName);
            var screenPrefabGOLoading = _ctx.GetBundledPrefab(_ctx.MenuData.MenuBundle.BundleName, _ctx.MenuData.MenuBundle.AssetName);
            
            var (screenBackgroundSprite, screenPrefabGO) = await UniTask.WhenAll(
                screenBackgroundSpriteLoading,
                screenPrefabGOLoading
            );

            var screenGO = GameObject.Instantiate(screenPrefabGO);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.Setup(new View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                DescriptionText = _ctx.MenuData.DescriptionText,
                ButtonText = _ctx.MenuData.ButtonText,
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