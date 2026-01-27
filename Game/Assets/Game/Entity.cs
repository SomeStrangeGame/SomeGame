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

            await ShowMenuProcess(_ctx.Data.Chapter_intro.Menu, _ctx.Data.Chapter_intro.MenuScreen);
            ChapterProcess(0).Forget();
        }

        private async UniTask ShowMenuProcess(Chapter_OnlyScreen.Data.MenuData menuData, Chapter_OnlyScreen.Data.MenuScreenData menuScreenData)
        {
            var ctx = new Chapter_OnlyScreen.Entity.Ctx
            {
                MenuData = menuData,
                MenuScreenData = menuScreenData,
                GetBundledPrefab = _bundles.GetBundledPrefab,
                GetBundledSprite = _bundles.GetBundledSprite
            };
            using (var chapter = new Chapter_OnlyScreen.Entity(ctx).AddTo(this))
            {
                await chapter.Init();
                await _loading.Hide();
                await chapter.WaitResult();
                await _loading.Show();
            }
        }

        private async UniTask<int> ShowBattleProcess(int index)
        {
            var result = 0;
            var ctx = new Chapter_ScreenAndBattle.Entity.Ctx
            {
                Data = _ctx.Data.Chapters[index],
                GetBundledPrefab = _bundles.GetBundledPrefab,
                GetBundledCameraData = _bundles.GetBundledSO<Chapter_ScreenAndBattle.CameraDataSO>,
            };
            using (var chapter = new Chapter_ScreenAndBattle.Entity(ctx).AddTo(this))
            {
                await chapter.InitBattle();
                await _loading.Hide();
                result = await chapter.WaitBattleResult();
                await UniTask.Delay(3000);
                await _loading.Show();
                chapter.ReleaseBattle();
            }
            return result;
        }

        private async UniTask ChapterProcess(int index)
        {
            await ShowMenuProcess(_ctx.Data.Chapters[index].MenuStart, _ctx.Data.Chapters[index].MenuScreen);

            var battleResult = await ShowBattleProcess(index);
            if (battleResult == 0) //failed
            {
                await ShowMenuProcess(_ctx.Data.Chapters[index].MenuFailed, _ctx.Data.Chapters[index].MenuScreen);
                ChapterProcess(index).Forget();
                return;
            }

            await ShowMenuProcess(_ctx.Data.Chapters[index].MenuSuccess, _ctx.Data.Chapters[index].MenuScreen);

            index++;
            if (index < _ctx.Data.Chapters.Length)
            {
                ChapterProcess(index).Forget();
                return;
            }

            await ShowMenuProcess(_ctx.Data.Chapter_intro.Menu, _ctx.Data.Chapter_intro.MenuScreen);
            ChapterProcess(0).Forget();
        }
    }
}