using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    internal sealed class DialogueQueueBuilder
    {
        private readonly struct DialogueFrame
        {
            internal DialogueFrame(
                StoryCommands.DialogueCommandData dialogue,
                StoryContracts.StorySpeakerRole role,
                BubbleContracts.BubblePresentationKind presentation,
                StoryContracts.StoryCharacterPosition position,
                StoryContracts.StoryDialogueAlignment alignment)
            {
                Dialogue = dialogue;
                Role = role;
                Presentation = presentation;
                Position = position;
                Alignment = alignment;
            }

            internal StoryCommands.DialogueCommandData Dialogue { get; }
            internal StoryContracts.StorySpeakerRole Role { get; }
            internal BubbleContracts.BubblePresentationKind Presentation { get; }
            internal StoryContracts.StoryCharacterPosition Position { get; }
            internal StoryContracts.StoryDialogueAlignment Alignment { get; }
        }

        private readonly StoryQueueBuilder.Dependencies _dependencies;
        private readonly HashSet<string> _hiddenCharacters = new(StringComparer.Ordinal);

        private string _lastCharacterId = string.Empty;

        internal DialogueQueueBuilder(StoryQueueBuilder.Dependencies dependencies)
        {
            _dependencies = dependencies;
        }

        internal (
            StoryExecution.IStoryOperation[] BeforeCommands,
            StoryExecution.IStoryOperation[] AfterCommands) Build(
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone,
            bool hasPendingCommands)
        {
            if (string.IsNullOrEmpty(dialogue.Speaker) && string.IsNullOrEmpty(dialogue.Text))
            {
                return (
                    new StoryExecution.IStoryOperation[]
                    {
                        CreateSetBubbleQueue(
                            new DialogueFrame(
                                dialogue,
                                StoryContracts.StorySpeakerRole.Character,
                                BubbleContracts.BubblePresentationKind.Dialogue,
                                StoryContracts.StoryCharacterPosition.Center,
                                StoryContracts.StoryDialogueAlignment.Center),
                            choices,
                            bubbleDone),
                    },
                    CreatePresentationLifecycle(
                        BubbleContracts.BubblePresentationKind.Dialogue,
                        bubbleDone));
            }

            var frame = CreateFrame(dialogue);
            var setBubble = CreateSetBubbleQueue(frame, choices, bubbleDone);

            var characterName = dialogue.Speaker;
            if (dialogue.Character.IsChild)
                characterName += "_child";

            var isNewCharacter = _lastCharacterId != characterName;
            if (isNewCharacter)
                _lastCharacterId = characterName;

            var visibility = dialogue.Character.Visibility;
            if (visibility == StoryContracts.StoryCharacterVisibilityCommand.Hide)
                _hiddenCharacters.Add(dialogue.Speaker);
            else if (visibility == StoryContracts.StoryCharacterVisibilityCommand.Show)
                _hiddenCharacters.Remove(dialogue.Speaker);

            var isHidden = _hiddenCharacters.Contains(dialogue.Speaker);
            var shouldHide = isNewCharacter
                || visibility == StoryContracts.StoryCharacterVisibilityCommand.Hide;
            var shouldShow = isNewCharacter
                || visibility == StoryContracts.StoryCharacterVisibilityCommand.Show;
            var hideBeforePendingCommands = shouldHide && hasPendingCommands;
            var hideDuringDialogueTransition = shouldHide && !hasPendingCommands;

            var afterCommands = new List<StoryExecution.IStoryOperation>
            {
                CreateSetDialogueOperation(
                    frame.Alignment,
                    hideDuringDialogueTransition),
            };
            if (!isHidden
                && StoryContracts.StorySpeakerRoleResolver.ShowsCharacter(frame.Role))
            {
                afterCommands.Add(
                    CreateShowCharacterOperation(
                        shouldShow,
                        new StoryContracts.CharacterRenderRequest(
                            dialogue.Speaker,
                            frame.Role,
                            frame.Position,
                            dialogue.Character)));
            }
            afterCommands.AddRange(CreatePresentationLifecycle(
                frame.Presentation,
                bubbleDone));

            var beforeCommands = new List<StoryExecution.IStoryOperation>();
            if (hideBeforePendingCommands)
            {
                beforeCommands.Add(
                    CreateHideCharacterOperation());
            }

            beforeCommands.Add(setBubble);
            return (
                beforeCommands.ToArray(),
                afterCommands.ToArray());
        }

        private StoryExecution.BubbleOperation.SetBubbleQueue CreateSetBubbleQueue(
            DialogueFrame frame,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource bubbleDone)
        {
            return new StoryExecution.BubbleOperation.SetBubbleQueue(
                new StoryExecution.BubbleOperationRequest(
                    _dependencies,
                    frame.Dialogue,
                    choices,
                    bubbleDone,
                    frame.Role,
                frame.Presentation));
        }

        private StoryExecution.IStoryOperation CreateSetDialogueOperation(
            StoryContracts.StoryDialogueAlignment alignment,
            bool hideCharacter)
        {
            return new StoryExecution.DelegateStoryOperation(async context =>
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!hideCharacter)
                {
                    await _dependencies.Location.SetDialogue(
                        alignment,
                        context.PresentationMode);
                    return;
                }
                await UniTask.WhenAll(
                    _dependencies.Character.Hide(context.PresentationMode),
                    _dependencies.Location.SetDialogue(
                        alignment,
                        context.PresentationMode));
            });
        }

        private StoryExecution.IStoryOperation CreateShowCharacterOperation(
            bool animate,
            StoryContracts.CharacterRenderRequest character)
        {
            return new StoryExecution.DelegateStoryOperation(async context =>
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _dependencies.Character.SetImage(character);
                if (animate || context.Mode == StoryExecution.QueueExecutionMode.Replay)
                {
                    await _dependencies.Character.Show(
                        character.Position,
                        context.PresentationMode);
                }
            });
        }

        private StoryExecution.IStoryOperation CreateHideCharacterOperation()
        {
            return new StoryExecution.DelegateStoryOperation(context =>
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                return _dependencies.Character.Hide(context.PresentationMode);
            });
        }

        private DialogueFrame CreateFrame(StoryCommands.DialogueCommandData dialogue)
        {
            var role = ResolveSpeakerRole(dialogue);
            var layout = GetDialogueLayout(role);
            return new DialogueFrame(
                dialogue,
                role,
                ResolvePresentationKind(dialogue),
                dialogue.Character.Position ?? layout.Position,
                layout.Alignment);
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
                _dependencies.MainCharacter);

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
                    _dependencies.Wardrobe.Show,
                    _dependencies.Wardrobe.Hide),
                BubbleContracts.BubblePresentationKind.Choose => (
                    _dependencies.Choose.Show,
                    _dependencies.Choose.Hide),
                _ => (
                    _dependencies.Bubble.Show,
                    _dependencies.Bubble.Hide),
            };
            return new StoryExecution.IStoryOperation[]
            {
                new StoryExecution.DelegateStoryOperation(async context =>
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    await lifecycle.Show(context.PresentationMode);
                    await bubbleDone.Task.AttachExternalCancellation(
                        context.CancellationToken);
                }),
                new StoryExecution.DelegateStoryOperation(context =>
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    return lifecycle.Hide(context.PresentationMode);
                }),
            };
        }
    }
}
