using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    internal sealed class StoryQueueBuilder
    {
        internal struct Dependencies
        {
            internal string MainCharacter;
            internal Notification.NotificationController Notification;
            internal Location.LocationController Location;
            internal Audio.AudioController Audio;
            internal Bubble.BubbleController Bubble;
            internal Wardrobe.WardrobeController Wardrobe;
            internal Choose.ChooseController Choose;
            internal Character.CharacterController Character;
            internal Save.SaveSystem Save;
            internal StoryProcessor.Entity Story;
            internal Func<float, UniTask> Wait;
            internal Func<string, UniTask<UnityEngine.Sprite>> LoadChooseThumbnail;
        }

        private readonly StoryCommandQueueBuilder _storyCommandQueueBuilder;
        private readonly DialogueQueueBuilder _dialogueQueueBuilder;

        private Queue<StoryExecution.IStoryOperation> _pendingQueue = new();

        internal StoryQueueBuilder(Dependencies dependencies)
        {
            _storyCommandQueueBuilder = new StoryCommandQueueBuilder(dependencies);
            _dialogueQueueBuilder = new DialogueQueueBuilder(dependencies);
        }

        internal bool TryBuild(
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

        internal bool TryComplete(out Queue<StoryExecution.IStoryOperation> queue)
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
