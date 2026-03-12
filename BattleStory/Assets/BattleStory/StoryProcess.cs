using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using BattleStory.SOData;
using UnityEngine;

namespace BattleStory
{
    internal class StoryProcess : BaseDisposable
    {
        internal struct Ctx
        {
            public bool SkipVoice;

            internal ThreadPriority DefaultThreadPriority;

            internal Func<string, UniTask<string>> GetText;
            internal Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            internal Func<string, string, UniTask<Sprite>> GetBundledSprite;
            internal Func<string, UniTask<AssetBundle>> GetAssetBundle;

            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;
        }

        private Ctx _ctx;

        internal StoryProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowMenuProcess(ScreenData data)
        {
            await ShowMenuProcessInternal(data, null, null);
        }

        internal async UniTask ShowMenuProcess(ScreenData data, params ScreenData[] screensPreloadData)
        {
            await ShowMenuProcessInternal(data, screensPreloadData, null);
        }

        internal async UniTask ShowMenuProcess(ScreenData data, params BattleData[] battlesPreloadData)
        {
            await ShowMenuProcessInternal(data, null, battlesPreloadData);
        }

        private async UniTask ShowMenuProcessInternal(ScreenData data, ScreenData[] screensPreloadData, BattleData[] battlesPreloadData)
        {
            var ctx = new Story.Entity.Ctx
            {
                SkipVoice = _ctx.SkipVoice,
                GetTextAsset = () => _ctx.GetText($"BattleStoryTexts/{data.TextAssetName}.ink.json"),
                GetMenuPrefab = () => _ctx.GetBundledPrefab(data.ScreenBundle.ScreenBundle.BundleName, data.ScreenBundle.ScreenBundle.AssetName),
                GetBackgroundSprite = () => _ctx.GetBundledSprite(data.BackgroundBundle.BundleName, data.BackgroundBundle.AssetName),
                GetAudioBundle = () => _ctx.GetAssetBundle(data.VoiceAssetsName)
            };
            using (var chapter = new Story.Entity(ctx).AddTo(this))
            {
                using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                    await chapter.Init();

                var preloading = new List<UniTask>();
                var preloadingCtx = new Preloading.Entity.Ctx
                {
                    GetAssetBundle = path => _ctx.GetAssetBundle(path),
                };
                using(var preloadingEntity = new Preloading.Entity(preloadingCtx).AddTo(this))
                {
                    preloading.AddRange(preloadingEntity.GetPreloading(screensPreloadData, battlesPreloadData));
                }

                await _ctx.HideLoading();
                await chapter.WaitResult(); 
                await _ctx.ShowLoading();
                using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                    await UniTask.WhenAll(preloading);
            }
        }
    }
}

