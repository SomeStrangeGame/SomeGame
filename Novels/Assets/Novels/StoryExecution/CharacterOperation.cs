using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public class CharacterOperation
    {
        public readonly struct SetDialogueQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.StoryDialogueAlignment, UniTask> _setDialogue;
            private readonly Func<StoryContracts.StoryDialogueAlignment, UniTask> _setDialogueImmediate;
            private readonly Func<UniTask> _characterHide;
            private readonly Action _characterHideImmediate;
            private readonly StoryContracts.StoryDialogueAlignment _alignment;
            private readonly bool _shouldHideCharacter;

            public SetDialogueQueue(
                Func<StoryContracts.StoryDialogueAlignment, UniTask> setDialogue,
                Func<StoryContracts.StoryDialogueAlignment, UniTask> setDialogueImmediate,
                Func<UniTask> characterHide,
                Action characterHideImmediate,
                StoryContracts.StoryDialogueAlignment alignment,
                bool shouldHideCharacter)
            {
                _setDialogue = setDialogue ?? throw new ArgumentNullException(nameof(setDialogue));
                _setDialogueImmediate = setDialogueImmediate
                    ?? throw new ArgumentNullException(nameof(setDialogueImmediate));
                _characterHide = characterHide
                    ?? throw new ArgumentNullException(nameof(characterHide));
                _characterHideImmediate = characterHideImmediate
                    ?? throw new ArgumentNullException(nameof(characterHideImmediate));
                _alignment = alignment;
                _shouldHideCharacter = shouldHideCharacter;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    if (_shouldHideCharacter)
                        _characterHideImmediate();
                    await _setDialogueImmediate(_alignment);
                    return;
                }

                if (!_shouldHideCharacter)
                {
                    await _setDialogue(_alignment);
                    return;
                }

                await UniTask.WhenAll(
                    _characterHide(),
                    _setDialogue(_alignment));
            }
        }

        public readonly struct HideCharacterQueue : IStoryOperation
        {
            private readonly Func<UniTask> _characterHide;
            private readonly Action _characterHideImmediate;
            private readonly bool _shouldHide;

            public HideCharacterQueue(
                Func<UniTask> characterHide,
                Action characterHideImmediate,
                bool shouldHide)
            {
                _characterHide = characterHide
                    ?? throw new ArgumentNullException(nameof(characterHide));
                _characterHideImmediate = characterHideImmediate
                    ?? throw new ArgumentNullException(nameof(characterHideImmediate));
                _shouldHide = shouldHide;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!_shouldHide)
                    return;

                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _characterHideImmediate();
                    return;
                }

                await _characterHide();
            }
        }

        public readonly struct ShowCharacterQueue : IStoryOperation
        {
            private readonly Func<StoryContracts.CharacterRenderRequest, UniTask> _characterSetImage;
            private readonly Func<StoryContracts.StoryCharacterPosition, UniTask> _characterShow;
            private readonly Action<StoryContracts.StoryCharacterPosition> _characterShowImmediate;
            private readonly bool _shouldShow;
            private readonly StoryContracts.CharacterRenderRequest _character;

            public ShowCharacterQueue(
                Func<StoryContracts.CharacterRenderRequest, UniTask> characterSetImage,
                Func<StoryContracts.StoryCharacterPosition, UniTask> characterShow,
                Action<StoryContracts.StoryCharacterPosition> characterShowImmediate,
                bool shouldShow,
                StoryContracts.CharacterRenderRequest character)
            {
                _characterSetImage = characterSetImage
                    ?? throw new ArgumentNullException(nameof(characterSetImage));
                _characterShow = characterShow
                    ?? throw new ArgumentNullException(nameof(characterShow));
                _characterShowImmediate = characterShowImmediate
                    ?? throw new ArgumentNullException(nameof(characterShowImmediate));
                _shouldShow = shouldShow;
                _character = character;
            }

            public async UniTask Run(StoryExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _characterSetImage(_character);

                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _characterShowImmediate(_character.Position);
                    return;
                }

                if (_shouldShow)
                    await _characterShow(_character.Position);
            }
        }
    }
}
