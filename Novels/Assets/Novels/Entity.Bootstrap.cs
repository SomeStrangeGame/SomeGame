using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Content.NovelDefinition> LoadContent(
            Bundles.Scope bundles,
            Catalog.NovelCatalogEntry entry)
        {
            await _priorityLoader.Run(() => bundles
                .GetAssetBundle(entry.ContentBundleName)
                .AttachExternalCancellation(_ctx.CancellationToken));

            var content = await _priorityLoader.Run(() => bundles
                .GetBundledSO<Content.NovelContentAsset>(
                    entry.ContentBundleName,
                    entry.ContentAssetName)
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (content == null)
            {
                throw new System.InvalidOperationException(
                    $"Content '{entry.ContentId}' could not be loaded from "
                    + $"AssetBundle '{entry.ContentBundleName}'.");
            }

            _audioMixer = content.AudioMixer;
            return content.ToDefinition();
        }

        private sealed class PreparedNovelResources
        {
            internal PreparedNovelResources(
                Save.Entity saveSystem,
                PathGetter.Entity pathGetter,
                Bundles.Scope novelBundles,
                EpisodeRuntime episodeRuntime,
                Bundles.Scope episodeBundles,
                Loading.Entity mainLoading,
                Localization.Entity localization,
                UniTask<string> episodePreloading)
            {
                SaveSystem = saveSystem;
                PathGetter = pathGetter;
                NovelBundles = novelBundles;
                EpisodeRuntime = episodeRuntime;
                EpisodeScope = episodeRuntime.Scope;
                EpisodeBundles = episodeBundles;
                MainLoading = mainLoading;
                Localization = localization;
                EpisodePreloading = episodePreloading;
            }

            internal Save.Entity SaveSystem { get; }
            internal PathGetter.Entity PathGetter { get; }
            internal Bundles.Scope NovelBundles { get; }
            internal EpisodeRuntime EpisodeRuntime { get; }
            internal EpisodeScope EpisodeScope { get; }
            internal Bundles.Scope EpisodeBundles { get; }
            internal Loading.Entity MainLoading { get; }
            internal Localization.Entity Localization { get; }
            internal UniTask<string> EpisodePreloading { get; }
        }

        private async UniTask<NovelStartSession> PrepareApplication(
            Bundles.Scope novelBundles)
        {
            var saveSystem = CreateSaveSystem();
            var pathGetter = CreatePathGetter();
            var episodeRuntime = CreateEpisodeRuntime().AddTo(this);
            var episodeBundles = _ctx.Bundles.CreateScope()
                .AddTo(episodeRuntime.Scope);

            await _priorityLoader.Run(() => novelBundles
                    .GetAssetBundle(_definition.MainLoadingBundleName)
                    .AttachExternalCancellation(_ctx.CancellationToken));

            GameObject mainLoadingScreen;
            mainLoadingScreen = await _priorityLoader.Run(() =>
                novelBundles.GetBundledPrefab(
                        _definition.MainLoadingBundleName,
                        pathGetter.GetMainLoadingPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var mainLoading = CreateMainLoading(mainLoadingScreen);

            var applicationPreloading = UniTask.WhenAll(
                novelBundles.GetAssetBundle(_definition.SettingBundleName),
                novelBundles.GetAssetBundle(_definition.LocalizationBundleName));
            var episodePreloading = PreloadEpisode(pathGetter, episodeBundles)
                .Preserve();

            await mainLoading.Show()
                .AttachExternalCancellation(_ctx.CancellationToken);

            await _priorityLoader.Run(() => applicationPreloading
                .AttachExternalCancellation(_ctx.CancellationToken));

            LocalizationData localizationData;
            localizationData = await _priorityLoader.Run(() => novelBundles
                    .GetBundledSO<LocalizationData>(
                        _definition.LocalizationBundleName,
                        pathGetter.GetLocalizationDataAssetName(
                            BootstrapAddresses.LocalizationDataAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var localization = CreateLocalization(localizationData);

            GameObject settingsScreen;
            settingsScreen = await _priorityLoader.Run(() =>
                novelBundles.GetBundledPrefab(
                        _definition.SettingBundleName,
                        pathGetter.GetSettingPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));

            var settingProcess = new SettingProcess(
                new SettingProcess.Ctx
                {
                    BundledPrefab = settingsScreen,
                    ShowLoading = mainLoading.Show,
                    HideLoading = mainLoading.Hide,
                    ContainAnySave = () => saveSystem.ContainAnySave,
                    GetLocalizationValue = localization.GetValue,
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);
            var resources = new PreparedNovelResources(
                saveSystem,
                pathGetter,
                novelBundles,
                episodeRuntime,
                episodeBundles,
                mainLoading,
                localization,
                episodePreloading);
            var selection = await settingProcess.ShowSettingProcess();
            return new NovelStartSession(
                selection,
                saveSystem.Clear,
                () => RunEpisode(resources));
        }

        private async UniTask<string> PreloadEpisode(
            PathGetter.Entity pathGetter,
            Bundles.Scope episodeBundles)
        {
            var result = await UniTask.WhenAll(
                _ctx.Bundles.GetText(
                    pathGetter.GetNovelTextPath(_episode.StoryPath)),
                episodeBundles.GetAssetBundle(_episode.BubbleBundleName),
                episodeBundles.GetAssetBundle(_episode.LocationBundleName),
                episodeBundles.GetAssetBundle(_episode.CharacterBundleName),
                episodeBundles.GetAssetBundle(_episode.NotificationBundleName),
                episodeBundles.GetAssetBundle(_definition.LoadingBundleName));
            return result.Item1;
        }

        private async UniTask RunEpisode(PreparedNovelResources state)
        {
            GameObject loadingScreen;
            loadingScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _definition.LoadingBundleName,
                        state.PathGetter.GetLoadingPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var loading = CreateLoading(state.EpisodeScope, loadingScreen);
            await loading.Show().AttachExternalCancellation(_ctx.CancellationToken);
            await state.MainLoading.Hide()
                .AttachExternalCancellation(_ctx.CancellationToken);

            var storyText = await _priorityLoader.Run(() =>
                state.EpisodePreloading
                    .AttachExternalCancellation(_ctx.CancellationToken));

            var storyProcessor = CreateStoryProcessor(state.EpisodeScope, storyText);
            var storyCommands = CreateStoryCommands();

            GameObject bubblePrefab;
            bubblePrefab = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _episode.BubbleBundleName,
                        state.PathGetter.GetBubblePrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var bubble = CreateBubble(state.EpisodeScope, bubblePrefab);

            GameObject locationScreen;
            locationScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _episode.LocationBundleName,
                        state.PathGetter.GetLocationPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var location = CreateLocation(
                state.EpisodeScope,
                locationScreen,
                async assetName =>
                {
                    return await _priorityLoader.Run(() =>
                        state.EpisodeBundles.GetBundledSprite(
                                _episode.LocationBundleName,
                                state.PathGetter.GetLocationImagePath(assetName))
                            .AttachExternalCancellation(
                                _ctx.CancellationToken));
                },
                state.EpisodeBundles.ResolveVideoUrl);

            GameObject characterScreen;
            characterScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _episode.CharacterBundleName,
                        state.PathGetter.GetCharacterPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var character = CreateCharacter(
                state.EpisodeScope,
                characterScreen,
                async assetName =>
                {
                    return await _priorityLoader.Run(() =>
                        state.EpisodeBundles.GetBundledSprite(
                                _episode.CharacterBundleName,
                                assetName)
                            .AttachExternalCancellation(
                                _ctx.CancellationToken));
                },
                state.PathGetter.GetCharacterMainBodyPath,
                state.PathGetter.GetCharacterEmotionPath,
                state.PathGetter.GetCharacterClothesPath,
                state.PathGetter.GetCharacterHairPath,
                state.PathGetter.GetCharacterAccessoriesPath);

            GameObject notificationScreen;
            notificationScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _episode.NotificationBundleName,
                        state.PathGetter.GetNotificationPrefabAssetName(
                            BootstrapAddresses.ScreenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var notification = CreateNotification(
                state.EpisodeScope,
                notificationScreen);
            var waiting = CreateWaiting(state.EpisodeScope);
            var audio = CreateAudio(
                state.EpisodeScope,
                state.EpisodeBundles.ResolveAudioUrl);
            var storyQueue = CreateStoryQueue(
                storyProcessor,
                notification,
                location,
                waiting,
                audio,
                state.Localization,
                bubble,
                state.SaveSystem,
                character);
            var queueExecutor = CreateQueueExecutor();

            var novelProcess = new NovelProcess(
                new NovelProcess.Ctx
                {
                    ReadNext = storyProcessor.ReadNext,
                    ParseStep = storyCommands.ParseStep,
                    BuildQueue = storyQueue.TryBuild,
                    CompleteQueue = storyQueue.TryComplete,
                    ExecuteQueue = queueExecutor.Run,
                    GetNextSavedChoice = state.SaveSystem.GetNextSavedChoice,
                    HideLoading = loading.Hide,
                    CancellationToken = _ctx.CancellationToken,
                    OnError = _ctx.OnError,
                }).AddTo(state.EpisodeScope);
            state.EpisodeRuntime.Configure(
                novelProcess.ShowNovelProcess,
                state.SaveSystem.FlushAsync);
            try
            {
                await state.EpisodeRuntime.Run();
            }
            finally
            {
                state.EpisodeRuntime.Dispose();
            }
        }
    }
}
