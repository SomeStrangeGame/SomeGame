using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    internal readonly struct DialogueQueueBuildResult
    {
        internal DialogueQueueBuildResult(
            QueueProcess.IQueue[] beforeCommands,
            QueueProcess.IQueue[] afterCommands)
        {
            BeforeCommands = beforeCommands;
            AfterCommands = afterCommands;
        }

        internal QueueProcess.IQueue[] BeforeCommands { get; }
        internal QueueProcess.IQueue[] AfterCommands { get; }
    }

    internal sealed class DialogueQueueBuilder
    {
        private readonly Entity.DialogueCtx _ctx;
        private readonly HashSet<string> _hiddenCharacters = new(StringComparer.Ordinal);

        private string _lastCharacterId = string.Empty;

        internal DialogueQueueBuilder(Entity.DialogueCtx ctx)
        {
            _ctx = ctx;
        }

        internal DialogueQueueBuildResult Build(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone,
            bool hasPendingCommands)
        {
            var name = dialogue.Speaker;
            if (string.IsNullOrEmpty(dialogue.Speaker) && string.IsNullOrEmpty(dialogue.Text))
            {
                return new DialogueQueueBuildResult(
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
            var position = dialogue.Character.Position ?? GetCharacterPosition(role);
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
                    GetDialogueAlignment(role),
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
            return new DialogueQueueBuildResult(
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

        private static StoryContracts.StoryCharacterPosition GetCharacterPosition(
            StoryContracts.StorySpeakerRole role)
        {
            return role switch
            {
                StoryContracts.StorySpeakerRole.MainCharacter => StoryContracts.StoryCharacterPosition.Left,
                StoryContracts.StorySpeakerRole.Wardrobe => StoryContracts.StoryCharacterPosition.Center,
                StoryContracts.StorySpeakerRole.Choose => StoryContracts.StoryCharacterPosition.Center,
                StoryContracts.StorySpeakerRole.Narrator => StoryContracts.StoryCharacterPosition.Center,
                _ => StoryContracts.StoryCharacterPosition.Right,
            };
        }

        private static StoryContracts.StoryDialogueAlignment GetDialogueAlignment(
            StoryContracts.StorySpeakerRole role)
        {
            return role switch
            {
                StoryContracts.StorySpeakerRole.MainCharacter => StoryContracts.StoryDialogueAlignment.Left,
                StoryContracts.StorySpeakerRole.Wardrobe => StoryContracts.StoryDialogueAlignment.Center,
                StoryContracts.StorySpeakerRole.Choose => StoryContracts.StoryDialogueAlignment.Center,
                StoryContracts.StorySpeakerRole.Narrator => StoryContracts.StoryDialogueAlignment.Center,
                _ => StoryContracts.StoryDialogueAlignment.Right,
            };
        }

        private QueueProcess.IQueue[] CreatePresentationLifecycle(
            BubbleContracts.BubblePresentationKind kind,
            UniTaskCompletionSource bubbleDone)
        {
            var show = _ctx.Bubble.BubbleShow;
            var showImmediate = _ctx.Bubble.BubbleShowImmediate;
            var hide = _ctx.Bubble.BubbleHide;
            var hideImmediate = _ctx.Bubble.BubbleHideImmediate;
            if (kind == BubbleContracts.BubblePresentationKind.Wardrobe)
            {
                show = _ctx.Wardrobe.Show;
                showImmediate = _ctx.Wardrobe.ShowImmediate;
                hide = _ctx.Wardrobe.Hide;
                hideImmediate = _ctx.Wardrobe.HideImmediate;
            }
            else if (kind == BubbleContracts.BubblePresentationKind.Choose)
            {
                show = _ctx.Choose.Show;
                showImmediate = _ctx.Choose.ShowImmediate;
                hide = _ctx.Choose.Hide;
                hideImmediate = _ctx.Choose.HideImmediate;
            }
            return new QueueProcess.IQueue[]
            {
                new QueueProcess.BubbleQueue.ShowBubbleQueue(
                    bubbleDone,
                    show,
                    showImmediate),
                new QueueProcess.BubbleQueue.HideBubbleQueue(
                    hide,
                    hideImmediate),
            };
        }
    }
}
