using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using Novels.Location;
using SOData;
using UnityEngine;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _storyTextPath;
        [SerializeField] private string _novelsLocationsBundleName;
        [SerializeField] private string _novelsCharactersBundleName;
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private BundleData _settingData;
        [SerializeField] private BundleData _bubbleData;
        [SerializeField] private BundleData _locationScreenData;
        [SerializeField] private BundleData _videosScreenData;
        [SerializeField] private BundleData _characterScreenData;
        [SerializeField] private BundleData _notificationScreenData;

        internal readonly string StoryTextPath => _storyTextPath;
        internal readonly string NovelsLocationsBundleName => _novelsLocationsBundleName;
        internal readonly string NovelsCharactersBundleName => _novelsCharactersBundleName;
        internal readonly BundleData LoadingData => _loadingData;
        internal readonly BundleData SettingData => _settingData;
        internal readonly BundleData BubbleData => _bubbleData;
        internal readonly BundleData LocationScreenData => _locationScreenData;
        internal readonly BundleData VideosScreenData => _videosScreenData;
        internal readonly BundleData CharacterScreenData => _characterScreenData;
        internal readonly BundleData NotificationScreenData => _notificationScreenData;
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

        private string GetVideoPath(string assetName)
        {
            var result = $"{Application.streamingAssetsPath}/NovelsVideos/{assetName}.mp4";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
#endif
            return result;
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
                bundles.GetAssetBundle(_ctx.Data.NovelsLocationsBundleName),
                bundles.GetAssetBundle(_ctx.Data.NotificationScreenData.BundleName)
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
                var (storyTextTemp, bubbleTemp, locationScreenTemp, locationsBundlesTemp, notificationScreenTemp) = await secondPreloading;
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
                GetVideosList = () => bundles.GetBundledSO<VideosSO>(_ctx.Data.VideosScreenData.BundleName, _ctx.Data.VideosScreenData.AssetName),
                GetSpritePath = assetName => $"{assetName}.png",
                GetVideoURL = assetName => GetVideoPath(assetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await location.Init();

            var character = new Character.Entity(new Character.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.CharacterScreenData.BundleName, _ctx.Data.CharacterScreenData.AssetName),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsCharactersBundleName, assetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await character.Init();

            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                GetNotificationPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NotificationScreenData.BundleName, _ctx.Data.NotificationScreenData.AssetName),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await notification.Init();

            var waiting = new Waiting.Entity().AddTo(this);

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
                if (prefix.ToLower() == "звуки окружения") continue;

                if (prefix.ToLower() == "уведомление")
                {
                    notification.SetText(value).Forget();
                    continue;
                }

                if (prefix.ToLower() == "локация")
                {
                    Debug.Log(value);
                    await location.SetImage(value);
                    continue;
                }
                if (prefix.ToLower() == "камера")
                {
                    await location.SetCamera(value);
                    continue;
                }
                if (prefix.ToLower() == "ожидание")
                {
                    if (int.TryParse(value, out var seconds))
                        await waiting.Await(seconds);
                    continue;
                }

                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                character.SetImage(name, args).Forget();

                bubble.SetText(text);
                bubble.RemoveAllButtons();
                var choices = storyProcessor.GetChoices();
                if (choices.Count > 0)
                    bubble.ResetBackgroundButton();
                else if (string.IsNullOrEmpty(text))
                    bubbleDone.TrySetResult();
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