using System;

namespace Novels.StoryContracts
{
    public sealed class CharacterPresentation
    {
        public CharacterPresentation(
            bool isChild,
            bool removeClothes,
            bool removeHair,
            bool removeAccessory,
            string displayName,
            StoryCharacterPosition? position,
            StoryCharacterVisibilityCommand visibility,
            bool hasUnsupportedTimedChoice,
            string[] assetCandidates,
            string requestedClothes = null)
        {
            IsChild = isChild;
            RemoveClothes = removeClothes;
            RemoveHair = removeHair;
            RemoveAccessory = removeAccessory;
            DisplayName = displayName ?? string.Empty;
            Position = position;
            Visibility = visibility;
            HasUnsupportedTimedChoice = hasUnsupportedTimedChoice;
            AssetCandidates = assetCandidates ?? Array.Empty<string>();
            RequestedClothes = requestedClothes?.Trim() ?? string.Empty;
        }

        public bool IsChild { get; }
        public bool RemoveClothes { get; }
        public bool RemoveHair { get; }
        public bool RemoveAccessory { get; }
        public string DisplayName { get; }
        public StoryCharacterPosition? Position { get; }
        public StoryCharacterVisibilityCommand Visibility { get; }

        // Reserved for the future timed-choice implementation. Runtime intentionally
        // ignores this authoring instruction until the choice timer is supported.
        public bool HasUnsupportedTimedChoice { get; }

        public string[] AssetCandidates { get; }
        public string RequestedClothes { get; }
    }

    public enum StoryCharacterVisibilityCommand
    {
        Unchanged,
        Hide,
        Show,
    }
}
