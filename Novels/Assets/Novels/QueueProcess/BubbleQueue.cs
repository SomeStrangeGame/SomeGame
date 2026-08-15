using System;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class BubbleQueue
    {
        public class SetBubbleQueue : IQueue
        {
            private const string _wardrobeTrigger = "some wardrobe trigger";
            private const string _chooseTrigger = "some choose trigger";
            private const byte _noChoice = byte.MaxValue;

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
                public StoryContracts.StorySpeakerRole SpeakerRole;
                public StoryContracts.DialoguePresentation Presentation;
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

            public StoryContracts.StoryChoice[] Choices;

            public Action<string> SetMainCharacterView;
            public Action<string> SetMainCharacterClothes;
            public Action<string> SetMainCharacterHair;

            public Action<byte> SaveChoice;

            public Action<int> SetChoice;

            public string Name;
            public string Value;
            public StoryContracts.StorySpeakerRole SpeakerRole;
            public StoryContracts.DialoguePresentation Presentation;
            public StoryContracts.StoryChoiceAction ChoiceActions;

            public Action<BubbleCtx> SetBubbleScreen;
            public Action<WardrobeCtx> SetWardrobeScreen;
            public Action<ChooseCtx> SetChooseScreen;

            public async UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    var choice = context.SavedChoice;
                    if (choice != _noChoice)
                    {
                        var selectedChoice = Choices.First(item => item.Id == choice);
                        ApplyChoiceActions(selectedChoice);
                        SetChoice(selectedChoice.Id);
                    }
                    BubbleDone.TrySetResult();
                    return;
                }

                if (Name == _wardrobeTrigger)
                {
                    SetWardrobeScreen(new WardrobeCtx
                    {
                        // set wardrobe here...
                    });
                }
                else if (Name == _chooseTrigger)
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
                        SpeakerRole = SpeakerRole,
                        Presentation = Presentation,
                        Text = new BubbleCtx.TextCtx
                        {
                            Header = GetLocalizationValue(Name),
                            Text = Value
                        },
                        Buttons = Choices.Select(c => new BubbleCtx.ButtonCtx
                        {
                            Id = c.Id,
                            Text = c.Text,
                            OnClick = id =>
                            {
                                ApplyChoiceActions(c);

                                SaveChoice(ToSaveChoiceId(id));
                                SetChoice(id);
                                BubbleDone.TrySetResult();
                            }
                        }).ToArray(),
                        OnBackgroundClick = () =>
                        {
                            SaveChoice(_noChoice);
                            BubbleDone.TrySetResult();
                        }
                    });
                }
            }

            private static byte ToSaveChoiceId(int id)
            {
                if (id < byte.MinValue || id >= _noChoice)
                    throw new ArgumentOutOfRangeException(nameof(id), id, "Choice id must fit the save format range 0-254.");

                return (byte)id;
            }

            private void ApplyChoiceActions(StoryContracts.StoryChoice choice)
            {
                if ((ChoiceActions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                    SetMainCharacterView(choice.Text);

                if ((ChoiceActions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                    SetMainCharacterClothes(choice.Text);

                if ((ChoiceActions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                    SetMainCharacterHair(choice.Text);
            }
        }

        public struct ShowBubbleQueue : IQueue
        {
            public UniTaskCompletionSource BubbleDone;
            public Func<UniTask> BubbleShow;
            public Action BubbleShowImmediate;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                    BubbleShowImmediate();
                else
                    await BubbleShow();

                await BubbleDone.Task;
            }
        }
        public struct HideBubbleQueue : IQueue
        {
            public Func<UniTask> BubbleHide;
            public Action BubbleHideImmediate;

            public async readonly UniTask Run(QueueExecutionContext context)
            {
                if (context.Mode == QueueExecutionMode.Replay)
                    BubbleHideImmediate();
                else
                    await BubbleHide();
            }
        }
    }
}
