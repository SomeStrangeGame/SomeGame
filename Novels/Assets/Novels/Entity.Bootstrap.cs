using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private sealed class BootstrapState
        {
            internal Save.Entity SaveSystem;
            internal PathGetter.Entity PathGetter;
            internal Bundles.Entity Bundles;
            internal Bundles.Scope EpisodeBundles;
            internal Loading.Entity MainLoading;
            internal Localization.Entity Localization;
            internal UniTask<(
                string storyText,
                AssetBundle bubble,
                AssetBundle location,
                AssetBundle character,
                AssetBundle notification)> EpisodePreloading;
        }

        private async UniTask<SettingSelection> PrepareApplication(
            BootstrapState state)
        {
            state.SaveSystem = CreateSaveSystem();
            state.PathGetter = CreatePathGetter();
            state.Bundles = CreateBundles();
            state.EpisodeBundles = state.Bundles.CreateScope().AddTo(this);

            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                await state.Bundles
                    .GetAssetBundle(_definition.LoadingBundleName)
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }

            GameObject mainLoadingScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                mainLoadingScreen = await state.Bundles.GetBundledPrefab(
                        _definition.LoadingBundleName,
                        state.PathGetter.GetMainLoadingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            state.MainLoading = CreateMainLoading(mainLoadingScreen);

            var applicationPreloading = UniTask.WhenAll(
                state.Bundles.GetAssetBundle(_definition.SettingBundleName),
                state.Bundles.GetAssetBundle(_definition.LocalizationBundleName));
            state.EpisodePreloading = UniTask.WhenAll(
                state.Bundles.GetText(
                    state.PathGetter.GetNovelTextPath(
                        _definition.Episode.StoryPath)),
                state.EpisodeBundles.GetAssetBundle(
                    _definition.Episode.BubbleBundleName),
                state.EpisodeBundles.GetAssetBundle(
                    _definition.Episode.LocationBundleName),
                state.EpisodeBundles.GetAssetBundle(
                    _definition.Episode.CharacterBundleName),
                state.EpisodeBundles.GetAssetBundle(
                    _definition.Episode.NotificationBundleName));

            await state.MainLoading.Show()
                .AttachExternalCancellation(_ctx.CancellationToken);

            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                await applicationPreloading
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }

            LocalizationData localizationData;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                localizationData = await state.Bundles
                    .GetBundledSO<LocalizationData>(
                        _definition.LocalizationBundleName,
                        state.PathGetter.GetLocalizationDataAssetName(
                            _localizationDataAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            state.Localization = CreateLocalization(localizationData);

            GameObject settingsScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                settingsScreen = await state.Bundles.GetBundledPrefab(
                        _definition.SettingBundleName,
                        state.PathGetter.GetSettingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }

            var settingProcess = new SettingProcess(
                new SettingProcess.Ctx
                {
                    BundledPrefab = settingsScreen,
                    ShowLoading = state.MainLoading.Show,
                    HideLoading = state.MainLoading.Hide,
                    ContainAnySave = () => state.SaveSystem.ContainAnySave,
                    GetLocalizationValue = state.Localization.GetValue,
                    CancellationToken = _ctx.CancellationToken,
                }).AddTo(this);
            return await settingProcess.ShowSettingProcess();
        }

        private async UniTask RunEpisode(BootstrapState state)
        {
            GameObject loadingScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                loadingScreen = await state.Bundles.GetBundledPrefab(
                        _definition.LoadingBundleName,
                        state.PathGetter.GetLoadingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            var loading = CreateLoading(loadingScreen);
            await loading.Show().AttachExternalCancellation(_ctx.CancellationToken);
            await state.MainLoading.Hide()
                .AttachExternalCancellation(_ctx.CancellationToken);

            string storyText;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                var preloaded = await state.EpisodePreloading
                    .AttachExternalCancellation(_ctx.CancellationToken);
                storyText = preloaded.storyText;
            }

            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                await state.EpisodeBundles.LoadVideosToDict(
                    _definition.Episode.LocationBundleName);
            }

            var storyProcessor = CreateStoryProcessor(storyText);
            var storyCommands = CreateStoryCommands();

            GameObject bubblePrefab;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                bubblePrefab = await state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.BubbleBundleName,
                        state.PathGetter.GetBubblePrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            var bubble = CreateBubble(bubblePrefab);

            GameObject locationScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                locationScreen = await state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.LocationBundleName,
                        state.PathGetter.GetLocationPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            var location = CreateLocation(
                locationScreen,
                async assetName =>
                {
                    using (new LoadingPriority.Entity(
                               ThreadPriority.High,
                               _defaultThreadPriority))
                    {
                        return await state.EpisodeBundles.GetBundledSprite(
                                _definition.Episode.LocationBundleName,
                                state.PathGetter.GetLocationImagePath(assetName))
                            .AttachExternalCancellation(
                                _ctx.CancellationToken);
                    }
                },
                state.Bundles.GetVideoURL);

            GameObject characterScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                characterScreen = await state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.CharacterBundleName,
                        state.PathGetter.GetCharacterPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            var character = CreateCharacter(
                characterScreen,
                async assetName =>
                {
                    using (new LoadingPriority.Entity(
                               ThreadPriority.High,
                               _defaultThreadPriority))
                    {
                        return await state.EpisodeBundles.GetBundledSprite(
                                _definition.Episode.CharacterBundleName,
                                assetName)
                            .AttachExternalCancellation(
                                _ctx.CancellationToken);
                    }
                },
                state.PathGetter.GetCharacterMainBodyPath,
                state.PathGetter.GetCharacterEmotionPath,
                state.PathGetter.GetCharacterClothesPath,
                state.PathGetter.GetCharacterHairPath,
                state.PathGetter.GetCharacterAccessoriesPath);

            GameObject notificationScreen;
            using (new LoadingPriority.Entity(
                       ThreadPriority.High,
                       _defaultThreadPriority))
            {
                notificationScreen = await state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.NotificationBundleName,
                        state.PathGetter.GetNotificationPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken);
            }
            var notification = CreateNotification(notificationScreen);
            var waiting = CreateWaiting();
            var audio = CreateAudio(
                state.Bundles.GetAudioURL,
                state.Bundles.LoadAudioToDict);
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
                }).AddTo(this);
            await novelProcess.ShowNovelProcess();
            state.EpisodeBundles.Dispose();
        }
    }
}
