using System;
using System.Collections.Generic;
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
            public Func<BundleData, UniTask<GameObject>> GetBundledPrefab;
            public Func<BundleData, UniTask<Sprite>> GetBundledSprite;
        }

        public sealed class Preload : BaseDisposable
        {
            public struct Ctx
            {
                public Func<List<UniTask>> GetAssets;
            }

            private Ctx _ctx;

            public Preload(Ctx ctx)
            {
                _ctx = ctx;
            }

            public async UniTask Process()
            {
                await UniTask.WhenAll(_ctx.GetAssets());
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
            var screenBackgroundSpriteLoading = _ctx.GetBundledSprite(_ctx.Data.BackgroundBundle);
            var screenPrefabGOLoading = _ctx.GetBundledPrefab(_ctx.Data.MenuBundle);
            
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