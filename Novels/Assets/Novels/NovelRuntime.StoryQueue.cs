using System;
using System.Collections.Generic;
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
            System.Func<string, Cysharp.Threading.Tasks.UniTask<UnityEngine.Sprite>> loadBubbleChoiceIcon,
            Save.SaveSystem save)
        {
            RestoreWardrobe(presentation.Character, save, _definition.MainCharacter);
            presentation.Wardrobe.ConfigureFree(() =>
                CreateFreeWardrobePresentation(
                    presentation.Character,
                    save,
                    _definition.MainCharacter),
                async characterTarget =>
                {
                    await presentation.Bubble.Hide(
                        StoryContracts.PresentationMode.Immediate);
                    await presentation.Character.BeginWardrobePreview(characterTarget);
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
                    LoadBubbleChoiceIcon = loadBubbleChoiceIcon,
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
            string characterName) =>
            new FreeWardrobeSession(character, save, characterName).CreateInitial();

        private sealed class FreeWardrobeSession
        {
            private readonly Character.CharacterController _character;
            private readonly Save.SaveSystem _save;
            private readonly WardrobeCharacter[] _characters;
            private readonly Dictionary<string, Dictionary<
                WardrobeContracts.WardrobeCategory, string>> _original;
            private readonly Dictionary<string, Dictionary<
                WardrobeContracts.WardrobeCategory, string>> _pending;
            private int _characterIndex;

            internal FreeWardrobeSession(
                Character.CharacterController character,
                Save.SaveSystem save,
                string mainCharacter)
            {
                _character = character;
                _save = save;
                var names = save.GetWardrobeCharacters()
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                names.RemoveAll(name => string.Equals(
                    name, mainCharacter, StringComparison.OrdinalIgnoreCase));
                names.Insert(0, mainCharacter);
                _characters = names.Select(name => new WardrobeCharacter(
                        name,
                        string.Equals(name, mainCharacter, StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : name))
                    .ToArray();
                _original = CaptureSelections();
                _pending = CloneSelections(_original);
            }

            internal WardrobeContracts.WardrobePresentation CreateInitial()
            {
                _characterIndex = 0;
                return CreatePresentation();
            }

            private WardrobeContracts.WardrobePresentation CreatePresentation()
            {
                var current = _characters[_characterIndex];
                var categories = GetCategories(current.Name);
                var initialCategory = categories.Contains(
                    WardrobeContracts.WardrobeCategory.Clothes)
                        ? WardrobeContracts.WardrobeCategory.Clothes
                        : categories.FirstOrDefault();
                var counts = new[] { 0, 0, 0, 0 };
                foreach (WardrobeContracts.WardrobeCategory category in
                         Enum.GetValues(typeof(WardrobeContracts.WardrobeCategory)))
                {
                    counts[(int)category] = _save.GetUnlockedWardrobeItems(
                        current.Name,
                        (byte)category).Length;
                }
                return new WardrobeContracts.WardrobePresentation(
                    current.Name,
                    initialCategory,
                    WardrobeCategoryTitle(initialCategory),
                    "Готово",
                    Array.Empty<WardrobeContracts.WardrobeOption>(),
                    category => LoadUnlockedWardrobeItems(
                        _character,
                        _save,
                        current.Name,
                        category),
                    (category, value) =>
                    {
                        _character.SetWardrobeTarget(current.Target);
                        return _character.LoadWardrobeThumbnail(
                            ToChoiceAction(category), value);
                    },
                    (category, value) =>
                    {
                        _pending[current.Name][category] = value;
                        _character.SetWardrobeTarget(current.Target);
                        return _character.PreviewWardrobeChoice(
                            ToChoiceAction(category), value);
                    },
                    _ => UniTask.FromResult<Sprite>(null),
                    _ => UniTask.CompletedTask,
                    _ => { },
                    true,
                    true,
                    availableCategories: categories,
                    getSelectedCategoryValue: category =>
                        _pending[current.Name][category],
                    characterTarget: current.Target,
                    characterCount: _characters.Length,
                    loadRelativeCharacter: SwitchCharacter,
                    commitFreeSession: Commit,
                    cancelFreeSession: Cancel,
                    categoryItemCounts: counts);
            }

            private async UniTask<WardrobeContracts.WardrobePresentation>
                SwitchCharacter(int direction)
            {
                _characterIndex = (_characterIndex + direction + _characters.Length)
                    % _characters.Length;
                var current = _characters[_characterIndex];
                ApplySelections(current, _pending[current.Name]);
                await _character.SwitchWardrobePreview(current.Target);
                return CreatePresentation();
            }

            private WardrobeContracts.WardrobeCategory[] GetCategories(string name) =>
                Enum.GetValues(typeof(WardrobeContracts.WardrobeCategory))
                    .Cast<WardrobeContracts.WardrobeCategory>()
                    .Where(category => _save.GetUnlockedWardrobeItems(
                        name, (byte)category).Length > 0)
                    .ToArray();

            private Dictionary<string, Dictionary<
                WardrobeContracts.WardrobeCategory, string>> CaptureSelections()
            {
                var result = new Dictionary<string, Dictionary<
                    WardrobeContracts.WardrobeCategory, string>>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var current in _characters)
                {
                    var selections = new Dictionary<
                        WardrobeContracts.WardrobeCategory, string>();
                    foreach (WardrobeContracts.WardrobeCategory category in
                             Enum.GetValues(typeof(WardrobeContracts.WardrobeCategory)))
                    {
                        var equipped = _save.GetEquippedWardrobeItem(
                            current.Name, (byte)category);
                        selections[category] = string.IsNullOrWhiteSpace(equipped)
                            ? _character.GetCurrentWardrobeValue(
                                current.Target, ToChoiceAction(category))
                            : equipped;
                    }
                    result[current.Name] = selections;
                }
                return result;
            }

            private static Dictionary<string, Dictionary<
                WardrobeContracts.WardrobeCategory, string>> CloneSelections(
                Dictionary<string, Dictionary<
                    WardrobeContracts.WardrobeCategory, string>> source)
            {
                var result = new Dictionary<string, Dictionary<
                    WardrobeContracts.WardrobeCategory, string>>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var pair in source)
                {
                    result[pair.Key] = new Dictionary<
                        WardrobeContracts.WardrobeCategory, string>(pair.Value);
                }
                return result;
            }

            private void Commit()
            {
                foreach (var current in _characters)
                {
                    ApplySelections(current, _pending[current.Name]);
                    foreach (var selection in _pending[current.Name])
                    {
                        if (string.IsNullOrWhiteSpace(selection.Value))
                            continue;
                        _save.UnlockWardrobeItem(
                            current.Name,
                            (byte)selection.Key,
                            selection.Value,
                            false);
                    }
                }
                _save.PersistWardrobe();
            }

            private void Cancel()
            {
                foreach (var current in _characters)
                    ApplySelections(current, _original[current.Name]);
            }

            private void ApplySelections(
                WardrobeCharacter current,
                Dictionary<WardrobeContracts.WardrobeCategory, string> selections)
            {
                foreach (var selection in selections)
                {
                    if (!string.IsNullOrWhiteSpace(selection.Value))
                    {
                        _character.ApplyWardrobeSelection(
                            current.Target,
                            ToChoiceAction(selection.Key),
                            selection.Value);
                    }
                }
            }

            private readonly struct WardrobeCharacter
            {
                internal WardrobeCharacter(string name, string target)
                {
                    Name = name ?? string.Empty;
                    Target = target ?? string.Empty;
                }

                internal string Name { get; }
                internal string Target { get; }
            }
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
            var characters = save.GetWardrobeCharacters()
                .Concat(new[] { characterName })
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var current in characters)
            {
                var target = string.Equals(
                    current, characterName, StringComparison.OrdinalIgnoreCase)
                        ? string.Empty
                        : current;
                foreach (WardrobeContracts.WardrobeCategory category in
                         Enum.GetValues(typeof(WardrobeContracts.WardrobeCategory)))
                {
                    var value = save.GetEquippedWardrobeItem(current, (byte)category);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        character.ApplyWardrobeSelection(
                            target,
                            ToChoiceAction(category),
                            value);
                    }
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
