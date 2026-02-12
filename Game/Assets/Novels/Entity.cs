using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using SOData;
using UnityEngine;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _storyTextPath;
        [SerializeField] private string _novelsLocationsBundleName;
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private BundleData _settingData;
        [SerializeField] private BundleData _bubbleData;
        [SerializeField] private BundleData _locationScreenData;

        internal readonly string StoryTextPath => _storyTextPath;
        internal readonly string NovelsLocationsBundleName => _novelsLocationsBundleName;
        internal readonly BundleData LoadingData => _loadingData;
        internal readonly BundleData SettingData => _settingData;
        internal readonly BundleData BubbleData => _bubbleData;
        internal readonly BundleData LocationScreenData => _locationScreenData;
    }

    internal class Entity : BaseDisposable
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

            //preloading init
            var firstPreloding = UniTask.WhenAll(
                bundles.GetAssetBundle(_ctx.Data.SettingData.BundleName)
            );
            var secondPreloading = UniTask.WhenAll(
                bundles.GetText(_ctx.Data.StoryTextPath),
                bundles.GetAssetBundle(_ctx.Data.BubbleData.BundleName),
                bundles.GetAssetBundle(_ctx.Data.LocationScreenData.BundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocationsBundleName)
            );

            await loading.Show();

            //preloading loading first
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await firstPreloding;

            var settingProcessCtx = new SettingProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                SettingData = _ctx.Data.SettingData,
                GetBundledPrefab = bundles.GetBundledPrefab,
                ShowLoading = loading.Show,
                HideLoading = loading.Hide,
            };
            var settingProcess = new SettingProcess(settingProcessCtx).AddTo(this);
            await settingProcess.ShowSettingProcess();

            //preloading loading second
            var storyText = string.Empty;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
            {
                var (storyTextTemp, bubbleTemp, locationScreenTemp, locationsBundlesTemp) = await secondPreloading;
                storyText = storyTextTemp;
            }

            var storyProcessor = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
            }).AddTo(this);

            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.BubbleData.BundleName, _ctx.Data.BubbleData.AssetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bubble.Init();

            var location = new Location.Entity(new Location.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.LocationScreenData.BundleName, _ctx.Data.LocationScreenData.AssetName),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsLocationsBundleName, assetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await location.Init();

            await loading.Hide();

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                storyProcessor.TryGetNextText(out var text);

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();

                if (prefix.ToLower() == "название") continue;
                if (prefix.ToLower() == "серия") continue;
                if (prefix.ToLower() == "жанры") continue;
                if (prefix.ToLower() == "аннотация") continue;
                if (prefix.ToLower() == "статы") continue;

                if (prefix.ToLower().Contains("клавиатура")) continue;

                if (prefix.ToLower() == "музыка") continue;
                if (prefix.ToLower() == "звук") continue;
                if (prefix.ToLower() == "локация")
                {
                    location.SetImage($"{value}.png").Forget();
                    continue;
                }

                bubble.SetText(text);

                bubble.RemoveAllButtons();
                var choices = storyProcessor.GetChoices();
                if (choices.Count > 0)
                    bubble.ResetBackgroundButton();
                else
                    bubble.SetBackgroundButton(() => bubbleDone.TrySetResult());
                foreach (var choice in choices)
                {
                    bubble.AddOrUpdateButton(choice.index, choice.text, id =>
                    {
                        storyProcessor.SetChoice(id);
                        bubbleDone.TrySetResult();
                    });
                }

                await bubble.Show();
                await bubbleDone.Task;
                await bubble.Hide();
            }
        }
    }
}