using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.QueueProcess
{
    public class CharacterQueue
    {
        public struct DialogQueue : IQueue
        {
            public Func<bool, TextAlignment, UniTask> SetDialogue;
            public Func<bool> IsLoadingInProcess;
            public TextAlignment DialogAlign;

            public async readonly UniTask Run()
            {
                await SetDialogue(IsLoadingInProcess(), DialogAlign);
            }
        }
        public struct HideCharacterQueue : IQueue
        {
            public Func<bool> IsLoadingInProcess;
            public Func<UniTask> CharacterHide;
            public Action CharacterHideImmediate;
            public bool IsNewCharacter;
            public Action OnHidecharacter;

            public async readonly UniTask Run()
            {
                if (IsNewCharacter)
                {
                    OnHidecharacter();
                    if (!IsLoadingInProcess())
                        await CharacterHide();
                    else
                        CharacterHideImmediate();
                }
            }
        }
        public struct ShowCharacterQueue : IQueue
        {
            public Func<string, string[], UniTask> CharacterSetImage;
            public Func<bool> IsLoadingInProcess;
            public Func<bool, UniTask> CharacterShow;
            public Action<bool> CharacterShowImmediate;
            public bool IsNewCharacter;
            public string Name;
            public string[] Args;
            public string MainCharacter;

            public async readonly UniTask Run()
            {
                await CharacterSetImage(Name, Args);
                if (IsNewCharacter)
                {
                    if (!IsLoadingInProcess())
                        await CharacterShow(Name == MainCharacter);
                    else
                        CharacterShowImmediate(Name == MainCharacter);
                }
            }
        }
    }
}
