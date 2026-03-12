using System;
using System.Collections.Generic;
using BattleStory.SOData;
using Cysharp.Threading.Tasks;
using Disposable;

namespace BattleStory.Preloading
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<string, UniTask> GetAssetBundle;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public List<UniTask> GetPreloading(ScreenData[] screensPreloadData, BattleData[] battlesPreloadData)
        {
            var preloading = new List<UniTask>();
            if (screensPreloadData != null)
            {
                foreach (var preloadData in screensPreloadData)
                {
                    preloading.Add(_ctx.GetAssetBundle(preloadData.BackgroundBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.ScreenBundle.ScreenBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.VoiceAssetsName));
                }
            }
            if (battlesPreloadData != null)
            {
                foreach (var preloadData in battlesPreloadData)
                {
                    preloading.Add(_ctx.GetAssetBundle(preloadData.ScreenBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.SceneBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.MeleeCharacterBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.MeleeCharacterScreenBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.DistanceCharacterBundle.BundleName));
                    preloading.Add(_ctx.GetAssetBundle(preloadData.DistanceCharacterScreenBundle.BundleName));
                }
            }
            return preloading;
        }
    }
}

