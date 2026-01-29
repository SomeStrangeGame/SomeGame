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
            public ScreenData Data;
            public Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            public Func<string, string, UniTask<Sprite>> GetBundledSprite;
        }

        public sealed class Preload : BaseDisposable
        {
            public struct Ctx
            {
                public ScreenData Data;
                public Func<string, UniTask<AssetBundle>> GetAssets;
            }

            private Ctx _ctx;

            public Preload(Ctx ctx)
            {
                _ctx = ctx;
            }

            public async UniTask Process()
            {
                var screenBackgroundAssetLoading = _ctx.GetAssets(_ctx.Data.BackgroundBundle.BundleName);
                var screenMenuAssetLoading = _ctx.GetAssets(_ctx.Data.MenuBundle.BundleName);
                
                await UniTask.WhenAll(
                    screenBackgroundAssetLoading,
                    screenMenuAssetLoading
                );
            }
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
            var screenBackgroundSpriteLoading = _ctx.GetBundledSprite(_ctx.Data.BackgroundBundle.BundleName, _ctx.Data.BackgroundBundle.AssetName);
            var screenPrefabGOLoading = _ctx.GetBundledPrefab(_ctx.Data.MenuBundle.BundleName, _ctx.Data.MenuBundle.AssetName);
            
            var (screenBackgroundSprite, screenPrefabGO) = await UniTask.WhenAll(
                screenBackgroundSpriteLoading,
                screenPrefabGOLoading
            );

            var screenGO = GameObject.Instantiate(screenPrefabGO);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.Setup(new View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                DescriptionText = _ctx.Data.DescriptionText,
                ButtonText = _ctx.Data.ButtonText,
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