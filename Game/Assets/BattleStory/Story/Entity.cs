using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace BattleStory.Story
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<UniTask<string>> GetTextAsset;
            public Func<UniTask<GameObject>> GetMenuPrefab;
            public Func<UniTask<Sprite>> GetBackgroundSprite;
            public Func<UniTask<AssetBundle>> GetAudioBundle;
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

        private readonly Ctx _ctx;

        private View.Screen _screen;
        private Ink.Runtime.Story _story;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var (screenBackgroundSprite, screenPrefabGO, story, audioBundle) = await UniTask.WhenAll(
                _ctx.GetBackgroundSprite(),
                _ctx.GetMenuPrefab(),
                _ctx.GetTextAsset(),
                _ctx.GetAudioBundle()
            );

            _story = new Ink.Runtime.Story(story);

            var screenGO = GameObject.Instantiate(screenPrefabGO);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.Setup(new View.Screen.Ctx
            {
                BackgroundSprite = screenBackgroundSprite,
                GetAudioClip = async assetName => (await audioBundle.LoadAssetAsync<AudioClip>(assetName)) as AudioClip, 
            });
        }

        public async UniTask WaitResult() 
        {
            while (_story.canContinue)
            {
                await _screen.TryProcessText(_story.Continue());
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) _screen.Release();
        }
    }
}