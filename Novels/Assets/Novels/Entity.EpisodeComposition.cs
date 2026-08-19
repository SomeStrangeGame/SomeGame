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
            ValidateSavedReplay(state.SaveSystem, storyText);
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
                GetNextSavedDecision = state.SaveSystem.GetNextSavedDecision,
                HideLoading = loading.Hide,
                CancellationToken = cancellationToken,
                OnError = ReportError,
            }).AddTo(state.EpisodeScope);
            state.EpisodeRuntime.Configure(
                novelProcess.ShowNovelProcess,
                state.SaveSystem.FlushAsync);
            return await state.EpisodeRuntime.Run();
        }

        private static void ValidateSavedReplay(Save.Entity saveSystem, string storyText)
        {
            var decisions = saveSystem.GetInitialDecisionsSnapshot();
            if (decisions.Length == 0
                || TryValidateReplay(storyText, decisions, out var reason))
            {
                return;
            }

            saveSystem.DiscardIncompatibleReplay(reason);
        }

        private static bool TryValidateReplay(
            string storyText,
            StoryContracts.StoryDecision[] decisions,
            out string reason)
        {
            using var story = new StoryProcessor.Entity(
                new StoryProcessor.Entity.Ctx { StoryText = storyText });
            var parser = new StoryCommands.Entity();

            for (var decisionIndex = 0; decisionIndex < decisions.Length; decisionIndex++)
            {
                if (!TryReadNextReplayDialogue(story, parser, out var step, out reason))
                    return false;

                var decision = decisions[decisionIndex];
                var hasChoices = step.Choices.Length > 0;
                if (decision.HasChoice != hasChoices)
                {
                    reason = decision.HasChoice
                        ? $"Saved choice #{decisionIndex} points to a dialogue without choices."
                        : $"Saved advance #{decisionIndex} points to a dialogue with choices.";
                    return false;
                }

                if (!decision.HasChoice)
                    continue;

                if (!ContainsChoice(step.Choices, decision.ChoiceId))
                {
                    reason = $"Saved choice #{decisionIndex} references unavailable option "
                        + $"'{decision.ChoiceId}'.";
                    return false;
                }
                story.SetChoice(decision.ChoiceId);
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryReadNextReplayDialogue(
            StoryProcessor.Entity story,
            StoryCommands.Entity parser,
            out StoryCommands.StoryStep step,
            out string reason)
        {
            while (true)
            {
                var read = story.ReadNext();
                if (read.Status == StoryProcessor.StoryReadStatus.Completed)
                {
                    step = null;
                    reason = "The save contains more decisions than the current story.";
                    return false;
                }

                var parsed = parser.ParseStep(read.Source, read.Choices);
                if (!parsed.IsSuccess)
                {
                    step = null;
                    reason = $"The current story cannot replay the save: "
                        + $"[{parsed.Error.Code}] {parsed.Error.Message}";
                    return false;
                }

                if (parsed.Step.Command is StoryCommands.DialogueStoryCommand)
                {
                    step = parsed.Step;
                    reason = string.Empty;
                    return true;
                }
            }
        }

        private static bool ContainsChoice(
            StoryContracts.StoryChoice[] choices,
            int choiceId)
        {
            foreach (var choice in choices)
            {
                if (choice.Id == choiceId)
                    return true;
            }
            return false;
        }
    }
}
