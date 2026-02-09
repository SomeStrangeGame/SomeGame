using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using BattleStory.SOData;
using UnityEngine;

namespace BattleStory
{
    internal class BackgrounLoadingPriority : IDisposable
    {
        private ThreadPriority _defaultPriority;

        public BackgrounLoadingPriority(ThreadPriority currentPriority, ThreadPriority defaultPriority)
        {
            _defaultPriority = defaultPriority;
            Application.backgroundLoadingPriority = currentPriority;
        }

        public void Dispose()
        {
            Application.backgroundLoadingPriority = _defaultPriority;
        }
    }

    [Serializable]
    internal struct Data
    {
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private BundleData _chaptersData;

        internal readonly BundleData LoadingData => _loadingData;
        internal readonly BundleData ChaptersData => _chaptersData;
    }

    internal sealed class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        private readonly Ctx _ctx;
        private readonly Bundles.Entity _bundles;
        private Loading.Entity _loading;
        private ChaptersData _chaptersData;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            _bundles = new Bundles.Entity().AddTo(this);

            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => _bundles.GetBundledPrefab(_ctx.Data.LoadingData.BundleName, _ctx.Data.LoadingData.AssetName),
            };
            
            _loading = new Loading.Entity(loadingCtx).AddTo(this);
            using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                await _loading.Init();

            await _loading.Show();

            using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                _chaptersData = await _bundles.GetBundledSO<ChaptersData>(_ctx.Data.ChaptersData.BundleName, _ctx.Data.ChaptersData.AssetName);
            ChapterProcess(0).Forget();
        }

        private async UniTask ShowMenuProcess(ScreenData data, params ScreenData[] screensPreloadData)
        {
            await ShowMenuProcess(data, screensPreloadData, null);
        }

        private async UniTask ShowMenuProcess(ScreenData data, params BattleData[] battlesPreloadData)
        {
            await ShowMenuProcess(data, null, battlesPreloadData);
        }

        private async UniTask ShowMenuProcess(ScreenData data, ScreenData[] screensPreloadData = null, BattleData[] battlesPreloadData = null)
        {
            var ctx = new Story.Entity.Ctx
            {
                GetTextAsset = () => _bundles.GetText($"Texts/{data.TextAssetName}.ink.json"),
                GetMenuPrefab = () => _bundles.GetBundledPrefab(data.ScreenBundle.ScreenBundle.BundleName, data.ScreenBundle.ScreenBundle.AssetName),
                GetBackgroundSprite = () => _bundles.GetBundledSprite(data.BackgroundBundle.BundleName, data.BackgroundBundle.AssetName),
                GetAudioBundle = () => _bundles.GetAssetBundle(data.VoiceAssetsName)
            };
            using (var chapter = new Story.Entity(ctx).AddTo(this))
            {
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                    await chapter.Init();
                var preloading = new List<UniTask>();
                if (screensPreloadData != null)
                {
                    foreach (var preloadData in screensPreloadData)
                    {
                        var preloadCtx = new Story.Entity.Preload.Ctx
                        {
                            GetAssets = () =>
                            {
                                return new List<UniTask>
                                {
                                    _bundles.GetAssetBundle(preloadData.BackgroundBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.ScreenBundle.ScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.VoiceAssetsName),
                                };
                            },
                        };
                        using (var preload = new Story.Entity.Preload(preloadCtx).AddTo(this))
                            preloading.Add(preload.Process());
                    }
                }
                if (battlesPreloadData != null)
                {
                    foreach (var preloadData in battlesPreloadData)
                    {
                        var preloadCtx = new Battle.Entity.Preload.Ctx
                        {
                            GetAssets = () =>
                            {
                                return new List<UniTask>
                                {
                                    _bundles.GetAssetBundle(preloadData.ScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.SceneBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.MeleeCharacterBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.MeleeCharacterScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.DistanceCharacterBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.DistanceCharacterScreenBundle.BundleName),
                                };
                            },
                        };
                        using (var preload = new Battle.Entity.Preload(preloadCtx).AddTo(this))
                            preloading.Add(preload.Process());
                    }
                }
                await _loading.Hide();
                await chapter.WaitResult(); 
                await _loading.Show();
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                    await UniTask.WhenAll(preloading);
            }
        }

        private async UniTask<int> ShowBattleProcess(BattleData data, params ScreenData[] screensPreloadData)
        {
            return await ShowBattleProcess(data, screensPreloadData, null);
        }

        private async UniTask<int> ShowBattleProcess(BattleData data, params BattleData[] battlesPreloadData)
        {
            return await ShowBattleProcess(data, null, battlesPreloadData);
        }

        private async UniTask<int> ShowBattleProcess(BattleData data, ScreenData[] screensPreloadData = null, BattleData[] battlesPreloadData = null)
        {
            var result = 0;
            var ctx = new Battle.Entity.Ctx
            {
                CameraData = data.Camera,
                GetBattleScenePrefab = () => _bundles.GetBundledPrefab(data.SceneBundle.BundleName, data.SceneBundle.AssetName),
                GetBattleScreenPrefab = () => _bundles.GetBundledPrefab(data.ScreenBundle.BundleName, data.ScreenBundle.AssetName),
                GetMeleeCharacterInputScreenPrefab = () => _bundles.GetBundledPrefab(data.MeleeCharacterScreenBundle.BundleName, data.MeleeCharacterScreenBundle.AssetName),
                GetMeleeCharacterPrefab = () => _bundles.GetBundledPrefab(data.MeleeCharacterBundle.BundleName, data.MeleeCharacterBundle.AssetName),
                GetDistanceCharacterInputScreenPrefab = () => _bundles.GetBundledPrefab(data.DistanceCharacterScreenBundle.BundleName, data.DistanceCharacterScreenBundle.AssetName),
                GetDistanceCharacterPrefab = () => _bundles.GetBundledPrefab(data.DistanceCharacterBundle.BundleName, data.DistanceCharacterBundle.AssetName),
            };
            using (var battle = new Battle.Entity(ctx).AddTo(this))
            {
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                    await battle.Init();
                var preloading = new List<UniTask>();
                if (screensPreloadData != null)
                {
                    foreach (var preloadData in screensPreloadData)
                    {
                        var preloadCtx = new Story.Entity.Preload.Ctx
                        {
                            GetAssets = () =>
                            {
                                return new List<UniTask>
                                {
                                    _bundles.GetAssetBundle(preloadData.BackgroundBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.ScreenBundle.ScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.VoiceAssetsName)
                                };
                            },
                        };
                        using (var preload = new Story.Entity.Preload(preloadCtx).AddTo(this))
                            preloading.Add(preload.Process());
                    }
                }
                if (battlesPreloadData != null)
                {
                    foreach (var preloadData in battlesPreloadData)
                    {
                        var preloadCtx = new Battle.Entity.Preload.Ctx
                        {
                            GetAssets = () =>
                            {
                                return new List<UniTask>
                                {
                                    _bundles.GetAssetBundle(preloadData.ScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.SceneBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.MeleeCharacterBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.MeleeCharacterScreenBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.DistanceCharacterBundle.BundleName),
                                    _bundles.GetAssetBundle(preloadData.DistanceCharacterScreenBundle.BundleName),
                                };
                            },
                        };
                        using (var preload = new Battle.Entity.Preload(preloadCtx).AddTo(this))
                            preloading.Add(preload.Process());
                    }
                }
                await _loading.Hide();
                result = await battle.WaitBattleResult();
                await UniTask.Delay(3000);
                await _loading.Show();
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                    await UniTask.WhenAll(preloading);
                battle.ReleaseBattle();
            }
            return result;
        }

        private async UniTask ChapterProcess(int index, bool skipIntro = false, bool skipStart = false, bool skipBattle = false, bool skipFailed = false, bool skipSuccess = false)
        {
            var chapterData = _chaptersData.Chapters[index];
            if (!skipIntro)
            {
                for (var i = 0; i < chapterData.IntroMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.IntroMenu.Length;
                    if (!isLast)
                        await ShowMenuProcess(chapterData.IntroMenu[i], chapterData.IntroMenu[i + 1]);
                    else if (chapterData.StartMenu.Length > 0) 
                        await ShowMenuProcess(chapterData.IntroMenu[i], chapterData.StartMenu[0]);
                    else
                        await ShowMenuProcess(chapterData.IntroMenu[i]);
                    
                }
            }

            if (!skipStart)
            {
                for (var i = 0 ; i < chapterData.StartMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.StartMenu.Length;
                    if (!isLast)
                        await ShowMenuProcess(chapterData.StartMenu[i], chapterData.StartMenu[i + 1]);
                    else if (chapterData.Battles.Length > 0)
                        await ShowMenuProcess(chapterData.StartMenu[i], chapterData.Battles[0]);
                    else
                        await ShowMenuProcess(chapterData.StartMenu[i]);
                }
            }

            var battleResult = 1;
            if (!skipBattle)
            {
                for (var i = 0 ; i < chapterData.Battles.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.Battles.Length;
                    if (!isLast)
                        battleResult = await ShowBattleProcess(chapterData.Battles[i], chapterData.Battles[i + 1]);
                    else if (chapterData.FailedMenu.Length > 0 && chapterData.SuccessMenu.Length > 0)
                        battleResult = await ShowBattleProcess(chapterData.Battles[i], chapterData.FailedMenu[0], chapterData.SuccessMenu[0]);
                    else
                        battleResult = await ShowBattleProcess(chapterData.Battles[i]);
                    if (battleResult == 0)
                        break;
                }
            }

            if (battleResult == 0) //failed
            {
                if (!skipFailed)
                {
                    for (var i = 0 ; i < chapterData.FailedMenu.Length; i++)
                    {
                        var isLast = i + 1 >= chapterData.FailedMenu.Length;
                        if (!isLast)
                            await ShowMenuProcess(chapterData.FailedMenu[i], chapterData.FailedMenu[i + 1]);
                        else
                            await ShowMenuProcess(chapterData.FailedMenu[i]);
                    }
                }

                ChapterProcess(index, true).Forget();
                return;
            }

            if (!skipSuccess)
            {
                for (var i = 0; i < chapterData.SuccessMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.SuccessMenu.Length;
                    if (!isLast)
                        await ShowMenuProcess(chapterData.SuccessMenu[i], chapterData.SuccessMenu[i + 1]);
                    else
                        await ShowMenuProcess(chapterData.SuccessMenu[i]);
                }
            }

            index++;
            if (index < _chaptersData.Chapters.Length)
            {
                ChapterProcess(index).Forget();
                return;
            }

            ChapterProcess(0).Forget(); //restart
        }
    }
}