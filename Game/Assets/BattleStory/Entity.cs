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

        internal Entity(Ctx ctx)
        {
            _ctx = ctx;

            Application.backgroundLoadingPriority = _defaultThreadPriority;
        }

        internal async UniTask Init()
        {
            var bundles = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                OnLog = _ctx.OnLog,
            }).AddTo(this);

            var loadingCtx = new Loading.Entity.Ctx
            {
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.LoadingData.BundleName, _ctx.Data.LoadingData.AssetName),
            };
            
            var loading = new Loading.Entity(loadingCtx).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await loading.Init();

            await loading.Show();

            ChaptersData chaptersData = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                chaptersData = await bundles.GetBundledSO<ChaptersData>(_ctx.Data.ChaptersData.BundleName, _ctx.Data.ChaptersData.AssetName);

            var chapterProcessCtx = new ChapterProcess.Ctx
            {
                ChaptersData = chaptersData,
                DefaultThreadPriority = _defaultThreadPriority,
                GetText = bundles.GetText,
                GetBundledPrefab = bundles.GetBundledPrefab,
                GetBundledSprite = bundles.GetBundledSprite,
                GetAssetBundle = bundles.GetAssetBundle,
                ShowLoading = loading.Show,
                HideLoading = loading.Hide,
            };
            var chapterProcess = new ChapterProcess(chapterProcessCtx).AddTo(this);
            chapterProcess.ShowChapterProcess(0, false, false, false, false, false).Forget();
        }
    }
}