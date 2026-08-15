using System;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public class BubbleQueue
    {
        public class SetBubbleQueue : IQueue
        {
            private const byte _noChoice = byte.MaxValue;

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

            public Action<BubbleContracts.BubblePresentation> SetBubbleScreen;
            public Action<BubbleContracts.WardrobePresentation> SetWardrobeScreen;
            public Action<BubbleContracts.ChoosePresentation> SetChooseScreen;

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

                if (Name == "some wardrobe trigger...")
                {
                    SetWardrobeScreen(new BubbleContracts.WardrobePresentation());
                }
                else if (Name == "some choose trigger...")
                {
                    SetChooseScreen(new BubbleContracts.ChoosePresentation());
                }
                else
                {
                    SetBubbleScreen(new BubbleContracts.BubblePresentation(
                        Name,
                        SpeakerRole,
                        Presentation,
                        new BubbleContracts.BubbleText(
                            GetLocalizationValue(Name),
                            Value),
                        Choices.Select(c => new BubbleContracts.BubbleChoice(
                            c.Id,
                            c.Text,
                            id =>
                            {
                                ApplyChoiceActions(c);

                                SaveChoice(ToSaveChoiceId(id));
                                SetChoice(id);
                                BubbleDone.TrySetResult();
                            })).ToArray(),
                        () =>
                        {
                            SaveChoice(_noChoice);
                            BubbleDone.TrySetResult();
                        }));
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
