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

            private readonly UniTaskCompletionSource _bubbleDone;
            private readonly Func<string, string> _getLocalizationValue;
            private readonly StoryContracts.StoryChoice[] _choices;
            private readonly Action<string> _setMainCharacterView;
            private readonly Action<string> _setMainCharacterClothes;
            private readonly Action<string> _setMainCharacterHair;
            private readonly Action<byte> _saveChoice;
            private readonly Action<int> _setChoice;
            private readonly string _name;
            private readonly string _value;
            private readonly StoryContracts.StorySpeakerRole _speakerRole;
            private readonly StoryContracts.DialoguePresentation _presentation;
            private readonly StoryContracts.StoryChoiceAction _choiceActions;
            private readonly Action<BubbleContracts.BubblePresentation> _setBubbleScreen;
            private readonly Action<BubbleContracts.WardrobePresentation> _setWardrobeScreen;
            private readonly Action<BubbleContracts.ChoosePresentation> _setChooseScreen;

            public SetBubbleQueue(
                UniTaskCompletionSource bubbleDone,
                Func<string, string> getLocalizationValue,
                StoryContracts.StoryChoice[] choices,
                Action<string> setMainCharacterView,
                Action<string> setMainCharacterClothes,
                Action<string> setMainCharacterHair,
                Action<byte> saveChoice,
                Action<int> setChoice,
                string name,
                string value,
                StoryContracts.StorySpeakerRole speakerRole,
                StoryContracts.DialoguePresentation presentation,
                StoryContracts.StoryChoiceAction choiceActions,
                Action<BubbleContracts.BubblePresentation> setBubbleScreen,
                Action<BubbleContracts.WardrobePresentation> setWardrobeScreen,
                Action<BubbleContracts.ChoosePresentation> setChooseScreen)
            {
                _bubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
                _getLocalizationValue = getLocalizationValue
                    ?? throw new ArgumentNullException(nameof(getLocalizationValue));
                _choices = choices ?? Array.Empty<StoryContracts.StoryChoice>();
                _setMainCharacterView = setMainCharacterView
                    ?? throw new ArgumentNullException(nameof(setMainCharacterView));
                _setMainCharacterClothes = setMainCharacterClothes
                    ?? throw new ArgumentNullException(nameof(setMainCharacterClothes));
                _setMainCharacterHair = setMainCharacterHair
                    ?? throw new ArgumentNullException(nameof(setMainCharacterHair));
                _saveChoice = saveChoice ?? throw new ArgumentNullException(nameof(saveChoice));
                _setChoice = setChoice ?? throw new ArgumentNullException(nameof(setChoice));
                _name = name ?? string.Empty;
                _value = value ?? string.Empty;
                _speakerRole = speakerRole;
                _presentation = presentation;
                _choiceActions = choiceActions;
                _setBubbleScreen = setBubbleScreen
                    ?? throw new ArgumentNullException(nameof(setBubbleScreen));
                _setWardrobeScreen = setWardrobeScreen
                    ?? throw new ArgumentNullException(nameof(setWardrobeScreen));
                _setChooseScreen = setChooseScreen
                    ?? throw new ArgumentNullException(nameof(setChooseScreen));
            }

            public UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                {
                    var choice = context.SavedChoice;
                    if (choice != _noChoice)
                    {
                        var selectedChoice = _choices.First(item => item.Id == choice);
                        ApplyChoiceActions(selectedChoice);
                        _setChoice(selectedChoice.Id);
                    }
                    _bubbleDone.TrySetResult();
                    return UniTask.CompletedTask;
                }

                if (_name == BubbleContracts.BubbleTriggers.Wardrobe)
                {
                    _setWardrobeScreen(new BubbleContracts.WardrobePresentation());
                }
                else if (_name == BubbleContracts.BubbleTriggers.Choose)
                {
                    _setChooseScreen(new BubbleContracts.ChoosePresentation());
                }
                else
                {
                    _setBubbleScreen(new BubbleContracts.BubblePresentation(
                        _name,
                        _speakerRole,
                        _presentation,
                        new BubbleContracts.BubbleText(
                            GetHeader(),
                            _value),
                        _choices.Select(c => new BubbleContracts.BubbleChoice(
                            c.Id,
                            c.Text,
                            id =>
                            {
                                ApplyChoiceActions(c);

                                _saveChoice(ToSaveChoiceId(id));
                                _setChoice(id);
                                _bubbleDone.TrySetResult();
                            })).ToArray(),
                        () =>
                        {
                            _saveChoice(_noChoice);
                            _bubbleDone.TrySetResult();
                        }));
                }

                return UniTask.CompletedTask;
            }

            private string GetHeader()
            {
                if (_presentation == StoryContracts.DialoguePresentation.Disclaimer)
                    return _getLocalizationValue(BubbleContracts.BubbleTextKeys.Disclaimer);

                if (_presentation == StoryContracts.DialoguePresentation.Hint)
                    return _getLocalizationValue(BubbleContracts.BubbleTextKeys.Hint);

                return _getLocalizationValue(_name);
            }

            private static byte ToSaveChoiceId(int id)
            {
                if (id < byte.MinValue || id >= _noChoice)
                    throw new ArgumentOutOfRangeException(nameof(id), id, "Choice id must fit the save format range 0-254.");

                return (byte)id;
            }

            private void ApplyChoiceActions(StoryContracts.StoryChoice choice)
            {
                if ((_choiceActions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                    _setMainCharacterView(choice.Text);

                if ((_choiceActions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                    _setMainCharacterClothes(choice.Text);

                if ((_choiceActions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                    _setMainCharacterHair(choice.Text);
            }
        }

        public readonly struct ShowBubbleQueue : IQueue
        {
            private readonly UniTaskCompletionSource _bubbleDone;
            private readonly Func<UniTask> _bubbleShow;
            private readonly Action _bubbleShowImmediate;

            public ShowBubbleQueue(
                UniTaskCompletionSource bubbleDone,
                Func<UniTask> bubbleShow,
                Action bubbleShowImmediate)
            {
                _bubbleDone = bubbleDone ?? throw new ArgumentNullException(nameof(bubbleDone));
                _bubbleShow = bubbleShow ?? throw new ArgumentNullException(nameof(bubbleShow));
                _bubbleShowImmediate = bubbleShowImmediate
                    ?? throw new ArgumentNullException(nameof(bubbleShowImmediate));
            }

            public async UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    _bubbleShowImmediate();
                else
                    await _bubbleShow();

                await _bubbleDone.Task.AttachExternalCancellation(context.CancellationToken);
            }
        }
        public readonly struct HideBubbleQueue : IQueue
        {
            private readonly Func<UniTask> _bubbleHide;
            private readonly Action _bubbleHideImmediate;

            public HideBubbleQueue(
                Func<UniTask> bubbleHide,
                Action bubbleHideImmediate)
            {
                _bubbleHide = bubbleHide ?? throw new ArgumentNullException(nameof(bubbleHide));
                _bubbleHideImmediate = bubbleHideImmediate
                    ?? throw new ArgumentNullException(nameof(bubbleHideImmediate));
            }

            public async UniTask Run(QueueExecutionContext context)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (context.Mode == QueueExecutionMode.Replay)
                    _bubbleHideImmediate();
                else
                    await _bubbleHide();
            }
        }
    }
}
