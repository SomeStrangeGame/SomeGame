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
            UniTaskCompletionSource bubbleDone)
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
                    CreateBubbleLifecycle(bubbleDone));
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

            var afterCommands = new List<QueueProcess.IQueue>
            {
                new QueueProcess.CharacterQueue.SetDialogueQueue(
                    _ctx.Location.SetDialogue,
                    _ctx.Location.SetDialogueImmediate,
                    GetDialogueAlignment(role)),
            };
            if (!isHidden)
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
            afterCommands.AddRange(CreateBubbleLifecycle(bubbleDone));

            return new DialogueQueueBuildResult(
                new QueueProcess.IQueue[]
                {
                    new QueueProcess.CharacterQueue.HideCharacterQueue(
                        _ctx.Character.CharacterHide,
                        _ctx.Character.CharacterHideImmediate,
                        shouldHide),
                    setBubble,
                },
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
                _ctx.Choice.SaveDecision,
                _ctx.Choice.SetChoice,
                string.IsNullOrEmpty(dialogue.Character.DisplayName)
                    ? dialogue.Speaker
                    : dialogue.Character.DisplayName,
                dialogue.Text,
                role,
                dialogue.Presentation,
                dialogue.ChoiceActions,
                BubbleContracts.BubbleTriggers.Resolve(dialogue.Speaker),
                _ctx.Bubble.SetBubbleScreen,
                _ctx.Bubble.SetWardrobeScreen,
                _ctx.Bubble.SetChooseScreen));
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
                StoryContracts.StorySpeakerRole.Narrator => StoryContracts.StoryDialogueAlignment.Center,
                _ => StoryContracts.StoryDialogueAlignment.Right,
            };
        }

        private QueueProcess.IQueue[] CreateBubbleLifecycle(
            UniTaskCompletionSource bubbleDone)
        {
            return new QueueProcess.IQueue[]
            {
                new QueueProcess.BubbleQueue.ShowBubbleQueue(
                    bubbleDone,
                    _ctx.Bubble.BubbleShow,
                    _ctx.Bubble.BubbleShowImmediate),
                new QueueProcess.BubbleQueue.HideBubbleQueue(
                    _ctx.Bubble.BubbleHide,
                    _ctx.Bubble.BubbleHideImmediate),
            };
        }
    }
}
