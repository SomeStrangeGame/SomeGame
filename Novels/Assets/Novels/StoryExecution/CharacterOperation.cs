using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public class CharacterOperation
    {
        public readonly struct SetDialogueQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.StoryDialogueAlignment,
                StoryContracts.PresentationMode, UniTask> _setDialogue;
            private readonly Func<StoryContracts.PresentationMode, UniTask> _characterHide;
            private readonly StoryContracts.StoryDialogueAlignment _alignment;
            private readonly bool _shouldHideCharacter;

            public SetDialogueQueue(
                Func<StoryContracts.StoryDialogueAlignment,
                    StoryContracts.PresentationMode, UniTask> setDialogue,
                Func<StoryContracts.PresentationMode, UniTask> characterHide,
                StoryContracts.StoryDialogueAlignment alignment,
                bool shouldHideCharacter)
            {
                _setDialogue = setDialogue ?? throw new ArgumentNullException(nameof(setDialogue));
                _characterHide = characterHide
                    ?? throw new ArgumentNullException(nameof(characterHide));
                _alignment = alignment;
                _shouldHideCharacter = shouldHideCharacter;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!_shouldHideCharacter)
                {
                    await _setDialogue(_alignment, context.PresentationMode);
                    return;
                }
                await UniTask.WhenAll(
                    _characterHide(context.PresentationMode),
                    _setDialogue(_alignment, context.PresentationMode));
            }
        }

        public readonly struct HideCharacterQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.PresentationMode, UniTask> _characterHide;
            private readonly bool _shouldHide;

            public HideCharacterQueue(
                Func<StoryContracts.PresentationMode, UniTask> characterHide,
                bool shouldHide)
            {
                _characterHide = characterHide
                    ?? throw new ArgumentNullException(nameof(characterHide));
                _shouldHide = shouldHide;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!_shouldHide)
                    return;

                await _characterHide(context.PresentationMode);
            }
        }

        public readonly struct ShowCharacterQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.CharacterRenderRequest, UniTask> _characterSetImage;
            private readonly Func<StoryContracts.StoryCharacterPosition,
                StoryContracts.PresentationMode, UniTask> _characterShow;
            private readonly bool _shouldShow;
            private readonly StoryContracts.CharacterRenderRequest _character;

            public ShowCharacterQueue(
                Func<StoryContracts.CharacterRenderRequest, UniTask> characterSetImage,
                Func<StoryContracts.StoryCharacterPosition,
                    StoryContracts.PresentationMode, UniTask> characterShow,
                bool shouldShow,
                StoryContracts.CharacterRenderRequest character)
            {
                _characterSetImage = characterSetImage
                    ?? throw new ArgumentNullException(nameof(characterSetImage));
                _characterShow = characterShow
                    ?? throw new ArgumentNullException(nameof(characterShow));
                _shouldShow = shouldShow;
                _character = character;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _characterSetImage(_character);
                if (_shouldShow || context.Mode == QueueExecutionMode.Replay)
                    await _characterShow(_character.Position, context.PresentationMode);
            }
        }
    }
}
