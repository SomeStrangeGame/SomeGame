using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.StoryQueue
{
    public sealed class StoryQueueBuilder
    {
        public struct CommandCtx
        {
            public Action<string> ShowNotification;
            public LocationCommandPort Location;
            public AudioPort Audio;
        }

        public struct LocationCommandPort
        {
            public Func<string, StoryContracts.StoryBackgroundPresentation,
                StoryContracts.PresentationMode, UniTask> SetImage;
            public Func<StoryContracts.StoryCameraAction,
                StoryContracts.PresentationMode, UniTask> SetCamera;
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
            public BubblePort Bubble;
            public WardrobePort Wardrobe;
            public ChoosePort Choose;
            public ChoicePort Choice;
            public CharacterPort Character;
        }

        public struct LocationDialoguePort
        {
            public Func<StoryContracts.StoryDialogueAlignment,
                StoryContracts.PresentationMode, UniTask> SetDialogue;
        }

        public struct BubblePort
        {
            public Func<StoryContracts.PresentationMode, UniTask> Show;
            public Func<StoryContracts.PresentationMode, UniTask> Hide;
            public Action<BubbleContracts.BubblePresentation> SetBubbleScreen;
        }

        public struct WardrobePort
        {
            public Func<StoryContracts.PresentationMode, UniTask> Show;
            public Func<StoryContracts.PresentationMode, UniTask> Hide;
            public Action<WardrobeContracts.WardrobePresentation> SetScreen;
        }

        public struct ChoosePort
        {
            public Func<StoryContracts.PresentationMode, UniTask> Show;
            public Func<StoryContracts.PresentationMode, UniTask> Hide;
            public Func<string, UniTask<UnityEngine.Sprite>> LoadThumbnail;
            public Action<ChooseContracts.ChoosePresentation> SetScreen;
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
            public Action<string> SetMainCharacterAccessory;
            public Func<StoryContracts.StoryChoiceAction, string, UniTask<UnityEngine.Sprite>> LoadWardrobeThumbnail;
            public Func<StoryContracts.StoryChoiceAction, string, UniTask> PreviewWardrobeChoice;
            public Func<StoryContracts.PresentationMode, UniTask> CharacterHide;
            public Func<StoryContracts.StoryCharacterPosition,
                StoryContracts.PresentationMode, UniTask> CharacterShow;
            public Func<StoryContracts.CharacterRenderRequest, UniTask> CharacterSetImage;
        }

        public struct Dependencies
        {
            public CommandCtx Command;
            public DialogueCtx Dialogue;
        }

        private readonly StoryCommandQueueBuilder _storyCommandQueueBuilder;
        private readonly DialogueQueueBuilder _dialogueQueueBuilder;

        private Queue<StoryExecution.IStoryOperation> _pendingQueue = new();

        public StoryQueueBuilder(Dependencies ctx)
        {
            _storyCommandQueueBuilder = new StoryCommandQueueBuilder(ctx.Command);
            _dialogueQueueBuilder = new DialogueQueueBuilder(ctx.Dialogue);
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
