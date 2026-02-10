using System;
using Cysharp.Threading.Tasks;
using Disposable;
using BattleStory.SOData;
using UnityEngine;

namespace BattleStory
{
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
            public Action<(LogType type, string message)> OnLog;
        }

        private const ThreadPriority _defaultThreadPriority = ThreadPriority.Low;

        private readonly Ctx _ctx;
        private readonly Bundles.Entity _bundles;
        private Loading.Entity _loading;
        private ChaptersData _chaptersData;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            _bundles = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                OnLog = _ctx.OnLog,
            }).AddTo(this);

            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => _bundles.GetBundledPrefab(_ctx.Data.LoadingData.BundleName, _ctx.Data.LoadingData.AssetName),
            };
            
            _loading = new Loading.Entity(loadingCtx).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await _loading.Init();

            await _loading.Show();

            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                _chaptersData = await _bundles.GetBundledSO<ChaptersData>(_ctx.Data.ChaptersData.BundleName, _ctx.Data.ChaptersData.AssetName);
            ChapterProcess().Forget();
        }

        private async UniTask ChapterProcess(int index = 0, bool skipIntro = false, bool skipStart = false, bool skipBattle = false, bool skipFailed = false, bool skipSuccess = false)
        {
            var storyProcessCtx = new StoryProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                GetText = _bundles.GetText,
                GetBundledPrefab = _bundles.GetBundledPrefab,
                GetBundledSprite = _bundles.GetBundledSprite,
                GetAssetBundle = _bundles.GetAssetBundle,
                ShowLoading = _loading.Show,
                HideLoading = _loading.Hide,
            };

            var battleProcessCtx = new BattleProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                GetBundledPrefab = _bundles.GetBundledPrefab,
                GetAssetBundle = _bundles.GetAssetBundle,
                ShowLoading = _loading.Show,
                HideLoading = _loading.Hide,
            };

            var chapterData = _chaptersData.Chapters[index];
            
            if (!skipIntro)
            {
                for (var i = 0; i < chapterData.IntroMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.IntroMenu.Length;
                    if (!isLast)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i], chapterData.IntroMenu[i + 1]);
                    else if (chapterData.StartMenu.Length > 0) 
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i], chapterData.StartMenu[0]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i]);
                }
            }

            if (!skipStart)
            {
                for (var i = 0 ; i < chapterData.StartMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.StartMenu.Length;
                    if (!isLast)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i], chapterData.StartMenu[i + 1]);
                    else if (chapterData.Battles.Length > 0)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i], chapterData.Battles[0]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i]);
                }
            }

            var battleResult = 1;
            if (!skipBattle)
            {
                for (var i = 0 ; i < chapterData.Battles.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.Battles.Length;
                    if (!isLast)
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i], chapterData.Battles[i + 1]);
                    else if (chapterData.FailedMenu.Length > 0 && chapterData.SuccessMenu.Length > 0)
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i], chapterData.FailedMenu[0], chapterData.SuccessMenu[0]);
                    else
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i]);
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
                            using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                                await storyProcess.ShowMenuProcess(chapterData.FailedMenu[i], chapterData.FailedMenu[i + 1]);
                        else
                            using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                                await storyProcess.ShowMenuProcess(chapterData.FailedMenu[i]);
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
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.SuccessMenu[i], chapterData.SuccessMenu[i + 1]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.SuccessMenu[i]);
                }
            }

            index++;
            if (index < _chaptersData.Chapters.Length)
            {
                ChapterProcess(index).Forget();
                return;
            }

            ChapterProcess().Forget(); //restart
        }
    }
}