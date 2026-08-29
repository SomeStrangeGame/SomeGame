using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.Character
{
    [Serializable]
    public struct CharacterSpriteTrimEntry
    {
        [SerializeField] private string _assetAddress;
        [SerializeField] private int _originalWidth;
        [SerializeField] private int _originalHeight;
        [SerializeField] private int _cropX;
        [SerializeField] private int _cropY;
        [SerializeField] private int _cropWidth;
        [SerializeField] private int _cropHeight;
        [SerializeField] private string _trimmedSha256;

        public CharacterSpriteTrimEntry(
            string assetAddress,
            int originalWidth,
            int originalHeight,
            RectInt crop,
            string trimmedSha256 = null)
        {
            _assetAddress = assetAddress;
            _originalWidth = originalWidth;
            _originalHeight = originalHeight;
            _cropX = crop.x;
            _cropY = crop.y;
            _cropWidth = crop.width;
            _cropHeight = crop.height;
            _trimmedSha256 = trimmedSha256;
        }

        public string AssetAddress => _assetAddress;
        public int OriginalWidth => _originalWidth;
        public int OriginalHeight => _originalHeight;
        public RectInt Crop => new(_cropX, _cropY, _cropWidth, _cropHeight);
        public string TrimmedSha256 => _trimmedSha256;

        internal CharacterSpriteTrimLayout Layout => new(
            _originalWidth,
            _originalHeight,
            Crop);
    }

    public sealed class CharacterSpriteTrimManifest : ScriptableObject
    {
        [SerializeField] private List<CharacterSpriteTrimEntry> _entries = new();

        private Dictionary<string, CharacterSpriteTrimLayout> _byAddress;

        internal bool TryGetLayout(
            string assetAddress,
            out CharacterSpriteTrimLayout layout)
        {
            EnsureIndex();
            return _byAddress.TryGetValue(assetAddress, out layout);
        }

        public IReadOnlyList<CharacterSpriteTrimEntry> Entries => _entries;

#if UNITY_EDITOR
        public void ReplaceEntries(IEnumerable<CharacterSpriteTrimEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
            _entries.Sort((left, right) => string.Compare(
                left.AssetAddress,
                right.AssetAddress,
                StringComparison.Ordinal));
            _byAddress = null;
        }
#endif

        private void EnsureIndex()
        {
            if (_byAddress != null)
                return;
            _byAddress = new Dictionary<string, CharacterSpriteTrimLayout>(
                _entries.Count,
                StringComparer.OrdinalIgnoreCase);
            foreach (var entry in _entries)
            {
                if (!string.IsNullOrWhiteSpace(entry.AssetAddress))
                    _byAddress[entry.AssetAddress] = entry.Layout;
            }
        }
    }

    internal readonly struct CharacterSpriteTrimLayout
    {
        internal readonly int OriginalWidth;
        internal readonly int OriginalHeight;
        internal readonly RectInt Crop;

        internal CharacterSpriteTrimLayout(
            int originalWidth,
            int originalHeight,
            RectInt crop)
        {
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            Crop = crop;
        }

        internal bool IsValid => OriginalWidth > 0
            && OriginalHeight > 0
            && Crop.width > 0
            && Crop.height > 0;
    }
}
