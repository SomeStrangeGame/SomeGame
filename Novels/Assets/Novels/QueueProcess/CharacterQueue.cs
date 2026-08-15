using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class CharacterQueue
    {
        public readonly struct SetDialogueQueue : IQueue
        {
            private readonly Func<StoryContracts.StoryDialogueAlignment, UniTask> _setDialogue;
            private readonly Func<StoryContracts.StoryDialogueAlignment, UniTask> _setDialogueImmediate;
            private readonly StoryContracts.StoryDialogueAlignment _alignment;

            public SetDialogueQueue(
                Func<StoryContracts.StoryDialogueAlignment, UniTask> setDialogue,
                Func<StoryContracts.StoryDialogueAlignment, UniTask> setDialogueImmediate,
                StoryContracts.StoryDialogueAlignment alignment)
            {
                _setDialogue = setDialogue ?? throw new ArgumentNullException(nameof(setDialogue));
                _setDialogueImmediate = setDialogueImmediate
                    ?? throw new ArgumentNullException(nameof(setDialogueImmediate));
                _alignment = alignment;
            }

            public async UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    await _setDialogueImmediate(_alignment);
                else
                    await _setDialogue(_alignment);
            }
        }
        public readonly struct HideCharacterQueue : IQueue
        {
            private readonly Func<UniTask> _characterHide;
            private readonly Action _characterHideImmediate;
            private readonly bool _isNewCharacter;

            public HideCharacterQueue(
                Func<UniTask> characterHide,
                Action characterHideImmediate,
                bool isNewCharacter)
            {
                _characterHide = characterHide
                    ?? throw new ArgumentNullException(nameof(characterHide));
                _characterHideImmediate = characterHideImmediate
                    ?? throw new ArgumentNullException(nameof(characterHideImmediate));
                _isNewCharacter = isNewCharacter;
            }

            public async UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _characterHideImmediate();
                    return;
                }

                if (_isNewCharacter)
                    await _characterHide();
            }
        }
        public readonly struct ShowCharacterQueue : IQueue
        {
            private readonly Func<StoryContracts.CharacterRenderRequest, UniTask> _characterSetImage;
            private readonly Func<StoryContracts.StoryCharacterPosition, UniTask> _characterShow;
            private readonly Action<StoryContracts.StoryCharacterPosition> _characterShowImmediate;
            private readonly bool _isNewCharacter;
            private readonly StoryContracts.CharacterRenderRequest _character;

            public ShowCharacterQueue(
                Func<StoryContracts.CharacterRenderRequest, UniTask> characterSetImage,
                Func<StoryContracts.StoryCharacterPosition, UniTask> characterShow,
                Action<StoryContracts.StoryCharacterPosition> characterShowImmediate,
                bool isNewCharacter,
                StoryContracts.CharacterRenderRequest character)
            {
                _characterSetImage = characterSetImage
                    ?? throw new ArgumentNullException(nameof(characterSetImage));
                _characterShow = characterShow
                    ?? throw new ArgumentNullException(nameof(characterShow));
                _characterShowImmediate = characterShowImmediate
                    ?? throw new ArgumentNullException(nameof(characterShowImmediate));
                _isNewCharacter = isNewCharacter;
                _character = character;
            }

            public async UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await _characterSetImage(_character);

                if (context.Mode == QueueExecutionMode.Replay)
                {
                    _characterShowImmediate(_character.Position);
                    return;
                }

                if (_isNewCharacter)
                    await _characterShow(_character.Position);
            }
        }
    }
}
