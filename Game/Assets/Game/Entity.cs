using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private Chapter_OnlyScreen.Data _chapter_intro;
        [SerializeField] private Chapter_ScreenAndBattle.Data[] _chapters;

        internal readonly Loading.Data LoadingData => _loadingData;
        internal readonly Chapter_OnlyScreen.Data Chapter_intro => _chapter_intro;
        internal readonly Chapter_ScreenAndBattle.Data[] Chapters => _chapters;
    }

    internal sealed partial class Entity : BaseDisposable
    {
        internal struct Ctx
        {
            internal Data Data;
        }

        private readonly Ctx _ctx;
        private readonly Bundles.Entity _bundles;
        private Loading.Entity _loading;

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;
            _bundles = new Bundles.Entity().AddTo(this);
        }

        internal async UniTask Init()
        {
            var loadingCtx = new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,
                GetBundledPrefab = data => _bundles.GetBundledPrefab(data.bundleName, data.prefabName),
            };
            
            _loading = new Loading.Entity(loadingCtx).AddTo(this);
            await _loading.Init();
            await _loading.Show();

            var chapterCtx = new Chapter_OnlyScreen.Entity.Ctx
            {
                Data = _ctx.Data.Chapter_intro,
                GetBundledPrefab = data => _bundles.GetBundledPrefab(data.BundleMenuName, data.PrefabMenuName),
                GetBundledSprite = data => _bundles.GetBundledSprite(data.BackgroundBundleName, data.BackgroundSpriteName)
            };
            ChapterIntroProcess(chapterCtx).Forget();
        }

        private async UniTask ChapterIntroProcess(Chapter_OnlyScreen.Entity.Ctx ctx)
        {
            using (var chapter = new Chapter_OnlyScreen.Entity(ctx).AddTo(this))
            {
                await chapter.Init();
                await _loading.Hide();
                await chapter.WaitResult();
                await _loading.Show();
            }

            ChapterBattleProcess(0).Forget();
        }

        private async UniTask ChapterBattleProcess(int index)
        {
            var ctx = new Chapter_ScreenAndBattle.Entity.Ctx
            {
                Data = _ctx.Data.Chapters[index],
                GetBundledPrefab = data => _bundles.GetBundledPrefab(data.bundleName, data.prefabName),
                GetBundledSprite = data => _bundles.GetBundledSprite(data.bundleName, data.spriteName),
                GetBundledCameraData = data => _bundles.GetBundledSO<Chapter_ScreenAndBattle.CameraDataSO>(data.bundleName, data.soName),
            };

            using (var chapter = new Chapter_ScreenAndBattle.Entity(ctx).AddTo(this))
            {
                await chapter.InitStartScreen();
                await _loading.Hide();
                await chapter.WaitStartScreenResult();
                await _loading.Show();

                chapter.ReleaseStartScreen();

                await chapter.InitBattle();
                await _loading.Hide();

                var battleResult = await chapter.WaitBattleResult();
                await UniTask.Delay(3000);
                await _loading.Show();

                chapter.ReleaseBattle();

                if (battleResult == 0) //failed
                {
                    await chapter.InitFailedScreen();
                    await _loading.Hide();
                    await chapter.WaitResult();
                    
                    ChapterBattleProcess(index).Forget();
                }
                else //success
                {
                    await chapter.InitSuccessScreen();
                    await _loading.Hide();
                    await chapter.WaitResult();
                    
                    index++;
                    if (index >= _ctx.Data.Chapters.Length)
                    {
                        var chapterCtx = new Chapter_OnlyScreen.Entity.Ctx
                        {
                            Data = _ctx.Data.Chapter_intro,
                            GetBundledPrefab = data => _bundles.GetBundledPrefab(data.BundleMenuName, data.PrefabMenuName),
                            GetBundledSprite = data => _bundles.GetBundledSprite(data.BackgroundBundleName, data.BackgroundSpriteName)
                        };
                        ChapterIntroProcess(chapterCtx).Forget();
                    }
                    else
                    {
                        ChapterBattleProcess(index).Forget();
                    }
                }
            }
        }
    }
}