using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public sealed class BubbleQueueRequest
    {
        public BubbleQueueRequest(
            UniTaskCompletionSource bubbleDone,
            Func<string, string> getLocalizationValue,
            StoryContracts.StoryChoice[] choices,
            Action<string> setMainCharacterView,
            Action<string> setMainCharacterClothes,
            Action<string> setMainCharacterHair,
            Action<byte> saveChoice,
            Action<int> setChoice,
            string name,
            string value,
            StoryContracts.StorySpeakerRole speakerRole,
            StoryContracts.DialoguePresentation presentation,
            StoryContracts.StoryChoiceAction choiceActions,
            BubbleContracts.BubblePresentationKind presentationKind,
            Action<BubbleContracts.BubblePresentation> setBubbleScreen,
            Action<BubbleContracts.WardrobePresentation> setWardrobeScreen,
            Action<BubbleContracts.ChoosePresentation> setChooseScreen)
        {
            BubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
            GetLocalizationValue = getLocalizationValue
                ?? throw new ArgumentNullException(nameof(getLocalizationValue));
            Choices = choices ?? Array.Empty<StoryContracts.StoryChoice>();
            SetMainCharacterView = setMainCharacterView
                ?? throw new ArgumentNullException(nameof(setMainCharacterView));
            SetMainCharacterClothes = setMainCharacterClothes
                ?? throw new ArgumentNullException(nameof(setMainCharacterClothes));
            SetMainCharacterHair = setMainCharacterHair
                ?? throw new ArgumentNullException(nameof(setMainCharacterHair));
            SaveChoice = saveChoice ?? throw new ArgumentNullException(nameof(saveChoice));
            SetChoice = setChoice ?? throw new ArgumentNullException(nameof(setChoice));
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
            SpeakerRole = speakerRole;
            Presentation = presentation;
            ChoiceActions = choiceActions;
            PresentationKind = presentationKind;
            SetBubbleScreen = setBubbleScreen
                ?? throw new ArgumentNullException(nameof(setBubbleScreen));
            SetWardrobeScreen = setWardrobeScreen
                ?? throw new ArgumentNullException(nameof(setWardrobeScreen));
            SetChooseScreen = setChooseScreen
                ?? throw new ArgumentNullException(nameof(setChooseScreen));
        }

        internal UniTaskCompletionSource BubbleDone { get; }
        internal Func<string, string> GetLocalizationValue { get; }
        internal StoryContracts.StoryChoice[] Choices { get; }
        internal Action<string> SetMainCharacterView { get; }
        internal Action<string> SetMainCharacterClothes { get; }
        internal Action<string> SetMainCharacterHair { get; }
        internal Action<byte> SaveChoice { get; }
        internal Action<int> SetChoice { get; }
        internal string Name { get; }
        internal string Value { get; }
        internal StoryContracts.StorySpeakerRole SpeakerRole { get; }
        internal StoryContracts.DialoguePresentation Presentation { get; }
        internal StoryContracts.StoryChoiceAction ChoiceActions { get; }
        internal BubbleContracts.BubblePresentationKind PresentationKind { get; }
        internal Action<BubbleContracts.BubblePresentation> SetBubbleScreen { get; }
        internal Action<BubbleContracts.WardrobePresentation> SetWardrobeScreen { get; }
        internal Action<BubbleContracts.ChoosePresentation> SetChooseScreen { get; }
    }
}
