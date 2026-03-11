using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using Novels.Location;
using SOData;
using UnityEngine;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _prefix;

        [Space]
        [SerializeField] private string _storyTextPath;

        [Space]
        [SerializeField] private BundleData _loadingData;
        [SerializeField] private string _novelsSettingBundleName;
        [SerializeField] private string _novelsBubbleBundleName;
        [SerializeField] private string _novelsLocationBundleName;
        [SerializeField] private string _novelsCharacterBundleName;
        [SerializeField] private string _novelsNotificationBundleName;
        [SerializeField] private string _novelsLocalizationBundleName;

        internal readonly string Prefix => _prefix;

        internal readonly string StoryTextPath => _storyTextPath;

        internal readonly BundleData LoadingData => _loadingData;
        internal readonly string NovelsSettingBundleName => _novelsSettingBundleName;
        internal readonly string NovelsBubbleBundleName => _novelsBubbleBundleName;
        internal readonly string NovelsLocationBundleName => _novelsLocationBundleName;
        internal readonly string NovelsCharacterBundleName => _novelsCharacterBundleName;
        internal readonly string NovelsNotificationBundleName => _novelsNotificationBundleName;
        internal readonly string NovelsLocalizationBundleName => _novelsLocalizationBundleName;
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

        private string GetNovelTextPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return $"NovelTexts/{_ctx.Data.Prefix}/{path}";
        }

        private string GetSettingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Setting/Novels/{_ctx.Data.Prefix}/{assetName}.prefab";
        }

        private string GetBubblePrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Bubble/RemoteAssets/{_ctx.Data.Prefix}/{assetName}.prefab";
        }

        private string GetLocationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Data.Prefix}/{assetName}.prefab";
        }

        private string GetLocationImagePath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpper(assetName[0]);
            var otherText = assetName.Substring(1).ToLower();
            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Data.Prefix}/Locations/{firstChar}{otherText}.png";
        }

        private string GetVideoPath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpper(assetName[0]);
            var otherText = assetName.Substring(1).ToLower();
            var result = $"{Application.streamingAssetsPath}/NovelsVideos/{_ctx.Data.Prefix}/{firstChar}{otherText}.mp4";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
#endif
            return result;
        }

        private string GetCharacterPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Data.Prefix}/{assetName}.prefab";
        }

        private string GetCharacterMainBodyPath(string name, string view)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Data.Prefix}/Characters/{name}/{view}/Main.png";
        }

        private string GetCharacterEmotionPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Data.Prefix}/Characters/{name}/{view}/Эмоции/{firstChar}{otherText}.png";
        }

        private string GetCharacterWeatherPath(string name, string arg, int index)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Data.Prefix}/Characters/{name}/Weather/{firstChar}{otherText}/{index}.png";
        }

        private string GetNotificationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Notification/RemoteAssets/{_ctx.Data.Prefix}/{assetName}.prefab";
        }

        private string GetLocalizationDataAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Localization/Novels/{_ctx.Data.Prefix}/{assetName}.asset";
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
                bundles.GetAssetBundle(_ctx.Data.NovelsSettingBundleName)
            );
            var secondPreloading = UniTask.WhenAll(
                bundles.GetText(GetNovelTextPath(_ctx.Data.StoryTextPath)),
                bundles.GetAssetBundle(_ctx.Data.NovelsBubbleBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocationBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsCharacterBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsNotificationBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocalizationBundleName)
            );

            await loading.Show();

            //preloading loading first
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await firstPreloding;

            var settingProcessCtx = new SettingProcess.Ctx
            {
                DefaultThreadPriority = _defaultThreadPriority,
                GetBundledPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsSettingBundleName, GetSettingPrefabAssetName("Screen")),
                ShowLoading = loading.Show,
                HideLoading = loading.Hide,
            };
            var settingProcess = new SettingProcess(settingProcessCtx).AddTo(this);
            await settingProcess.ShowSettingProcess();

            //preloading loading second
            var storyText = string.Empty;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
            {
                var (storyTextTemp, _, _, _, _, _) = await secondPreloading;
                storyText = storyTextTemp;
            }

            var localization = new Localization.Entity(new Localization.Entity.Ctx
            {
                Language = LocalizationData.Language.Rus,
                GetLocalizationSO = () => bundles.GetBundledSO<LocalizationData>(_ctx.Data.NovelsLocalizationBundleName, "LocalizationData"),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await localization.Init();

            if (localization.TryGetValue("Ben", out var test))
                Debug.Log(test);

            var storyProcessor = new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
            }).AddTo(this);

            var bubble = new Bubble.Entity(new Bubble.Entity.Ctx
            {
                GetBubblePrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsBubbleBundleName, GetBubblePrefabAssetName("Screen")),
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bubble.Init();

            var location = new Location.Entity(new Location.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsLocationBundleName, GetLocationPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsLocationBundleName, GetLocationImagePath(assetName)),
                GetVideoURL = GetVideoPath,
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await location.Init();

            var character = new Character.Entity(new Character.Entity.Ctx
            {
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsCharacterBundleName, GetCharacterPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsCharacterBundleName, assetName),
                GetMainBodyPath = GetCharacterMainBodyPath,
                GetEmotionPath = GetCharacterEmotionPath,
                GetWeatherPath = GetCharacterWeatherPath
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await character.Init();

            var notification = new Notification.Entity(new Notification.Entity.Ctx
            {
                GetNotificationPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsNotificationBundleName, GetNotificationPrefabAssetName("Screen")),
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
                    notification.Show(value).Forget();
                    continue;
                }

                if (prefix.ToLower().Contains("локация"))
                {
                    var locationRawArgsData = prefix.Split("(");
                    var locationArgs = locationRawArgsData.Length <= 1
                    ? new string[0]
                    : locationRawArgsData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                    await location.SetImage(value, false, locationArgs);
                    continue;
                }
                if (prefix.ToLower() == "кат-сцена")
                {
                    await location.SetImage(value, true, null);
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
                        if (args.Any(a => a == "Выбери внешность"))
                            character.SetMainCharacterView(choice.text);
                        if (args.Any(a => a == "Выбери одежду"))
                            character.SetMainCharacterWeather(choice.text);

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