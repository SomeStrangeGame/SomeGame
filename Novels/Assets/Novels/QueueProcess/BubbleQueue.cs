using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class BubbleQueue
    {
        public class SetBubbleQueue : IQueue
        {
            public struct BubbleCtx
            {
                public struct TextCtx
                {
                    public string Header;
                    public string Text;
                }

                public struct ButtonCtx
                {
                    public int Id;
                    public string Text;
                    public Action<int> OnClick;
                }

                public string Name;
                public string[] Args;
                public TextCtx Text;
                public ButtonCtx[] Buttons;
                public Action OnBackgroundClick;
            }

            public struct WardrobeCtx
            {

            }

            public struct ChooseCtx
            {

            }

            public UniTaskCompletionSource BubbleDone;
            public Func<string, string> GetLocalizationValue;
            public List<Ink.Runtime.Choice> Choices;
            public Action<string[], Ink.Runtime.Choice> SetCharacterView;
            public Action<byte> SaveChoice;
            public Action<int> SetChoice;
            public string Name;
            public string Value;
            public string[] Args;

            public Action<BubbleCtx> SetBubbleScreen;
            public Action<WardrobeCtx> SetWardrobeScreen;
            public Action<ChooseCtx> SetChooseScreen;

            public async UniTask Run()
            {
                if (Name == "some wardrobe trigger")
                {
                    SetWardrobeScreen(new WardrobeCtx
                    {
                        // set wardrobe here...
                    });
                }
                else if (Name == "some choose trigger")
                {
                    SetChooseScreen(new ChooseCtx
                    {
                        //set choose here...
                    });
                }
                else
                {
                    SetBubbleScreen(new BubbleCtx
                    {
                        Name = Name,
                        Args = Args,
                        Text = new BubbleCtx.TextCtx
                        {
                            Header = GetLocalizationValue(Name),
                            Text = Value
                        },
                        Buttons = Choices.Select(c => new BubbleCtx.ButtonCtx
                        {
                            Id = c.index,
                            Text = c.text,
                            OnClick = id =>
                            {
                                SetCharacterView(Args, c);
                                SaveChoice((byte)id);
                                SetChoice(id);
                                BubbleDone.TrySetResult();
                            }
                        }).ToArray(),
                        OnBackgroundClick = () =>
                        {
                            SaveChoice(255);
                            BubbleDone.TrySetResult();
                        }
                    });
                }
            }

            public async UniTask RunImmediate(byte choice)
            {
                if (choice != 255)
                {
                    SetCharacterView(Args, Choices[choice]);
                    SetChoice(choice);
                }
                BubbleDone.TrySetResult();
            }
        }

        public struct ShowBubbleQueue : IQueue
        {
            public UniTaskCompletionSource BubbleDone;
            public Func<UniTask> BubbleShow;
            public Action BubbleShowImmediate;

            public async readonly UniTask Run()
            {
                await BubbleShow();

                await BubbleDone.Task;
            }

            public async readonly UniTask RunImmediate(byte choice)
            {
                BubbleShowImmediate();
                await BubbleDone.Task;
            }
        }
        public struct HideBubbleQueue : IQueue
        {
            public Func<UniTask> BubbleHide;
            public Action BubbleHideImmediate;

            public async readonly UniTask Run()
            {
                await BubbleHide();
            }

            public async readonly UniTask RunImmediate(byte choice)
            {
                BubbleHideImmediate();
            }
        }
    }
}
