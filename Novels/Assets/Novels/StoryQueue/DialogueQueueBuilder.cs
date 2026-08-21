using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    internal sealed class DialogueQueueBuilder
    {
        private readonly StoryQueueBuilder.DialogueCtx _ctx;
        private readonly HashSet<string> _hiddenCharacters = new(StringComparer.Ordinal);

        private string _lastCharacterId = string.Empty;

        internal DialogueQueueBuilder(StoryQueueBuilder.DialogueCtx ctx)
        {
            _ctx = ctx;
        }

        internal (
            StoryExecution.IStoryOperation[] BeforeCommands,
            StoryExecution.IStoryOperation[] AfterCommands) Build(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone,
            bool hasPendingCommands)
        {
            var name = dialogue.Speaker;
            if (string.IsNullOrEmpty(dialogue.Speaker) && string.IsNullOrEmpty(dialogue.Text))
            {
                return (
                    new StoryExecution.IStoryOperation[]
                    {
                        CreateSetBubbleQueue(
                            dialogue,
                            StoryContracts.StorySpeakerRole.Character,
                            choices,
                            bubbleDone),
                    },
                    CreatePresentationLifecycle(
                        BubbleContracts.BubblePresentationKind.Dialogue,
                        bubbleDone));
            }

            var role = ResolveSpeakerRole(dialogue);
            var layout = GetDialogueLayout(role);
            var position = dialogue.Character.Position ?? layout.Position;
            var setBubble = CreateSetBubbleQueue(dialogue, role, choices, bubbleDone);

            var characterName = name;
            if (dialogue.Character.IsChild)
                characterName += "_child";

            var isNewCharacter = _lastCharacterId != characterName;
            if (isNewCharacter)
                _lastCharacterId = characterName;

            var visibility = dialogue.Character.Visibility;
            if (visibility == StoryContracts.StoryCharacterVisibilityCommand.Hide)
                _hiddenCharacters.Add(name);
            else if (visibility == StoryContracts.StoryCharacterVisibilityCommand.Show)
                _hiddenCharacters.Remove(name);

            var isHidden = _hiddenCharacters.Contains(name);
            var shouldHide = isNewCharacter
                || visibility == StoryContracts.StoryCharacterVisibilityCommand.Hide;
            var shouldShow = isNewCharacter
                || visibility == StoryContracts.StoryCharacterVisibilityCommand.Show;
            var hideBeforePendingCommands = shouldHide && hasPendingCommands;
            var hideDuringDialogueTransition = shouldHide && !hasPendingCommands;

            var afterCommands = new List<StoryExecution.IStoryOperation>
            {
                new StoryExecution.CharacterOperation.SetDialogueQueue(
                    _ctx.Location.SetDialogue,
                    _ctx.Character.CharacterHide,
                    layout.Alignment,
                    hideDuringDialogueTransition),
            };
            if (!isHidden
                && StoryContracts.StorySpeakerRoleResolver.ShowsCharacter(role))
            {
                afterCommands.Add(
                    new StoryExecution.CharacterOperation.ShowCharacterQueue(
                        _ctx.Character.CharacterSetImage,
                        _ctx.Character.CharacterShow,
                        shouldShow,
                        new StoryContracts.CharacterRenderRequest(
                            name,
                            role,
                            position,
                            dialogue.Character)));
            }
            afterCommands.AddRange(CreatePresentationLifecycle(
                ResolvePresentationKind(dialogue),
                bubbleDone));

            var beforeCommands = new List<StoryExecution.IStoryOperation>();
            if (hideBeforePendingCommands)
            {
                beforeCommands.Add(
                    new StoryExecution.CharacterOperation.HideCharacterQueue(
                        _ctx.Character.CharacterHide,
                        shouldHide: true));
            }

            beforeCommands.Add(setBubble);
            return (
                beforeCommands.ToArray(),
                afterCommands.ToArray());
        }

        private StoryExecution.BubbleOperation.SetBubbleQueue CreateSetBubbleQueue(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StorySpeakerRole role,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone)
        {
            return new StoryExecution.BubbleOperation.SetBubbleQueue(
                new StoryExecution.BubbleOperationRequest(
                bubbleDone,
                choices,
                _ctx.Character.SetMainCharacterView,
                _ctx.Character.SetMainCharacterClothes,
                _ctx.Character.SetMainCharacterHair,
                _ctx.Character.SetMainCharacterAccessory,
                _ctx.Character.LoadWardrobeThumbnail,
                _ctx.Character.PreviewWardrobeChoice,
                _ctx.Choose.LoadThumbnail,
                _ctx.Choice.SaveDecision,
                _ctx.Choice.SetChoice,
                string.IsNullOrEmpty(dialogue.Character.DisplayName)
                    ? dialogue.Speaker
                    : dialogue.Character.DisplayName,
                dialogue.Text,
                dialogue.ChoiceConfirmationText,
                role,
                dialogue.Presentation,
                dialogue.ChoiceActions,
                ResolvePresentationKind(dialogue),
                _ctx.Bubble.SetBubbleScreen,
                _ctx.Wardrobe.SetScreen,
                _ctx.Choose.SetScreen));
        }

        private static BubbleContracts.BubblePresentationKind ResolvePresentationKind(
            StoryCommands.DialogueCommandData dialogue)
        {
            if (dialogue.Presentation == StoryContracts.DialoguePresentation.Wardrobe)
                return BubbleContracts.BubblePresentationKind.Wardrobe;
            if (dialogue.Presentation == StoryContracts.DialoguePresentation.Choose)
                return BubbleContracts.BubblePresentationKind.Choose;
            return BubbleContracts.BubblePresentationKind.Dialogue;
        }

        private StoryContracts.StorySpeakerRole ResolveSpeakerRole(
            StoryCommands.DialogueCommandData dialogue)
            => StoryContracts.StorySpeakerRoleResolver.Resolve(
                dialogue.Speaker,
                dialogue.Presentation,
                _ctx.MainCharacter);

        private static (
            StoryContracts.StoryCharacterPosition Position,
            StoryContracts.StoryDialogueAlignment Alignment) GetDialogueLayout(
            StoryContracts.StorySpeakerRole role)
        {
            return role switch
            {
                StoryContracts.StorySpeakerRole.MainCharacter => (
                    StoryContracts.StoryCharacterPosition.Left,
                    StoryContracts.StoryDialogueAlignment.Left),
                StoryContracts.StorySpeakerRole.Wardrobe
                    or StoryContracts.StorySpeakerRole.Choose
                    or StoryContracts.StorySpeakerRole.Narrator => (
                        StoryContracts.StoryCharacterPosition.Center,
                        StoryContracts.StoryDialogueAlignment.Center),
                _ => (
                    StoryContracts.StoryCharacterPosition.Right,
                    StoryContracts.StoryDialogueAlignment.Right),
            };
        }

        private StoryExecution.IStoryOperation[] CreatePresentationLifecycle(
            BubbleContracts.BubblePresentationKind kind,
            UniTaskCompletionSource bubbleDone)
        {
            (Func<StoryContracts.PresentationMode, UniTask> Show,
                Func<StoryContracts.PresentationMode, UniTask> Hide) lifecycle = kind switch
            {
                BubbleContracts.BubblePresentationKind.Wardrobe => (
                    _ctx.Wardrobe.Show,
                    _ctx.Wardrobe.Hide),
                BubbleContracts.BubblePresentationKind.Choose => (
                    _ctx.Choose.Show,
                    _ctx.Choose.Hide),
                _ => (
                    _ctx.Bubble.Show,
                    _ctx.Bubble.Hide),
            };
            return new StoryExecution.IStoryOperation[]
            {
                new StoryExecution.BubbleOperation.ShowBubbleQueue(
                    bubbleDone,
                    lifecycle.Show),
                new StoryExecution.BubbleOperation.HideBubbleQueue(
                    lifecycle.Hide),
            };
        }
    }
}
