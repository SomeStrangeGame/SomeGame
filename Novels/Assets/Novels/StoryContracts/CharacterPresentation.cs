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
            string[] assetCandidates)
        {
            IsChild = isChild;
            RemoveClothes = removeClothes;
            RemoveHair = removeHair;
            RemoveAccessory = removeAccessory;
            AssetCandidates = assetCandidates ?? Array.Empty<string>();
        }

        public bool IsChild { get; }
        public bool RemoveClothes { get; }
        public bool RemoveHair { get; }
        public bool RemoveAccessory { get; }
        public string[] AssetCandidates { get; }
    }
}
