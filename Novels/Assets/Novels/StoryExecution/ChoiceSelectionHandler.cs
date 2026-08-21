using System;
using System.Linq;

namespace Novels.StoryExecution
{
    internal sealed class ChoiceSelectionHandler
    {
        private readonly BubbleOperationRequest _request;

        internal ChoiceSelectionHandler(BubbleOperationRequest request)
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
                _request.Dialogue.ChoiceConfirmationText,
                options,
                id =>
                {
                    var choice = GetChoice(id);
                    return _request.Services.Character.LoadWardrobeThumbnail(
                        _request.Dialogue.ChoiceActions,
                        choice.Text);
                },
                id =>
                {
                    var choice = GetChoice(id);
                    return _request.Services.Character.PreviewWardrobeChoice(
                        _request.Dialogue.ChoiceActions,
                        choice.Text);
                },
                id =>
                {
                    var choice = GetChoice(id);
                    Select(choice, id);
                });
        }

        internal ChooseContracts.ChoosePresentation CreateChoosePresentation()
        {
            var options = _request.Choices.Select(choice =>
                new ChooseContracts.ChooseOption(choice.Id, choice.Text)).ToArray();
            return new ChooseContracts.ChoosePresentation(
                _request.Dialogue.Text,
                _request.Dialogue.ChoiceConfirmationText,
                options,
                id => _request.Services.LoadChooseThumbnail(GetChoice(id).Text),
                id =>
                {
                    var choice = GetChoice(id);
                    Select(choice, id);
                });
        }

        private string GetWardrobeTitle()
        {
            if (!string.IsNullOrWhiteSpace(_request.Dialogue.Text))
                return _request.Dialogue.Text;
            if ((_request.Dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                return WardrobeContracts.WardrobeLabels.Appearance;
            if ((_request.Dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                return WardrobeContracts.WardrobeLabels.Hair;
            if ((_request.Dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                return WardrobeContracts.WardrobeLabels.Accessory;
            return WardrobeContracts.WardrobeLabels.Clothes;
        }

        internal void ApplySaved(StoryContracts.StoryDecision decision)
        {
            if (!decision.HasChoice)
                return;
            var choice = GetSavedChoice(decision.ChoiceId);
            ApplyActions(choice);
            _request.Services.Story.SetChoice(choice.Id);
        }

        internal void CompleteWithoutChoice()
        {
            _request.Services.Save.SaveDecision(StoryContracts.StoryDecision.Advance);
            _request.Completed.TrySetResult();
        }

        private void Select(StoryContracts.StoryChoice choice, int id)
        {
            ApplyActions(choice);
            _request.Services.Save.SaveDecision(StoryContracts.StoryDecision.Choice(id));
            _request.Services.Story.SetChoice(id);
            _request.Completed.TrySetResult();
        }

        private void ApplyActions(StoryContracts.StoryChoice choice)
        {
            var actions = _request.Dialogue.ChoiceActions;
            var character = _request.Services.Character;
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                character.SetMainCharacterView(choice.Text);
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                character.SetMainCharacterClothes(choice.Text);
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                character.SetMainCharacterHair(choice.Text);
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                character.SetMainCharacterAccessory(choice.Text);
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
