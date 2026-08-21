using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public sealed class BubbleOperationRequest
    {
        public BubbleOperationRequest(
            UniTaskCompletionSource bubbleDone,
            StoryContracts.StoryChoice[] choices,
            Action<string> setMainCharacterView,
            Action<string> setMainCharacterClothes,
            Action<string> setMainCharacterHair,
            Action<string> setMainCharacterAccessory,
            Func<StoryContracts.StoryChoiceAction, string, UniTask<UnityEngine.Sprite>> loadWardrobeThumbnail,
            Func<StoryContracts.StoryChoiceAction, string, UniTask> previewWardrobeChoice,
            Func<string, UniTask<UnityEngine.Sprite>> loadChooseThumbnail,
            Action<StoryContracts.StoryDecision> saveDecision,
            Action<int> setChoice,
            string name,
            string value,
            string choiceConfirmationText,
            StoryContracts.StorySpeakerRole speakerRole,
            StoryContracts.DialoguePresentation presentation,
            StoryContracts.StoryChoiceAction choiceActions,
            BubbleContracts.BubblePresentationKind presentationKind,
            Action<BubbleContracts.BubblePresentation> setBubbleScreen,
            Action<WardrobeContracts.WardrobePresentation> setWardrobeScreen,
            Action<ChooseContracts.ChoosePresentation> setChooseScreen)
        {
            BubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
            Choices = choices ?? Array.Empty<StoryContracts.StoryChoice>();
            SetMainCharacterView = setMainCharacterView
                ?? throw new ArgumentNullException(nameof(setMainCharacterView));
            SetMainCharacterClothes = setMainCharacterClothes
                ?? throw new ArgumentNullException(nameof(setMainCharacterClothes));
            SetMainCharacterHair = setMainCharacterHair
                ?? throw new ArgumentNullException(nameof(setMainCharacterHair));
            SetMainCharacterAccessory = setMainCharacterAccessory
                ?? throw new ArgumentNullException(nameof(setMainCharacterAccessory));
            LoadWardrobeThumbnail = loadWardrobeThumbnail
                ?? throw new ArgumentNullException(nameof(loadWardrobeThumbnail));
            PreviewWardrobeChoice = previewWardrobeChoice
                ?? throw new ArgumentNullException(nameof(previewWardrobeChoice));
            LoadChooseThumbnail = loadChooseThumbnail
                ?? throw new ArgumentNullException(nameof(loadChooseThumbnail));
            SaveDecision = saveDecision ?? throw new ArgumentNullException(nameof(saveDecision));
            SetChoice = setChoice ?? throw new ArgumentNullException(nameof(setChoice));
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
            ChoiceConfirmationText = choiceConfirmationText ?? string.Empty;
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
        internal StoryContracts.StoryChoice[] Choices { get; }
        internal Action<string> SetMainCharacterView { get; }
        internal Action<string> SetMainCharacterClothes { get; }
        internal Action<string> SetMainCharacterHair { get; }
        internal Action<string> SetMainCharacterAccessory { get; }
        internal Func<StoryContracts.StoryChoiceAction, string, UniTask<UnityEngine.Sprite>> LoadWardrobeThumbnail { get; }
        internal Func<StoryContracts.StoryChoiceAction, string, UniTask> PreviewWardrobeChoice { get; }
        internal Func<string, UniTask<UnityEngine.Sprite>> LoadChooseThumbnail { get; }
        internal Action<StoryContracts.StoryDecision> SaveDecision { get; }
        internal Action<int> SetChoice { get; }
        internal string Name { get; }
        internal string Value { get; }
        internal string ChoiceConfirmationText { get; }
        internal StoryContracts.StorySpeakerRole SpeakerRole { get; }
        internal StoryContracts.DialoguePresentation Presentation { get; }
        internal StoryContracts.StoryChoiceAction ChoiceActions { get; }
        internal BubbleContracts.BubblePresentationKind PresentationKind { get; }
        internal Action<BubbleContracts.BubblePresentation> SetBubbleScreen { get; }
        internal Action<WardrobeContracts.WardrobePresentation> SetWardrobeScreen { get; }
        internal Action<ChooseContracts.ChoosePresentation> SetChooseScreen { get; }
    }
}
