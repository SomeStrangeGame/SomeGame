using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    internal sealed class DialogueQueueBuilder
    {
        private readonly Entity.DialogueCtx _ctx;
        private readonly HashSet<string> _hiddenCharacters = new(StringComparer.Ordinal);

        private string _lastCharacterId = string.Empty;

        internal DialogueQueueBuilder(Entity.DialogueCtx ctx)
        {
            _ctx = ctx;
        }

        internal (
            QueueProcess.IQueue[] BeforeCommands,
            QueueProcess.IQueue[] AfterCommands) Build(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone,
            bool hasPendingCommands)
        {
            var name = dialogue.Speaker;
            if (string.IsNullOrEmpty(dialogue.Speaker) && string.IsNullOrEmpty(dialogue.Text))
            {
                return (
                    new QueueProcess.IQueue[]
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

            var afterCommands = new List<QueueProcess.IQueue>
            {
                new QueueProcess.CharacterQueue.SetDialogueQueue(
                    _ctx.Location.SetDialogue,
                    _ctx.Location.SetDialogueImmediate,
                    _ctx.Character.CharacterHide,
                    _ctx.Character.CharacterHideImmediate,
                    layout.Alignment,
                    hideDuringDialogueTransition),
            };
            if (!isHidden
                && StoryContracts.StorySpeakerRoleResolver.ShowsCharacter(role))
            {
                afterCommands.Add(
                    new QueueProcess.CharacterQueue.ShowCharacterQueue(
                        _ctx.Character.CharacterSetImage,
                        _ctx.Character.CharacterShow,
                        _ctx.Character.CharacterShowImmediate,
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

            var beforeCommands = new List<QueueProcess.IQueue>();
            if (hideBeforePendingCommands)
            {
                beforeCommands.Add(
                    new QueueProcess.CharacterQueue.HideCharacterQueue(
                        _ctx.Character.CharacterHide,
                        _ctx.Character.CharacterHideImmediate,
                        shouldHide: true));
            }

            beforeCommands.Add(setBubble);
            return (
                beforeCommands.ToArray(),
                afterCommands.ToArray());
        }

        private QueueProcess.BubbleQueue.SetBubbleQueue CreateSetBubbleQueue(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StorySpeakerRole role,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone)
        {
            return new QueueProcess.BubbleQueue.SetBubbleQueue(
                new QueueProcess.BubbleQueueRequest(
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

        private QueueProcess.IQueue[] CreatePresentationLifecycle(
            BubbleContracts.BubblePresentationKind kind,
            UniTaskCompletionSource bubbleDone)
        {
            (Func<UniTask> Show, Action ShowImmediate, Func<UniTask> Hide,
                Action HideImmediate) lifecycle = kind switch
            {
                BubbleContracts.BubblePresentationKind.Wardrobe => (
                    _ctx.Wardrobe.Show,
                    _ctx.Wardrobe.ShowImmediate,
                    _ctx.Wardrobe.Hide,
                    _ctx.Wardrobe.HideImmediate),
                BubbleContracts.BubblePresentationKind.Choose => (
                    _ctx.Choose.Show,
                    _ctx.Choose.ShowImmediate,
                    _ctx.Choose.Hide,
                    _ctx.Choose.HideImmediate),
                _ => (
                    _ctx.Bubble.BubbleShow,
                    _ctx.Bubble.BubbleShowImmediate,
                    _ctx.Bubble.BubbleHide,
                    _ctx.Bubble.BubbleHideImmediate),
            };
            return new QueueProcess.IQueue[]
            {
                new QueueProcess.BubbleQueue.ShowBubbleQueue(
                    bubbleDone,
                    lifecycle.Show,
                    lifecycle.ShowImmediate),
                new QueueProcess.BubbleQueue.HideBubbleQueue(
                    lifecycle.Hide,
                    lifecycle.HideImmediate),
            };
        }
    }
}
