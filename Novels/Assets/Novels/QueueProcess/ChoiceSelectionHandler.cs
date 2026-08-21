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

        internal WardrobeContracts.WardrobePresentation CreateWardrobePresentation()
        {
            var options = _request.Choices.Select(choice =>
                new WardrobeContracts.WardrobeOption(choice.Id, choice.Text)).ToArray();
            return new WardrobeContracts.WardrobePresentation(
                GetWardrobeTitle(),
                _request.ChoiceConfirmationText,
                options,
                id =>
                {
                    var choice = GetChoice(id);
                    return _request.LoadWardrobeThumbnail(_request.ChoiceActions, choice.Text);
                },
                id =>
                {
                    var choice = GetChoice(id);
                    return _request.PreviewWardrobeChoice(_request.ChoiceActions, choice.Text);
                },
                id =>
                {
                    var choice = GetChoice(id);
                    Select(choice, id);
                });
        }

        private string GetWardrobeTitle()
        {
            if (!string.IsNullOrWhiteSpace(_request.Value))
                return _request.Value;
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                return WardrobeContracts.WardrobeLabels.Appearance;
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                return WardrobeContracts.WardrobeLabels.Hair;
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                return WardrobeContracts.WardrobeLabels.Accessory;
            return WardrobeContracts.WardrobeLabels.Clothes;
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
            if ((_request.ChoiceActions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                _request.SetMainCharacterAccessory(choice.Text);
        }

        private StoryContracts.StoryChoice GetSavedChoice(int choiceId)
            => GetChoice(choiceId);

        private StoryContracts.StoryChoice GetChoice(int choiceId)
        {
            foreach (var choice in _request.Choices)
            {
                if (choice.Id == choiceId)
                    return choice;
            }
            throw new InvalidOperationException(
                $"Choice '{choiceId}' is not available in the current dialogue.");
        }

    }
}
