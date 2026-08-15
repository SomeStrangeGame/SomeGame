using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    public sealed class Entity
    {
        public struct CommandCtx
        {
            public Func<string, UniTask> ShowNotification;

            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImage;
            public Func<string, StoryContracts.StoryBackgroundPresentation, UniTask> SetImageImmediate;
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCamera;
            public Func<StoryContracts.StoryCameraAction, UniTask> SetCameraImmediate;
            public Func<float, UniTask> Wait;

            public Func<string, UniTask> PlayMusic;
            public Func<string, UniTask> PlaySound;
            public Func<string, UniTask> PlayAmbient;
        }

        public struct DialogueCtx
        {
            public string MainCharacter;

            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogue;
            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogueImmediate;

            public Func<string, string> GetLocalizationValue;

            public Func<UniTask> BubbleShow;
            public Action BubbleShowImmediate;
            public Func<UniTask> BubbleHide;
            public Action BubbleHideImmediate;
            public Action<QueueProcess.BubbleQueue.SetBubbleQueue.BubbleCtx> SetBubbleScreen;
            public Action<QueueProcess.BubbleQueue.SetBubbleQueue.WardrobeCtx> SetWardrobeScreen;
            public Action<QueueProcess.BubbleQueue.SetBubbleQueue.ChooseCtx> SetChooseScreen;

            public Action<byte> SaveChoice;
            public Action<int> SetChoice;

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

            if (command.Type != StoryCommands.StoryCommandType.Dialogue)
            {
                _pendingQueue.Enqueue(_storyCommandQueueBuilder.Build(command));
                queue = null;
                return false;
            }

            var bubbleDone = new UniTaskCompletionSource();
            var dialogueQueue = _dialogueQueueBuilder.Build(
                command.Dialogue,
                step.Choices,
                bubbleDone);

            queue = new Queue<QueueProcess.IQueue>();
            EnqueueRange(queue, dialogueQueue.BeforeCommands);
            EnqueueRange(queue, _pendingQueue);
            EnqueueRange(queue, dialogueQueue.AfterCommands);

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
