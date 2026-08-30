using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private static StoryProcessor.Entity CreateStoryProcessor(
            IBaseDisposable owner,
            string storyText,
            string initialState,
            string sourceMapText)
        {
            return new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
                InitialState = initialState,
                SourceMapText = sourceMapText,
            }).AddTo(owner);
        }

        private StoryQueue.StoryQueueBuilder CreateStoryQueue(
            StoryProcessor.Entity storyProcessor,
            EpisodePresentation presentation,
            CancellationToken cancellationToken,
            System.Func<string, Cysharp.Threading.Tasks.UniTask<UnityEngine.Sprite>> loadChooseThumbnail,
            Save.SaveSystem save)
        {
            RestoreWardrobe(presentation.Character, save, _definition.MainCharacter);
            presentation.Wardrobe.ConfigureFree(() =>
                CreateFreeWardrobePresentation(
                    presentation.Character,
                    save,
                    _definition.MainCharacter),
                async () =>
                {
                    await presentation.Bubble.Hide(
                        StoryContracts.PresentationMode.Immediate);
                    await presentation.Character.BeginWardrobePreview(string.Empty);
                },
                async () =>
                {
                    await presentation.Character.EndWardrobePreview();
                    await presentation.Bubble.Show(
                        StoryContracts.PresentationMode.Immediate);
                });
            return new StoryQueue.StoryQueueBuilder(
                new StoryQueue.StoryQueueBuilder.Dependencies
                {
                    MainCharacter = _definition.MainCharacter,
                    Notification = presentation.Notification,
                    Location = presentation.Location,
                    Audio = presentation.Audio,
                    Bubble = presentation.Bubble,
                    Wardrobe = presentation.Wardrobe,
                    Choose = presentation.Choose,
                    Character = presentation.Character,
                    Save = save,
                    Story = storyProcessor,
                    Wait = seconds => Wait(seconds, cancellationToken),
                    LoadChooseThumbnail = loadChooseThumbnail,
                    PeekWardrobeSteps = () => storyProcessor.PeekConsecutiveWardrobeSteps(
                        new StoryCommands.Entity().ParseStep,
                        2),
                    WardrobeSequence = new StoryExecution.WardrobeSequenceState(),
                    OnDialogueReady = (presentationKind, choiceCount) =>
                        _ctx.SmokeTelemetry?.Emit(
                            "dialogue.ready",
                            ("contentId", _definition.Id),
                            ("episodeId", _episode.Id),
                            ("presentation", presentationKind),
                            ("choiceCount", choiceCount.ToString())),
                    OnChoiceSelected = choiceId => _ctx.SmokeTelemetry?.Emit(
                        "choice.selected",
                        ("contentId", _definition.Id),
                        ("episodeId", _episode.Id),
                        ("choiceId", choiceId.ToString())),
                });
        }

        private static WardrobeContracts.WardrobePresentation CreateFreeWardrobePresentation(
            Character.CharacterController character,
            Save.SaveSystem save,
            string characterName)
        {
            character.SetWardrobeTarget(string.Empty);
            var categories = System.Enum
                .GetValues(typeof(WardrobeContracts.WardrobeCategory))
                .Cast<WardrobeContracts.WardrobeCategory>()
                .Where(category => save.GetUnlockedWardrobeItems(
                    characterName,
                    (byte)category).Length > 0)
                .ToArray();
            var initialCategory = categories.Contains(
                WardrobeContracts.WardrobeCategory.Clothes)
                    ? WardrobeContracts.WardrobeCategory.Clothes
                    : categories.FirstOrDefault();
            return new WardrobeContracts.WardrobePresentation(
                characterName,
                initialCategory,
                WardrobeCategoryTitle(initialCategory),
                "Применить",
                System.Array.Empty<WardrobeContracts.WardrobeOption>(),
                category => LoadUnlockedWardrobeItems(
                    character,
                    save,
                    characterName,
                    category),
                (category, value) => character.LoadWardrobeThumbnail(
                    ToChoiceAction(category),
                    value),
                (category, value) => character.PreviewWardrobeChoice(
                        ToChoiceAction(category),
                        value)
                    .ContinueWith(() => save.UnlockWardrobeItem(
                        characterName,
                        (byte)category,
                        value,
                        true)),
                _ => UniTask.FromResult<Sprite>(null),
                _ => UniTask.CompletedTask,
                _ => { },
                true,
                true,
                availableCategories: categories,
                getSelectedCategoryValue: category =>
                {
                    var equipped = save.GetEquippedWardrobeItem(
                        characterName,
                        (byte)category);
                    return string.IsNullOrWhiteSpace(equipped)
                        ? character.GetCurrentWardrobeValue(ToChoiceAction(category))
                        : equipped;
                });
        }

        private static string WardrobeCategoryTitle(
            WardrobeContracts.WardrobeCategory category) => category switch
            {
                WardrobeContracts.WardrobeCategory.Appearance =>
                    WardrobeContracts.WardrobeLabels.Appearance,
                WardrobeContracts.WardrobeCategory.Hair =>
                    WardrobeContracts.WardrobeLabels.Hair,
                WardrobeContracts.WardrobeCategory.Accessory =>
                    WardrobeContracts.WardrobeLabels.Accessory,
                _ => WardrobeContracts.WardrobeLabels.Clothes,
            };

        private static async UniTask<string[]> LoadUnlockedWardrobeItems(
            Character.CharacterController character,
            Save.SaveSystem save,
            string characterName,
            WardrobeContracts.WardrobeCategory category)
        {
            var available = await character.LoadWardrobeCategory(ToChoiceAction(category));
            save.RemoveUnavailableWardrobeItems(
                characterName,
                (byte)category,
                available,
                true);
            var availableSet = new System.Collections.Generic.HashSet<string>(
                available,
                System.StringComparer.OrdinalIgnoreCase);
            return save.GetUnlockedWardrobeItems(characterName, (byte)category)
                .Where(availableSet.Contains)
                .ToArray();
        }

        private static void RestoreWardrobe(
            Character.CharacterController character,
            Save.SaveSystem save,
            string characterName)
        {
            foreach (WardrobeContracts.WardrobeCategory category in
                     System.Enum.GetValues(typeof(WardrobeContracts.WardrobeCategory)))
            {
                var value = save.GetEquippedWardrobeItem(characterName, (byte)category);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    character.ApplyWardrobeSelection(
                        string.Empty,
                        ToChoiceAction(category),
                        value);
                }
            }
        }

        private static StoryContracts.StoryChoiceAction ToChoiceAction(
            WardrobeContracts.WardrobeCategory category) => category switch
            {
                WardrobeContracts.WardrobeCategory.Appearance =>
                    StoryContracts.StoryChoiceAction.SelectAppearance,
                WardrobeContracts.WardrobeCategory.Hair =>
                    StoryContracts.StoryChoiceAction.SelectHair,
                WardrobeContracts.WardrobeCategory.Accessory =>
                    StoryContracts.StoryChoiceAction.SelectAccessory,
                _ => StoryContracts.StoryChoiceAction.SelectClothes,
            };

        private static async UniTask Wait(
            float seconds,
            CancellationToken cancellationToken)
        {
            while (seconds > 0f)
            {
                await UniTask.Yield(cancellationToken);
                seconds -= Time.deltaTime;
            }
        }
    }
}
