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
            internal EpisodeRuntime EpisodeRuntime;
            internal EpisodeScope EpisodeScope;
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
            state.EpisodeRuntime = CreateEpisodeRuntime().AddTo(this);
            state.EpisodeScope = state.EpisodeRuntime.Scope;
            state.EpisodeBundles = state.Bundles.CreateScope()
                .AddTo(state.EpisodeScope);

            await _priorityLoader.Run(() => state.Bundles
                    .GetAssetBundle(_definition.LoadingBundleName)
                    .AttachExternalCancellation(_ctx.CancellationToken));

            GameObject mainLoadingScreen;
            mainLoadingScreen = await _priorityLoader.Run(() =>
                state.Bundles.GetBundledPrefab(
                        _definition.LoadingBundleName,
                        state.PathGetter.GetMainLoadingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
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

            await _priorityLoader.Run(() => applicationPreloading
                .AttachExternalCancellation(_ctx.CancellationToken));

            LocalizationData localizationData;
            localizationData = await _priorityLoader.Run(() => state.Bundles
                    .GetBundledSO<LocalizationData>(
                        _definition.LocalizationBundleName,
                        state.PathGetter.GetLocalizationDataAssetName(
                            _localizationDataAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            state.Localization = CreateLocalization(localizationData);

            GameObject settingsScreen;
            settingsScreen = await _priorityLoader.Run(() =>
                state.Bundles.GetBundledPrefab(
                        _definition.SettingBundleName,
                        state.PathGetter.GetSettingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));

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
            loadingScreen = await _priorityLoader.Run(() =>
                state.Bundles.GetBundledPrefab(
                        _definition.LoadingBundleName,
                        state.PathGetter.GetLoadingPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var loading = CreateLoading(state.EpisodeScope, loadingScreen);
            await loading.Show().AttachExternalCancellation(_ctx.CancellationToken);
            await state.MainLoading.Hide()
                .AttachExternalCancellation(_ctx.CancellationToken);

            string storyText;
            var preloaded = await _priorityLoader.Run(() =>
                state.EpisodePreloading
                    .AttachExternalCancellation(_ctx.CancellationToken));
            storyText = preloaded.storyText;

            var storyProcessor = CreateStoryProcessor(state.EpisodeScope, storyText);
            var storyCommands = CreateStoryCommands();

            GameObject bubblePrefab;
            bubblePrefab = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.BubbleBundleName,
                        state.PathGetter.GetBubblePrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var bubble = CreateBubble(state.EpisodeScope, bubblePrefab);

            GameObject locationScreen;
            locationScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.LocationBundleName,
                        state.PathGetter.GetLocationPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var location = CreateLocation(
                state.EpisodeScope,
                locationScreen,
                async assetName =>
                {
                    return await _priorityLoader.Run(() =>
                        state.EpisodeBundles.GetBundledSprite(
                                _definition.Episode.LocationBundleName,
                                state.PathGetter.GetLocationImagePath(assetName))
                            .AttachExternalCancellation(
                                _ctx.CancellationToken));
                },
                state.EpisodeBundles.ResolveVideoUrl);

            GameObject characterScreen;
            characterScreen = await _priorityLoader.Run(() =>
                state.EpisodeBundles.GetBundledPrefab(
                        _definition.Episode.CharacterBundleName,
                        state.PathGetter.GetCharacterPrefabAssetName(
                            _screenAssetName))
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var character = CreateCharacter(
                state.EpisodeScope,
                characterScreen,
                async assetName =>
                {
                    return await _priorityLoader.Run(() =>
                        state.EpisodeBundles.GetBundledSprite(
                                _definition.Episode.CharacterBundleName,
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
                        _definition.Episode.NotificationBundleName,
                        state.PathGetter.GetNotificationPrefabAssetName(
                            _screenAssetName))
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
