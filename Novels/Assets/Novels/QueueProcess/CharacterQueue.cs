using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.QueueProcess
{
    public class CharacterQueue
    {
        public struct DialogQueue : IQueue
        {
            public Func<TextAlignment, UniTask> SetDialogue;
            public Func<TextAlignment, UniTask> SetDialogueImmediate;
            public TextAlignment DialogAlign;

            public async readonly UniTask Run()
            {
                await SetDialogue(DialogAlign);
            }

            public async readonly UniTask RunImmediate(byte choice)
            {
                await SetDialogueImmediate(DialogAlign);
            }
        }
        public struct HideCharacterQueue : IQueue
        {
            public Func<UniTask> CharacterHide;
            public Action CharacterHideImmediate;
            public bool IsNewCharacter;

            public async readonly UniTask Run()
            {
                if (IsNewCharacter)
                {
                    await CharacterHide();
                }
            }

            public async readonly UniTask RunImmediate(byte choice)
            {
                CharacterHideImmediate();
            }
        }
        public struct ShowCharacterQueue : IQueue
        {
            private const string _wardrobe = "Wardrobe";

            public Func<string, string[], UniTask> CharacterSetImage;
            public Func<bool?, UniTask> CharacterShow;
            public Action<bool?> CharacterShowImmediate;
            public bool IsNewCharacter;
            public string Name;
            public string[] Args;
            public string MainCharacter;

            public async readonly UniTask Run()
            {
                await CharacterSetImage(Name, Args);
                if (IsNewCharacter)
                {
                    await CharacterShow(Name != _wardrobe ? Name == MainCharacter : null);
                }
            }

            public async readonly UniTask RunImmediate(byte choice)
            {
                CharacterShowImmediate(Name != _wardrobe ? Name == MainCharacter : null);
            }
        }
    }
}
