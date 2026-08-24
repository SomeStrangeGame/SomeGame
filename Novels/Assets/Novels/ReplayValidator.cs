using Novels.Save;
using Novels.StoryCommands;
using Novels.StoryContracts;
using Story = Novels.StoryProcessor.Entity;
using StoryParser = Novels.StoryCommands.Entity;

namespace Novels
{
    internal static class ReplayValidator
    {
        internal static void ValidateOrDiscard(
            SaveSystem saveSystem,
            string storyText,
            string initialState)
        {
            var decisions = saveSystem.GetInitialDecisionsSnapshot();
            if (decisions.Length > 0
                && !IsCompatible(storyText, initialState, decisions, out var reason))
            {
                saveSystem.DiscardIncompatibleReplay(reason);
            }
        }

        private static bool IsCompatible(
            string storyText,
            string initialState,
            StoryDecision[] decisions,
            out string reason)
        {
            using var story = new Story(new Story.Ctx
            {
                StoryText = storyText,
                InitialState = initialState,
            });
            var parser = new StoryParser();

            for (var index = 0; index < decisions.Length; index++)
            {
                if (!TryReadDialogue(story, parser, out var step, out reason))
                    return false;

                var decision = decisions[index];
                if (decision.HasChoice != (step.Choices.Length > 0))
                {
                    reason = decision.HasChoice
                        ? $"Saved choice #{index} points to a dialogue without choices."
                        : $"Saved advance #{index} points to a dialogue with choices.";
                    return false;
                }
                if (!decision.HasChoice)
                    continue;
                if (!Contains(step.Choices, decision.ChoiceId))
                {
                    reason = $"Saved choice #{index} references unavailable option "
                        + $"'{decision.ChoiceId}'.";
                    return false;
                }
                story.SetChoice(decision.ChoiceId);
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryReadDialogue(
            Story story,
            StoryParser parser,
            out StoryStep step,
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
                if (parsed.Step.Command is DialogueStoryCommand)
                {
                    step = parsed.Step;
                    reason = string.Empty;
                    return true;
                }
            }
        }

        private static bool Contains(StoryChoice[] choices, int choiceId)
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
