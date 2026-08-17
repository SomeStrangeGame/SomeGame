using System;
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
            var position = GetCharacterPosition(role);
            var setBubble = CreateSetBubbleQueue(dialogue, role, choices, bubbleDone);

            var characterName = name;
            if (dialogue.Character.IsChild)
                characterName += "_child";

            var isNewCharacter = _lastCharacterId != characterName;
            if (isNewCharacter)
                _lastCharacterId = characterName;

            return new DialogueQueueBuildResult(
                new QueueProcess.IQueue[]
                {
                    new QueueProcess.CharacterQueue.HideCharacterQueue(
                        _ctx.Character.CharacterHide,
                        _ctx.Character.CharacterHideImmediate,
                        isNewCharacter),
                    setBubble,
                },
                new QueueProcess.IQueue[]
                {
                    new QueueProcess.CharacterQueue.SetDialogueQueue(
                        _ctx.Location.SetDialogue,
                        _ctx.Location.SetDialogueImmediate,
                        GetDialogueAlignment(role)),
                    new QueueProcess.CharacterQueue.ShowCharacterQueue(
                        _ctx.Character.CharacterSetImage,
                        _ctx.Character.CharacterShow,
                        _ctx.Character.CharacterShowImmediate,
                        isNewCharacter,
                        new StoryContracts.CharacterRenderRequest(
                            name,
                            role,
                            position,
                            dialogue.Character)),
                    new QueueProcess.BubbleQueue.ShowBubbleQueue(
                        bubbleDone,
                        _ctx.Bubble.BubbleShow,
                        _ctx.Bubble.BubbleShowImmediate),
                    new QueueProcess.BubbleQueue.HideBubbleQueue(
                        _ctx.Bubble.BubbleHide,
                        _ctx.Bubble.BubbleHideImmediate),
                });
        }

        private QueueProcess.BubbleQueue.SetBubbleQueue CreateSetBubbleQueue(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StorySpeakerRole role,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone)
        {
            return new QueueProcess.BubbleQueue.SetBubbleQueue(
                bubbleDone,
                _ctx.Localization.GetLocalizationValue,
                choices,
                _ctx.Character.SetMainCharacterView,
                _ctx.Character.SetMainCharacterClothes,
                _ctx.Character.SetMainCharacterHair,
                _ctx.Choice.SaveChoice,
                _ctx.Choice.SetChoice,
                dialogue.Speaker,
                dialogue.Text,
                role,
                dialogue.Presentation,
                dialogue.ChoiceActions,
                _ctx.Bubble.SetBubbleScreen,
                _ctx.Bubble.SetWardrobeScreen,
                _ctx.Bubble.SetChooseScreen);
        }

        private StoryContracts.StorySpeakerRole ResolveSpeakerRole(
            StoryCommands.DialogueCommandData dialogue)
        {
            if (dialogue.Presentation == StoryContracts.DialoguePresentation.Narrator)
                return StoryContracts.StorySpeakerRole.Narrator;

            if (dialogue.Presentation == StoryContracts.DialoguePresentation.Wardrobe)
                return StoryContracts.StorySpeakerRole.Wardrobe;

            return dialogue.Speaker == _ctx.MainCharacter
                ? StoryContracts.StorySpeakerRole.MainCharacter
                : StoryContracts.StorySpeakerRole.Character;
        }

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
