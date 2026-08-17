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
            var storyText = await _priorityLoader.Run(() => state.EpisodePreloading
                .AttachExternalCancellation(cancellationToken));
            var assets = await new EpisodeAssetLoader(new EpisodeAssetLoader.Ctx
            {
                Bundles = state.EpisodeBundles,
                PriorityLoader = _priorityLoader,
                Addresses = state.Addresses,
                BundleName = _episode.BundleName,
                CancellationToken = cancellationToken,
            }).Load();
            var loading = CreateLoading(
                state.EpisodeScope,
                assets.Loading,
                cancellationToken);
            await loading.Show().AttachExternalCancellation(cancellationToken);
            await state.MainLoading.Hide().AttachExternalCancellation(cancellationToken);

            var storyProcessor = CreateStoryProcessor(state.EpisodeScope, storyText);
            var storyCommands = CreateStoryCommands();

            var bubble = CreateBubble(
                state.EpisodeScope,
                assets.Bubble,
                cancellationToken);

            var location = CreateLocation(
                state.EpisodeScope,
                assets.Location,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .GetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.BundleName,
                        state.Addresses.LocationImage(assetName)))
                    .AttachExternalCancellation(cancellationToken)),
                state.EpisodeBundles.ResolveVideoUrl,
                cancellationToken);

            var character = CreateCharacter(
                state.EpisodeScope,
                assets.Character,
                assetName => _priorityLoader.Run(() => state.EpisodeBundles
                    .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                        _episode.BundleName,
                        assetName))
                    .AttachExternalCancellation(cancellationToken)),
                cancellationToken);

            var notification = CreateNotification(
                state.EpisodeScope,
                assets.Notification,
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
