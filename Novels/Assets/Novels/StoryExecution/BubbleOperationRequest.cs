using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
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
