using System;
using System.Collections.Generic;
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
            _request.Services.Character.SetWardrobeTarget(
                _request.Dialogue.WardrobeTarget);
            var options = _request.Choices.Select(choice =>
                new WardrobeContracts.WardrobeOption(choice.Id, choice.Text)).ToArray();
            var sequencePages = CreateWardrobeSequencePages();
            return new WardrobeContracts.WardrobePresentation(
                string.IsNullOrWhiteSpace(_request.Dialogue.WardrobeTarget)
                    ? _request.Services.MainCharacter
                    : _request.Dialogue.WardrobeTarget,
                GetWardrobeCategory(),
                GetWardrobeTitle(),
                _request.Dialogue.ChoiceConfirmationText,
                options,
                category => _request.Services.Character.LoadWardrobeCategory(
                    ToChoiceAction(category)),
                (category, value) => _request.Services.Character.LoadWardrobeThumbnail(
                    ToChoiceAction(category), value),
                (category, value) => _request.Services.Character.PreviewWardrobeChoice(
                    ToChoiceAction(category), value),
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
                },
                sequencePages.Length > 1,
                false,
                sequencePages,
                selected =>
                {
                    if (selected == null || selected.Length == 0)
                        return;
                    _request.Services.WardrobeSequence.SetPending(selected.Skip(1));
                    var choice = GetChoice(selected[0]);
                    Select(choice, selected[0]);
                });
        }

        private WardrobeContracts.WardrobeSequencePage[] CreateWardrobeSequencePages()
        {
            var pages = new List<WardrobeContracts.WardrobeSequencePage>
            {
                CreateWardrobeSequencePage(_request.Dialogue, _request.Choices),
            };
            var future = _request.Services.PeekWardrobeSteps?.Invoke()
                ?? Array.Empty<StoryCommands.StoryStep>();
            foreach (var step in future)
            {
                if (step.Command is not StoryCommands.DialogueStoryCommand dialogue
                    || dialogue.Data.WardrobeTarget != _request.Dialogue.WardrobeTarget)
                {
                    break;
                }
                var category = GetWardrobeCategory(dialogue.Data);
                if (pages.Any(page => page.Category == category))
                    break;
                pages.Add(CreateWardrobeSequencePage(dialogue.Data, step.Choices));
            }
            return pages.ToArray();
        }

        private WardrobeContracts.WardrobeSequencePage CreateWardrobeSequencePage(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices)
        {
            var options = choices.Select(choice =>
                new WardrobeContracts.WardrobeOption(choice.Id, choice.Text)).ToArray();
            StoryContracts.StoryChoice FindChoice(int id)
            {
                foreach (var choice in choices)
                {
                    if (choice.Id == id)
                        return choice;
                }
                throw new InvalidOperationException($"Choice '{id}' is unavailable.");
            }
            return new WardrobeContracts.WardrobeSequencePage(
                GetWardrobeCategory(dialogue),
                GetWardrobeTitle(dialogue),
                options,
                id => _request.Services.Character.LoadWardrobeThumbnail(
                    dialogue.ChoiceActions,
                    FindChoice(id).Text),
                id => _request.Services.Character.PreviewWardrobeChoice(
                    dialogue.ChoiceActions,
                    FindChoice(id).Text));
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
            => GetWardrobeTitle(_request.Dialogue);

        private static string GetWardrobeTitle(StoryCommands.DialogueCommandData dialogue)
        {
            if (!string.IsNullOrWhiteSpace(dialogue.Text))
                return dialogue.Text;
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                return WardrobeContracts.WardrobeLabels.Appearance;
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                return WardrobeContracts.WardrobeLabels.Hair;
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                return WardrobeContracts.WardrobeLabels.Accessory;
            return WardrobeContracts.WardrobeLabels.Clothes;
        }

        private WardrobeContracts.WardrobeCategory GetWardrobeCategory()
            => GetWardrobeCategory(_request.Dialogue);

        private static WardrobeContracts.WardrobeCategory GetWardrobeCategory(
            StoryCommands.DialogueCommandData dialogue)
        {
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                return WardrobeContracts.WardrobeCategory.Appearance;
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                return WardrobeContracts.WardrobeCategory.Hair;
            if ((dialogue.ChoiceActions
                    & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                return WardrobeContracts.WardrobeCategory.Accessory;
            return WardrobeContracts.WardrobeCategory.Clothes;
        }

        private static StoryContracts.StoryChoiceAction ToChoiceAction(
            WardrobeContracts.WardrobeCategory category) => category switch
            {
                WardrobeContracts.WardrobeCategory.Appearance =>
                    StoryContracts.StoryChoiceAction.SelectAppearance,
                WardrobeContracts.WardrobeCategory.Hair =>
                    StoryContracts.StoryChoiceAction.SelectHair,
                WardrobeContracts.WardrobeCategory.Accessory =>
                    StoryContracts.StoryChoiceAction.SelectAccessory,
                _ => StoryContracts.StoryChoiceAction.SelectClothes,
            };

        internal void ApplySaved(StoryContracts.StoryDecision decision)
        {
            if (!decision.HasChoice)
                return;
            var choice = GetSavedChoice(decision.ChoiceId);
            ApplyActions(choice);
            UnlockWardrobeChoices(choice, false);
            _request.Services.Story.SetChoice(choice.Id);
        }

        internal void CompleteWithoutChoice()
        {
            _request.Services.Save.SaveDecision(StoryContracts.StoryDecision.Advance);
            _request.Completed.TrySetResult();
        }

        internal bool TryApplyQueuedWardrobeChoice()
        {
            if (_request.Dialogue.Presentation
                    != StoryContracts.DialoguePresentation.Wardrobe
                || !_request.Services.WardrobeSequence.TryTake(out var id))
            {
                return false;
            }
            var choice = GetChoice(id);
            Select(choice, id);
            return true;
        }

        private void Select(StoryContracts.StoryChoice choice, int id)
        {
            ApplyActions(choice);
            UnlockWardrobeChoices(choice, true);
            _request.Services.Save.SaveDecision(StoryContracts.StoryDecision.Choice(id));
            _request.Services.Story.SetChoice(id);
            _request.Services.OnChoiceSelected?.Invoke(id);
            _request.Completed.TrySetResult();
        }

        private void UnlockWardrobeChoices(
            StoryContracts.StoryChoice selectedChoice,
            bool persist)
        {
            if (_request.Dialogue.ChoiceActions == StoryContracts.StoryChoiceAction.None)
                return;
            if (_request.Dialogue.Presentation != StoryContracts.DialoguePresentation.Wardrobe
                && _request.Dialogue.Presentation != StoryContracts.DialoguePresentation.Choose)
                return;
            var character = string.IsNullOrWhiteSpace(_request.Dialogue.WardrobeTarget)
                ? _request.Services.MainCharacter
                : _request.Dialogue.WardrobeTarget;
            var category = (byte)GetWardrobeCategory();
            foreach (var choice in _request.Choices)
            {
                _request.Services.Save.UnlockWardrobeItem(
                    character,
                    category,
                    choice.Text,
                    false,
                    false);
            }
            _request.Services.Save.UnlockWardrobeItem(
                character,
                category,
                selectedChoice.Text,
                persist);
        }

        private void ApplyActions(StoryContracts.StoryChoice choice)
        {
            var actions = _request.Dialogue.ChoiceActions;
            var character = _request.Services.Character;
            if (!string.IsNullOrWhiteSpace(_request.Dialogue.WardrobeTarget))
            {
                character.ApplyWardrobeSelection(
                    _request.Dialogue.WardrobeTarget,
                    actions,
                    choice.Text);
                return;
            }
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
