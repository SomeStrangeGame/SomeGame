using System;
using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using UnityEngine;
using UnityEngine.Audio;
using CancellationToken = System.Threading.CancellationToken;

namespace Novels
{
    [Serializable]
    internal struct Data
    {
        [SerializeField] private string _prefix;
        [SerializeField] private string _mainCharacter;

        [Space]
        [SerializeField] private string _storyTextPath;

        [Space]
        [SerializeField] private string _novelsLoadingBundleName;
        [SerializeField] private string _novelsSettingBundleName;
        [SerializeField] private string _novelsBubbleBundleName;
        [SerializeField] private string _novelsLocationBundleName;
        [SerializeField] private string _novelsCharacterBundleName;
        [SerializeField] private string _novelsNotificationBundleName;
        [SerializeField] private string _novelsLocalizationBundleName;

        [SerializeField] private AudioMixer _audioMixer;

        internal readonly string Prefix => _prefix;
        internal readonly string MainCharacter => _mainCharacter;

        internal readonly string StoryTextPath => _storyTextPath;

        internal readonly string NovelsLoadingBundleName => _novelsLoadingBundleName;
        internal readonly string NovelsSettingBundleName => _novelsSettingBundleName;
        internal readonly string NovelsBubbleBundleName => _novelsBubbleBundleName;
        internal readonly string NovelsLocationBundleName => _novelsLocationBundleName;
        internal readonly string NovelsCharacterBundleName => _novelsCharacterBundleName;
        internal readonly string NovelsNotificationBundleName => _novelsNotificationBundleName;
        internal readonly string NovelsLocalizationBundleName => _novelsLocalizationBundleName;

        internal readonly AudioMixer AudioMixer => _audioMixer;
    }

    internal partial class Entity : BaseDisposable
    {
        private const string _screenAssetName = "Screen";
        private const string _localizationDataAssetName = "LocalizationData";
        internal struct Ctx
        {
            internal Data Data;
            internal CancellationToken CancellationToken;
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
            var saveSystem = CreateSaveSystem();

            var pathGetter = CreatePathGetter();

            var bundles = CreateBundles();
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bundles.GetAssetBundle(_ctx.Data.NovelsLoadingBundleName).AttachExternalCancellation(_ctx.CancellationToken);

            GameObject mainLoadingScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                mainLoadingScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetMainLoadingPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var mainLoading = CreateMainLoading(mainLoadingScreen);

            //preloading init
            var firstPreloading = UniTask.WhenAll(
                bundles.GetAssetBundle(_ctx.Data.NovelsSettingBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocalizationBundleName)
            );
            var secondPreloading = UniTask.WhenAll(
                bundles.GetText(pathGetter.GetNovelTextPath(_ctx.Data.StoryTextPath)),
                bundles.GetAssetBundle(_ctx.Data.NovelsBubbleBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsLocationBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsCharacterBundleName),
                bundles.GetAssetBundle(_ctx.Data.NovelsNotificationBundleName)
            );

            await mainLoading.Show().AttachExternalCancellation(_ctx.CancellationToken);

            //preloading loading first
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await firstPreloading.AttachExternalCancellation(_ctx.CancellationToken);

            LocalizationData localizationData = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                localizationData = await bundles.GetBundledSO<LocalizationData>(_ctx.Data.NovelsLocalizationBundleName, pathGetter.GetLocalizationDataAssetName(_localizationDataAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var localization = CreateLocalization(localizationData);

            GameObject settingsScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                settingsScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsSettingBundleName, pathGetter.GetSettingPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var settingProcessCtx = new SettingProcess.Ctx
            {
                BundledPrefab = settingsScreen,
                ShowLoading = mainLoading.Show,
                HideLoading = mainLoading.Hide,
                ContainAnySave = () => saveSystem.ContainAnySave,
                GetLocalizationValue = localization.GetValue,
                CancellationToken = _ctx.CancellationToken,
            };
            var settingProcess = new SettingProcess(settingProcessCtx).AddTo(this);
            var settingSelection = await settingProcess.ShowSettingProcess();
            if (settingSelection == SettingSelection.NewGame)
                saveSystem.Clear();

            GameObject loadingScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                loadingScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsLoadingBundleName, pathGetter.GetLoadingPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var loading = CreateLoading(loadingScreen);
            await loading.Show().AttachExternalCancellation(_ctx.CancellationToken);

            await mainLoading.Hide().AttachExternalCancellation(_ctx.CancellationToken);

            //preloading loading second
            var storyText = string.Empty;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
            {
                var (storyTextTemp, _, _, _, _) = await secondPreloading.AttachExternalCancellation(_ctx.CancellationToken);
                storyText = storyTextTemp;
            }

            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await bundles.LoadVideosToDict(_ctx.Data.NovelsLocationBundleName);

            var storyProcessor = CreateStoryProcessor(storyText);
            var storyCommands = CreateStoryCommands();

            GameObject bubblePrefab = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                bubblePrefab = await bundles.GetBundledPrefab(_ctx.Data.NovelsBubbleBundleName, pathGetter.GetBubblePrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var bubble = CreateBubble(bubblePrefab);

            GameObject locationScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                locationScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var location = CreateLocation(locationScreen, async a =>
                {
                    Sprite locationImage = null;
                    using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                        locationImage = await bundles.GetBundledSprite(_ctx.Data.NovelsLocationBundleName, pathGetter.GetLocationImagePath(a)).AttachExternalCancellation(_ctx.CancellationToken);
                    return locationImage;
                }, a => bundles.GetVideoURL(a));

            GameObject characterScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                characterScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsCharacterBundleName, pathGetter.GetCharacterPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var character = CreateCharacter(characterScreen, async a => 
                {
                    Sprite characterSprite = null;
                    using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                        characterSprite = await bundles.GetBundledSprite(_ctx.Data.NovelsCharacterBundleName, a).AttachExternalCancellation(_ctx.CancellationToken);
                    return characterSprite;
                }, pathGetter.GetCharacterMainBodyPath, pathGetter.GetCharacterEmotionPath, pathGetter.GetCharacterClothesPath, pathGetter.GetCharacterHairPath, pathGetter.GetCharacterAccessoriesPath);

            GameObject notificationScreen = null;
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                notificationScreen = await bundles.GetBundledPrefab(_ctx.Data.NovelsNotificationBundleName, pathGetter.GetNotificationPrefabAssetName(_screenAssetName)).AttachExternalCancellation(_ctx.CancellationToken);
            var notification = CreateNotification(notificationScreen);

            var waiting = CreateWaiting();

            var audio = CreateAudio(a => bundles.GetAudioURL(a), a => bundles.LoadAudioToDict(a));
            var storyQueue = CreateStoryQueue(
                storyProcessor,
                notification,
                location,
                waiting,
                audio,
                localization,
                bubble,
                saveSystem,
                character);
            var queueExecutor = CreateQueueExecutor();

            var novelProcessCtx = new NovelProcess.Ctx
            {
                ReadNext = storyProcessor.ReadNext,
                ParseStep = storyCommands.ParseStep,
                BuildQueue = storyQueue.TryBuild,
                ExecuteQueue = queueExecutor.Run,

                GetNextSavedChoice = saveSystem.GetNextSavedChoice,
                HideLoading = loading.Hide,
                CancellationToken = _ctx.CancellationToken,

                OnLog = _ctx.OnLog,
            };
            var novelProcess = new NovelProcess(novelProcessCtx).AddTo(this);
            await novelProcess.ShowNovelProcess();
        }
    }
}
