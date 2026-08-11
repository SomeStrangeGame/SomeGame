using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct LoadChoiceQueue : IQueue
    {
        public UniTaskCompletionSource BubbleDone;
        public Func<bool> IsLoadingInProcess;
        public Func<byte> LoadChoice;
        public Func<List<Ink.Runtime.Choice>> GetChoices;
        public Action<int> SetChoice;
        public Action<string[], Ink.Runtime.Choice> SetCharacterView;
        public string[] Args;

        public async readonly UniTask Run()
        {
            if (IsLoadingInProcess())
            {
                var savedChoice = LoadChoice();
                if (savedChoice != 255)
                {
                    SetCharacterView(Args, GetChoices()[savedChoice]);
                    SetChoice(savedChoice);
                }
                BubbleDone.TrySetResult();
            }
        }
    }
}
