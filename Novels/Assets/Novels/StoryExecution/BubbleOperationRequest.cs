using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    internal sealed class WardrobeSequenceState
    {
        private readonly Queue<int> _pendingChoices = new();

        internal void SetPending(System.Collections.Generic.IEnumerable<int> choices)
        {
            _pendingChoices.Clear();
            foreach (var choice in choices)
                _pendingChoices.Enqueue(choice);
        }

        internal bool TryTake(out int choice)
        {
            if (_pendingChoices.Count == 0)
            {
                choice = default;
                return false;
            }
            choice = _pendingChoices.Dequeue();
            return true;
        }
    }

    internal sealed class BubbleOperationRequest
    {
        internal BubbleOperationRequest(
            StoryQueue.StoryQueueBuilder.Dependencies services,
            StoryCommands.DialogueCommandData dialogue,
            StoryContracts.StoryChoice[] choices,
            UniTaskCompletionSource completed,
            StoryContracts.StorySpeakerRole speakerRole,
            BubbleContracts.BubblePresentationKind presentationKind)
        {
            Services = services;
            Dialogue = dialogue;
            Choices = choices ?? Array.Empty<StoryContracts.StoryChoice>();
            Completed = completed ?? throw new ArgumentNullException(nameof(completed));
            SpeakerRole = speakerRole;
            PresentationKind = presentationKind;
        }

        internal StoryQueue.StoryQueueBuilder.Dependencies Services { get; }
        internal StoryCommands.DialogueCommandData Dialogue { get; }
        internal StoryContracts.StoryChoice[] Choices { get; }
        internal UniTaskCompletionSource Completed { get; }
        internal StoryContracts.StorySpeakerRole SpeakerRole { get; }
        internal BubbleContracts.BubblePresentationKind PresentationKind { get; }
    }
}
