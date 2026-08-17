using System;
using System.Linq;

namespace Novels.QueueProcess
{
    internal sealed class ChoiceSelectionHandler
    {
        internal const byte NoChoice = byte.MaxValue;

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

        internal void ApplySaved(byte choiceId)
        {
            if (choiceId == NoChoice)
                return;
            var choice = GetSavedChoice(choiceId);
            ApplyActions(choice);
            _request.SetChoice(choice.Id);
        }

        internal void CompleteWithoutChoice()
        {
            _request.SaveChoice(NoChoice);
            _request.BubbleDone.TrySetResult();
        }

        private void Select(StoryContracts.StoryChoice choice, int id)
        {
            ApplyActions(choice);
            _request.SaveChoice(ToSaveChoiceId(id));
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

        private StoryContracts.StoryChoice GetSavedChoice(byte choiceId)
        {
            foreach (var choice in _request.Choices)
            {
                if (choice.Id == choiceId)
                    return choice;
            }
            throw new InvalidOperationException(
                $"Saved choice '{choiceId}' is not available in the current dialogue.");
        }

        private static byte ToSaveChoiceId(int id)
        {
            if (id < byte.MinValue || id >= NoChoice)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    id,
                    "Choice id must fit the save format range 0-254.");
            }
            return (byte)id;
        }
    }
}
