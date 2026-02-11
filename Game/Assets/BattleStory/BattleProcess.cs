using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using BattleStory.SOData;
using UnityEngine;

namespace BattleStory
{
    internal class BattleProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal ThreadPriority DefaultThreadPriority;

            internal Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            internal Func<string, UniTask<AssetBundle>> GetAssetBundle;

            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;
        }

        private Ctx _ctx;

        internal BattleProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask<int> ShowBattleProcess(BattleData data)
        {
            return await ShowBattleProcessInternal(data, null, null);
        }

        internal async UniTask<int> ShowBattleProcess(BattleData data, params ScreenData[] screensPreloadData)
        {
            return await ShowBattleProcessInternal(data, screensPreloadData, null);
        }

        internal async UniTask<int> ShowBattleProcess(BattleData data, params BattleData[] battlesPreloadData)
        {
            return await ShowBattleProcessInternal(data, null, battlesPreloadData);
        }

        private async UniTask<int> ShowBattleProcessInternal(BattleData data, ScreenData[] screensPreloadData, BattleData[] battlesPreloadData)
        {
            var result = 0;
            var ctx = new Battle.Entity.Ctx
            {
                CameraData = data.Camera,
                GetBattleScenePrefab = () => _ctx.GetBundledPrefab(data.SceneBundle.BundleName, data.SceneBundle.AssetName),
                GetBattleScreenPrefab = () => _ctx.GetBundledPrefab(data.ScreenBundle.BundleName, data.ScreenBundle.AssetName),
                GetMeleeCharacterInputScreenPrefab = () => _ctx.GetBundledPrefab(data.MeleeCharacterScreenBundle.BundleName, data.MeleeCharacterScreenBundle.AssetName),
                GetMeleeCharacterPrefab = () => _ctx.GetBundledPrefab(data.MeleeCharacterBundle.BundleName, data.MeleeCharacterBundle.AssetName),
                GetDistanceCharacterInputScreenPrefab = () => _ctx.GetBundledPrefab(data.DistanceCharacterScreenBundle.BundleName, data.DistanceCharacterScreenBundle.AssetName),
                GetDistanceCharacterPrefab = () => _ctx.GetBundledPrefab(data.DistanceCharacterBundle.BundleName, data.DistanceCharacterBundle.AssetName),
            };
            using (var battle = new Battle.Entity(ctx).AddTo(this))
            {
                using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                    await battle.Init();
                
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
                result = await battle.WaitBattleResult();
                await UniTask.Delay(3000);
                await _ctx.ShowLoading();
                using (new LoadingPriority.Entity(ThreadPriority.High, _ctx.DefaultThreadPriority))
                    await UniTask.WhenAll(preloading);
                battle.ReleaseBattle();
            }
            return result;
        }
    }
}
