using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    public sealed class Entity
    {
        public struct CommandCtx
        {
            public Action<string> ShowNotification;
            public LocationCommandPort Location;
            public AudioPort Audio;
        }

        public struct LocationCommandPort
        {
            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImage;
            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImageImmediate;
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCamera;
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCameraImmediate;
            public Func<float, UniTask> Wait;
        }

        public struct AudioPort
        {
            public Func<string, UniTask> PlayMusic;
            public Func<string, UniTask> PlaySound;
            public Func<string, UniTask> PlayAmbient;
        }

        public struct DialogueCtx
        {
            public string MainCharacter;
            public LocationDialoguePort Location;
            public LocalizationPort Localization;
            public BubblePort Bubble;
            public ChoicePort Choice;
            public CharacterPort Character;
        }

        public struct LocationDialoguePort
        {
            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogue;
            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogueImmediate;
        }

        public struct LocalizationPort
        {
            public Func<string, string> GetLocalizationValue;
        }

        public struct BubblePort
        {
            public Func<UniTask> BubbleShow;
            public Action BubbleShowImmediate;
            public Func<UniTask> BubbleHide;
            public Action BubbleHideImmediate;
            public Action<BubbleContracts.BubblePresentation> SetBubbleScreen;
            public Action<BubbleContracts.WardrobePresentation> SetWardrobeScreen;
            public Action<BubbleContracts.ChoosePresentation> SetChooseScreen;
        }

        public struct ChoicePort
        {
            public Action<StoryContracts.StoryDecision> SaveDecision;
            public Action<int> SetChoice;
        }

        public struct CharacterPort
        {
            public Action<string> SetMainCharacterView;
            public Action<string> SetMainCharacterClothes;
            public Action<string> SetMainCharacterHair;
            public Func<UniTask> CharacterHide;
            public Action CharacterHideImmediate;
            public Func<StoryContracts.StoryCharacterPosition, UniTask> CharacterShow;
            public Action<StoryContracts.StoryCharacterPosition> CharacterShowImmediate;
            public Func<StoryContracts.CharacterRenderRequest, UniTask> CharacterSetImage;
        }

        public struct Ctx
        {
            public CommandCtx Command;
            public DialogueCtx Dialogue;
        }

        private readonly StoryCommandQueueBuilder _storyCommandQueueBuilder;
        private readonly DialogueQueueBuilder _dialogueQueueBuilder;

        private Queue<QueueProcess.IQueue> _pendingQueue = new();

        public Entity(Ctx ctx)
        {
            _storyCommandQueueBuilder = new StoryCommandQueueBuilder(ctx.Command);
            _dialogueQueueBuilder = new DialogueQueueBuilder(ctx.Dialogue);
        }

        public bool TryBuild(
            StoryCommands.StoryStep step,
            out Queue<QueueProcess.IQueue> queue)
        {
            var command = step.Command;

            if (command is not StoryCommands.DialogueStoryCommand dialogueCommand)
            {
                _pendingQueue.Enqueue(_storyCommandQueueBuilder.Build(command));
                queue = null;
                return false;
            }

            var bubbleDone = new UniTaskCompletionSource();
            var dialogueQueue = _dialogueQueueBuilder.Build(
                dialogueCommand.Data,
                step.Choices,
                bubbleDone);

            queue = new Queue<QueueProcess.IQueue>();
            EnqueueRange(queue, dialogueQueue.BeforeCommands);
            EnqueueRange(queue, _pendingQueue);
            EnqueueRange(queue, dialogueQueue.AfterCommands);

            _pendingQueue = new Queue<QueueProcess.IQueue>();
            return true;
        }

        public bool TryComplete(out Queue<QueueProcess.IQueue> queue)
        {
            if (_pendingQueue.Count == 0)
            {
                queue = null;
                return false;
            }

            queue = _pendingQueue;
            _pendingQueue = new Queue<QueueProcess.IQueue>();
            return true;
        }

        private static void EnqueueRange(
            Queue<QueueProcess.IQueue> target,
            IEnumerable<QueueProcess.IQueue> source)
        {
            foreach (var item in source)
                target.Enqueue(item);
        }
    }
}
