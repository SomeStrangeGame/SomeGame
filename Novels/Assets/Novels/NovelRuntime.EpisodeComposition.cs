using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private async UniTask<EpisodeRunResult> RunEpisode(PreparedEpisode state)
        {
            var cancellationToken = state.CancellationToken;
            var storyData = await _priorityLoader.Run(() => state.EpisodePreloading
                .AttachExternalCancellation(cancellationToken));
            var storyText = storyData.StoryText;
            var initialState = _progress.GetEntryState(_episode);
            ReplayValidator.ValidateOrDiscard(
                state.SaveSystem,
                storyText,
                initialState);
            var assets = await new EpisodeAssetLoader(new EpisodeAssetLoader.Dependencies
            {
                Bundles = state.StoryAssets,
                PriorityLoader = _priorityLoader,
                Addresses = state.Addresses,
                BundleName = _assetBundleName,
                Fallbacks = _ctx.FallbackAssets,
                CancellationToken = cancellationToken,
            }).Load();
            var loading = CreateLoading(
                state.EpisodeScope,
                assets.Loading,
                cancellationToken);
            await loading.Show().AttachExternalCancellation(cancellationToken);
            await state.MainLoading.Hide().AttachExternalCancellation(cancellationToken);

            var storyProcessor = CreateStoryProcessor(
                state.EpisodeScope,
                storyText,
                initialState,
                storyData.SourceMapText);
            var storyCommands = new StoryCommands.Entity();
            var presentation = CreateEpisodePresentation(state, assets, loading);
            var storyQueue = CreateStoryQueue(
                storyProcessor,
                presentation,
                cancellationToken,
                assetName => GetChooseSprite(state, assetName),
                assetName => GetBubbleChoiceIcon(state, assetName),
                state.SaveSystem);
            var queueExecutor = new StoryExecution.StoryOperationExecutor();
            var novelProcess = new NovelProcess(new NovelProcess.Dependencies
            {
                ReadNext = storyProcessor.ReadNext,
                ExportStoryState = storyProcessor.ExportState,
                IsEpisodeEnd = source => IsEpisodeEnd(source, _definition.EndMarker),
                ParseStep = storyCommands.ParseStep,
                BuildQueue = storyQueue.TryBuild,
                CompleteQueue = storyQueue.TryComplete,
                ExecuteQueue = queueExecutor.Run,
                GetNextSavedDecision = state.SaveSystem.GetNextSavedDecision,
                HideLoading = presentation.Loading.Hide,
                OnReady = () => _ctx.SmokeTelemetry?.Emit(
                    "episode.ready",
                    ("contentId", _definition.Id),
                    ("episodeId", _episode.Id)),
                CancellationToken = cancellationToken,
                OnError = ReportError,
                OnStorySourceChanged = _ctx.OnStorySourceChanged,
            }).AddTo(state.EpisodeScope);
            state.EpisodeRuntime.Configure(
                novelProcess.Run,
                state.SaveSystem.FlushAsync);
            return await state.EpisodeRuntime.Run();
        }

        private async UniTask<Sprite> GetCharacterSprite(
            PreparedEpisode state,
            string episodeAssetPath)
        {
            var cancellationToken = state.CancellationToken;
            return await _priorityLoader.Run(() => GetStorySprite(
                    state,
                    episodeAssetPath)
                .AttachExternalCancellation(cancellationToken));
        }

        private UniTask<Sprite> GetBubbleChoiceIcon(
            PreparedEpisode state,
            string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return UniTask.FromResult<Sprite>(null);
            return GetStorySprite(state, state.Addresses.ChooseItem(assetName));
        }

        private async UniTask<Sprite> GetFullCharacterSprite(
            PreparedEpisode state,
            string episodeAssetPath)
        {
            var cancellationToken = state.CancellationToken;
            return await _priorityLoader.Run(() => GetFullStorySprite(
                    state,
                    episodeAssetPath)
                .AttachExternalCancellation(cancellationToken));
        }

        private UniTask<Sprite> GetStorySprite(
            PreparedEpisode state,
            string assetPath) => _streaming != null
            ? _streaming.GetSprite(assetPath)
            : state.StoryAssets.TryGetBundledSprite(
                new Bundles.BundleAssetAddress(_assetBundleName, assetPath));

        private UniTask<Sprite> GetFullStorySprite(
            PreparedEpisode state,
            string assetPath) => _streaming != null
            ? _streaming.GetFullSprite(assetPath)
            : state.StoryAssets.TryGetBundledSprite(
                new Bundles.BundleAssetAddress(_definition.BundleName, assetPath));

        private async UniTask<Character.CharacterSpriteTrimManifest>
            GetCharacterSpriteTrimManifest(PreparedEpisode state)
        {
            var cancellationToken = state.CancellationToken;
            var bundle = await state.StoryAssets.GetAssetBundle(_assetBundleName);
            var address = state.Addresses.CharacterSpriteTrimManifest();
            return await bundle
                .LoadAssetAsync<Character.CharacterSpriteTrimManifest>(address)
                .WithCancellation(cancellationToken)
                as Character.CharacterSpriteTrimManifest;
        }

        private static bool IsEpisodeEnd(string source, string marker)
        {
            if (string.IsNullOrWhiteSpace(marker))
                return false;
            return (source ?? string.Empty).TrimStart().StartsWith(
                marker,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
