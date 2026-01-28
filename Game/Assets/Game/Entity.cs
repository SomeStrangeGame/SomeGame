using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using Game.SOData;
using UnityEngine;

namespace Game
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
                Data = _ctx.Data.LoadingData,
                GetBundledPrefab = data => _bundles.GetBundledPrefab(data.bundleName, data.prefabName),
            };
            
            _loading = new Loading.Entity(loadingCtx).AddTo(this);
            using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
            {
                await _loading.Init();
            }

            await _loading.Show();

            _chaptersData = await _bundles.GetBundledSO<ChaptersData>(_ctx.Data.ChaptersData.BundleName, _ctx.Data.ChaptersData.AssetName);
            ChapterProcess(0).Forget();
        }

        private async UniTask ShowMenuProcess(ScreenData menuData, ScreenData preloadData = null)
        {
            var ctx = new Story.Entity.Ctx
            {
                MenuData = menuData,
                GetBundledPrefab = _bundles.GetBundledPrefab,
                GetBundledSprite = _bundles.GetBundledSprite
            };
            using (var chapter = new Story.Entity(ctx).AddTo(this))
            {
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                {
                    await chapter.Init();
                }
                await _loading.Hide();
                if (preloadData != null)
                {
                    var preloadCtx = new Story.Entity.Ctx
                    {
                        MenuData = preloadData,
                        GetBundledPrefab = _bundles.GetBundledPrefab,
                        GetBundledSprite = _bundles.GetBundledSprite
                    };
                    using (var preload = new Story.Entity.Preload(preloadCtx).AddTo(this))
                    {
                        await preload.Process();
                        Debug.Log("PreloadDone");
                    }
                }
                await chapter.WaitResult();
                await _loading.Show();
            }
        }

        private async UniTask<int> ShowBattleProcess(BattleData data)
        {
            var result = 0;
            var ctx = new Battle.Entity.Ctx
            {
                Data = data,
                GetBundledPrefab = _bundles.GetBundledPrefab,
            };
            using (var chapter = new Battle.Entity(ctx).AddTo(this))
            {
                using (new BackgrounLoadingPriority(ThreadPriority.High, _defaultThreadPriority))
                {
                    await chapter.Init();
                }
                await _loading.Hide();
                result = await chapter.WaitBattleResult();
                await UniTask.Delay(3000);
                await _loading.Show();
                chapter.ReleaseBattle();
            }
            return result;
        }

        private async UniTask ChapterProcess(int index, bool skipIntro = false)
        {
            var chapterData = _chaptersData.Chapters[index];
            if (!skipIntro)
            {
                for (var i = 0; i < chapterData.IntroMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.IntroMenu.Length;
                    var preloadData = (!isLast) ? chapterData.IntroMenu[i + 1] : null;
                    await ShowMenuProcess(chapterData.IntroMenu[i], preloadData);
                }
            }

            foreach (var start in chapterData.StartMenu)
                await ShowMenuProcess(start);

            var battleResult = 0;
            foreach (var battle in chapterData.Battles)
            {
                battleResult = await ShowBattleProcess(battle);
                if (battleResult == 0)
                    break;
            }

            if (battleResult == 0) //failed
            {
                foreach (var failed in chapterData.FailedMenu)
                    await ShowMenuProcess(failed);
                ChapterProcess(index, true).Forget();
                return;
            }

            foreach (var success in chapterData.SuccessMenu)
                await ShowMenuProcess(success);

            index++;
            if (index < _chaptersData.Chapters.Length)
            {
                ChapterProcess(index).Forget();
                return;
            }

            ChapterProcess(0).Forget();
        }
    }
}