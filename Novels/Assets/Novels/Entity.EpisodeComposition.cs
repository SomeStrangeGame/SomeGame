using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<EpisodeRunResult> RunEpisode(PreparedNovelResources state)
        {
            var cancellationToken = state.CancellationToken;
            var loadingAddress = new Bundles.BundleAssetAddress(
                _episode.BundleName,
                state.PathGetter.GetLoadingPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var loadingScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(loadingAddress)
                .AttachExternalCancellation(cancellationToken));
            var loading = CreateLoading(
                state.EpisodeScope,
                loadingScreen,
                cancellationToken);
            await loading.Show().AttachExternalCancellation(cancellationToken);
            await state.MainLoading.Hide().AttachExternalCancellation(cancellationToken);

            var storyText = await _priorityLoader.Run(() => state.EpisodePreloading
                .AttachExternalCancellation(cancellationToken));
            var storyProcessor = CreateStoryProcessor(state.EpisodeScope, storyText);
            var storyCommands = CreateStoryCommands();

            var bubbleAddress = new Bundles.BundleAssetAddress(
                _episode.BundleName,
                state.PathGetter.GetBubblePrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var bubblePrefab = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(bubbleAddress)
                .AttachExternalCancellation(cancellationToken));
            var bubble = CreateBubble(
                state.EpisodeScope,
                bubblePrefab,
                cancellationToken);

            var locationAddress = new Bundles.BundleAssetAddress(
                _episode.BundleName,
                state.PathGetter.GetLocationPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var locationScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(locationAddress)
                .AttachExternalCancellation(cancellationToken));
            var location = CreateLocation(
                state.EpisodeScope,
                locationScreen,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .GetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.BundleName,
                        state.PathGetter.GetLocationImagePath(assetName)))
                    .AttachExternalCancellation(cancellationToken)),
                state.EpisodeBundles.ResolveVideoUrl,
                cancellationToken);

            var characterAddress = new Bundles.BundleAssetAddress(
                _episode.BundleName,
                state.PathGetter.GetCharacterPrefabAssetName(BootstrapAddresses.ScreenAssetName));
            var characterScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(characterAddress)
                .AttachExternalCancellation(cancellationToken));
            var character = CreateCharacter(
                state.EpisodeScope,
                characterScreen,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.BundleName,
                        assetName))
                    .AttachExternalCancellation(cancellationToken)),
                cancellationToken);

            var notificationAddress = new Bundles.BundleAssetAddress(
                _episode.BundleName,
                state.PathGetter.GetNotificationPrefabAssetName(
                    BootstrapAddresses.ScreenAssetName));
            var notificationScreen = await _priorityLoader.Run(() => state.EpisodeBundles
                .GetBundledPrefab(notificationAddress)
                .AttachExternalCancellation(cancellationToken));
            var notification = CreateNotification(
                state.EpisodeScope,
                notificationScreen,
                cancellationToken);
            var waiting = CreateWaiting(state.EpisodeScope, cancellationToken);
            var audio = CreateAudio(
                state.EpisodeScope,
                state.EpisodeBundles.ResolveAudioUrl,
                cancellationToken);
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
                CancellationToken = cancellationToken,
                OnError = ReportError,
            }).AddTo(state.EpisodeScope);
            state.EpisodeRuntime.Configure(
                novelProcess.ShowNovelProcess,
                state.SaveSystem.FlushAsync);
            try
            {
                return await state.EpisodeRuntime.Run();
            }
            finally
            {
                state.EpisodeRuntime.Dispose();
            }
        }
    }
}
