using System;
using System.Linq;

namespace Novels.QueueProcess
{
    internal sealed class ChoiceSelectionHandler
    {
        private readonly BubbleQueueRequest _request;

        internal ChoiceSelectionHandler(BubbleQueueRequest request)
        {
            _request = request;
        }

        internal BubbleContracts.BubbleChoice[] CreatePresentations()
        {
            return _request.Choices.Select(choice => new BubbleContracts.BubbleChoice(
                choice.Id,
                choice.Text,
                id => Select(choice, id))).ToArray();
        }

        internal void ApplySaved(StoryContracts.StoryDecision decision)
        {
            if (!decision.HasChoice)
                return;
            var choice = GetSavedChoice(decision.ChoiceId);
            ApplyActions(choice);
            _request.SetChoice(choice.Id);
        }

        internal void CompleteWithoutChoice()
        {
            _request.SaveDecision(StoryContracts.StoryDecision.Advance);
            _request.BubbleDone.TrySetResult();
        }

        private void Select(StoryContracts.StoryChoice choice, int id)
        {
            ApplyActions(choice);
            _request.SaveDecision(StoryContracts.StoryDecision.Choice(id));
            _request.SetChoice(id);
            _request.BubbleDone.TrySetResult();
        }

        private void ApplyActions(StoryContracts.StoryChoice choice)
        {
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                _request.SetMainCharacterView(choice.Text);
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                _request.SetMainCharacterClothes(choice.Text);
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                _request.SetMainCharacterHair(choice.Text);
        }

        private StoryContracts.StoryChoice GetSavedChoice(int choiceId)
        {
            foreach (var choice in _request.Choices)
            {
                if (choice.Id == choiceId)
                    return choice;
            }
            throw new InvalidOperationException(
                $"Saved choice '{choiceId}' is not available in the current dialogue.");
        }

    }
}
