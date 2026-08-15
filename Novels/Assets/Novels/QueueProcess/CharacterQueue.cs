using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class CharacterQueue
    {
        public struct SetDialogueQueue : IQueue
        {
            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogue;
            public Func<StoryContracts.StoryDialogueAlignment, UniTask> SetDialogueImmediate;
            public StoryContracts.StoryDialogueAlignment Alignment;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                    await SetDialogueImmediate(Alignment);
                else
                    await SetDialogue(Alignment);
            }
        }
        public struct HideCharacterQueue : IQueue
        {
            public Func<UniTask> CharacterHide;
            public Action CharacterHideImmediate;
            public bool IsNewCharacter;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    CharacterHideImmediate();
                    return;
                }

                if (IsNewCharacter)
                    await CharacterHide();
            }
        }
        public struct ShowCharacterQueue : IQueue
        {
            public Func<StoryContracts.CharacterRenderRequest, UniTask> CharacterSetImage;
            public Func<StoryContracts.StoryCharacterPosition, UniTask> CharacterShow;
            public Action<StoryContracts.StoryCharacterPosition> CharacterShowImmediate;
            public bool IsNewCharacter;
            public StoryContracts.CharacterRenderRequest Character;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    CharacterShowImmediate(Character.Position);
                    return;
                }

                await CharacterSetImage(Character);
                if (IsNewCharacter)
                    await CharacterShow(Character.Position);
            }
        }
    }
}
