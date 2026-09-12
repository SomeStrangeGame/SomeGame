using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    public sealed class StoryQueueBuilder
    {
        public struct Dependencies
        {
            public string MainCharacter;
            public Notification.NotificationController Notification;
            public Location.LocationController Location;
            public Func<string, StoryCommands.StoryCommandType, UniTask> PlayAudio;
            public Func<UniTask> FlushCheckpoint;
            public Bubble.BubbleController Bubble;
            public Wardrobe.WardrobeController Wardrobe;
            public Choose.ChooseController Choose;
            public Character.CharacterController Character;
            public Save.SaveSystem Save;
            public StoryProcessor.Entity Story;
            public Func<float, UniTask> Wait;
            public Func<string, UniTask<UnityEngine.Sprite>> LoadChooseThumbnail;
            public Func<string, UniTask<UnityEngine.Sprite>> LoadBubbleChoiceIcon;
            public Func<StoryCommands.StoryStep[]> PeekWardrobeSteps;
            public StoryExecution.WardrobeSequenceState WardrobeSequence;
            public Action<string, int> OnDialogueReady;
            public Action<int> OnChoiceSelected;
            public Action<string> OnEndingReached;
        }

        private readonly StoryCommandQueueBuilder _storyCommandQueueBuilder;
        private readonly DialogueQueueBuilder _dialogueQueueBuilder;

        private Queue<StoryExecution.IStoryOperation> _pendingQueue = new();

        public StoryQueueBuilder(Dependencies dependencies)
        {
            dependencies.WardrobeSequence ??= new StoryExecution.WardrobeSequenceState();
            _storyCommandQueueBuilder = new StoryCommandQueueBuilder(dependencies);
            _dialogueQueueBuilder = new DialogueQueueBuilder(dependencies);
        }

        public bool TryBuild(
            StoryCommands.StoryStep step,
            out Queue<StoryExecution.IStoryOperation> queue)
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
                bubbleDone,
                _pendingQueue.Count > 0);

            queue = new Queue<StoryExecution.IStoryOperation>();
            EnqueueRange(queue, dialogueQueue.BeforeCommands);
            EnqueueRange(queue, _pendingQueue);
            EnqueueRange(queue, dialogueQueue.AfterCommands);

            _pendingQueue = new Queue<StoryExecution.IStoryOperation>();
            return true;
        }

        public bool TryComplete(out Queue<StoryExecution.IStoryOperation> queue)
        {
            if (_pendingQueue.Count == 0)
            {
                queue = null;
                return false;
            }

            queue = _pendingQueue;
            _pendingQueue = new Queue<StoryExecution.IStoryOperation>();
            return true;
        }

        private static void EnqueueRange(
            Queue<StoryExecution.IStoryOperation> target,
            IEnumerable<StoryExecution.IStoryOperation> source)
        {
            foreach (var item in source)
                target.Enqueue(item);
        }
    }
}
