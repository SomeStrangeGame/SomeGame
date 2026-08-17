using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask RunEpisode(PreparedNovelResources state)
        {
            var loadingAddress = new Bundles.BundleAssetAddress(
                _definition.LoadingBundleName,
                state.PathGetter.GetLoadingPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var loadingScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(loadingAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var loading = CreateLoading(state.EpisodeScope, loadingScreen);
            await loading.Show().AttachExternalCancellation(_ctx.CancellationToken);
            await state.MainLoading.Hide().AttachExternalCancellation(_ctx.CancellationToken);

            var storyText = await _priorityLoader.Run(() => state.EpisodePreloading
                .AttachExternalCancellation(_ctx.CancellationToken));
            var storyProcessor = CreateStoryProcessor(state.EpisodeScope, storyText);
            var storyCommands = CreateStoryCommands();

            var bubbleAddress = new Bundles.BundleAssetAddress(
                _episode.BubbleBundleName,
                state.PathGetter.GetBubblePrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var bubblePrefab = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(bubbleAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var bubble = CreateBubble(state.EpisodeScope, bubblePrefab);

            var locationAddress = new Bundles.BundleAssetAddress(
                _episode.LocationBundleName,
                state.PathGetter.GetLocationPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var locationScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(locationAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var location = CreateLocation(
                state.EpisodeScope,
                locationScreen,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .GetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.LocationBundleName,
                        state.PathGetter.GetLocationImagePath(assetName)))
                    .AttachExternalCancellation(_ctx.CancellationToken)),
                state.EpisodeBundles.ResolveVideoUrl);

            var characterAddress = new Bundles.BundleAssetAddress(
                _episode.CharacterBundleName,
                state.PathGetter.GetCharacterPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var characterScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(characterAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var character = CreateCharacter(
                state.EpisodeScope,
                characterScreen,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.CharacterBundleName,
                        assetName))
                    .AttachExternalCancellation(_ctx.CancellationToken)));

            var notificationAddress = new Bundles.BundleAssetAddress(
                _episode.NotificationBundleName,
                state.PathGetter.GetNotificationPrefabAssetName(
                    BootstrapAddresses.ScreenAssetName));
            var notificationScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(notificationAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            var notification = CreateNotification(state.EpisodeScope, notificationScreen);
            var waiting = CreateWaiting(state.EpisodeScope);
            var audio = CreateAudio(state.EpisodeScope, state.EpisodeBundles.ResolveAudioUrl);
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
            var novelProcess = new NovelProcess(new NovelProcess.Ctx
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
